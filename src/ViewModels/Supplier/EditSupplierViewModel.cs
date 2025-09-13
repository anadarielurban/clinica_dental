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

        public Models.Supplier OriginalSupplier { get; }
        public Models.Supplier EditingSupplier { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditSupplierViewModel(DentalClinicContext context, Models.Supplier supplier)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OriginalSupplier = supplier ?? throw new ArgumentNullException(nameof(supplier));

            // Create a copy of the supplier for editing
            EditingSupplier = new Models.Supplier
            {
                SupplierID = supplier.SupplierID,
                CompanyName = supplier.CompanyName,
                ContactName = supplier.ContactName,
                PhoneNumber = supplier.PhoneNumber,
                Email = supplier.Email,
                Address = supplier.Address
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Save()
        {
            try
            {
                if (!IsSupplierDataValid())
                {
                    MessageBox.Show("Please fill in all required fields correctly!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update the original supplier with changes
                OriginalSupplier.CompanyName = EditingSupplier.CompanyName;
                OriginalSupplier.ContactName = EditingSupplier.ContactName;
                OriginalSupplier.PhoneNumber = EditingSupplier.PhoneNumber;
                OriginalSupplier.Email = EditingSupplier.Email;
                OriginalSupplier.Address = EditingSupplier.Address;

                _context.Supplier.Update(OriginalSupplier);
                _context.SaveChanges();

                MessageBox.Show("Supplier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void Delete()
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this supplier?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Supplier.Remove(OriginalSupplier);
                    _context.SaveChanges();

                    MessageBox.Show("Supplier deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while deleting supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private bool IsSupplierDataValid()
        {
            if (string.IsNullOrWhiteSpace(EditingSupplier.CompanyName) ||
                string.IsNullOrWhiteSpace(EditingSupplier.PhoneNumber) ||
                string.IsNullOrWhiteSpace(EditingSupplier.Email))
            {
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(EditingSupplier.PhoneNumber, @"^\+?\d+$"))
            {
                MessageBox.Show("Invalid phone number format!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(EditingSupplier.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Invalid email format!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
