using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Appointment
{
    public class AddAppointmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

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

        private DateTime _appointmentDate = DateTime.Today;
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

        private string _startTime = "09:00";
        public string StartTime
        {
            get => _startTime;
            set
            {
                if (SetProperty(ref _startTime, value))
                {
                    ValidateProperty(nameof(StartTime));
                    NotifySaveCommand();
                }
            }
        }

        private string _endTime = "10:00";
        public string EndTime
        {
            get => _endTime;
            set
            {
                if (SetProperty(ref _endTime, value))
                {
                    ValidateProperty(nameof(EndTime));
                    NotifySaveCommand();
                }
            }
        }

        private string _notes = string.Empty;
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

        public ObservableCollection<Models.Patient> Patients { get; }
        public ObservableCollection<Models.Dentist> Dentists { get; }
        public ObservableCollection<Models.Treatment> Treatments { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddAppointmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Patients = new ObservableCollection<Models.Patient>(_context.Patient.ToList());
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());
            Treatments = new ObservableCollection<Models.Treatment>(_context.Treatment.ToList());

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            PropertyChanged += (s, e) => ValidateProperty(e.PropertyName);
        }

        protected override void ValidateProperty(string propertyName)
        {
            base.ValidateProperty(propertyName);

            switch (propertyName)
            {
                case nameof(PatientID):
                    if (PatientID == 0)
                        AddError(propertyName, "El paciente es obligatorio.");
                    break;

                case nameof(DentistID):
                    if (DentistID == 0)
                        AddError(propertyName, "El dentista es obligatorio.");
                    break;

                case nameof(TreatmentID):
                    if (TreatmentID == 0)
                        AddError(propertyName, "El tratamiento es obligatorio.");
                    break;

                case nameof(AppointmentDate):
                    if (AppointmentDate.Date < DateTime.Today)
                        AddError(propertyName, "La fecha de la cita no puede ser en el pasado.");
                    break;

                case nameof(StartTime):
                case nameof(EndTime):
                    ValidateTimeFormat(propertyName);
                    break;

                case nameof(Notes):
                    if (string.IsNullOrWhiteSpace(Notes))
                        AddError(propertyName, "Las notas son obligatorias.");
                    break;
            }
        }

        private void ValidateTimeFormat(string propertyName)
        {
            var time = propertyName == nameof(StartTime) ? StartTime : EndTime;

            if (!TimeSpan.TryParse(time, out var parsedTime))
                AddError(propertyName, "Formato de hora inválido. Use HH:mm.");
            else if (parsedTime < TimeSpan.FromHours(9) || parsedTime > TimeSpan.FromHours(18))
                AddError(propertyName, "La hora debe estar entre 09:00 y 18:00.");

            if (TimeSpan.TryParse(StartTime, out var start) && TimeSpan.TryParse(EndTime, out var end) && start >= end)
                AddError(nameof(StartTime), "La hora de inicio debe ser anterior a la hora de fin.");
        }

        private bool CanSave()
        {
            return !HasErrors &&
                   PatientID > 0 &&
                   DentistID > 0 &&
                   TreatmentID > 0 &&
                   AppointmentDate >= DateTime.Today &&
                   TimeSpan.TryParse(StartTime, out var start) &&
                   TimeSpan.TryParse(EndTime, out var end) && start < end &&
                   !string.IsNullOrWhiteSpace(Notes);
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
                var appointment = new Models.Appointment
                {
                    PatientID = PatientID,
                    DentistID = DentistID,
                    TreatmentID = TreatmentID,
                    AppointmentDate = AppointmentDate,
                    StartTime = TimeSpan.Parse(StartTime),
                    EndTime = TimeSpan.Parse(EndTime),
                    Notes = Notes
                };

                _context.Appointment.Add(appointment);
                _context.SaveChanges();

                MessageBox.Show("¡Cita agregada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la cita: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow(false);

        private void NotifySaveCommand() => ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();

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
