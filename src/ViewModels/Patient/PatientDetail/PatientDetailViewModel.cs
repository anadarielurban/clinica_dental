using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;  // for SaveFileDialog
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.ViewModels.Appointment;
using DentalClinicWPF.ViewModels.Invoice;
using DentalClinicWPF.ViewModels.MedicalRecord;
using DentalClinicWPF.ViewModels.Prescription;
using DentalClinicWPF.ViewModels.TreatmentPlan;
using DentalClinicWPF.Views.Appointment;
using DentalClinicWPF.Views.Invoice;
using DentalClinicWPF.Views.MedicalRecord;
using DentalClinicWPF.Views.Prescription;
using DentalClinicWPF.Views.TreatmentPlan;

namespace DentalClinicWPF.ViewModels.Patient.PatientDetail
{
    public class PatientDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public PatientDetailViewModel(DentalClinicContext context, Models.Patient patient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));

            // Initialize collections
            Appointments = new ObservableCollection<Models.Appointment>();
            Prescriptions = new ObservableCollection<Models.Prescription>();
            TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>();
            Invoices = new ObservableCollection<Models.Invoice>();
            MedicalRecords = new ObservableCollection<Models.MedicalRecord>();

            // Initialize commands
            // Appointments
            AddAppointmentCommand = new RelayCommand(OpenAddAppointmentWindow);
            EditAppointmentCommand = new RelayCommand<Models.Appointment>(OpenEditAppointmentWindow);
            ViewAppointmentCommand = new RelayCommand<Models.Appointment>(ViewAppointmentOverlay);

            // Prescriptions
            AddPrescriptionCommand = new RelayCommand(OpenAddPrescriptionWindow);
            EditPrescriptionCommand = new RelayCommand<Models.Prescription>(OpenEditPrescriptionWindow);
            ViewPrescriptionCommand = new RelayCommand<Models.Prescription>(ViewPrescriptionOverlay);

            // Treatment Plans
            AddTreatmentPlanCommand = new RelayCommand(OpenAddTreatmentPlanWindow);
            EditTreatmentPlanCommand = new RelayCommand<Models.TreatmentPlan>(OpenEditTreatmentPlanWindow);
            ViewTreatmentPlanCommand = new RelayCommand<Models.TreatmentPlan>(ViewTreatmentPlanOverlay);

            // Invoices
            AddInvoiceCommand = new RelayCommand(OpenAddInvoiceWindow);
            EditInvoiceCommand = new RelayCommand<Models.Invoice>(OpenEditInvoiceWindow);
            ViewInvoiceCommand = new RelayCommand<Models.Invoice>(ViewInvoiceOverlay);

            // Medical Records
            AddMedicalRecordCommand = new RelayCommand(OpenAddMedicalRecordWindow);
            EditMedicalRecordCommand = new RelayCommand<Models.MedicalRecord>(OpenEditMedicalRecordWindow);
            ViewMedicalRecordCommand = new RelayCommand<Models.MedicalRecord>(ViewMedicalRecordOverlay);
            DownloadAttachmentCommand = new RelayCommand<Models.MedicalRecord>(DownloadAttachment);

            // Filter / Export
            FilterCommand = new RelayCommand(ApplyFilter);
            ExportPdfCommand = new RelayCommand(OnExportPdf);

            // Preview overlay
            ClosePreviewCommand = new RelayCommand(ClosePreview);

            // Load data from DB
            _ = LoadDataAsync();
        }

        #region Patient & Collections

        private Models.Patient _patient;
        public Models.Patient Patient
        {
            get => _patient;
            private set => SetProperty(ref _patient, value);
        }

        public ObservableCollection<Models.Appointment> Appointments { get; }
        public ObservableCollection<Models.Prescription> Prescriptions { get; }
        public ObservableCollection<Models.TreatmentPlan> TreatmentPlans { get; }
        public ObservableCollection<Models.Invoice> Invoices { get; }
        public ObservableCollection<Models.MedicalRecord> MedicalRecords { get; }

        #endregion

        #region Statistics

        public int TotalVisits => Appointments?.Count ?? 0;

        public string IsEverythingPaid
        {
            get
            {
                if (Invoices != null && Invoices.Any())
                {
                    return Invoices.All(i => i.IsPaid) ? "Yes" : "No";
                }
                return "No";
            }
        }

        #endregion

        #region Overlay Preview Properties

        private bool _isPreviewOpen;
        public bool IsPreviewOpen
        {
            get => _isPreviewOpen;
            set => SetProperty(ref _isPreviewOpen, value);
        }

        private string _previewTitle;
        public string PreviewTitle
        {
            get => _previewTitle;
            set => SetProperty(ref _previewTitle, value);
        }

        private string _previewContent;
        public string PreviewContent
        {
            get => _previewContent;
            set => SetProperty(ref _previewContent, value);
        }

        #endregion

        #region Commands (Add/Edit/View + Filter + PDF + Download)

        // Appointments
        public ICommand AddAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand ViewAppointmentCommand { get; }

        // Prescriptions
        public ICommand AddPrescriptionCommand { get; }
        public ICommand EditPrescriptionCommand { get; }
        public ICommand ViewPrescriptionCommand { get; }

        // Treatment Plans
        public ICommand AddTreatmentPlanCommand { get; }
        public ICommand EditTreatmentPlanCommand { get; }
        public ICommand ViewTreatmentPlanCommand { get; }

        // Invoices
        public ICommand AddInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand ViewInvoiceCommand { get; }

        // Medical Records
        public ICommand AddMedicalRecordCommand { get; }
        public ICommand EditMedicalRecordCommand { get; }
        public ICommand ViewMedicalRecordCommand { get; }
        public ICommand DownloadAttachmentCommand { get; }

        // Overlay preview
        public ICommand ClosePreviewCommand { get; }

        // Filtering
        public ICommand FilterCommand { get; }

        // Export PDF
        public ICommand ExportPdfCommand { get; }

        #endregion

        #region Loading + Filtering

        private async Task LoadDataAsync()
        {
            try
            {
                // Appointments
                var dbAppointments = await _context.Appointment
                    .Include(a => a.Dentist)
                    .Include(a => a.Treatment)
                    .Where(a => a.PatientID == Patient.PatientID)
                    .ToListAsync();

                Appointments.Clear();
                foreach (var item in dbAppointments)
                    Appointments.Add(item);

                // Prescriptions
                var dbPrescriptions = await _context.Prescription
                    .Include(p => p.Dentist)
                    .Where(p => p.PatientID == Patient.PatientID)
                    .ToListAsync();

                Prescriptions.Clear();
                foreach (var item in dbPrescriptions)
                    Prescriptions.Add(item);

                // TreatmentPlans
                var dbTreatmentPlans = await _context.TreatmentPlan
                    .Include(tp => tp.Dentist)
                    .Where(tp => tp.PatientID == Patient.PatientID)
                    .ToListAsync();

                TreatmentPlans.Clear();
                foreach (var item in dbTreatmentPlans)
                    TreatmentPlans.Add(item);

                // Invoices
                var dbInvoices = await _context.Invoice
                    .Include(i => i.Appointment)
                    .Where(i => i.Appointment.PatientID == Patient.PatientID)
                    .ToListAsync();

                Invoices.Clear();
                foreach (var item in dbInvoices)
                    Invoices.Add(item);

                // MedicalRecords
                var dbRecords = await _context.MedicalRecord
                    .Include(mr => mr.Dentist)
                    .Where(mr => mr.PatientID == Patient.PatientID)
                    .ToListAsync();

                MedicalRecords.Clear();
                foreach (var item in dbRecords)
                    MedicalRecords.Add(item);

                // Update stats
                OnPropertyChanged(nameof(TotalVisits));
                OnPropertyChanged(nameof(IsEverythingPaid));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading patient data: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private void ApplyFilter()
        {
            try
            {
                // Re-load everything with a filter
                // 1) Appointments
                var apps = _context.Appointment
                    .Include(a => a.Dentist)
                    .Include(a => a.Treatment)
                    .Where(a => a.PatientID == Patient.PatientID);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lower = SearchText.ToLower();
                    apps = apps.Where(a =>
                        (a.Dentist.FirstName + " " + a.Dentist.LastName).ToLower().Contains(lower)
                        || a.Treatment.Name.ToLower().Contains(lower)
                        || (a.Notes != null && a.Notes.ToLower().Contains(lower)));
                }
                var filteredApps = apps.ToList();
                Appointments.Clear();
                foreach (var item in filteredApps)
                    Appointments.Add(item);

                // 2) Prescriptions
                var pres = _context.Prescription
                    .Include(p => p.Dentist)
                    .Where(p => p.PatientID == Patient.PatientID);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lower = SearchText.ToLower();
                    pres = pres.Where(p =>
                        p.Medication.ToLower().Contains(lower)
                        || p.Dosage.ToLower().Contains(lower)
                        || (p.Instructions != null && p.Instructions.ToLower().Contains(lower))
                        || (p.Dentist != null &&
                            (p.Dentist.FirstName + " " + p.Dentist.LastName).ToLower().Contains(lower))
                    );
                }
                var filteredPres = pres.ToList();
                Prescriptions.Clear();
                foreach (var item in filteredPres)
                    Prescriptions.Add(item);

                // 3) TreatmentPlans
                var plans = _context.TreatmentPlan
                    .Include(tp => tp.Dentist)
                    .Where(tp => tp.PatientID == Patient.PatientID);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lower = SearchText.ToLower();
                    plans = plans.Where(tp =>
                        (tp.Dentist.FirstName + " " + tp.Dentist.LastName).ToLower().Contains(lower)
                        || (tp.Details != null && tp.Details.ToLower().Contains(lower)));
                }
                var filteredPlans = plans.ToList();
                TreatmentPlans.Clear();
                foreach (var item in filteredPlans)
                    TreatmentPlans.Add(item);

                // 4) Invoices
                var invs = _context.Invoice
                    .Include(i => i.Appointment)
                    .ThenInclude(a => a.Treatment)
                    .Include(i => i.Appointment.Dentist)
                    .Where(i => i.Appointment.PatientID == Patient.PatientID);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lower = SearchText.ToLower();
                    invs = invs.Where(i =>
                        (i.Appointment.Dentist.FirstName + " " + i.Appointment.Dentist.LastName).ToLower().Contains(lower)
                        || (i.Appointment.Treatment != null && i.Appointment.Treatment.Name.ToLower().Contains(lower))
                    );
                }
                var filteredInvs = invs.ToList();
                Invoices.Clear();
                foreach (var item in filteredInvs)
                    Invoices.Add(item);

                // 5) MedicalRecords
                var recs = _context.MedicalRecord
                    .Include(m => m.Dentist)
                    .Where(m => m.PatientID == Patient.PatientID);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lower = SearchText.ToLower();
                    recs = recs.Where(m =>
                        (m.Dentist.FirstName + " " + m.Dentist.LastName).ToLower().Contains(lower)
                        || (m.Notes != null && m.Notes.ToLower().Contains(lower)));
                }
                var filteredRecs = recs.ToList();
                MedicalRecords.Clear();
                foreach (var item in filteredRecs)
                    MedicalRecords.Add(item);

                // Update stats
                OnPropertyChanged(nameof(TotalVisits));
                OnPropertyChanged(nameof(IsEverythingPaid));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter error: {ex.Message}",
                                "Filter Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        #endregion

        #region Export to PDF

        private void OnExportPdf()
        {
            try
            {
                // Let user pick a save location
                var dialog = new SaveFileDialog
                {
                    Title = "Save PDF file",
                    Filter = "PDF Files|*.pdf",
                    FileName = "Patient_" + Patient.FullName.Replace(" ", "_") + ".pdf"
                };
                if (dialog.ShowDialog() == true)
                {
                    string path = dialog.FileName;

                    // TODO: Actually generate a real PDF. For now, just create a dummy file:
                    File.WriteAllText(path, "This is a dummy PDF content for demonstration.");

                    MessageBox.Show($"PDF saved successfully at:\n{path}",
                                    "Export PDF",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export PDF error: {ex.Message}",
                                "Export Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        #endregion

        #region Overlay Preview Logic

        public void ClosePreview()
        {
            IsPreviewOpen = false;
        }

        // 1) Appointments
        private void ViewAppointmentOverlay(Models.Appointment appt)
        {
            if (appt == null) return;

            PreviewTitle = "Appointment Preview";
            PreviewContent =
                $"Appointment ID: {appt.AppointmentID}\n" +
                $"Date: {appt.AppointmentDate:yyyy-MM-dd}\n" +
                $"Time: {appt.StartTime:hh\\:mm}\n\n" +
                $"Patient: {Patient.FullName} (DOB: {Patient.DateOfBirth:yyyy-MM-dd})\n" +
                $"Dentist: {appt.Dentist?.DisplayInfo}\n" +
                $"Treatment: {appt.Treatment?.Name}\n\n" +
                $"Notes:\n{appt.Notes ?? ""}\n";

            IsPreviewOpen = true;
        }

        // 2) Prescriptions
        private void ViewPrescriptionOverlay(Models.Prescription prescription)
        {
            if (prescription == null) return;

            PreviewTitle = "Prescription Preview";
            PreviewContent =
                $"Prescription ID: {prescription.PrescriptionID}\n" +
                $"Date Issued: {prescription.DateIssued:yyyy-MM-dd}\n\n" +
                $"Patient: {Patient.FullName} (DOB: {Patient.DateOfBirth:yyyy-MM-dd})\n" +
                $"Dentist: {prescription.Dentist?.DisplayInfo}\n\n" +
                $"Medication: {prescription.Medication}\n" +
                $"Dosage: {prescription.Dosage}\n" +
                $"Instructions: {prescription.Instructions}\n\n" +
                "**Disclaimer**: Please follow dosage instructions carefully and consult with your dentist if any side effects occur.";

            IsPreviewOpen = true;
        }

        // 3) Treatment Plans
        private void ViewTreatmentPlanOverlay(Models.TreatmentPlan plan)
        {
            if (plan == null) return;

            PreviewTitle = "Treatment Plan Preview";
            PreviewContent =
                $"Plan ID: {plan.TreatmentPlanID}\n" +
                $"Creation Date: {plan.CreationDate:yyyy-MM-dd}\n\n" +
                $"Patient: {Patient.FullName} (DOB: {Patient.DateOfBirth:yyyy-MM-dd})\n" +
                $"Dentist: {plan.Dentist?.DisplayInfo}\n\n" +
                $"Details:\n{plan.Details ?? ""}";

            IsPreviewOpen = true;
        }

        // 4) Invoices
        private void ViewInvoiceOverlay(Models.Invoice invoice)
        {
            if (invoice == null) return;

            PreviewTitle = "Invoice Preview";
            PreviewContent =
                $"Invoice ID: {invoice.InvoiceID}\n" +
                $"Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}\n\n" +
                $"Patient: {Patient.FullName} (DOB: {Patient.DateOfBirth:yyyy-MM-dd})\n" +
                $"Amount: {invoice.TotalAmount:C}\n" +
                $"Paid: {invoice.IsPaid}\n\n" +
                $"Linked Appointment: {(invoice.Appointment?.DisplayInfo ?? "N/A")}";

            IsPreviewOpen = true;
        }

        // 5) Medical Records
        private void ViewMedicalRecordOverlay(Models.MedicalRecord record)
        {
            if (record == null) return;

            PreviewTitle = "Medical Record Preview";
            PreviewContent =
                $"Record ID: {record.RecordID}\n" +
                $"Date: {record.Date:yyyy-MM-dd}\n\n" +
                $"Patient: {Patient.FullName} (DOB: {Patient.DateOfBirth:yyyy-MM-dd})\n" +
                $"Dentist: {record.Dentist?.DisplayInfo}\n\n" +
                $"Notes:\n{record.Notes ?? ""}";

            IsPreviewOpen = true;
        }

        #endregion

        #region Add/Edit Windows

        // APPOINTMENTS
        private void OpenAddAppointmentWindow()
        {
            var w = new AddAppointmentView
            {
                DataContext = new AddAppointmentViewModel(_context)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        private void OpenEditAppointmentWindow(Models.Appointment a)
        {
            if (a == null) return;
            var w = new EditAppointmentView
            {
                DataContext = new EditAppointmentViewModel(_context, a)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        // PRESCRIPTIONS
        private void OpenAddPrescriptionWindow()
        {
            var w = new AddPrescriptionView
            {
                DataContext = new AddPrescriptionViewModel(_context)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        private void OpenEditPrescriptionWindow(Models.Prescription p)
        {
            if (p == null) return;
            var w = new EditPrescriptionView
            {
                DataContext = new EditPrescriptionViewModel(p, _context)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        // TREATMENT PLANS
        private void OpenAddTreatmentPlanWindow()
        {
            var w = new AddTreatmentPlanView
            {
                DataContext = new AddTreatmentPlanViewModel(_context)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        private void OpenEditTreatmentPlanWindow(Models.TreatmentPlan plan)
        {
            if (plan == null) return;
            var w = new EditTreatmentPlanView
            {
                DataContext = new EditTreatmentPlanViewModel(_context, plan)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        // INVOICES
        private void OpenAddInvoiceWindow()
        {
            var w = new AddInvoiceView
            {
                DataContext = new AddInvoiceViewModel(_context)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        private void OpenEditInvoiceWindow(Models.Invoice inv)
        {
            if (inv == null) return;
            var w = new EditInvoiceView
            {
                DataContext = new EditInvoiceViewModel(_context, inv)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        // MEDICAL RECORDS
        private void OpenAddMedicalRecordWindow()
        {
            var w = new AddMedicalRecordView
            {
                DataContext = new AddMedicalRecordViewModel(_context)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        private void OpenEditMedicalRecordWindow(Models.MedicalRecord rec)
        {
            if (rec == null) return;
            var w = new EditMedicalRecordView
            {
                DataContext = new EditMedicalRecordViewModel(_context, rec)
            };
            if (w.ShowDialog() == true)
                _ = LoadDataAsync();
        }

        private void DownloadAttachment(Models.MedicalRecord rec)
        {
            if (rec == null) return;
            MessageBox.Show($"Downloading attachment for record {rec.RecordID}...",
                            "Download Attachment",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        #endregion
    }
}
