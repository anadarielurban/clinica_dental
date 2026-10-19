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

namespace DentalClinicWPF.ViewModels.TreatmentPlan
{
    public class TreatmentPlanViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.TreatmentPlan> TreatmentPlans { get; private set; }

        private Models.TreatmentPlan _selectedTreatmentPlan;
        public Models.TreatmentPlan SelectedTreatmentPlan
        {
            get => _selectedTreatmentPlan;
            set
            {
                _selectedTreatmentPlan = value;
                OnPropertyChanged();
                (EditTreatmentPlanCommand as RelayCommand<Models.TreatmentPlan>)?.NotifyCanExecuteChanged();
                (DeleteTreatmentPlanCommand as RelayCommand<Models.TreatmentPlan>)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Fecha de Creación", "Paciente", "Dentista"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }
        public ICommand AddTreatmentPlanCommand { get; }
        public ICommand EditTreatmentPlanCommand { get; }
        public ICommand DeleteTreatmentPlanCommand { get; }
        public ICommand ReloadTreatmentPlansCommand { get; }
        public ICommand SearchCommand { get; }

        public TreatmentPlanViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>();

            AddTreatmentPlanCommand = new RelayCommand(OpenAdd);
            EditTreatmentPlanCommand = new RelayCommand<Models.TreatmentPlan>(OpenEditWithParam, t => t != null);
            DeleteTreatmentPlanCommand = new RelayCommand<Models.TreatmentPlan>(DeleteWithParam, t => t != null);
            ReloadTreatmentPlansCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchData);
            SortCommand = new RelayCommand(SortData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var list = _context.TreatmentPlan.Include(t => t.Patient).Include(t => t.Dentist).ToList();
                TreatmentPlans.Clear();
                foreach (var t in list) TreatmentPlans.Add(t);
            }
            catch (Exception ex) { MessageBox.Show($"Error al cargar: {ex.Message}"); }
        }

        private void SearchData()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadData(); return; }
            var term = SearchText.ToLower();
            var filtered = _context.TreatmentPlan.Include(t => t.Patient).Include(t => t.Dentist).ToList()
                .Where(t => (t.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                            (t.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                            (t.Dentist?.LastName?.ToLower().Contains(term) ?? false)).ToList();
            TreatmentPlans.Clear();
            foreach (var t in filtered) TreatmentPlans.Add(t);
        }

        private void SortData()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Fecha de Creación" => TreatmentPlans.OrderBy(t => t.CreationDate).ToList(),
                "Paciente" => TreatmentPlans.OrderBy(t => t.Patient?.LastName).ToList(),
                "Dentista" => TreatmentPlans.OrderBy(t => t.Dentist?.LastName).ToList(),
                _ => TreatmentPlans.ToList()
            };
            TreatmentPlans.Clear();
            foreach (var t in sorted) TreatmentPlans.Add(t);
        }

        private void OpenAdd()
        {
            var w = new Views.TreatmentPlan.AddTreatmentPlanView { DataContext = new AddTreatmentPlanViewModel(_context) };
            if (w.ShowDialog() == true) LoadData();
        }

        private void OpenEditWithParam(Models.TreatmentPlan plan)
        {
            if (plan == null) return;
            SelectedTreatmentPlan = plan;
            var w = new Views.TreatmentPlan.EditTreatmentPlanView { DataContext = new EditTreatmentPlanViewModel(_context, plan) };
            if (w.ShowDialog() == true) LoadData();
        }

        private void DeleteWithParam(Models.TreatmentPlan plan)
        {
            if (plan == null) return;
            if (MessageBox.Show($"¿Seguro que quieres eliminar este plan de tratamiento?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try { _context.TreatmentPlan.Remove(plan); _context.SaveChanges(); LoadData(); }
                catch (Exception ex) { MessageBox.Show($"Error al eliminar: {ex.Message}"); }
            }
        }
    }
}
