using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        public ObservableCollection<Models.Inventory> InventoryItems { get; private set; } = new();
        private Models.Inventory? _selectedInventory;

        public Models.Inventory? SelectedInventory
        {
            get => _selectedInventory;
            set
            {
                _selectedInventory = value;
                OnPropertyChanged();
                (EditInventoryCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string SearchText { get; set; } = string.Empty;

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Product Name",
            "Quantity",
            "Reorder Level",
            "Supplier"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddInventoryCommand { get; }
        public ICommand EditInventoryCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public InventoryViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            AddInventoryCommand = new RelayCommand(OpenAddInventory);
            EditInventoryCommand = new RelayCommand(OpenEditInventory, CanEditInventory);
            ReloadCommand = new RelayCommand(LoadInventory);
            SearchCommand = new RelayCommand(SearchInventory);
            SortCommand = new RelayCommand(SortInventory); // Komenda sortowania

            LoadInventory();
        }

        private void LoadInventory()
        {
            try
            {
                InventoryItems = new ObservableCollection<Models.Inventory>(
                    _context.Inventory
                        .Include(i => i.Supplier) // Include related Supplier
                        .Include(i => i.Room)    // Include related Room
                        .ToList()
                );
                OnPropertyChanged(nameof(InventoryItems));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditInventory() => SelectedInventory != null;

        private void OpenAddInventory()
        {
            var addInventoryView = new AddInventoryView
            {
                DataContext = new AddInventoryViewModel(_context)
            };
            addInventoryView.ShowDialog();
            LoadInventory();
        }

        private void OpenEditInventory()
        {
            if (SelectedInventory == null) return;

            var editInventoryView = new EditInventoryView
            {
                DataContext = new EditInventoryViewModel(_context, SelectedInventory)
            };
            editInventoryView.ShowDialog();
            LoadInventory();
        }

        private void SearchInventory()
        {
            var filtered = _context.Inventory
                .Include(i => i.Supplier)
                .Include(i => i.Room)
                .Where(i => i.ProductName.Contains(SearchText) || i.Supplier.CompanyName.Contains(SearchText))
                .ToList();

            InventoryItems = new ObservableCollection<Models.Inventory>(filtered);
            OnPropertyChanged(nameof(InventoryItems));
        }

        private void SortInventory()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || InventoryItems == null)
                return;

            IEnumerable<Models.Inventory> sortedItems = SelectedSortOption switch
            {
                "Product Name" => InventoryItems.OrderBy(i => i.ProductName),
                "Quantity" => InventoryItems.OrderBy(i => i.Quantity),
                "Reorder Level" => InventoryItems.OrderBy(i => i.ReorderLevel),
                "Supplier" => InventoryItems.OrderBy(i => i.Supplier.CompanyName),
                _ => InventoryItems
            };

            InventoryItems = new ObservableCollection<Models.Inventory>(sortedItems);
            OnPropertyChanged(nameof(InventoryItems));
        }
    }
}
