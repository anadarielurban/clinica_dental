using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Invoice
{
    public class AddInvoiceViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Invoice Invoice { get; set; }
        public ObservableCollection<Models.Appointment> Appointments { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddInvoiceViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Invoice = new Models.Invoice
            {
                InvoiceDate = DateTime.Now,
                IsPaid = false,
                TotalAmount = 0
            };

            Appointments = new ObservableCollection<Models.Appointment>(_context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Dentist)
                .Include(a => a.Treatment)
                .ToList());

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                if (!IsInvoiceDataValid())
                {
                    MessageBox.Show("Please select an appointment and provide a valid amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Invoice.Add(Invoice);
                _context.SaveChanges();

                MessageBox.Show("Invoice created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while creating invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private bool IsInvoiceDataValid()
        {
            if (Invoice.AppointmentID == 0)
            {
                MessageBox.Show("Please select an Appointment.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Invoice.TotalAmount <= 0)
            {
                MessageBox.Show("Total Amount must be greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
