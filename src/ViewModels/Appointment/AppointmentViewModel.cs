using System;
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
                (EditAppointmentCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (DeleteAppointmentCommand as RelayCommand)?.NotifyCanExecuteChanged();
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

        public AppointmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Appointments = new ObservableCollection<Models.Appointment>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); SearchAppointments(); };

            AddAppointmentCommand = new RelayCommand(OpenAddAppointmentView);
            EditAppointmentCommand = new RelayCommand(OpenEditAppointmentView, CanEditOrDelete);
            DeleteAppointmentCommand = new RelayCommand(DeleteAppointment, CanEditOrDelete);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(SearchAppointments);
            SortCommand = new RelayCommand(SortAppointments);

            LoadAppointments();
        }

        private void LoadAppointments()
        {
            try
            {
                var list = _context.Appointment
                    .Include(a => a.Patient)
                    .Include(a => a.Dentist)
                    .Include(a => a.Treatment)
                    .ToList();
                Appointments.Clear();
                foreach (var a in list) Appointments.Add(a);
                UpdateResultsLabel(list.Count, false);
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
            OnPropertyChanged(nameof(SearchText));
            LoadAppointments();
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

        private bool CanEditOrDelete() => SelectedAppointment != null;

        private void OpenAddAppointmentView()
        {
            var w = new AddAppointmentView { DataContext = new AddAppointmentViewModel(_context) };
            if (w.ShowDialog() == true) LoadAppointments();
        }

        private void OpenEditAppointmentView()
        {
            if (SelectedAppointment == null) return;
            var w = new EditAppointmentView { DataContext = new EditAppointmentViewModel(_context, SelectedAppointment) };
            if (w.ShowDialog() == true) LoadAppointments();
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
                    LoadAppointments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la cita: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchAppointments()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadAppointments(); return; }
            var term = SearchText.Trim().ToLower();
            var filtered = _context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Dentist)
                .Include(a => a.Treatment)
                .ToList()
                .Where(a =>
                    (a.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                    (a.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                    (a.Dentist?.FirstName?.ToLower().Contains(term) ?? false) ||
                    (a.Dentist?.LastName?.ToLower().Contains(term) ?? false) ||
                    (a.Treatment?.Name?.ToLower().Contains(term) ?? false))
                .ToList();
            Appointments.Clear();
            foreach (var a in filtered) Appointments.Add(a);
            UpdateResultsLabel(filtered.Count, true);
        }
    }
}
