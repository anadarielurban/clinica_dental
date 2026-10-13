using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Patient> Patients { get; private set; }

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
                    // Reiniciar el timer: si el usuario sigue escribiendo, no busca aún
                    _searchTimer.Stop();
                    _searchTimer.Start();
                }
            }
        }

        private int _resultsCount;
        public int ResultsCount
        {
            get => _resultsCount;
            set { _resultsCount = value; OnPropertyChanged(); }
        }

        private string _resultsLabel;
        public string ResultsLabel
        {
            get => _resultsLabel;
            set { _resultsLabel = value; OnPropertyChanged(); }
        }

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

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre",
            "Apellido",
            "Fecha de Nacimiento"
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
                    SortPatients();
                }
            }
        }

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

            // Timer para debounce de la búsqueda (300ms)
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                SearchPatients();
            };

            InitializeCommands();
            LoadPatients();
        }

        private void InitializeCommands()
        {
            OpenAddPatientCommand = new RelayCommand(OpenAddPatient);
            OpenEditPatientCommand = new RelayCommand(OpenEditPatient, CanEditPatient);
            SearchCommand = new RelayCommand(SearchPatients);
            ReloadCommand = new RelayCommand(ReloadAll);
            OpenPatientDetailCommand = new RelayCommand(OpenPatientDetail, CanOpenPatientDetail);
            SortCommand = new RelayCommand(SortPatients);
        }

        private void LoadPatients()
        {
            Patients.Clear();
            var all = _context.Patient.ToList();
            foreach (var patient in all)
            {
                Patients.Add(patient);
            }
            UpdateResultsLabel(all.Count, false);
        }

        private void ReloadAll()
        {
            // Limpia el texto de búsqueda sin disparar el timer
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadPatients();
        }

        private void SearchPatients()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadPatients();
                return;
            }

            var term = SearchText.Trim().ToLower();

            var filteredPatients = _context.Patient
                .ToList()
                .Where(p =>
                    (p.FirstName?.ToLower().Contains(term) ?? false) ||
                    (p.LastName?.ToLower().Contains(term) ?? false) ||
                    (p.PhoneNumber?.ToLower().Contains(term) ?? false) ||
                    (p.Email?.ToLower().Contains(term) ?? false))
                .ToList();

            Patients.Clear();
            foreach (var patient in filteredPatients)
            {
                Patients.Add(patient);
            }
            UpdateResultsLabel(filteredPatients.Count, true);
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            ResultsCount = count;
            if (count == 0)
                ResultsLabel = "Sin resultados";
            else if (count == 1)
                ResultsLabel = filtered ? "1 resultado" : "1 paciente";
            else
                ResultsLabel = filtered
                    ? $"{count} resultados"
                    : $"{count} pacientes";
        }

        private void SortPatients()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Patients == null)
                return;

            var sorted = SelectedSortOption switch
            {
                "Nombre" => Patients.OrderBy(p => p.FirstName).ToList(),
                "Apellido" => Patients.OrderBy(p => p.LastName).ToList(),
                "Fecha de Nacimiento" => Patients.OrderBy(p => p.DateOfBirth).ToList(),
                _ => Patients.ToList()
            };

            Patients.Clear();
            foreach (var patient in sorted)
            {
                Patients.Add(patient);
            }
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
