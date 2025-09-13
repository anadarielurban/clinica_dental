using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.ViewModels.Patient.PatientDetail;
using DentalClinicWPF.Views.Patient;

namespace DentalClinicWPF.ViewModels.Patient
{
    public class PatientViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Action<string, BaseViewModel> _openTabAction;

        public ObservableCollection<Models.Patient> Patients { get; private set; }

        public string SearchText { get; set; }

        private Models.Patient _selectedPatient;
        public Models.Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
                ((RelayCommand)OpenEditPatientCommand).NotifyCanExecuteChanged();
                ((RelayCommand)OpenPatientDetailCommand).NotifyCanExecuteChanged();
            }
        }

        // Nowe właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "First Name",
            "Last Name",
            "Date of Birth"
        };

        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; private set; }

        public ICommand OpenAddPatientCommand { get; private set; }
        public ICommand OpenEditPatientCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand ReloadCommand { get; private set; }
        public ICommand OpenPatientDetailCommand { get; private set; }

        public PatientViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));
            Patients = new ObservableCollection<Models.Patient>();

            InitializeCommands();
            LoadPatients();
        }

        private void InitializeCommands()
        {
            OpenAddPatientCommand = new RelayCommand(OpenAddPatient);
            OpenEditPatientCommand = new RelayCommand(OpenEditPatient, CanEditPatient);
            SearchCommand = new RelayCommand(SearchPatients);
            ReloadCommand = new RelayCommand(LoadPatients);
            OpenPatientDetailCommand = new RelayCommand(OpenPatientDetail, CanOpenPatientDetail);
            SortCommand = new RelayCommand(SortPatients); // Nowa komenda do sortowania
        }

        private void LoadPatients()
        {
            Patients.Clear();
            foreach (var patient in _context.Patient.ToList())
            {
                Patients.Add(patient);
            }
        }

        private void SearchPatients()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadPatients();
                return;
            }

            var filteredPatients = _context.Patient.Where(p =>
                p.FirstName.Contains(SearchText) ||
                p.LastName.Contains(SearchText) ||
                p.PhoneNumber.Contains(SearchText) ||
                p.Email.Contains(SearchText)).ToList();

            Patients.Clear();
            foreach (var patient in filteredPatients)
            {
                Patients.Add(patient);
            }
        }

        private void SortPatients()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Patients == null)
                return;

            IEnumerable<Models.Patient> sortedPatients = SelectedSortOption switch
            {
                "First Name" => Patients.OrderBy(p => p.FirstName),
                "Last Name" => Patients.OrderBy(p => p.LastName),
                "Date of Birth" => Patients.OrderBy(p => p.DateOfBirth),
                _ => Patients
            };

            Patients = new ObservableCollection<Models.Patient>(sortedPatients);
            OnPropertyChanged(nameof(Patients));
        }


        private void OpenAddPatient()
        {
            var addPatientWindow = new AddPatientWindow
            {
                DataContext = new AddPatientViewModel(_context),
                Owner = Application.Current.MainWindow
            };

            if (addPatientWindow.ShowDialog() == true)
            {
                LoadPatients();
            }
        }

        private void OpenEditPatient()
        {
            if (SelectedPatient == null) return;

            var editPatientWindow = new EditPatientWindow
            {
                DataContext = new EditPatientViewModel(_context, SelectedPatient, _openTabAction),
                Owner = Application.Current.MainWindow
            };

            if (editPatientWindow.ShowDialog() == true)
            {
                LoadPatients();
            }
        }

        private void OpenPatientDetail()
        {
            if (SelectedPatient == null) return;

            var detailViewModel = new PatientDetailViewModel(_context, SelectedPatient);
            _openTabAction.Invoke($"Patient: {SelectedPatient.FullName}", detailViewModel);
        }

        private bool CanEditPatient() => SelectedPatient != null;
        private bool CanOpenPatientDetail() => SelectedPatient != null;
    }
}
