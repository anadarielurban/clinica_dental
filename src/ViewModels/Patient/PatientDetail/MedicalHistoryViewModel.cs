using System;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Patient.PatientDetail
{
    public class MedicalHistoryViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Models.Patient _patient;

        // ═══ Tipo de sangre ═══
        private string? _bloodType;
        public string? BloodType
        {
            get => _bloodType;
            set { if (_bloodType == value) return; _bloodType = value; OnPropertyChanged(nameof(BloodType)); }
        }

        // ═══ Condiciones médicas ═══
        private bool _hasDiabetes;
        public bool HasDiabetes
        {
            get => _hasDiabetes;
            set { if (_hasDiabetes == value) return; _hasDiabetes = value; OnPropertyChanged(nameof(HasDiabetes)); }
        }

        private bool _hasHypertension;
        public bool HasHypertension
        {
            get => _hasHypertension;
            set { if (_hasHypertension == value) return; _hasHypertension = value; OnPropertyChanged(nameof(HasHypertension)); }
        }

        private bool _hasHeartConditions;
        public bool HasHeartConditions
        {
            get => _hasHeartConditions;
            set { if (_hasHeartConditions == value) return; _hasHeartConditions = value; OnPropertyChanged(nameof(HasHeartConditions)); }
        }

        private bool _hasCoagulationIssues;
        public bool HasCoagulationIssues
        {
            get => _hasCoagulationIssues;
            set { if (_hasCoagulationIssues == value) return; _hasCoagulationIssues = value; OnPropertyChanged(nameof(HasCoagulationIssues)); }
        }

        private bool _takesAnticoagulants;
        public bool TakesAnticoagulants
        {
            get => _takesAnticoagulants;
            set { if (_takesAnticoagulants == value) return; _takesAnticoagulants = value; OnPropertyChanged(nameof(TakesAnticoagulants)); }
        }

        private bool _isPregnant;
        public bool IsPregnant
        {
            get => _isPregnant;
            set { if (_isPregnant == value) return; _isPregnant = value; OnPropertyChanged(nameof(IsPregnant)); }
        }

        private bool _isSmoker;
        public bool IsSmoker
        {
            get => _isSmoker;
            set { if (_isSmoker == value) return; _isSmoker = value; OnPropertyChanged(nameof(IsSmoker)); }
        }

        // ═══ Alergias ═══
        private bool _hasAllergies;
        public bool HasAllergies
        {
            get => _hasAllergies;
            set { if (_hasAllergies == value) return; _hasAllergies = value; OnPropertyChanged(nameof(HasAllergies)); }
        }

        private string? _allergiesDetails;
        public string? AllergiesDetails
        {
            get => _allergiesDetails;
            set { if (_allergiesDetails == value) return; _allergiesDetails = value; OnPropertyChanged(nameof(AllergiesDetails)); }
        }

        // ═══ Contacto de emergencia ═══
        private string? _emergencyContactName;
        public string? EmergencyContactName
        {
            get => _emergencyContactName;
            set { if (_emergencyContactName == value) return; _emergencyContactName = value; OnPropertyChanged(nameof(EmergencyContactName)); }
        }

        private string? _emergencyContactPhone;
        public string? EmergencyContactPhone
        {
            get => _emergencyContactPhone;
            set { if (_emergencyContactPhone == value) return; _emergencyContactPhone = value; OnPropertyChanged(nameof(EmergencyContactPhone)); }
        }

        // ═══ Notas ═══
        private string? _medicalNotes;
        public string? MedicalNotes
        {
            get => _medicalNotes;
            set { if (_medicalNotes == value) return; _medicalNotes = value; OnPropertyChanged(nameof(MedicalNotes)); }
        }

        public IRelayCommand SaveCommand { get; }

        public MedicalHistoryViewModel(DentalClinicContext context, Models.Patient patient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _patient = patient ?? throw new ArgumentNullException(nameof(patient));

            LoadFromPatient();
            SaveCommand = new RelayCommand(Save);
        }

        private void LoadFromPatient()
        {
            BloodType = _patient.BloodType;
            HasDiabetes = _patient.HasDiabetes;
            HasHypertension = _patient.HasHypertension;
            HasHeartConditions = _patient.HasHeartConditions;
            HasCoagulationIssues = _patient.HasCoagulationIssues;
            TakesAnticoagulants = _patient.TakesAnticoagulants;
            IsPregnant = _patient.IsPregnant;
            IsSmoker = _patient.IsSmoker;
            HasAllergies = _patient.HasAllergies;
            AllergiesDetails = _patient.AllergiesDetails;
            EmergencyContactName = _patient.EmergencyContactName;
            EmergencyContactPhone = _patient.EmergencyContactPhone;
            MedicalNotes = _patient.MedicalNotes;
        }

        private void Save()
        {
            try
            {
                _patient.BloodType = BloodType;
                _patient.HasDiabetes = HasDiabetes;
                _patient.HasHypertension = HasHypertension;
                _patient.HasHeartConditions = HasHeartConditions;
                _patient.HasCoagulationIssues = HasCoagulationIssues;
                _patient.TakesAnticoagulants = TakesAnticoagulants;
                _patient.IsPregnant = IsPregnant;
                _patient.IsSmoker = IsSmoker;
                _patient.HasAllergies = HasAllergies;
                _patient.AllergiesDetails = HasAllergies ? AllergiesDetails : null;
                _patient.EmergencyContactName = EmergencyContactName;
                _patient.EmergencyContactPhone = EmergencyContactPhone;
                _patient.MedicalNotes = MedicalNotes;

                _context.SaveChanges();
                MessageBox.Show("Historia clínica guardada correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
