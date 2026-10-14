using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private readonly Action<string, BaseViewModel> _openTabAction;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Dentist> Dentists { get; private set; }

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
                    _searchTimer.Stop();
                    _searchTimer.Start();
                }
            }
        }

        private string _resultsLabel = string.Empty;
        public string ResultsLabel { get => _resultsLabel; set { _resultsLabel = value; OnPropertyChanged(); } }

        private Models.Dentist _selectedDentist;
        public Models.Dentist SelectedDentist
        {
            get => _selectedDentist;
            set
            {
                _selectedDentist = value;
                OnPropertyChanged();
                (EditDentistCommand as RelayCommand<Models.Dentist>)?.NotifyCanExecuteChanged();
                (OpenDentistDetailCommand as RelayCommand<Models.Dentist>)?.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre", "Apellido", "Especialización"
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
                    SortDentists();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand AddDentistCommand { get; }
        public ICommand EditDentistCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand OpenDentistDetailCommand { get; }

        public DentistViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));
            Dentists = new ObservableCollection<Models.Dentist>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); SearchDentists(); };

            AddDentistCommand = new RelayCommand(OpenAddDentist);
            EditDentistCommand = new RelayCommand<Models.Dentist>(OpenEditDentistWithParam, p => p != null);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(SearchDentists);
            SortCommand = new RelayCommand(SortDentists);
            OpenDentistDetailCommand = new RelayCommand<Models.Dentist>(OpenDentistDetailWithParam, p => p != null);

            LoadDentists();
        }

        private void LoadDentists()
        {
            try
            {
                var list = _context.Dentist.Include(d => d.Room).ToList();
                Dentists.Clear();
                foreach (var d in list) Dentists.Add(d);
                UpdateResultsLabel(list.Count, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los dentistas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadDentists();
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            if (count == 0) ResultsLabel = "Sin resultados";
            else if (count == 1) ResultsLabel = filtered ? "1 resultado" : "1 dentista";
            else ResultsLabel = filtered ? $"{count} resultados" : $"{count} dentistas";
        }

        private void SearchDentists()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadDentists(); return; }
            var term = SearchText.Trim().ToLower();
            var filtered = _context.Dentist.Include(d => d.Room).ToList()
                .Where(d =>
                    (d.FirstName?.ToLower().Contains(term) ?? false) ||
                    (d.LastName?.ToLower().Contains(term) ?? false) ||
                    (d.Specialization?.ToLower().Contains(term) ?? false))
                .ToList();
            Dentists.Clear();
            foreach (var d in filtered) Dentists.Add(d);
            UpdateResultsLabel(filtered.Count, true);
        }

        private void SortDentists()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Nombre" => Dentists.OrderBy(d => d.FirstName).ToList(),
                "Apellido" => Dentists.OrderBy(d => d.LastName).ToList(),
                "Especialización" => Dentists.OrderBy(d => d.Specialization).ToList(),
                _ => Dentists.ToList()
            };
            Dentists.Clear();
            foreach (var d in sorted) Dentists.Add(d);
        }

        private bool CanEditDentist() => SelectedDentist != null;

        private void OpenAddDentist()
        {
            var w = new AddDentistWindow { DataContext = new AddDentistViewModel(_context), Owner = Application.Current.MainWindow };
            if (w.ShowDialog() == true) LoadDentists();
        }

        private void OpenEditDentistWithParam(Models.Dentist dentist)
        {
            if (dentist == null) return;
            SelectedDentist = dentist;
            OpenEditDentist();
        }

        private void OpenEditDentist()
        {
            if (SelectedDentist == null) return;
            var w = new EditDentistWindow { DataContext = new EditDentistViewModel(SelectedDentist, _context), Owner = Application.Current.MainWindow };
            if (w.ShowDialog() == true) LoadDentists();
        }

        private void OpenDentistDetailWithParam(Models.Dentist dentist)
        {
            if (dentist == null) return;
            SelectedDentist = dentist;
            OpenDentistDetail();
        }

        private void OpenDentistDetail()
        {
            if (SelectedDentist == null) return;
            var detailVm = new DentistDetailViewModel(_context, SelectedDentist);
            _openTabAction.Invoke($"Dentist: {SelectedDentist.FirstName} {SelectedDentist.LastName}", detailVm);
        }

        private bool CanOpenDentistDetail() => SelectedDentist != null;
    }
}
