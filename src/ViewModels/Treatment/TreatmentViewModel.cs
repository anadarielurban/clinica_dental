using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.Treatment;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Treatment
{
    public class TreatmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.Treatment> Treatments { get; private set; }

        private Models.Treatment _selectedTreatment;
        public Models.Treatment SelectedTreatment
        {
            get => _selectedTreatment;
            set
            {
                _selectedTreatment = value;
                OnPropertyChanged();
                ((RelayCommand)EditTreatmentCommand).NotifyCanExecuteChanged();
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
            "Name",
            "Price",
            "Duration"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddTreatmentCommand { get; }
        public ICommand EditTreatmentCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public TreatmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            LoadData();

            AddTreatmentCommand = new RelayCommand(OpenAddTreatment);
            EditTreatmentCommand = new RelayCommand(OpenEditTreatment, () => SelectedTreatment != null);
            ReloadCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchTreatments);
            SortCommand = new RelayCommand(SortTreatments); // Komenda sortowania
        }

        private void LoadData()
        {
            try
            {
                Treatments = new ObservableCollection<Models.Treatment>(_context.Treatment.ToList());
                OnPropertyChanged(nameof(Treatments));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading treatments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortTreatments()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Treatments == null)
                return;

            IEnumerable<Models.Treatment> sortedTreatments = SelectedSortOption switch
            {
                "Name" => Treatments.OrderBy(t => t.Name),
                "Price" => Treatments.OrderBy(t => t.Price),
                "Duration" => Treatments.OrderBy(t => t.Duration),
                _ => Treatments
            };

            Treatments = new ObservableCollection<Models.Treatment>(sortedTreatments);
            OnPropertyChanged(nameof(Treatments));
        }

        private void OpenAddTreatment()
        {
            var window = new AddEditTreatmentView
            {
                DataContext = new AddEditTreatmentViewModel(_context)
            };

            if (window.ShowDialog() == true)
                LoadData();
        }

        private void OpenEditTreatment()
        {
            if (SelectedTreatment == null)
            {
                MessageBox.Show("Please select a treatment to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EditTreatmentView
            {
                DataContext = new EditTreatmentViewModel(_context, SelectedTreatment)
            };

            if (editWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void SearchTreatments()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadData();
                    return;
                }

                var filtered = _context.Treatment
                    .Where(t => t.Name.Contains(SearchText) || t.Description.Contains(SearchText))
                    .ToList();

                Treatments = new ObservableCollection<Models.Treatment>(filtered);
                OnPropertyChanged(nameof(Treatments));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching treatments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
