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
    public class AppointmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Appointment> Appointments { get; private set; }
        public ObservableCollection<WeekDayCard> WeekDays { get; } = new ObservableCollection<WeekDayCard>();

        // ----- Week navigation -----
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

        // ----- Day filter -----
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

        // ----- Search -----
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

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre del Paciente", "Nombre del Dentista", "Nombre del Tratamiento", "Fecha de Cita"
        };

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged();
                    SortAppointments();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand AddAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand FilterByDayCommand { get; }
        public ICommand ClearDayFilterCommand { get; }

        public AppointmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Appointments = new ObservableCollection<Models.Appointment>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); ApplyFilters(); };

            AddAppointmentCommand = new RelayCommand(OpenAddAppointmentView);
            EditAppointmentCommand = new RelayCommand<Models.Appointment>(OpenEditAppointmentWithParam, p => p != null);
            DeleteAppointmentCommand = new RelayCommand<Models.Appointment>(DeleteAppointmentWithParam, p => p != null);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(ApplyFilters);
            SortCommand = new RelayCommand(SortAppointments);
            PreviousWeekCommand = new RelayCommand(() => WeekStart = WeekStart.AddDays(-7));
            NextWeekCommand = new RelayCommand(() => WeekStart = WeekStart.AddDays(7));
            FilterByDayCommand = new RelayCommand<WeekDayCard>(FilterByDay, c => c != null);
            ClearDayFilterCommand = new RelayCommand(() => SelectedDayFilter = null);

            BuildWeekDays();
            ApplyFilters();
        }

        // ---------- week helpers ----------

        private static DateTime StartOfWeek(DateTime d)
        {
            int diff = ((int)d.DayOfWeek + 6) % 7; // Monday = 0
            return d.Date.AddDays(-diff);
        }

        private static string GetSpanishDayName(DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Monday: return "Lun";
                case DayOfWeek.Tuesday: return "Mar";
                case DayOfWeek.Wednesday: return "Mié";
                case DayOfWeek.Thursday: return "Jue";
                case DayOfWeek.Friday: return "Vie";
                case DayOfWeek.Saturday: return "Sáb";
                case DayOfWeek.Sunday: return "Dom";
                default: return "";
            }
        }

        private void BuildWeekDays()
        {
            var start = WeekStart;
            var end = start.AddDays(7);
            var today = DateTime.Today;

            var weekAppts = _context.Appointment
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

        private void UpdateWeekDaySelection()
        {
            foreach (var d in WeekDays)
                d.IsSelected = _selectedDayFilter.HasValue && _selectedDayFilter.Value.Date == d.Date.Date;
        }

        private void FilterByDay(WeekDayCard card)
        {
            if (card == null) return;
            // Toggle: si ya estaba seleccionado, quitar filtro
            if (_selectedDayFilter.HasValue && _selectedDayFilter.Value.Date == card.Date.Date)
                SelectedDayFilter = null;
            else
                SelectedDayFilter = card.Date;
        }

        // ---------- data ----------

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

                var list = query.ToList();

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

                Appointments.Clear();
                foreach (var a in list) Appointments.Add(a);

                bool filtered = _selectedDayFilter.HasValue || !string.IsNullOrWhiteSpace(_searchText);
                UpdateResultsLabel(list.Count, filtered);
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
            _selectedDayFilter = null;
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(SelectedDayFilter));
            OnPropertyChanged(nameof(HasDayFilter));
            OnPropertyChanged(nameof(DayFilterLabel));
            BuildWeekDays();
            ApplyFilters();
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            if (count == 0) ResultsLabel = "Sin resultados";
            else if (count == 1) ResultsLabel = filtered ? "1 resultado" : "1 cita";
            else ResultsLabel = filtered ? $"{count} resultados" : $"{count} citas";
        }

        private void SortAppointments()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Nombre del Paciente" => Appointments.OrderBy(a => a.Patient?.LastName).ThenBy(a => a.Patient?.FirstName).ToList(),
                "Nombre del Dentista" => Appointments.OrderBy(a => a.Dentist?.LastName).ThenBy(a => a.Dentist?.FirstName).ToList(),
                "Nombre del Tratamiento" => Appointments.OrderBy(a => a.Treatment?.Name).ToList(),
                "Fecha de Cita" => Appointments.OrderBy(a => a.AppointmentDate).ToList(),
                _ => Appointments.ToList()
            };
            Appointments.Clear();
            foreach (var a in sorted) Appointments.Add(a);
        }

        private void OpenAddAppointmentView()
        {
            var w = new AddAppointmentView { DataContext = new AddAppointmentViewModel(_context) };
            if (w.ShowDialog() == true) { BuildWeekDays(); ApplyFilters(); }
        }

        private void OpenEditAppointmentWithParam(Models.Appointment appt)
        {
            if (appt == null) return;
            SelectedAppointment = appt;
            OpenEditAppointmentView();
        }

        private void OpenEditAppointmentView()
        {
            if (SelectedAppointment == null) return;
            var w = new EditAppointmentView { DataContext = new EditAppointmentViewModel(_context, SelectedAppointment) };
            if (w.ShowDialog() == true) { BuildWeekDays(); ApplyFilters(); }
        }

        private void DeleteAppointmentWithParam(Models.Appointment appt)
        {
            if (appt == null) return;
            SelectedAppointment = appt;
            DeleteAppointment();
        }

        private void DeleteAppointment()
        {
            if (SelectedAppointment == null) return;
            var result = MessageBox.Show("¿Seguro que quieres eliminar esta cita?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Appointment.Remove(SelectedAppointment);
                    _context.SaveChanges();
                    BuildWeekDays();
                    ApplyFilters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la cita: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
