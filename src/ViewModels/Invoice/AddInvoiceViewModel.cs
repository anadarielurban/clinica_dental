using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Invoice
{
    public class AddInvoiceViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.Appointment> AvailableAppointments { get; set; }

        private Models.Appointment _selectedAppointment;
        public Models.Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set { _selectedAppointment = value; OnPropertyChanged(nameof(SelectedAppointment)); OnPropertyChanged(nameof(TotalAmount)); }
        }

        private string _paymentMethod = "Efectivo";
        public string PaymentMethod
        {
            get => _paymentMethod;
            set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod)); }
        }

        private decimal _amountPaid;
        public decimal AmountPaid
        {
            get => _amountPaid;
            set { _amountPaid = value; OnPropertyChanged(nameof(AmountPaid)); }
        }

        public decimal TotalAmount => SelectedAppointment?.Treatment?.Price ?? 0;

        public IRelayCommand SaveCommand { get; }

        public AddInvoiceViewModel(DentalClinicContext context)
        {
            _context = context;
            AvailableAppointments = new ObservableCollection<Models.Appointment>(_context.Appointment.ToList());
            SaveCommand = new RelayCommand(Save);
        }

        private void Save()
        {
            if (SelectedAppointment == null)
            {
                MessageBox.Show("Selecciona una cita primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var invoice = new Models.Invoice
                {
                    PatientID = SelectedAppointment.PatientID,
                    AppointmentID = SelectedAppointment.AppointmentID,
                    InvoiceDate = DateTime.Now,
                    TotalAmount = TotalAmount,
                    AmountPaid = AmountPaid,
                    PaymentMethod = PaymentMethod
                };

                _context.Invoice.Add(invoice);
                _context.SaveChanges();

                MessageBox.Show("Factura guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
