using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.Dentist;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Dentist
{
    public class DentistViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Action<string, BaseViewModel> _openTabAction;  // Akcja otwierająca zakładkę

        public ObservableCollection<Models.Dentist> Dentists { get; private set; }

        private Models.Dentist _selectedDentist;
        public Models.Dentist SelectedDentist
        {
            get => _selectedDentist;
            set
            {
                _selectedDentist = value;
                OnPropertyChanged();
                (EditDentistCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (OpenDentistDetailCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "First Name",
            "Last Name",
            "Specialization"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddDentistCommand { get; }
        public ICommand EditDentistCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        // Komenda otwierania szczegółów dentysty
        public ICommand OpenDentistDetailCommand { get; }

        // Konstruktor przyjmuje dodatkowo Action<string, BaseViewModel> openTabAction
        public DentistViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));

            AddDentistCommand = new RelayCommand(OpenAddDentist);
            EditDentistCommand = new RelayCommand(OpenEditDentist, CanEditDentist);
            ReloadCommand = new RelayCommand(LoadDentists);
            SearchCommand = new RelayCommand(SearchDentists);
            SortCommand = new RelayCommand(SortDentists);
            OpenDentistDetailCommand = new RelayCommand(OpenDentistDetail, CanOpenDentistDetail);

            LoadDentists();
        }

        private void LoadDentists()
        {
            try
            {
                var dentists = _context.Dentist
                    .Include(d => d.Room)
                    .ToList();
                Dentists = new ObservableCollection<Models.Dentist>(dentists);
                OnPropertyChanged(nameof(Dentists));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dentists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditDentist() => SelectedDentist != null;

        private void OpenAddDentist()
        {
            var addDentistWindow = new AddDentistWindow
            {
                DataContext = new AddDentistViewModel(_context),
                Owner = Application.Current.MainWindow
            };
            if (addDentistWindow.ShowDialog() == true)
            {
                LoadDentists();
            }
        }

        private void OpenEditDentist()
        {
            if (SelectedDentist == null)
            {
                MessageBox.Show("Please select a dentist to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editDentistWindow = new EditDentistWindow
            {
                DataContext = new EditDentistViewModel(SelectedDentist, _context),
                Owner = Application.Current.MainWindow
            };
            if (editDentistWindow.ShowDialog() == true)
            {
                LoadDentists();
            }
        }

        private void SearchDentists()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadDentists();
                    return;
                }
                var filtered = _context.Dentist
                    .Include(d => d.Room)
                    .Where(d => d.FirstName.Contains(SearchText) ||
                                d.LastName.Contains(SearchText) ||
                                d.Specialization.Contains(SearchText))
                    .ToList();
                Dentists = new ObservableCollection<Models.Dentist>(filtered);
                OnPropertyChanged(nameof(Dentists));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortDentists()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Dentists == null)
                return;
            IEnumerable<Models.Dentist> sortedDentists = SelectedSortOption switch
            {
                "First Name" => Dentists.OrderBy(d => d.FirstName),
                "Last Name" => Dentists.OrderBy(d => d.LastName),
                "Specialization" => Dentists.OrderBy(d => d.Specialization),
                _ => Dentists
            };
            Dentists = new ObservableCollection<Models.Dentist>(sortedDentists);
            OnPropertyChanged(nameof(Dentists));
        }

        // Metoda otwierająca widok szczegółów dentysty jako zakładka
        private void OpenDentistDetail()
        {
            if (SelectedDentist == null)
                return;
            var detailViewModel = new DentalClinicWPF.ViewModels.Dentist.DentistDetailViewModel(_context, SelectedDentist);
            _openTabAction.Invoke($"Dentist: {SelectedDentist.FirstName} {SelectedDentist.LastName}", detailViewModel);
        }

        private bool CanOpenDentistDetail() => SelectedDentist != null;
    }
}
