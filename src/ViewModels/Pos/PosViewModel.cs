using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.IO;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Pos
{
    public class PosViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<PosItemViewModel> Cart { get; } = new();
        public ObservableCollection<Models.Patient> Patients { get; } = new();
        public ObservableCollection<PosItemViewModel> SearchResults { get; } = new();
        public ObservableCollection<Models.Appointment> AvailableAppointments { get; } = new();

        private Models.Patient _selectedPatient;
        public Models.Patient SelectedPatient
        {
            get => _selectedPatient;
            set { 
                _selectedPatient = value; 
                OnPropertyChanged(nameof(SelectedPatient)); 
                LoadAppointments(); 
            }
        }

        private Models.Appointment _selectedAppointment;
        public Models.Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set { _selectedAppointment = value; OnPropertyChanged(nameof(SelectedAppointment)); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { if (_searchText == value) return; _searchText = value; OnPropertyChanged(nameof(SearchText)); SearchItems(); }
        }

        private string _paymentMethod = "Efectivo";
        public string PaymentMethod
        {
            get => _paymentMethod;
            set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod)); }
        }
        public ObservableCollection<string> PaymentMethods { get; } = new() { "Efectivo", "Tarjeta", "Transferencia" };

        private decimal _amountReceived;
        public decimal AmountReceived
        {
            get => _amountReceived;
            set { _amountReceived = value; OnPropertyChanged(nameof(AmountReceived)); OnPropertyChanged(nameof(Change)); }
        }

        public decimal Change => AmountReceived > Total ? AmountReceived - Total : 0;
        public decimal Total => Cart.Sum(c => c.Subtotal);

        public IRelayCommand<string> AddItemCommand { get; }
        public IRelayCommand<PosItemViewModel> RemoveItemCommand { get; }
        public IRelayCommand CheckoutCommand { get; }

        public PosViewModel(DentalClinicContext context)
        {
            _context = context;
            var pts = _context.Patient.ToList();
            foreach (var pt in pts) Patients.Add(pt);

            AddItemCommand = new RelayCommand<string>(AddItem);
            RemoveItemCommand = new RelayCommand<PosItemViewModel>(RemoveItem);
            CheckoutCommand = new RelayCommand(Checkout);
        }

        private void LoadAppointments()
        {
            AvailableAppointments.Clear();
            if (SelectedPatient == null) return;
            
            // Cargar citas del paciente que NO estén pagadas aún
            var apps = _context.Appointment
                .Include(a => a.Dentist)
                .Include(a => a.Treatment)
                .Where(a => a.PatientID == SelectedPatient.PatientID)
                .ToList();
                
            foreach (var a in apps) AvailableAppointments.Add(a);
        }

        private void SearchItems()
        {
            SearchResults.Clear();
            if (string.IsNullOrWhiteSpace(SearchText)) return;
            var query = SearchText.ToLower();

            var treatments = _context.Treatment.Where(t => t.Name.ToLower().Contains(query)).Take(5).ToList();
            foreach (var t in treatments)
                SearchResults.Add(new PosItemViewModel { Id = t.TreatmentID, Description = t.Name, UnitPrice = t.Price, IsInventory = false });

            var products = _context.Inventory.Where(p => p.ProductName.ToLower().Contains(query) && p.Quantity > 0).Take(5).ToList();
            foreach (var p in products)
                SearchResults.Add(new PosItemViewModel { Id = p.InventoryID, Description = p.ProductName, UnitPrice = p.Price ?? 0, IsInventory = true });
        }

        private void AddItem(string desc)
        {
            var item = SearchResults.FirstOrDefault(x => x.Description == desc);
            if (item == null) return;

            var existing = Cart.FirstOrDefault(c => c.Description == item.Description && c.IsInventory == item.IsInventory);
            if (existing != null) existing.Quantity++;
            else Cart.Add(new PosItemViewModel { Id = item.Id, Description = item.Description, UnitPrice = item.UnitPrice, IsInventory = item.IsInventory });

            SearchText = string.Empty;
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Change));
        }

        private void RemoveItem(PosItemViewModel item)
        {
            if (item == null) return;
            Cart.Remove(item);
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Change));
        }

        private void Checkout()
        {
            if (SelectedPatient == null) { MessageBox.Show("Selecciona un paciente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!Cart.Any()) { MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            
            if (PaymentMethod == "Efectivo" && AmountReceived < Total) 
            { 
                MessageBox.Show("El efectivo recibido es menor al total.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); 
                return; 
            }

            try
            {
                // Si el pago es con tarjeta o transferencia, el "recibido" es igual al total (no hay cambio)
                decimal finalAmountReceived = PaymentMethod == "Efectivo" ? AmountReceived : Total;
                decimal finalChange = PaymentMethod == "Efectivo" ? Change : 0;

                var invoice = new Models.Invoice
                {
                    PatientID = SelectedPatient.PatientID,
                    AppointmentID = SelectedAppointment?.AppointmentID, // Vincular a la cita seleccionada
                    InvoiceDate = DateTime.Now,
                    TotalAmount = Total,
                    AmountPaid = Total,
                    AmountReceived = finalAmountReceived,
                    Change = finalChange,
                    PaymentMethod = PaymentMethod
                };
                
                _context.Invoice.Add(invoice);

                foreach (var item in Cart.Where(c => c.IsInventory))
                {
                    var prod = _context.Inventory.Find(item.Id);
                    if (prod != null) prod.Quantity -= item.Quantity;
                }

                _context.SaveChanges();

                // Generar y abrir Ticket
                string ticketPath = Path.Combine(Path.GetTempPath(), $"Ticket_{invoice.InvoiceID}.pdf");
                Services.TicketPdfService.Generate(invoice, Cart.ToList(), ticketPath);
                
                // Abrir PDF automáticamente
                Process.Start(new ProcessStartInfo(ticketPath) { UseShellExecute = true });

                MessageBox.Show($"Venta #{invoice.InvoiceID} registrada.\nCambio: {finalChange:C}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                Cart.Clear();
                AmountReceived = 0;
                SelectedAppointment = null;
                OnPropertyChanged(nameof(Total));
            }
            catch (Exception ex) { MessageBox.Show($"Error al cobrar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
