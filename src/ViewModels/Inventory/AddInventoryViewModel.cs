using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Inventory
{
    public class AddInventoryViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Inventory Inventory { get; set; } = new();
        public ObservableCollection<Models.Supplier> Suppliers { get; }
        public ObservableCollection<Models.Room> Rooms { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddInventoryViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Inventory = new Models.Inventory
            {
                ProductName = string.Empty,
                Quantity = 0,
                ReorderLevel = 0,
                SupplierID = 0,
                RoomID = null
            };

            Suppliers = new ObservableCollection<Models.Supplier>(_context.Supplier.ToList());
            Rooms = new ObservableCollection<Models.Room>(_context.Room.ToList());

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                if (!IsInventoryDataValid())
                {
                    MessageBox.Show("Please fill in all required fields correctly!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Inventory.Add(Inventory);
                _context.SaveChanges();
                MessageBox.Show("Inventory item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }

        private bool IsInventoryDataValid()
        {
            if (string.IsNullOrWhiteSpace(Inventory.ProductName))
            {
                MessageBox.Show("Product Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Inventory.Quantity < 0)
            {
                MessageBox.Show("Quantity cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Inventory.ReorderLevel < 0)
            {
                MessageBox.Show("Reorder Level cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Inventory.SupplierID == 0)
            {
                MessageBox.Show("Please select a Supplier.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
