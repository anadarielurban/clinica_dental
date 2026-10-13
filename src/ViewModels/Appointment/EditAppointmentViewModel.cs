using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Appointment
{
    public class EditAppointmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        // Prywatne pola z publicznymi właściwościami
        private int _patientID;
        public int PatientID
        {
            get => _patientID;
            set
            {
                if (SetProperty(ref _patientID, value))
                {
                    ValidateProperty(nameof(PatientID));
                    NotifySaveCommand();
                }
            }
        }

        private int _dentistID;
        public int DentistID
        {
            get => _dentistID;
            set
            {
                if (SetProperty(ref _dentistID, value))
                {
                    ValidateProperty(nameof(DentistID));
                    NotifySaveCommand();
                }
            }
        }

        private int _treatmentID;
        public int TreatmentID
        {
            get => _treatmentID;
            set
            {
                if (SetProperty(ref _treatmentID, value))
                {
                    ValidateProperty(nameof(TreatmentID));
                    NotifySaveCommand();
                }
            }
        }

        private DateTime _appointmentDate;
        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set
            {
                if (SetProperty(ref _appointmentDate, value))
                {
                    ValidateProperty(nameof(AppointmentDate));
                    NotifySaveCommand();
                }
            }
        }

        private string _startTime;
        public string StartTime
        {
            get => _startTime;
            set
            {
                if (SetProperty(ref _startTime, value))
                {
                    ValidateProperty(nameof(StartTime));
                    ValidateProperty(nameof(EndTime)); // Aby upewnić się, że StartTime < EndTime
                    NotifySaveCommand();
                }
            }
        }

        private string _endTime;
        public string EndTime
        {
            get => _endTime;
            set
            {
                if (SetProperty(ref _endTime, value))
                {
                    ValidateProperty(nameof(EndTime));
                    ValidateProperty(nameof(StartTime)); // Aby upewnić się, że StartTime < EndTime
                    NotifySaveCommand();
                }
            }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                {
                    ValidateProperty(nameof(Notes));
                    NotifySaveCommand();
                }
            }
        }

        // Obiekt Appointment, który edytujemy
        public Models.Appointment Appointment { get; private set; }

        // Kolekcje do ComboBoxów
        public ObservableCollection<Models.Patient> Patients { get; }
        public ObservableCollection<Models.Dentist> Dentists { get; }
        public ObservableCollection<Models.Treatment> Treatments { get; }

        // Komendy
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditAppointmentViewModel(DentalClinicContext context, Models.Appointment appointment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            if (appointment == null) throw new ArgumentNullException(nameof(appointment));

            // Pobranie najnowszej wersji Appointment z bazy danych
            Appointment = _context.Appointment
                .FirstOrDefault(a => a.AppointmentID == appointment.AppointmentID) 
                ?? throw new ArgumentException("Cita no encontrada en la base de datos.");

            // Inicjalizacja kolekcji
            Patients = new ObservableCollection<Models.Patient>(_context.Patient.ToList());
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());
            Treatments = new ObservableCollection<Models.Treatment>(_context.Treatment.ToList());

            // Inicjalizacja pól z istniejącego Appointment
            _patientID = Appointment.PatientID;
            _dentistID = Appointment.DentistID;
            _treatmentID = Appointment.TreatmentID;
            _appointmentDate = Appointment.AppointmentDate;
            _startTime = Appointment.StartTime.ToString(@"hh\:mm");
            _endTime = Appointment.EndTime.ToString(@"hh\:mm");
            _notes = Appointment.Notes;

            // Inicjalizacja RelayCommand z metodami
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);

            // Subskrypcja PropertyChanged do walidacji
            PropertyChanged += (s, e) => ValidateProperty(e.PropertyName);
        }

        private void Save()
        {
            if (!CanSave())
            {
                MessageBox.Show("Corrige los errores de validación antes de guardar.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Przepisujemy z ViewModelu do obiektu Appointment
                Appointment.PatientID = PatientID;
                Appointment.DentistID = DentistID;
                Appointment.TreatmentID = TreatmentID;
                Appointment.AppointmentDate = AppointmentDate;
                Appointment.StartTime = TimeSpan.Parse(StartTime);
                Appointment.EndTime = TimeSpan.Parse(EndTime);
                Appointment.Notes = Notes;

                // Aktualizacja w bazie danych
                _context.Appointment.Update(Appointment);
                _context.SaveChanges();

                MessageBox.Show("¡Cita actualizada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la cita: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void Delete()
        {
            try
            {
                var result = MessageBox.Show(
                    "¿Seguro que quieres eliminar esta cita?",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Appointment.Remove(Appointment);
                    _context.SaveChanges();

                    MessageBox.Show("¡Cita eliminada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la cita: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSave()
        {
            return !HasErrors
                   && PatientID > 0
                   && DentistID > 0
                   && TreatmentID > 0
                   && AppointmentDate >= DateTime.Today
                   && TimeSpan.TryParse(StartTime, out var start)
                   && TimeSpan.TryParse(EndTime, out var end)
                   && start < end
                   && !string.IsNullOrWhiteSpace(Notes);
        }

        protected override void ValidateProperty(string propertyName)
        {
            // Najpierw czyścimy stare błędy
            base.ValidateProperty(propertyName);

            switch (propertyName)
            {
                case nameof(PatientID):
                    if (PatientID == 0)
                        AddError(propertyName, "El paciente es obligatorio.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(DentistID):
                    if (DentistID == 0)
                        AddError(propertyName, "El dentista es obligatorio.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(TreatmentID):
                    if (TreatmentID == 0)
                        AddError(propertyName, "El tratamiento es obligatorio.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(AppointmentDate):
                    if (AppointmentDate.Date < DateTime.Today)
                        AddError(propertyName, "La fecha de la cita no puede ser en el pasado.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(StartTime):
                case nameof(EndTime):
                    ValidateTimeFormat(propertyName);
                    break;

                case nameof(Notes):
                    if (string.IsNullOrWhiteSpace(Notes))
                        AddError(propertyName, "Las notas son obligatorias.");
                    else
                        ClearErrors(propertyName);
                    break;
            }
        }

        private void ValidateTimeFormat(string propertyName)
        {
            string time = propertyName == nameof(StartTime) ? StartTime : EndTime;

            if (!TimeSpan.TryParse(time, out var parsedTime))
            {
                AddError(propertyName, "Formato de hora inválido. Use HH:mm.");
            }
            else if (parsedTime < TimeSpan.FromHours(9) || parsedTime > TimeSpan.FromHours(18))
            {
                AddError(propertyName, "La hora debe estar entre 09:00 y 18:00.");
            }
            else
            {
                ClearErrors(propertyName);
            }

            // Sprawdzenie, czy StartTime < EndTime
            if (TimeSpan.TryParse(StartTime, out var start) && TimeSpan.TryParse(EndTime, out var end))
            {
                if (start >= end)
                {
                    AddError(nameof(StartTime), "La hora de inicio debe ser anterior a la hora de fin.");
                }
                else
                {
                    ClearErrors(nameof(StartTime));
                }
            }
        }

        private void NotifySaveCommand()
        {
            ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
        }

        private void CloseWindow(bool dialogResult)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }
    }
}
