using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Drawing.Printing;
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
        private List<Models.Patient> _allPatients = new();
        public ObservableCollection<Models.Patient> FilteredPatients { get; } = new();
        public ObservableCollection<Models.Appointment> AvailableAppointments { get; } = new();
        public ObservableCollection<object> SearchResults { get; } = new();
        public ObservableCollection<string> PaymentMethods { get; } = new() { "Efectivo", "Tarjeta", "Transferencia" };

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) { SearchCatalog(); } }
        }

        private string _patientSearchText;
        public string PatientSearchText
        {
            get => _patientSearchText;
            set { if (SetProperty(ref _patientSearchText, value)) { FilterPatients(); } }
        }

        private Models.Patient _selectedPatient;
        public Models.Patient SelectedPatient
        {
            get => _selectedPatient;
            set 
            { 
                if (SetProperty(ref _selectedPatient, value)) 
                {
                    if (value != null)
                    {
                        _patientSearchText = $"{value.FirstName} {value.LastName}";
                        OnPropertyChanged(nameof(PatientSearchText));
                    }
                    LoadPatientAppointments(); 
                } 
            }
        }

        private Models.Appointment _selectedAppointment;
        public Models.Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set => SetProperty(ref _selectedAppointment, value);
        }

        private string _paymentMethod = "Efectivo";
        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        private decimal _amountReceived;
        public decimal AmountReceived
        {
            get => _amountReceived;
            set { if (SetProperty(ref _amountReceived, value)) { OnPropertyChanged(nameof(Change)); } }
        }

        public decimal Total => Cart.Sum(x => x.Quantity * x.UnitPrice);
        public decimal Change => AmountReceived > Total ? AmountReceived - Total : 0;

        public IRelayCommand AddItemCommand { get; }
        public IRelayCommand<PosItemViewModel> RemoveItemCommand { get; }
        public IRelayCommand CheckoutCommand { get; }
        public IRelayCommand ImprimirTicketCommand { get; }

        public PosViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allPatients = _context.Patient.ToList();
            FilterPatients(); 

            AddItemCommand = new RelayCommand<object>(AddItem);
            RemoveItemCommand = new RelayCommand<PosItemViewModel>(RemoveItem);
            CheckoutCommand = new RelayCommand(Checkout);
            
            ImprimirTicketCommand = new RelayCommand(() => ImprimirTicket(null, "COPIA CLIENTE", false));

            Cart.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(Change));
            };

            SearchCatalog();
        }

        private void FilterPatients()
        {
            FilteredPatients.Clear();
            var term = PatientSearchText?.Trim().ToLower() ?? "";
            var matches = _allPatients
                .Where(p => string.IsNullOrEmpty(term) || p.FirstName.ToLower().Contains(term) || p.LastName.ToLower().Contains(term))
                .Take(20).ToList();

            foreach (var p in matches) FilteredPatients.Add(p);
        }

        private void SearchCatalog()
        {
            SearchResults.Clear();
            var term = SearchText?.Trim().ToLower() ?? "";

            var treatments = _context.Treatment.Where(t => string.IsNullOrEmpty(term) || t.Name.ToLower().Contains(term)).Take(50).ToList();
            foreach (var t in treatments) SearchResults.Add(t);

            var products = _context.Inventory.Where(i => string.IsNullOrEmpty(term) || i.ProductName.ToLower().Contains(term)).Take(50).ToList();
            foreach (var p in products) SearchResults.Add(p);
        }

        private void LoadPatientAppointments()
        {
            AvailableAppointments.Clear();
            if (SelectedPatient == null) return;

            var appts = _context.Appointment
                .Include(a => a.Treatment)
                .Where(a => a.PatientID == SelectedPatient.PatientID && a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate).ToList();

            foreach (var a in appts) AvailableAppointments.Add(a);
        }

        private void AddItem(object item)
        {
            if (item == null) return;
            if (item is Models.Treatment treatment)
                Cart.Add(new PosItemViewModel { Description = treatment.Name, UnitPrice = treatment.Price, Quantity = 1 });
            else if (item is Models.Inventory product)
                Cart.Add(new PosItemViewModel { Description = product.ProductName, UnitPrice = product.Price ?? 0, Quantity = 1 });

            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Change));
        }

        private void RemoveItem(PosItemViewModel item)
        {
            if (item != null && Cart.Contains(item))
            {
                Cart.Remove(item);
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(Change));
            }
        }

        private void Checkout()
        {
            if (!Cart.Any()) { MessageBox.Show("El carrito está vacío.", "Aviso"); return; }
            if (SelectedPatient == null) { MessageBox.Show("Selecciona un paciente para facturar.", "Aviso"); return; }

            try
            {
                var invoice = new Models.Invoice
                {
                    PatientID = SelectedPatient.PatientID,
                    AppointmentID = SelectedAppointment?.AppointmentID,
                    InvoiceDate = DateTime.Now,
                    TotalAmount = Total,
                    AmountPaid = AmountReceived > 0 ? AmountReceived : Total,
                    AmountReceived = AmountReceived,
                    Change = Change,
                    PaymentMethod = PaymentMethod
                };

                _context.Invoice.Add(invoice);
                _context.SaveChanges();

                // 1. IMPRIMIR PRIMER TICKET (CLIENTE) SIN CORTE AUTOMÁTICO
                ImprimirTicket(invoice, "COPIA CLIENTE", false);

                // 2. MENSAJE DE PAUSA PARA CORTAR EL PAPEL
                MessageBox.Show("Por favor, corte el ticket del cliente con la navaja y presione Aceptar para imprimir la copia del consultorio.", "Corte el ticket", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Threading.Thread.Sleep(2000);

                // 3. IMPRIMIR SEGUNDO TICKET (CONSULTORIO) CON CORTE AUTOMÁTICO
                ImprimirTicket(invoice, "COPIA CONSULTORIO", true);

                MessageBox.Show("Venta registrada y tickets enviados a la impresora.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                Cart.Clear();
                AmountReceived = 0;
                SelectedPatient = null;
                PatientSearchText = "";
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(Change));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cobrar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImprimirTicket(Models.Invoice invoice, string tipoCopia, bool cortarPapel)
        {
            try
            {
                string impresora = "";
                using (PrintDocument pd = new PrintDocument())
                {
                    impresora = pd.PrinterSettings.PrinterName;
                }

                if (string.IsNullOrEmpty(impresora)) return;

                var consultorio = _context.Room.FirstOrDefault();
                string direccion = RemoverAcentos(consultorio?.Address ?? "Direccion no registrada");
                string telefono = consultorio?.Phone ?? "Telefono no registrado";
                string numConsul = RemoverAcentos(consultorio?.RoomNumber ?? "");
                string paciente = SelectedPatient != null ? RemoverAcentos($"{SelectedPatient.FirstName} {SelectedPatient.LastName}") : "Publico General";

                var sb = new StringBuilder();

                // DISEÑO DEL TICKET (58mm / 32 caracteres por línea)
                sb.Append("\x1B\x40");           // Inicializar impresora
                sb.Append("\x1B\x61\x01");       // Centrar texto

                // 1. Encabezado (Nombre de la clínica en doble altura y negritas)
                sb.Append("\x1B\x21\x38"); 
                sb.AppendLine("CLINICA DENTAL");
                sb.Append("\x1B\x21\x00"); 
                
                // 2. Datos del Consultorio
                sb.AppendLine("--------------------------------");
                if (!string.IsNullOrEmpty(numConsul))
                    sb.AppendLine(numConsul);

                sb.AppendLine(direccion);
                sb.AppendLine("Tel: " + telefono);
                sb.AppendLine("--------------------------------");

                // 3. Tipo de copia
                sb.Append("\x1B\x21\x08"); // Negrita
                sb.AppendLine("      " + tipoCopia);
                sb.Append("\x1B\x21\x00"); // Normal
                sb.AppendLine("--------------------------------");

                // 4. Datos de la Venta (Alineados a la izquierda)
                sb.Append("\x1B\x61\x00"); 
                string folioStr = invoice != null ? $"Folio: #{invoice.InvoiceID:D6}" : "Folio: N/A";
                sb.AppendLine(folioStr);
                sb.AppendLine("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                sb.AppendLine("Paciente: " + paciente);
                sb.AppendLine("--------------------------------");

                // 5. Encabezado de productos (Negrita)
                sb.Append("\x1B\x21\x08"); // Negrita
                sb.AppendLine("CANT  DESCRIPCION        TOTAL");
                sb.Append("\x1B\x21\x00"); 
                
                // 6. Lista de productos
                foreach (var item in Cart)
                {
                    string nombre = RemoverAcentos(item.Description);
                    if (nombre.Length > 16) nombre = nombre.Substring(0, 16);
                    decimal subtotal = item.Quantity * item.UnitPrice;

                    sb.AppendLine(item.Quantity.ToString().PadRight(4) + nombre.PadRight(16) + "$" + subtotal.ToString("F2").PadLeft(9));
                }

                sb.AppendLine("--------------------------------");

                // 7. Desglose de Pago (Alineado a la derecha) - SIN IVA
                sb.Append("\x1B\x61\x02"); // Alinear a la derecha
                sb.AppendLine("Subtotal:     $" + Total.ToString("F2"));
                sb.AppendLine();
                
                if (invoice != null)
                {
                    sb.AppendLine("Recibido:     $" + invoice.AmountReceived.ToString("F2"));
                    sb.AppendLine("Cambio:       $" + invoice.Change.ToString("F2"));
                    sb.AppendLine("Metodo:       " + invoice.PaymentMethod);
                }

                sb.AppendLine();

                // 8. TOTAL (Centrado, Negrita y Doble Altura)
                sb.Append("\x1B\x61\x01"); // Centrar
                sb.Append("\x1B\x21\x18"); // Negrita + Doble Altura
                sb.AppendLine("TOTAL: $" + Total.ToString("F2"));
                sb.Append("\x1B\x21\x00"); // Normal
                sb.AppendLine();

                // 9. Pie de página elegante
                sb.AppendLine("--------------------------------");
                sb.AppendLine("      ¡Gracias por su visita!    ");
                sb.AppendLine("       Conserve su ticket        ");
                sb.AppendLine("--------------------------------");
                sb.AppendLine();
                sb.AppendLine();

                // 10. Comando de corte de papel (solo para la copia del consultorio)
                if (cortarPapel)
                {
                    sb.Append("\x1D\x56\x00"); 
                }

                DentalClinicWPF.Helpers.RawPrinterHelper.SendStringToPrinter(impresora, sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string RemoverAcentos(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            return texto
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                .Replace("ñ", "n").Replace("Ñ", "N");
        }
    }
}