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
using DentalClinicWPF.Views.Supplier;

namespace DentalClinicWPF.ViewModels.Supplier
{
    public class SupplierViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Supplier> Suppliers { get; private set; }

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

        private Models.Supplier _selectedSupplier;
        public Models.Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
                (EditSupplierCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre de la Empresa", "Nombre de Contacto", "Teléfono"
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
                    SortSuppliers();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand AddSupplierCommand { get; }
        public ICommand EditSupplierCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public SupplierViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Suppliers = new ObservableCollection<Models.Supplier>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); SearchSuppliers(); };

            AddSupplierCommand = new RelayCommand(OpenAddSupplier);
            EditSupplierCommand = new RelayCommand(OpenEditSupplier, CanEditSupplier);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(SearchSuppliers);
            SortCommand = new RelayCommand(SortSuppliers);

            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            try
            {
                var list = _context.Supplier.ToList();
                Suppliers.Clear();
                foreach (var s in list) Suppliers.Add(s);
                UpdateResultsLabel(list.Count, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los proveedores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadSuppliers();
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            if (count == 0) ResultsLabel = "Sin resultados";
            else if (count == 1) ResultsLabel = filtered ? "1 resultado" : "1 proveedor";
            else ResultsLabel = filtered ? $"{count} resultados" : $"{count} proveedores";
        }

        private bool CanEditSupplier() => SelectedSupplier != null;

        private void SearchSuppliers()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadSuppliers(); return; }
            var term = SearchText.Trim().ToLower();
            var filtered = _context.Supplier.ToList()
                .Where(s =>
                    (s.CompanyName?.ToLower().Contains(term) ?? false) ||
                    (s.ContactName?.ToLower().Contains(term) ?? false))
                .ToList();
            Suppliers.Clear();
            foreach (var s in filtered) Suppliers.Add(s);
            UpdateResultsLabel(filtered.Count, true);
        }

        private void SortSuppliers()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Nombre de la Empresa" => Suppliers.OrderBy(s => s.CompanyName).ToList(),
                "Nombre de Contacto" => Suppliers.OrderBy(s => s.ContactName).ToList(),
                "Teléfono" => Suppliers.OrderBy(s => s.PhoneNumber).ToList(),
                _ => Suppliers.ToList()
            };
            Suppliers.Clear();
            foreach (var s in sorted) Suppliers.Add(s);
        }

        private void OpenAddSupplier()
        {
            var v = new AddSupplierView { DataContext = new AddSupplierViewModel(_context) };
            if (v.ShowDialog() == true) LoadSuppliers();
        }

        private void OpenEditSupplier()
        {
            if (SelectedSupplier == null) return;
            var v = new EditSupplierView { DataContext = new EditSupplierViewModel(_context, SelectedSupplier) };
            if (v.ShowDialog() == true) LoadSuppliers();
        }
    }
}
