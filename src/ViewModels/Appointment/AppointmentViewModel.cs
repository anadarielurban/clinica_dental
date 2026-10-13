using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        public ObservableCollection<Models.Appointment> Appointments { get; set; }
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

        public string SearchText { get; set; }

        // Nowe właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre del Paciente",
            "Nombre del Dentista",
            "Nombre del Tratamiento",
            "Fecha de Cita"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public AppointmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            LoadAppointments();

            AddAppointmentCommand = new RelayCommand(OpenAddAppointmentView);
            EditAppointmentCommand = new RelayCommand(OpenEditAppointmentView, CanEditOrDeleteAppointment);
            DeleteAppointmentCommand = new RelayCommand(DeleteAppointment, CanEditOrDeleteAppointment);
            ReloadCommand = new RelayCommand(LoadAppointments);
            SearchCommand = new RelayCommand(SearchAppointments);
            SortCommand = new RelayCommand(SortAppointments); // Nowa komenda
        }

        private void LoadAppointments()
        {
            try
            {
                var appointmentsQuery = _context.Appointment
                    .Include(a => a.Patient)
                    .Include(a => a.Dentist)
                    .Include(a => a.Treatment)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    appointmentsQuery = appointmentsQuery.Where(a =>
                        a.Patient.FirstName.Contains(SearchText) ||
                        a.Patient.LastName.Contains(SearchText) ||
                        a.Treatment.Name.Contains(SearchText));
                }

                var appointments = appointmentsQuery.ToList();
                Appointments = new ObservableCollection<Models.Appointment>(appointments);
                OnPropertyChanged(nameof(Appointments));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las citas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortAppointments()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Appointments == null)
                return;

            IEnumerable<Models.Appointment> sortedAppointments = SelectedSortOption switch
            {
                "Nombre del Paciente" => Appointments.OrderBy(a => a.Patient.LastName).ThenBy(a => a.Patient.FirstName),
                "Nombre del Dentista" => Appointments.OrderBy(a => a.Dentist.LastName).ThenBy(a => a.Dentist.FirstName),
                "Nombre del Tratamiento" => Appointments.OrderBy(a => a.Treatment.Name),
                "Fecha de Cita" => Appointments.OrderBy(a => a.AppointmentDate),
                _ => Appointments
            };

            Appointments = new ObservableCollection<Models.Appointment>(sortedAppointments);
            OnPropertyChanged(nameof(Appointments));
        }

        private bool CanEditOrDeleteAppointment()
        {
            return SelectedAppointment != null;
        }

        private void OpenAddAppointmentView()
        {
            var addAppointmentWindow = new AddAppointmentView
            {
                DataContext = new AddAppointmentViewModel(_context)
            };

            if (addAppointmentWindow.ShowDialog() == true)
            {
                LoadAppointments();
            }
        }

        private void OpenEditAppointmentView()
        {
            if (SelectedAppointment == null)
            {
                MessageBox.Show("Selecciona una cita para editar.", "Sin Selección", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editAppointmentWindow = new EditAppointmentView
            {
                DataContext = new EditAppointmentViewModel(_context, SelectedAppointment)
            };

            if (editAppointmentWindow.ShowDialog() == true)
            {
                LoadAppointments();
            }
        }

        private void DeleteAppointment()
        {
            if (SelectedAppointment == null)
            {
                MessageBox.Show("Selecciona una cita para eliminar.", "Sin Selección", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "¿Seguro que quieres eliminar esta cita?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Appointment.Remove(SelectedAppointment);
                    _context.SaveChanges();
                    LoadAppointments();
                    MessageBox.Show("¡Cita eliminada correctamente!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la cita: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchAppointments()
        {
            LoadAppointments();
        }
    }
}
