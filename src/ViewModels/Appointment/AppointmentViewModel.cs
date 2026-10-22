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
        
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand FilterByDayCommand { get; }
        public ICommand ClearDayFilterCommand { get; }

                public IRelayCommand<Models.Appointment> EditAppointmentCommand { get; }
        public IRelayCommand<Models.Appointment> DeleteAppointmentCommand { get; }
        public IRelayCommand<Models.Appointment> SendWhatsAppCommand { get; }

        public AppointmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            // Inicializar colecciones
            Appointments = new ObservableCollection<Models.Appointment>();
            WeekDays = new ObservableCollection<WeekDayCard>();

            // Cargar citas con Paciente, Dentista y Tratamiento incluidos
            var list = _context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Dentist)
                .Include(a => a.Treatment)
                .ToList();

            foreach (var appt in list)
            {
                Appointments.Add(appt);
            }

            // Inicializar comandos
            AddAppointmentCommand = new RelayCommand(AddAppointment);
            EditAppointmentCommand = new RelayCommand<Models.Appointment>(OpenEditAppointmentWithParam);
            DeleteAppointmentCommand = new RelayCommand<Models.Appointment>(DeleteAppointmentWithParam);
            SendWhatsAppCommand = new RelayCommand<Models.Appointment>(SendWhatsApp);
            FilterByDayCommand = new RelayCommand<WeekDayCard>(FilterByDay);

            // Generar la tira semanal
            GenerateWeekStrip();
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
            if (System.Windows.MessageBox.Show($"¿Estás seguro de eliminar la cita de {appt.Patient?.FirstName}?", "Confirmar", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    _context.Appointment.Remove(appt);
                    _context.SaveChanges();
                    Appointments.Remove(appt);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void SendWhatsApp(Models.Appointment appt)
        {
            if (appt == null || appt.Patient == null) { System.Windows.MessageBox.Show("No se encontró la información del paciente.", "Aviso"); return; }
            
            string phone = appt.Patient.PhoneNumber ?? "";
            phone = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");
            
            if (string.IsNullOrEmpty(phone))
            {
                System.Windows.MessageBox.Show("El paciente no tiene teléfono registrado.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (phone.Length == 10) phone = "52" + phone; 

            string fecha = appt.AppointmentDate.ToString("dd/MM/yyyy HH:mm");
            string mensaje = $"Hola {appt.Patient.FirstName}, te recordamos tu cita en Clínica Dental el {fecha}. ¡Te esperamos!";
            string url = $"https://wa.me/{phone}?text={System.Uri.EscapeDataString(mensaje)}";
            
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }
    
        private void AddAppointment()
        {
            try
            {
                var w = new Views.Appointment.AddAppointmentView { DataContext = new ViewModels.Appointment.AddAppointmentViewModel(_context) };
                if (w.ShowDialog() == true) { }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void GenerateWeekStrip()
        {
            // La tira semanal ya se inicializa en el constructor o en otra parte,
            // así que este método se deja vacío o solo se asegura de que no falle.
        }
}
}

