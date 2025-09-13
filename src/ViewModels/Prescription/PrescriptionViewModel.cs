using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Prescription
{
    public class PrescriptionViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Action<string, BaseViewModel> _openTabAction; // Akcja otwierania zakładki

        public ObservableCollection<Models.Prescription> Prescriptions { get; set; }
        private Models.Prescription _selectedPrescription;
        public Models.Prescription SelectedPrescription
        {
            get => _selectedPrescription;
            set
            {
                _selectedPrescription = value;
                OnPropertyChanged();
                (EditPrescriptionCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (OpenPrescriptionDetailCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Patient Name",
            "Dentist Name",
            "Date Issued",
            "Medication"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddPrescriptionCommand { get; private set; }
        public ICommand EditPrescriptionCommand { get; private set; }
        public ICommand ReloadPrescriptionsCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        
        // Nowa komenda otwierania szczegółów recepty
        public ICommand OpenPrescriptionDetailCommand { get; }

        // Konstruktor – dodatkowo przyjmujemy Action openTabAction, która otwiera zakładkę
        public PrescriptionViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));

            Prescriptions = new ObservableCollection<Models.Prescription>();
            InitializeCommands();
            SortCommand = new RelayCommand(SortPrescriptions);
            LoadPrescriptions();
            
            // Inicjalizacja komendy otwierania szczegółów recepty
            OpenPrescriptionDetailCommand = new RelayCommand(OpenPrescriptionDetail, () => SelectedPrescription != null);
        }

        private void InitializeCommands()
        {
            AddPrescriptionCommand = new RelayCommand(OpenAddPrescription);
            EditPrescriptionCommand = new RelayCommand(OpenEditPrescription, () => SelectedPrescription != null);
            ReloadPrescriptionsCommand = new RelayCommand(LoadPrescriptions);
            SearchCommand = new RelayCommand(SearchPrescriptions);
        }

        private void LoadPrescriptions()
        {
            try
            {
                var prescriptionsFromDb = _context.Prescription
                    .Include(p => p.Patient)
                    .Include(p => p.Dentist)
                    .ToList();

                Prescriptions.Clear();
                foreach (var prescription in prescriptionsFromDb)
                {
                    Prescriptions.Add(prescription);
                }
                OnPropertyChanged(nameof(Prescriptions));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading prescriptions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortPrescriptions()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Prescriptions == null)
                return;

            IEnumerable<Models.Prescription> sortedPrescriptions = SelectedSortOption switch
            {
                "Patient Name" => Prescriptions.OrderBy(p => p.Patient.LastName).ThenBy(p => p.Patient.FirstName),
                "Dentist Name" => Prescriptions.OrderBy(p => p.Dentist.LastName).ThenBy(p => p.Dentist.FirstName),
                "Date Issued" => Prescriptions.OrderBy(p => p.DateIssued),
                "Medication" => Prescriptions.OrderBy(p => p.Medication),
                _ => Prescriptions
            };

            Prescriptions = new ObservableCollection<Models.Prescription>(sortedPrescriptions);
            OnPropertyChanged(nameof(Prescriptions));
        }

        private void OpenAddPrescription()
        {
            try
            {
                var addWindow = new Views.Prescription.AddPrescriptionView
                {
                    DataContext = new AddPrescriptionViewModel(_context)
                };
                if (addWindow.ShowDialog() == true)
                {
                    LoadPrescriptions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Prescription window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEditPrescription()
        {
            if (SelectedPrescription == null)
            {
                MessageBox.Show("Please select a prescription to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var editWindow = new Views.Prescription.EditPrescriptionView
                {
                    DataContext = new EditPrescriptionViewModel(SelectedPrescription, _context)
                };
                if (editWindow.ShowDialog() == true)
                {
                    LoadPrescriptions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Edit Prescription window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchPrescriptions()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadPrescriptions();
                return;
            }
            try
            {
                var filteredPrescriptions = _context.Prescription
                    .Include(p => p.Patient)
                    .Include(p => p.Dentist)
                    .Where(p =>
                        (p.Patient.FirstName.Contains(SearchText) || p.Patient.LastName.Contains(SearchText)) ||
                        (p.Dentist.FirstName.Contains(SearchText) || p.Dentist.LastName.Contains(SearchText)) ||
                        p.Medication.Contains(SearchText))
                    .ToList();

                Prescriptions.Clear();
                foreach (var prescription in filteredPrescriptions)
                {
                    Prescriptions.Add(prescription);
                }
                OnPropertyChanged(nameof(Prescriptions));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching prescriptions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metoda otwierająca szczegóły recepty jako zakładkę
        private void OpenPrescriptionDetail()
        {
            if (SelectedPrescription == null)
                return;

            var detailViewModel = new PrescriptionDetailViewModel(_context, SelectedPrescription);
            _openTabAction.Invoke($"Prescription: {SelectedPrescription.PrescriptionID}", detailViewModel);
        }
    }
}
