using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.TreatmentPlan;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.TreatmentPlan
{
    public class TreatmentPlanViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.TreatmentPlan> TreatmentPlans { get; set; }
        private Models.TreatmentPlan _selectedTreatmentPlan;

        public Models.TreatmentPlan SelectedTreatmentPlan
        {
            get => _selectedTreatmentPlan;
            set
            {
                _selectedTreatmentPlan = value;
                OnPropertyChanged();
                ((RelayCommand)EditTreatmentPlanCommand).NotifyCanExecuteChanged();
                ((RelayCommand)DeleteTreatmentPlanCommand).NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Patient",
            "Dentist",
            "Creation Date"
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

            ReloadTreatmentPlansCommand = new RelayCommand(ReloadTreatmentPlans);
            AddTreatmentPlanCommand = new RelayCommand(OpenAddTreatmentPlan);
            EditTreatmentPlanCommand = new RelayCommand(OpenEditTreatmentPlan, () => SelectedTreatmentPlan != null);
            DeleteTreatmentPlanCommand = new RelayCommand(DeleteTreatmentPlan, () => SelectedTreatmentPlan != null);
            SearchCommand = new RelayCommand(SearchTreatmentPlans);
            SortCommand = new RelayCommand(SortTreatmentPlans); // Komenda sortowania

            ReloadTreatmentPlans();
        }

        private void OpenAddTreatmentPlan()
        {
            var addWindow = new AddTreatmentPlanView
            {
                DataContext = new AddTreatmentPlanViewModel(_context),
                Owner = Application.Current.MainWindow
            };

            if (addWindow.ShowDialog() == true)
                ReloadTreatmentPlans();
        }

        private void OpenEditTreatmentPlan()
        {
            if (SelectedTreatmentPlan == null)
            {
                MessageBox.Show("Please select a treatment plan to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EditTreatmentPlanView
            {
                DataContext = new EditTreatmentPlanViewModel(_context, SelectedTreatmentPlan),
                Owner = Application.Current.MainWindow
            };

            if (editWindow.ShowDialog() == true)
                ReloadTreatmentPlans();
        }

        private void DeleteTreatmentPlan()
        {
            if (SelectedTreatmentPlan == null)
            {
                MessageBox.Show("Please select a treatment plan to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this treatment plan? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.TreatmentPlan.Remove(SelectedTreatmentPlan);
                    _context.SaveChanges();
                    TreatmentPlans.Remove(SelectedTreatmentPlan);
                    MessageBox.Show("Treatment Plan deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while deleting treatment plan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReloadTreatmentPlans()
        {
            try
            {
                var plans = _context.TreatmentPlan
                    .Include(tp => tp.Patient)
                    .Include(tp => tp.Dentist)
                    .ToList();

                TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>(plans);
                OnPropertyChanged(nameof(TreatmentPlans));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading treatment plans: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTreatmentPlans()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    ReloadTreatmentPlans();
                    return;
                }

                var filteredPlans = _context.TreatmentPlan
                    .Include(tp => tp.Patient)
                    .Include(tp => tp.Dentist)
                    .Where(tp =>
                        tp.Patient.FirstName.Contains(SearchText) ||
                        tp.Patient.LastName.Contains(SearchText) ||
                        tp.Dentist.FirstName.Contains(SearchText) ||
                        tp.Dentist.LastName.Contains(SearchText))
                    .ToList();

                TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>(filteredPlans);
                OnPropertyChanged(nameof(TreatmentPlans));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching treatment plans: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortTreatmentPlans()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || TreatmentPlans == null)
                return;

            IEnumerable<Models.TreatmentPlan> sortedPlans = SelectedSortOption switch
            {
                "Patient" => TreatmentPlans.OrderBy(tp => tp.Patient.LastName).ThenBy(tp => tp.Patient.FirstName),
                "Dentist" => TreatmentPlans.OrderBy(tp => tp.Dentist.LastName).ThenBy(tp => tp.Dentist.FirstName),
                "Creation Date" => TreatmentPlans.OrderBy(tp => tp.CreationDate),
                _ => TreatmentPlans
            };

            TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>(sortedPlans);
            OnPropertyChanged(nameof(TreatmentPlans));
        }
    }
}
