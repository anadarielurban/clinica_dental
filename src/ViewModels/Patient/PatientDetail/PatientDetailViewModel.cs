using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using System.Printing;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.Appointment;
using DentalClinicWPF.Views.Prescription;
using DentalClinicWPF.Views.TreatmentPlan;
using DentalClinicWPF.Views.Invoice;
using DentalClinicWPF.Views.MedicalRecord;

namespace DentalClinicWPF.ViewModels.Patient.PatientDetail
{
    public partial class PatientDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Patient Patient { get; }
        public ObservableCollection<Models.Appointment> Appointments { get; set; }
        public ObservableCollection<Models.Prescription> Prescriptions { get; set; }
        public ObservableCollection<Models.TreatmentPlan> TreatmentPlans { get; set; }
        public ObservableCollection<Models.Invoice> Invoices { get; set; }
        public ObservableCollection<Models.MedicalRecord> MedicalRecords { get; set; }
        public ObservableCollection<Models.Budget> Budgets { get; set; }

        public ObservableCollection<ToothViewModel> TeethUpperRight { get; set; } = new();
        public ObservableCollection<ToothViewModel> TeethUpperLeft { get; set; } = new();
        public ObservableCollection<ToothViewModel> TeethLowerRight { get; set; } = new();
        public ObservableCollection<ToothViewModel> TeethLowerLeft { get; set; } = new();

        public Models.Appointment SelectedAppointment { get; set; }
        public Models.Prescription SelectedPrescription { get; set; }
        public Models.TreatmentPlan SelectedTreatmentPlan { get; set; }
        public Models.Invoice SelectedInvoice { get; set; }
        public Models.MedicalRecord SelectedMedicalRecord { get; set; }

        public ICommand AddAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand PreviewAppointmentCommand { get; }
        public ICommand AddPrescriptionCommand { get; }
        public ICommand EditPrescriptionCommand { get; }
        public ICommand PreviewPrescriptionCommand { get; }
        public ICommand AddTreatmentPlanCommand { get; }
        public ICommand EditTreatmentPlanCommand { get; }
        public ICommand PreviewTreatmentPlanCommand { get; }
        public ICommand PayTreatmentPlanCommand { get; }
        public ICommand AddInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand PreviewInvoiceCommand { get; }
        public ICommand AddMedicalRecordCommand { get; }
        public ICommand EditMedicalRecordCommand { get; }
        public ICommand PreviewMedicalRecordCommand { get; }
        public ICommand SaveOdontogramCommand { get; }
        public ICommand SelectToothCommand { get; }
        public MedicalHistoryViewModel MedicalHistory { get; }

        private ToothViewModel _selectedTooth;
        public ToothViewModel SelectedTooth
        {
            get => _selectedTooth;
            set { if (_selectedTooth == value) return; _selectedTooth = value; OnPropertyChanged(nameof(SelectedTooth)); }
        }

        public ICommand PrintRecordCommand { get; }
        public ICommand DownloadExpedientePdfCommand { get; }

        public PatientDetailViewModel(DentalClinicContext context, Models.Patient patient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));

            Appointments = new ObservableCollection<Models.Appointment>();
            Prescriptions = new ObservableCollection<Models.Prescription>();
            TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>();
            Invoices = new ObservableCollection<Models.Invoice>();
            MedicalRecords = new ObservableCollection<Models.MedicalRecord>();
            Budgets = new ObservableCollection<Models.Budget>();
            LoadBudgets();

            MedicalHistory = new MedicalHistoryViewModel(_context, Patient);
            LoadOdontogram();
            LoadTreatmentPlans();
            LoadAppointments();
            LoadPrescriptions();
            LoadInvoices();
            LoadMedicalRecords();

            SaveOdontogramCommand = new RelayCommand(() => {
                try {
                    _context.SaveChanges();
                    MessageBox.Show("Odontograma guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                } catch (Exception ex) {
                    MessageBox.Show($"Error al guardar el odontograma:\n{ex.Message}\n\nDetalle: {ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            SelectToothCommand = new RelayCommand<ToothViewModel>(t =>
            {
                if (t == null) return;
                if (_selectedTooth != null) _selectedTooth.IsSelected = false;
                SelectedTooth = t;
                t.IsSelected = true;
            });

            PrintRecordCommand = new RelayCommand(PrintRecord);
            DownloadExpedientePdfCommand = new RelayCommand(DownloadExpedientePdf);

            AddAppointmentCommand = new RelayCommand(() => {
                try {
                    var w = new AddAppointmentView { DataContext = new ViewModels.Appointment.AddAppointmentViewModel(_context) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditAppointmentCommand = new RelayCommand(() => {
                if (SelectedAppointment == null) { MessageBox.Show("Selecciona una cita de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditAppointmentView { DataContext = new ViewModels.Appointment.EditAppointmentViewModel(_context, SelectedAppointment) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show($"Error al editar la cita: {ex.Message}", "Error"); }
            });
            PreviewAppointmentCommand = new RelayCommand(() => {
                if (SelectedAppointment == null) { MessageBox.Show("Selecciona una cita primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                MessageBox.Show($"Cita: {SelectedAppointment.AppointmentDate:dd/MM/yyyy} - {SelectedAppointment.Notes}", "Detalles de Cita");
            });

            AddPrescriptionCommand = new RelayCommand(() => {
                try {
                    var w = new AddPrescriptionView { DataContext = new ViewModels.Prescription.AddPrescriptionViewModel(_context) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditPrescriptionCommand = new RelayCommand(() => {
                if (SelectedPrescription == null) { MessageBox.Show("Selecciona una receta de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditPrescriptionView { DataContext = new ViewModels.Prescription.EditPrescriptionViewModel(SelectedPrescription, _context) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show($"Error al editar la receta: {ex.Message}", "Error"); }
            });
            PreviewPrescriptionCommand = new RelayCommand(() => {
                if (SelectedPrescription == null) { MessageBox.Show("Selecciona una receta primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var w = new PrescriptionPreviewWindow(SelectedPrescription);
                w.ShowDialog();
            });

            AddTreatmentPlanCommand = new RelayCommand(() => {
                try {
                    var w = new AddTreatmentPlanView { DataContext = new ViewModels.TreatmentPlan.AddTreatmentPlanViewModel(_context, Patient) };
                    if (w.ShowDialog() == true) { LoadTreatmentPlans(); LoadAppointments(); LoadPrescriptions(); LoadInvoices(); LoadMedicalRecords(); }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditTreatmentPlanCommand = new RelayCommand(() => {
                if (SelectedTreatmentPlan == null) { MessageBox.Show("Selecciona un plan de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditTreatmentPlanView { DataContext = new ViewModels.TreatmentPlan.EditTreatmentPlanViewModel(_context, SelectedTreatmentPlan) };
                    if (w.ShowDialog() == true) { LoadTreatmentPlans(); LoadAppointments(); LoadPrescriptions(); LoadInvoices(); LoadMedicalRecords(); }
                } catch (Exception ex) { MessageBox.Show($"Error al editar el plan: {ex.Message}", "Error"); }
            });
            PayTreatmentPlanCommand = new RelayCommand(() => {
                if (SelectedTreatmentPlan == null) { MessageBox.Show("Selecciona un presupuesto primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var invoice = new Models.Invoice {
                        PatientID = Patient.PatientID,
                        TreatmentPlanID = SelectedTreatmentPlan.TreatmentPlanID,
                        TotalAmount = SelectedTreatmentPlan.TotalAmount,
                        AmountPaid = SelectedTreatmentPlan.TotalAmount,
                        PaymentMethod = "Efectivo",
                        InvoiceDate = DateTime.Now
                    };
                    var planInDb = _context.TreatmentPlan.Find(SelectedTreatmentPlan.TreatmentPlanID);
                    if (planInDb != null) planInDb.Status = "Pagado";
                    _context.Invoice.Add(invoice);
                    _context.SaveChanges();
                    MessageBox.Show($"¡Cobro registrado con éxito!\nFolio: #{invoice.InvoiceID}\nTotal: {invoice.TotalAmount:C}", "Cobro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTreatmentPlans();
                    LoadInvoices();
                } catch (Exception ex) {
                    MessageBox.Show($"Error al cobrar: {ex.Message}\n{ex.InnerException?.Message}", "Error");
                }
            });
            PreviewTreatmentPlanCommand = new RelayCommand(() => {
                if (SelectedTreatmentPlan == null) { MessageBox.Show("Selecciona un plan primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Fecha: {SelectedTreatmentPlan.CreationDate:dd/MM/yyyy}");
                sb.AppendLine($"Estado: {SelectedTreatmentPlan.Status}");
                sb.AppendLine("--------------------------------------------------");
                if (SelectedTreatmentPlan.Items != null && SelectedTreatmentPlan.Items.Count > 0) {
                    foreach (var item in SelectedTreatmentPlan.Items) {
                        var name = item.Treatment != null ? item.Treatment.Name : "Tratamiento";
                        sb.AppendLine($"• {name} (x{item.Quantity}) - {item.Subtotal:C}");
                    }
                } else { sb.AppendLine("(No hay tratamientos cargados)"); }
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"TOTAL: {SelectedTreatmentPlan.TotalAmount:C}");
                MessageBox.Show(sb.ToString(), "Detalles del Presupuesto");
            });

            AddInvoiceCommand = new RelayCommand(() => {
                try {
                    var w = new AddInvoiceView { DataContext = new ViewModels.Invoice.AddInvoiceViewModel(_context) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditInvoiceCommand = new RelayCommand(() => {
                if (SelectedInvoice == null) { MessageBox.Show("Selecciona una factura de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditInvoiceView { DataContext = new ViewModels.Invoice.EditInvoiceViewModel(_context, SelectedInvoice) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show($"Error al editar la factura: {ex.Message}", "Error"); }
            });
            PreviewInvoiceCommand = new RelayCommand(() => {
                if (SelectedInvoice == null) { MessageBox.Show("Selecciona una factura primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                MessageBox.Show($"Factura Total: {SelectedInvoice.TotalAmount:C}", "Detalles de Factura");
            });

            AddMedicalRecordCommand = new RelayCommand(() => {
                try {
                    var w = new AddMedicalRecordView { DataContext = new ViewModels.MedicalRecord.AddMedicalRecordViewModel(_context) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditMedicalRecordCommand = new RelayCommand(() => {
                if (SelectedMedicalRecord == null) { MessageBox.Show("Selecciona un expediente de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditMedicalRecordView { DataContext = new ViewModels.MedicalRecord.EditMedicalRecordViewModel(_context, SelectedMedicalRecord) };
                    if (w.ShowDialog() == true) { LoadAppointments(); }
                } catch (Exception ex) { MessageBox.Show($"Error al editar el expediente: {ex.Message}", "Error"); }
            });
            PreviewMedicalRecordCommand = new RelayCommand(() => {
                if (SelectedMedicalRecord == null) { MessageBox.Show("Selecciona un expediente primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                MessageBox.Show($"Expediente: {SelectedMedicalRecord.Notes}", "Detalles");
            });
        }

        private void LoadAppointments()
        {
            Appointments.Clear();
            foreach (var a in _context.Appointment.Where(a => a.PatientID == Patient.PatientID).ToList())
                Appointments.Add(a);
        }

        private void LoadPrescriptions()
        {
            Prescriptions.Clear();
            foreach (var p in _context.Prescription.Where(p => p.PatientID == Patient.PatientID).ToList())
                Prescriptions.Add(p);
        }

        private void LoadInvoices()
        {
            Invoices.Clear();
            foreach (var i in _context.Invoice.Where(i => i.PatientID == Patient.PatientID).ToList())
                Invoices.Add(i);
        }

        private void LoadMedicalRecords()
        {
            MedicalRecords.Clear();
            foreach (var m in _context.MedicalRecord.Where(m => m.PatientID == Patient.PatientID).ToList())
                MedicalRecords.Add(m);
        }

        private void LoadTreatmentPlans()
        {
            TreatmentPlans.Clear();
            var plans = _context.TreatmentPlan
                .Include(t => t.Items)
                .ThenInclude(i => i.Treatment)
                .Where(t => t.PatientID == Patient.PatientID)
                .ToList();
            foreach (var plan in plans) TreatmentPlans.Add(plan);
        }

        private void LoadBudgets()
        {
            Budgets.Clear();
            if (_context != null && Patient != null)
            {
                // Filtrar SOLO los presupuestos del paciente actual
                var budgetsFromDb = _context.Budgets
                    .Where(b => b.PatientId == Patient.PatientID)
                    .OrderByDescending(b => b.Date)
                    .ToList();
                foreach (var b in budgetsFromDb)
                    Budgets.Add(b);
            }
        }

        private void LoadOdontogram()
        {
            for (int i = 18; i >= 11; i--) AddTooth(TeethUpperRight, i);
            for (int i = 21; i <= 28; i++) AddTooth(TeethUpperLeft, i);
            for (int i = 48; i >= 41; i--) AddTooth(TeethLowerRight, i);
            for (int i = 31; i <= 38; i++) AddTooth(TeethLowerLeft, i);
        }

        private void AddTooth(ObservableCollection<ToothViewModel> collection, int toothNumber)
        {
            var record = _context.ToothRecord.FirstOrDefault(t => t.PatientID == Patient.PatientID && t.ToothNumber == toothNumber);
            if (record == null)
            {
                record = new Models.ToothRecord { PatientID = Patient.PatientID, ToothNumber = toothNumber, Condition = "Sano", Notes = string.Empty, IsPrimaryTooth = false };
                _context.ToothRecord.Add(record);
            }
            collection.Add(new ToothViewModel(record));
        }

        private void PrintRecord()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() != true) return;
                var doc = BuildExpedienteDocument();
                printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Expediente Clínico - " + Patient.FullName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir el expediente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadExpedientePdf()
        {
            var safeName = string.Join("_", Patient.FullName.Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Documento PDF (*.pdf)|*.pdf",
                FileName = $"Expediente_{safeName}_{DateTime.Now:yyyyMMdd}.pdf",
                Title = "Guardar Expediente Clínico"
            };
            if (sfd.ShowDialog() != true) return;
            try
            {
                Services.ExpedientePdfService.Generate(this, sfd.FileName);
                MessageBox.Show($"Expediente guardado correctamente:\n\n{sfd.FileName}", "PDF generado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF:\n\n{ex.Message}\n\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument BuildExpedienteDocument()
        {
            var doc = new FlowDocument { PagePadding = new Thickness(40, 25, 40, 20), ColumnWidth = double.PositiveInfinity, FontFamily = new FontFamily("Segoe UI"), FontSize = 10, PageWidth = 780, Background = Brushes.White };
            doc.Blocks.Add(new Paragraph(new Run("EXPEDIENTE CLÍNICO")) { FontSize = 18, FontWeight = FontWeights.Bold });
            doc.Blocks.Add(new Paragraph(new Run($"Paciente: {Patient.FullName}\nFecha: {DateTime.Now:dd/MM/yyyy}")));
            return doc;
        }

        private Paragraph SectionHeader(string title, Brush accent, Brush text)
        {
            var p = new Paragraph { Margin = new Thickness(0, 10, 0, 8) };
            p.Inlines.Add(new Run("▌ ") { Foreground = accent, FontSize = 14, FontWeight = FontWeights.Bold });
            p.Inlines.Add(new Run(title) { Foreground = text, FontSize = 12, FontWeight = FontWeights.Bold });
            return p;
        }
    }
}

