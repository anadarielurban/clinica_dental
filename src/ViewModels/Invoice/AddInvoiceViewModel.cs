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
                    MessageBox.Show("Selecciona una cita y proporciona un monto válido.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Invoice.Add(Invoice);
                _context.SaveChanges();

                MessageBox.Show("¡Factura creada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear la factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Selecciona una cita.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Invoice.TotalAmount <= 0)
            {
                MessageBox.Show("El monto total debe ser mayor a cero.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
