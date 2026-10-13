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
    public class EditInventoryViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Inventory Inventory { get; set; }
        public ObservableCollection<Models.Supplier> Suppliers { get; }
        public ObservableCollection<Models.Room> Rooms { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditInventoryViewModel(DentalClinicContext context, Models.Inventory inventory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

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
                    MessageBox.Show("Completa todos los campos correctamente.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Inventory.Update(Inventory);
                _context.SaveChanges();
                MessageBox.Show("¡Elemento de inventario actualizado!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("El nombre del producto es obligatorio.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Inventory.Quantity < 0)
            {
                MessageBox.Show("La cantidad no puede ser negativa.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Inventory.ReorderLevel < 0)
            {
                MessageBox.Show("El nivel de reorden no puede ser negativo.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Inventory.SupplierID == 0)
            {
                MessageBox.Show("Selecciona un proveedor.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
