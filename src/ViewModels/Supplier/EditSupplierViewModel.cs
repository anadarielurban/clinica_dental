using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Supplier
{
    public class EditSupplierViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Supplier EditingSupplier { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditSupplierViewModel(DentalClinicContext context, Models.Supplier supplier)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            EditingSupplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
            
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(EditingSupplier.CompanyName))
            {
                MessageBox.Show("El nombre de la empresa es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _context.Supplier.Update(EditingSupplier);
                _context.SaveChanges();
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Error al guardar el proveedor:\n{msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete()
        {
            if (MessageBox.Show("¿Seguro que quieres eliminar este proveedor?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Supplier.Remove(EditingSupplier);
                    _context.SaveChanges();
                    CloseWindow(true);
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"Error al eliminar el proveedor:\n{msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Cancel() => CloseWindow(false);

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
    }
}
