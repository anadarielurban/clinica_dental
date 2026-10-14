using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.Views.Inventory;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Inventory
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Inventory> InventoryItems { get; private set; }

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

        private Models.Inventory _selectedInventory;
        public Models.Inventory SelectedInventory
        {
            get => _selectedInventory;
            set
            {
                _selectedInventory = value;
                OnPropertyChanged();
                (EditInventoryCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre del Producto", "Cantidad", "Nivel de Reorden", "Proveedor"
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
                    SortInventory();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand AddInventoryCommand { get; }
        public ICommand EditInventoryCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public InventoryViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            InventoryItems = new ObservableCollection<Models.Inventory>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); SearchInventory(); };

            AddInventoryCommand = new RelayCommand(OpenAddInventory);
            EditInventoryCommand = new RelayCommand(OpenEditInventory, CanEditInventory);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(SearchInventory);
            SortCommand = new RelayCommand(SortInventory);

            LoadInventory();
        }

        private void LoadInventory()
        {
            try
            {
                var list = _context.Inventory.Include(i => i.Supplier).Include(i => i.Room).ToList();
                InventoryItems.Clear();
                foreach (var i in list) InventoryItems.Add(i);
                UpdateResultsLabel(list.Count, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el inventario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadInventory();
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            if (count == 0) ResultsLabel = "Sin resultados";
            else if (count == 1) ResultsLabel = filtered ? "1 resultado" : "1 producto";
            else ResultsLabel = filtered ? $"{count} resultados" : $"{count} productos";
        }

        private bool CanEditInventory() => SelectedInventory != null;

        private void SearchInventory()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadInventory(); return; }
            var term = SearchText.Trim().ToLower();
            var filtered = _context.Inventory.Include(i => i.Supplier).Include(i => i.Room).ToList()
                .Where(i =>
                    (i.ProductName?.ToLower().Contains(term) ?? false) ||
                    (i.Supplier?.CompanyName?.ToLower().Contains(term) ?? false))
                .ToList();
            InventoryItems.Clear();
            foreach (var i in filtered) InventoryItems.Add(i);
            UpdateResultsLabel(filtered.Count, true);
        }

        private void SortInventory()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Nombre del Producto" => InventoryItems.OrderBy(i => i.ProductName).ToList(),
                "Cantidad" => InventoryItems.OrderBy(i => i.Quantity).ToList(),
                "Nivel de Reorden" => InventoryItems.OrderBy(i => i.ReorderLevel).ToList(),
                "Proveedor" => InventoryItems.OrderBy(i => i.Supplier?.CompanyName).ToList(),
                _ => InventoryItems.ToList()
            };
            InventoryItems.Clear();
            foreach (var i in sorted) InventoryItems.Add(i);
        }

        private void OpenAddInventory()
        {
            var v = new AddInventoryView { DataContext = new AddInventoryViewModel(_context) };
            if (v.ShowDialog() == true) LoadInventory();
        }

        private void OpenEditInventory()
        {
            if (SelectedInventory == null) return;
            var v = new EditInventoryView { DataContext = new EditInventoryViewModel(_context, SelectedInventory) };
            if (v.ShowDialog() == true) LoadInventory();
        }
    }
}
