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
        private readonly Action<string, BaseViewModel> _openTabAction;

        public ObservableCollection<Models.Prescription> Prescriptions { get; set; }
        private Models.Prescription _selectedPrescription;
        public Models.Prescription SelectedPrescription
        {
            get => _selectedPrescription;
            set
            {
                _selectedPrescription = value;
                OnPropertyChanged();
                (EditPrescriptionCommand as RelayCommand<Models.Prescription>)?.NotifyCanExecuteChanged();
                (DeletePrescriptionCommand as RelayCommand<Models.Prescription>)?.NotifyCanExecuteChanged();
                (OpenPrescriptionDetailCommand as RelayCommand<Models.Prescription>)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Paciente", "Dentista", "Fecha de Emisión", "Medicamento"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }
        public ICommand AddPrescriptionCommand { get; private set; }
        public ICommand EditPrescriptionCommand { get; private set; }
        public ICommand DeletePrescriptionCommand { get; private set; }
        public ICommand ReloadPrescriptionsCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand OpenPrescriptionDetailCommand { get; private set; }

        public PrescriptionViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
        DentalClinicWPF.Events.PrescriptionSelectedEvent.Instance.Subscribe(OnPrescriptionSelectedFromPatientDetail);
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));

            Prescriptions = new ObservableCollection<Models.Prescription>();
            InitializeCommands();
            SortCommand = new RelayCommand(SortPrescriptions);
            LoadPrescriptions();
        }

        private void InitializeCommands()
        {
            AddPrescriptionCommand = new RelayCommand(OpenAddPrescription);
            EditPrescriptionCommand = new RelayCommand<Models.Prescription>(OpenEditPrescriptionWithParam, p => p != null);
            DeletePrescriptionCommand = new RelayCommand<Models.Prescription>(DeletePrescriptionWithParam, p => p != null);
            ReloadPrescriptionsCommand = new RelayCommand(LoadPrescriptions);
            SearchCommand = new RelayCommand(SearchPrescriptions);
            OpenPrescriptionDetailCommand = new RelayCommand<Models.Prescription>(OpenDetailWithParam, p => p != null);
        }

        private void LoadPrescriptions()
        {
            try
            {
                var list = _context.Prescription.Include(p => p.Patient).Include(p => p.Dentist).ToList();
                Prescriptions.Clear();
                foreach (var p in list) Prescriptions.Add(p);
                OnPropertyChanged(nameof(Prescriptions));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las recetas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortPrescriptions()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Prescriptions == null) return;
            var sorted = SelectedSortOption switch
            {
                "Paciente" => Prescriptions.OrderBy(p => p.Patient?.LastName).ThenBy(p => p.Patient?.FirstName).ToList(),
                "Dentista" => Prescriptions.OrderBy(p => p.Dentist?.LastName).ThenBy(p => p.Dentist?.FirstName).ToList(),
                "Fecha de Emisión" => Prescriptions.OrderBy(p => p.DateIssued).ToList(),
                "Medicamento" => Prescriptions.OrderBy(p => p.Medication).ToList(),
                _ => Prescriptions.ToList()
            };
            Prescriptions.Clear();
            foreach (var p in sorted) Prescriptions.Add(p);
        }

        private void OpenAddPrescription()
        {
            var w = new Views.Prescription.AddPrescriptionView { DataContext = new AddPrescriptionViewModel(_context) };
            if (w.ShowDialog() == true) LoadPrescriptions();
        }

        private void OpenEditPrescriptionWithParam(Models.Prescription p)
        {
            if (p == null) return;
            SelectedPrescription = p;
            var w = new Views.Prescription.EditPrescriptionView { DataContext = new EditPrescriptionViewModel(p, _context) };
            if (w.ShowDialog() == true) LoadPrescriptions();
        }

        private void DeletePrescriptionWithParam(Models.Prescription p)
        {
            if (p == null) return;
            var result = MessageBox.Show(
                $"¿Seguro que quieres eliminar la receta #{p.PrescriptionID}?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Prescription.Remove(p);
                    _context.SaveChanges();
                    LoadPrescriptions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la receta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchPrescriptions()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadPrescriptions(); return; }
            var term = SearchText.ToLower();
            var filtered = _context.Prescription.Include(p => p.Patient).Include(p => p.Dentist).ToList()
                .Where(p =>
                    (p.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                    (p.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                    (p.Dentist?.FirstName?.ToLower().Contains(term) ?? false) ||
                    (p.Dentist?.LastName?.ToLower().Contains(term) ?? false) ||
                    (p.Medication?.ToLower().Contains(term) ?? false))
                .ToList();
            Prescriptions.Clear();
            foreach (var p in filtered) Prescriptions.Add(p);
        }

        private void OpenDetailWithParam(Models.Prescription p)
        {
            if (p == null) return;
            SelectedPrescription = p;
            var vm = new PrescriptionDetailViewModel(_context, p);
            _openTabAction.Invoke($"Prescription: {p.PrescriptionID}", vm);
        }
    
        private void OnPrescriptionSelectedFromPatientDetail(DentalClinicWPF.Models.Prescription prescription)
        {
            if (prescription == null) return;
            try
            {
                if (this.ReloadPrescriptionsCommand != null && this.ReloadPrescriptionsCommand.CanExecute(null))
                    this.ReloadPrescriptionsCommand.Execute(null);
                this.SelectedPrescription = prescription;
                var detailProp = GetType().GetProperty("OpenPrescriptionDetailCommand");
                if (detailProp != null)
                {
                    var detailCmd = detailProp.GetValue(this) as System.Windows.Input.ICommand;
                    if (detailCmd != null)
                    {
                        if (detailCmd.CanExecute(prescription)) detailCmd.Execute(prescription);
                        else if (detailCmd.CanExecute(null)) detailCmd.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
        }
}
}
