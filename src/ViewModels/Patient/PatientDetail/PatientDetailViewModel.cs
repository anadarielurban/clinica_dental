using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
    public class PatientDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Patient Patient { get; }
        public ObservableCollection<Models.Appointment> Appointments { get; set; }
        public ObservableCollection<Models.Prescription> Prescriptions { get; set; }
        public ObservableCollection<Models.TreatmentPlan> TreatmentPlans { get; set; }
        public ObservableCollection<Models.Invoice> Invoices { get; set; }
        public ObservableCollection<Models.MedicalRecord> MedicalRecords { get; set; }

        // Propiedades seleccionadas
        public Models.Appointment SelectedAppointment { get; set; }
        public Models.Prescription SelectedPrescription { get; set; }
        public Models.TreatmentPlan SelectedTreatmentPlan { get; set; }
        public Models.Invoice SelectedInvoice { get; set; }
        public Models.MedicalRecord SelectedMedicalRecord { get; set; }

        // Comandos
        public ICommand AddAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand PreviewAppointmentCommand { get; }

        public ICommand AddPrescriptionCommand { get; }
        public ICommand EditPrescriptionCommand { get; }
        public ICommand PreviewPrescriptionCommand { get; }

        public ICommand AddTreatmentPlanCommand { get; }
        public ICommand EditTreatmentPlanCommand { get; }
        public ICommand PreviewTreatmentPlanCommand { get; }

        public ICommand AddInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand PreviewInvoiceCommand { get; }

        public ICommand AddMedicalRecordCommand { get; }
        public ICommand EditMedicalRecordCommand { get; }
        public ICommand PreviewMedicalRecordCommand { get; }

        public PatientDetailViewModel(DentalClinicContext context, Models.Patient patient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));

            // Cargar datos
            Appointments = new ObservableCollection<Models.Appointment>(_context.Appointment.Where(a => a.PatientID == patient.PatientID).ToList());
            Prescriptions = new ObservableCollection<Models.Prescription>(_context.Prescription.Where(p => p.PatientID == patient.PatientID).ToList());
            TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>(_context.TreatmentPlan.Where(t => t.PatientID == patient.PatientID).ToList());
            Invoices = new ObservableCollection<Models.Invoice>(_context.Invoice.Where(i => i.Appointment.PatientID == patient.PatientID).ToList());
            MedicalRecords = new ObservableCollection<Models.MedicalRecord>(_context.MedicalRecord.Where(m => m.PatientID == patient.PatientID).ToList());

            // Comandos de Citas
            AddAppointmentCommand = new RelayCommand(() => {
                try {
                    var w = new AddAppointmentView { DataContext = new ViewModels.Appointment.AddAppointmentViewModel(_context) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditAppointmentCommand = new RelayCommand(() => {
                if (SelectedAppointment == null) { MessageBox.Show("Selecciona una cita de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditAppointmentView { DataContext = new ViewModels.Appointment.EditAppointmentViewModel(_context, SelectedAppointment) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show($"Error al editar la cita: {ex.Message}", "Error"); }
            });
            PreviewAppointmentCommand = new RelayCommand(() => {
                if (SelectedAppointment == null) { MessageBox.Show("Selecciona una cita primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                MessageBox.Show($"Cita: {SelectedAppointment.AppointmentDate:dd/MM/yyyy} - {SelectedAppointment.Notes}", "Detalles de Cita");
            });

            // Comandos de Recetas
            AddPrescriptionCommand = new RelayCommand(() => {
                try {
                    var w = new AddPrescriptionView { DataContext = new ViewModels.Prescription.AddPrescriptionViewModel(_context) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditPrescriptionCommand = new RelayCommand(() => {
                if (SelectedPrescription == null) { MessageBox.Show("Selecciona una receta de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditPrescriptionView { DataContext = new ViewModels.Prescription.EditPrescriptionViewModel(SelectedPrescription, _context) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show($"Error al editar la receta: {ex.Message}", "Error"); }
            });
            PreviewPrescriptionCommand = new RelayCommand(() => {
                if (SelectedPrescription == null) { MessageBox.Show("Selecciona una receta primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var w = new PrescriptionPreviewWindow(SelectedPrescription);
                w.ShowDialog();
            });

            // Comandos de Planes
            AddTreatmentPlanCommand = new RelayCommand(() => {
                try {
                    var w = new AddTreatmentPlanView { DataContext = new ViewModels.TreatmentPlan.AddTreatmentPlanViewModel(_context) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditTreatmentPlanCommand = new RelayCommand(() => {
                if (SelectedTreatmentPlan == null) { MessageBox.Show("Selecciona un plan de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditTreatmentPlanView { DataContext = new ViewModels.TreatmentPlan.EditTreatmentPlanViewModel(_context, SelectedTreatmentPlan) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show($"Error al editar el plan: {ex.Message}", "Error"); }
            });
            PreviewTreatmentPlanCommand = new RelayCommand(() => {
                if (SelectedTreatmentPlan == null) { MessageBox.Show("Selecciona un plan primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                MessageBox.Show($"Plan: {SelectedTreatmentPlan.Details}", "Detalles del Plan");
            });

            // Comandos de Facturas
            AddInvoiceCommand = new RelayCommand(() => {
                try {
                    var w = new AddInvoiceView { DataContext = new ViewModels.Invoice.AddInvoiceViewModel(_context) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditInvoiceCommand = new RelayCommand(() => {
                if (SelectedInvoice == null) { MessageBox.Show("Selecciona una factura de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditInvoiceView { DataContext = new ViewModels.Invoice.EditInvoiceViewModel(_context, SelectedInvoice) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show($"Error al editar la factura: {ex.Message}", "Error"); }
            });
            PreviewInvoiceCommand = new RelayCommand(() => {
                if (SelectedInvoice == null) { MessageBox.Show("Selecciona una factura primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                MessageBox.Show($"Factura Total: {SelectedInvoice.TotalAmount:C}", "Detalles de Factura");
            });

            // Comandos de Expedientes
            AddMedicalRecordCommand = new RelayCommand(() => {
                try {
                    var w = new AddMedicalRecordView { DataContext = new ViewModels.MedicalRecord.AddMedicalRecordViewModel(_context) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditMedicalRecordCommand = new RelayCommand(() => {
                if (SelectedMedicalRecord == null) { MessageBox.Show("Selecciona un expediente de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditMedicalRecordView { DataContext = new ViewModels.MedicalRecord.EditMedicalRecordViewModel(_context, SelectedMedicalRecord) };
                    if (w.ShowDialog() == true) { /* Recargar */ }
                } catch (Exception ex) { MessageBox.Show($"Error al editar el expediente: {ex.Message}", "Error"); }
            });
            PreviewMedicalRecordCommand = new RelayCommand(() => {
                if (SelectedMedicalRecord == null) { MessageBox.Show("Selecciona un expediente primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                MessageBox.Show($"Expediente: {SelectedMedicalRecord.Notes}", "Detalles");
            });
        }
    }
}

