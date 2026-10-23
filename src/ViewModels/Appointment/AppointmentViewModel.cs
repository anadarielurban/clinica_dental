using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.Views.Appointment;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Appointment
{
    public class MonthDayCard
    {
        public DateTime Date { get; set; }
        public string DayNumber => Date.Day.ToString();
        public bool IsCurrentMonth { get; set; }
        public bool IsToday => Date.Date == DateTime.Today;
        public int AppointmentCount { get; set; }
        public List<Models.Appointment> DayAppointments { get; set; } = new();
    }

    public class AppointmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly DispatcherTimer _searchTimer;

        private List<Models.Appointment> _allFilteredAppointments = new();
        private int _currentPage = 1;
        private int _pageSize = 15;
        public ObservableCollection<Models.Appointment> Appointments { get; private set; }

        public ObservableCollection<WeekDayCard> WeekDays { get; } = new ObservableCollection<WeekDayCard>();
        public ObservableCollection<MonthDayCard> MonthDays { get; } = new ObservableCollection<MonthDayCard>();

        public string PageInfo => $"Página {_currentPage} de {Math.Max(1, (int)Math.Ceiling((double)_allFilteredAppointments.Count / _pageSize))}";
        public bool CanGoNextPage => _currentPage < (int)Math.Ceiling((double)_allFilteredAppointments.Count / _pageSize);
        public bool CanGoPrevPage => _currentPage > 1;

        private bool _isMonthlyView;
        public bool IsMonthlyView
        {
            get => _isMonthlyView;
            set
            {
                if (SetProperty(ref _isMonthlyView, value))
                {
                    OnPropertyChanged(nameof(IsWeeklyView));
                    if (value) BuildMonthDays();
                    else BuildWeekDays();
                }
            }
        }
        public bool IsWeeklyView => !IsMonthlyView;

        private DateTime _weekStart = StartOfWeek(DateTime.Today);
        public DateTime WeekStart
        {
            get => _weekStart;
            set
            {
                if (SetProperty(ref _weekStart, value))
                {
                    OnPropertyChanged(nameof(WeekRangeLabel));
                    BuildWeekDays();
                }
            }
        }
        public string WeekRangeLabel
        {
            get
            {
                var end = WeekStart.AddDays(6);
                return $"{WeekStart:dd MMM}  -  {end:dd MMM yyyy}";
            }
        }

        private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                if (SetProperty(ref _currentMonth, value))
                {
                    OnPropertyChanged(nameof(MonthYearLabel));
                    BuildMonthDays();
                }
            }
        }
        public string MonthYearLabel => CurrentMonth.ToString("MMMM yyyy").ToUpper();

        private DateTime? _selectedDayFilter;
        public DateTime? SelectedDayFilter
        {
            get => _selectedDayFilter;
            set
            {
                if (SetProperty(ref _selectedDayFilter, value))
                {
                    OnPropertyChanged(nameof(HasDayFilter));
                    OnPropertyChanged(nameof(DayFilterLabel));
                    UpdateWeekDaySelection();
                    ApplyFilters();
                }
            }
        }
        public bool HasDayFilter => _selectedDayFilter.HasValue;
        public string DayFilterLabel
        {
            get
            {
                if (!_selectedDayFilter.HasValue) return string.Empty;
                var d = _selectedDayFilter.Value;
                return $"Filtrando por: {GetSpanishDayName(d.DayOfWeek)} {d:dd MMM}";
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    _searchTimer.Stop();
                    _searchTimer.Start();
                }
            }
        }

        private string _resultsLabel = string.Empty;
        public string ResultsLabel { get => _resultsLabel; set { _resultsLabel = value; OnPropertyChanged(); } }

        private Models.Appointment _selectedAppointment;
        public Models.Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                _selectedAppointment = value;
                OnPropertyChanged();
                (EditAppointmentCommand as RelayCommand<Models.Appointment>)?.NotifyCanExecuteChanged();
                (DeleteAppointmentCommand as RelayCommand<Models.Appointment>)?.NotifyCanExecuteChanged();
            }
        }

        public ICommand AddAppointmentCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand FilterByDayCommand { get; }
        public ICommand ClearDayFilterCommand { get; }
        public ICommand SwitchViewCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public IRelayCommand<Models.Appointment> EditAppointmentCommand { get; }
        public IRelayCommand<Models.Appointment> DeleteAppointmentCommand { get; }
        public IRelayCommand<Models.Appointment> SendWhatsAppCommand { get; }

        public AppointmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Appointments = new ObservableCollection<Models.Appointment>();
            
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); ApplyFilters(); };

            AddAppointmentCommand = new RelayCommand(AddAppointment);
            EditAppointmentCommand = new RelayCommand<Models.Appointment>(OpenEditAppointmentWithParam);
            DeleteAppointmentCommand = new RelayCommand<Models.Appointment>(DeleteWithParam);
            SendWhatsAppCommand = new RelayCommand<Models.Appointment>(SendWhatsApp);
            
            FilterByDayCommand = new RelayCommand<WeekDayCard>(FilterByDay);
            ClearDayFilterCommand = new RelayCommand(() => SelectedDayFilter = null);
            
            PreviousWeekCommand = new RelayCommand(() => WeekStart = WeekStart.AddDays(-7));
            NextWeekCommand = new RelayCommand(() => WeekStart = WeekStart.AddDays(7));
            
            PreviousMonthCommand = new RelayCommand(() => CurrentMonth = CurrentMonth.AddMonths(-1));
            NextMonthCommand = new RelayCommand(() => CurrentMonth = CurrentMonth.AddMonths(1));
            
            SwitchViewCommand = new RelayCommand(() => IsMonthlyView = !IsMonthlyView);

            NextPageCommand = new RelayCommand(() => ChangePage(1), () => CanGoNextPage);
            PrevPageCommand = new RelayCommand(() => ChangePage(-1), () => CanGoPrevPage);

            ReloadCommand = new RelayCommand(ReloadAll);

            BuildWeekDays();
            BuildMonthDays();
            SelectedDayFilter = DateTime.Today.AddDays(1); // Filtro por defecto: mañana
        }

        private void ChangePage(int direction)
        {
            _currentPage += direction;
            UpdatePagedAppointments();
            OnPropertyChanged(nameof(PageInfo));
            OnPropertyChanged(nameof(CanGoNextPage));
            OnPropertyChanged(nameof(CanGoPrevPage));
            (NextPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (PrevPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        private void UpdatePagedAppointments()
        {
            Appointments.Clear();
            var paged = _allFilteredAppointments.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            foreach (var a in paged) Appointments.Add(a);
        }

        private static DateTime StartOfWeek(DateTime d)
        {
            int diff = ((int)d.DayOfWeek + 6) % 7;
            return d.Date.AddDays(-diff);
        }

        private static string GetSpanishDayName(DayOfWeek dow)
        {
            return dow switch
            {
                DayOfWeek.Monday => "Lun",
                DayOfWeek.Tuesday => "Mar",
                DayOfWeek.Wednesday => "Mié",
                DayOfWeek.Thursday => "Jue",
                DayOfWeek.Friday => "Vie",
                DayOfWeek.Saturday => "Sáb",
                DayOfWeek.Sunday => "Dom",
                _ => ""
            };
        }

        private void BuildWeekDays()
        {
            var start = WeekStart;
            var end = start.AddDays(7);
            var today = DateTime.Today;

            var weekAppts = _context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Treatment)
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate < end)
                .ToList();

            var cards = new List<WeekDayCard>();
            for (int i = 0; i < 7; i++)
            {
                var date = start.AddDays(i);
                cards.Add(new WeekDayCard
                {
                    Date = date,
                    DayName = GetSpanishDayName(date.DayOfWeek),
                    DayNumber = date.Day.ToString(),
                    AppointmentCount = weekAppts.Count(a => a.AppointmentDate.Date == date.Date),
                    IsToday = date.Date == today,
                    IsSelected = _selectedDayFilter.HasValue && _selectedDayFilter.Value.Date == date.Date
                });
            }

            WeekDays.Clear();
            foreach (var c in cards) WeekDays.Add(c);
        }

        private void BuildMonthDays()
        {
            MonthDays.Clear();
            var firstOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            
            int diff = ((int)firstOfMonth.DayOfWeek + 6) % 7;
            var startDate = firstOfMonth.AddDays(-diff);

            var endDate = startDate.AddDays(42);
            var monthAppts = _context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Treatment)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate < endDate)
                .ToList();

            for (int i = 0; i < 42; i++)
            {
                var date = startDate.AddDays(i);
                var dayAppts = monthAppts.Where(a => a.AppointmentDate.Date == date.Date).ToList();

                MonthDays.Add(new MonthDayCard
                {
                    Date = date,
                    IsCurrentMonth = date.Month == CurrentMonth.Month,
                    AppointmentCount = dayAppts.Count,
                    DayAppointments = dayAppts
                });
            }
        }

        private void UpdateWeekDaySelection()
        {
            foreach (var d in WeekDays)
                d.IsSelected = _selectedDayFilter.HasValue && _selectedDayFilter.Value.Date == d.Date.Date;
        }

        private void FilterByDay(WeekDayCard card)
        {
            if (card == null) return;
            if (_selectedDayFilter.HasValue && _selectedDayFilter.Value.Date == card.Date.Date)
                SelectedDayFilter = null;
            else
                SelectedDayFilter = card.Date;
        }

        private void ApplyFilters()
        {
            try
            {
                var query = _context.Appointment
                    .Include(a => a.Patient)
                    .Include(a => a.Dentist)
                    .Include(a => a.Treatment)
                    .AsQueryable();

                if (_selectedDayFilter.HasValue)
                {
                    var d = _selectedDayFilter.Value.Date;
                    var next = d.AddDays(1);
                    query = query.Where(a => a.AppointmentDate >= d && a.AppointmentDate < next);
                }

                // FIX: Ordenar en memoria para evitar el error de TimeSpan en SQLite
                var list = query.ToList().OrderBy(a => a.StartTime).ToList();

                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    var term = _searchText.Trim().ToLower();
                    list = list.Where(a =>
                        (a.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                        (a.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                        (a.Dentist?.FirstName?.ToLower().Contains(term) ?? false) ||
                        (a.Dentist?.LastName?.ToLower().Contains(term) ?? false) ||
                        (a.Treatment?.Name?.ToLower().Contains(term) ?? false))
                        .ToList();
                }

                _allFilteredAppointments = list;
                _currentPage = 1;
                UpdatePagedAppointments();

                bool filtered = _selectedDayFilter.HasValue || !string.IsNullOrWhiteSpace(_searchText);
                UpdateResultsLabel(list.Count, filtered);
                
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoNextPage));
                OnPropertyChanged(nameof(CanGoPrevPage));
                (NextPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (PrevPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las citas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            SelectedDayFilter = DateTime.Today.AddDays(1);
            OnPropertyChanged(nameof(SearchText));
            BuildWeekDays();
            BuildMonthDays();
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            if (count == 0) ResultsLabel = "Sin resultados";
            else if (count == 1) ResultsLabel = filtered ? "1 resultado" : "1 cita";
            else ResultsLabel = filtered ? $"{count} resultados" : $"{count} citas";
        }

        private void AddAppointment()
        {
            try
            {
                var w = new AddAppointmentView { DataContext = new AddAppointmentViewModel(_context) };
                if (w.ShowDialog() == true) { ReloadAll(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error al abrir ventana"); }
        }

        private void OpenEditAppointmentWithParam(Models.Appointment appt)
        {
            if (appt == null) return;
            SelectedAppointment = appt;
            var w = new EditAppointmentView { DataContext = new EditAppointmentViewModel(_context, SelectedAppointment) };
            if (w.ShowDialog() == true) { ReloadAll(); }
        }

        private void DeleteWithParam(Models.Appointment appt)
        {
            if (appt == null) return;
            if (MessageBox.Show($"¿Estás seguro de eliminar la cita de {appt.Patient?.FirstName}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Appointment.Remove(appt);
                    _context.SaveChanges();
                    ReloadAll();
                }
                catch (Exception ex) { MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void SendWhatsApp(Models.Appointment appt)
        {
            if (appt == null || appt.Patient == null) return;
            string phone = System.Text.RegularExpressions.Regex.Replace(appt.Patient.PhoneNumber ?? "", @"[^\d]", "");
            if (string.IsNullOrEmpty(phone)) { MessageBox.Show("El paciente no tiene teléfono registrado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (phone.Length == 10) phone = "52" + phone;

            string fecha = appt.AppointmentDate.ToString("dd/MM/yyyy HH:mm");
            string mensaje = $"Hola {appt.Patient.FirstName}, te recordamos tu cita en Clínica Dental el {fecha}. ¡Te esperamos!";
            string url = $"https://wa.me/{phone}?text={System.Uri.EscapeDataString(mensaje)}";

            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error"); }
        }
    }
}