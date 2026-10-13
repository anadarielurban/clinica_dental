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
using DentalClinicWPF.Views.Treatment;

namespace DentalClinicWPF.ViewModels.Treatment
{
    public class TreatmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Treatment> Treatments { get; private set; }

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

        private Models.Treatment _selectedTreatment;
        public Models.Treatment SelectedTreatment
        {
            get => _selectedTreatment;
            set
            {
                _selectedTreatment = value;
                OnPropertyChanged();
                (EditTreatmentCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre", "Precio", "Duración"
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
                    SortTreatments();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand AddTreatmentCommand { get; }
        public ICommand EditTreatmentCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public TreatmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Treatments = new ObservableCollection<Models.Treatment>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); SearchTreatments(); };

            AddTreatmentCommand = new RelayCommand(OpenAddTreatment);
            EditTreatmentCommand = new RelayCommand(OpenEditTreatment, () => SelectedTreatment != null);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(SearchTreatments);
            SortCommand = new RelayCommand(SortTreatments);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var list = _context.Treatment.ToList();
                Treatments.Clear();
                foreach (var t in list) Treatments.Add(t);
                UpdateResultsLabel(list.Count, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los tratamientos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadData();
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            if (count == 0) ResultsLabel = "Sin resultados";
            else if (count == 1) ResultsLabel = filtered ? "1 resultado" : "1 tratamiento";
            else ResultsLabel = filtered ? $"{count} resultados" : $"{count} tratamientos";
        }

        private void SearchTreatments()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadData(); return; }
            var term = SearchText.Trim().ToLower();
            var filtered = _context.Treatment.ToList()
                .Where(t =>
                    (t.Name?.ToLower().Contains(term) ?? false) ||
                    (t.Description?.ToLower().Contains(term) ?? false))
                .ToList();
            Treatments.Clear();
            foreach (var t in filtered) Treatments.Add(t);
            UpdateResultsLabel(filtered.Count, true);
        }

        private void SortTreatments()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Nombre" => Treatments.OrderBy(t => t.Name).ToList(),
                "Precio" => Treatments.OrderBy(t => t.Price).ToList(),
                "Duración" => Treatments.OrderBy(t => t.Duration).ToList(),
                _ => Treatments.ToList()
            };
            Treatments.Clear();
            foreach (var t in sorted) Treatments.Add(t);
        }

        private void OpenAddTreatment()
        {
            var w = new AddEditTreatmentView { DataContext = new AddEditTreatmentViewModel(_context) };
            if (w.ShowDialog() == true) LoadData();
        }

        private void OpenEditTreatment()
        {
            if (SelectedTreatment == null) return;
            var w = new EditTreatmentView { DataContext = new EditTreatmentViewModel(_context, SelectedTreatment) };
            if (w.ShowDialog() == true) LoadData();
        }
    }
}
