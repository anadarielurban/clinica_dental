using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; // Necesario para UniformGrid
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
    public class PatientDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Patient Patient { get; }
        public ObservableCollection<Models.Appointment> Appointments { get; set; }
        public ObservableCollection<Models.Prescription> Prescriptions { get; set; }
        public ObservableCollection<Models.TreatmentPlan> TreatmentPlans { get; set; }
        public ObservableCollection<Models.Invoice> Invoices { get; set; }
        public ObservableCollection<Models.MedicalRecord> MedicalRecords { get; set; }

        // Colecciones del Odontograma
        public ObservableCollection<ToothViewModel> TeethUpperRight { get; set; } = new();
        public ObservableCollection<ToothViewModel> TeethUpperLeft { get; set; } = new();
        public ObservableCollection<ToothViewModel> TeethLowerRight { get; set; } = new();
        public ObservableCollection<ToothViewModel> TeethLowerLeft { get; set; } = new();

        // Propiedades Seleccionadas
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

        public ICommand SaveOdontogramCommand { get; }
        public ICommand SelectToothCommand { get; }

        // ═══ Historia Clínica ═══
        public MedicalHistoryViewModel MedicalHistory { get; }

        // ─── Diente seleccionado (para el panel lateral) ───
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

            // Cargar datos de pestañas
            Appointments = new ObservableCollection<Models.Appointment>();
            Prescriptions = new ObservableCollection<Models.Prescription>();
            TreatmentPlans = new ObservableCollection<Models.TreatmentPlan>();
            Invoices = new ObservableCollection<Models.Invoice>();
            MedicalRecords = new ObservableCollection<Models.MedicalRecord>();

            // Cargar Odontograma
            MedicalHistory = new MedicalHistoryViewModel(_context, Patient);

            LoadOdontogram();
            LoadTreatmentPlans();
            LoadAppointments();
            LoadPrescriptions();
            LoadInvoices();
            LoadMedicalRecords();

            // Comando Guardar Odontograma
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

            // Comandos de Citas
            AddAppointmentCommand = new RelayCommand(() => {
                try {
                    var w = new AddAppointmentView { DataContext = new ViewModels.Appointment.AddAppointmentViewModel(_context) };
                    if (w.ShowDialog() == true) { LoadAppointments();  }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditAppointmentCommand = new RelayCommand(() => {
                if (SelectedAppointment == null) { MessageBox.Show("Selecciona una cita de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditAppointmentView { DataContext = new ViewModels.Appointment.EditAppointmentViewModel(_context, SelectedAppointment) };
                    if (w.ShowDialog() == true) { LoadAppointments();  }
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
                    if (w.ShowDialog() == true) { LoadAppointments();  }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditPrescriptionCommand = new RelayCommand(() => {
                if (SelectedPrescription == null) { MessageBox.Show("Selecciona una receta de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditPrescriptionView { DataContext = new ViewModels.Prescription.EditPrescriptionViewModel(SelectedPrescription, _context) };
                    if (w.ShowDialog() == true) { LoadAppointments();  }
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
                    var w = new AddTreatmentPlanView { DataContext = new ViewModels.TreatmentPlan.AddTreatmentPlanViewModel(_context, Patient) };
                    if (w.ShowDialog() == true) {
                        LoadTreatmentPlans();
            LoadAppointments();
            LoadPrescriptions();
            LoadInvoices();
            LoadMedicalRecords(); // <-- Recargar tabla
                    }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditTreatmentPlanCommand = new RelayCommand(() => {
                if (SelectedTreatmentPlan == null) { MessageBox.Show("Selecciona un plan de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditTreatmentPlanView { DataContext = new ViewModels.TreatmentPlan.EditTreatmentPlanViewModel(_context, SelectedTreatmentPlan) };
                    if (w.ShowDialog() == true) {
                        LoadTreatmentPlans();
            LoadAppointments();
            LoadPrescriptions();
            LoadInvoices();
            LoadMedicalRecords(); // <-- Recargar tabla
                    }
                } catch (Exception ex) { MessageBox.Show($"Error al editar el plan: {ex.Message}", "Error"); }
            });
            PreviewTreatmentPlanCommand = new RelayCommand(() => {
                if (SelectedTreatmentPlan == null) { MessageBox.Show("Selecciona un plan primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Fecha: {SelectedTreatmentPlan.CreationDate:dd/MM/yyyy}");
                sb.AppendLine($"Estado: {SelectedTreatmentPlan.Status}");
                sb.AppendLine("--------------------------------------------------");
                if (SelectedTreatmentPlan.Items != null && SelectedTreatmentPlan.Items.Count > 0)
                {
                    foreach (var item in SelectedTreatmentPlan.Items)
                    {
                        var name = item.Treatment != null ? item.Treatment.Name : "Tratamiento";
                        sb.AppendLine($"• {name} (x{item.Quantity}) - {item.Subtotal:C}");
                    }
                }
                else
                {
                    sb.AppendLine("(No hay tratamientos cargados)");
                }
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"TOTAL: {SelectedTreatmentPlan.TotalAmount:C}");
                MessageBox.Show(sb.ToString(), "Detalles del Presupuesto");
            });

            // Comandos de Facturas
            AddInvoiceCommand = new RelayCommand(() => {
                try {
                    var w = new AddInvoiceView { DataContext = new ViewModels.Invoice.AddInvoiceViewModel(_context) };
                    if (w.ShowDialog() == true) { LoadAppointments();  }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditInvoiceCommand = new RelayCommand(() => {
                if (SelectedInvoice == null) { MessageBox.Show("Selecciona una factura de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditInvoiceView { DataContext = new ViewModels.Invoice.EditInvoiceViewModel(_context, SelectedInvoice) };
                    if (w.ShowDialog() == true) { LoadAppointments();  }
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
                    if (w.ShowDialog() == true) { LoadAppointments();  }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            EditMedicalRecordCommand = new RelayCommand(() => {
                if (SelectedMedicalRecord == null) { MessageBox.Show("Selecciona un expediente de la tabla primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                try {
                    var w = new EditMedicalRecordView { DataContext = new ViewModels.MedicalRecord.EditMedicalRecordViewModel(_context, SelectedMedicalRecord) };
                    if (w.ShowDialog() == true) { LoadAppointments();  }
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
            foreach (var i in _context.Invoice.Where(i => i.Appointment.PatientID == Patient.PatientID).ToList())
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
            
            foreach (var plan in plans)
            {
                TreatmentPlans.Add(plan);
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

private FlowDocument BuildExpedienteDocument()
        {
            // ── Paleta ─────────────────────────────────────────
                var tealBrush   = new SolidColorBrush(Color.FromRgb(20, 184, 166));
                var tealDark    = new SolidColorBrush(Color.FromRgb(13, 148, 136));
                var darkSlate   = new SolidColorBrush(Color.FromRgb(30, 41, 59));
                var medSlate    = new SolidColorBrush(Color.FromRgb(71, 85, 105));
                var lightSlate  = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                var borderGray  = new SolidColorBrush(Color.FromRgb(226, 232, 240));
                var bgSoft      = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                var bgTealSoft  = new SolidColorBrush(Color.FromRgb(240, 253, 250));

                FlowDocument doc = new FlowDocument
                {
                    PagePadding = new Thickness(40, 25, 40, 20),
                    ColumnWidth = double.PositiveInfinity,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 10,
                    PageWidth = 780,
                    Background = Brushes.White
                };

                // ═══════════════ 1. HEADER (banda teal) ═══════════════
                var headerCard = new Border
                {
                    Background = tealBrush,
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(20, 14, 20, 14),
                    Margin = new Thickness(0, 0, 0, 16)
                };
                var headerStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                headerStack.Children.Add(new TextBlock
                {
                    Text = "\U0001F9B7  CLÍNICA DENTAL",
                    Foreground = Brushes.White,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                headerStack.Children.Add(new TextBlock
                {
                    Text = "EXPEDIENTE CLÍNICO",
                    Foreground = new SolidColorBrush(Color.FromArgb(235, 255, 255, 255)),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 3, 0, 0)
                });
                headerCard.Child = headerStack;
                doc.Blocks.Add(new BlockUIContainer(headerCard));

                // ═══════════════ 2. DATOS DEL PACIENTE ═══════════════
                doc.Blocks.Add(SectionHeader("DATOS DEL PACIENTE", tealBrush, darkSlate));

                var infoTable = new Table
                {
                    CellSpacing = 0,
                    Margin = new Thickness(0, 0, 0, 14),
                    BorderBrush = borderGray,
                    BorderThickness = new Thickness(1)
                };
                infoTable.Columns.Add(new TableColumn { Width = new GridLength(140) });
                infoTable.Columns.Add(new TableColumn { Width = new GridLength(550) });

                var infoGroup = new TableRowGroup();
                infoTable.RowGroups.Add(infoGroup);

                void AddInfoRow(string label, string value, bool shaded)
                {
                    var row = new TableRow();
                    if (shaded) row.Background = bgSoft;
                    row.Cells.Add(new TableCell(new Paragraph(new Run(label))
                    {
                        Margin = new Thickness(0),
                        FontWeight = FontWeights.SemiBold,
                        Foreground = medSlate
                    })
                    { Padding = new Thickness(12, 6, 10, 6) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(string.IsNullOrWhiteSpace(value) ? "—" : value))
                    {
                        Margin = new Thickness(0),
                        Foreground = darkSlate
                    })
                    { Padding = new Thickness(12, 6, 10, 6) });
                    infoGroup.Rows.Add(row);
                }

                AddInfoRow("Paciente", Patient.FullName, false);
                AddInfoRow("Fecha de Nacimiento", Patient.DateOfBirth.ToString("dd 'de' MMMM 'de' yyyy"), true);
                AddInfoRow("Teléfono", Patient.PhoneNumber, false);
                AddInfoRow("Correo", Patient.Email, true);
                AddInfoRow("Dirección", Patient.Address, false);

                doc.Blocks.Add(infoTable);

                // ═══════════════ 2.5 HISTORIA CLÍNICA ═══════════════
                var conditions = new List<string>();
                if (Patient.HasDiabetes) conditions.Add("Diabetes");
                if (Patient.HasHypertension) conditions.Add("Hipertensión");
                if (Patient.HasHeartConditions) conditions.Add("Problemas cardíacos");
                if (Patient.HasCoagulationIssues) conditions.Add("Problemas de coagulación");
                if (Patient.TakesAnticoagulants) conditions.Add("Toma anticoagulantes");
                if (Patient.IsPregnant) conditions.Add("Embarazo");
                if (Patient.IsSmoker) conditions.Add("Fumador");

                bool hasAnyHistory = !string.IsNullOrWhiteSpace(Patient.BloodType)
                                     || conditions.Count > 0
                                     || Patient.HasAllergies
                                     || !string.IsNullOrWhiteSpace(Patient.EmergencyContactName)
                                     || !string.IsNullOrWhiteSpace(Patient.EmergencyContactPhone)
                                     || !string.IsNullOrWhiteSpace(Patient.MedicalNotes);

                if (hasAnyHistory)
                {
                    doc.Blocks.Add(SectionHeader("HISTORIA CLÍNICA", tealBrush, darkSlate));

                    var histBorder = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = borderGray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(14, 12, 14, 12),
                        Margin = new Thickness(0, 0, 0, 12)
                    };
                    var histStack = new StackPanel();

                    // Tipo de sangre
                    if (!string.IsNullOrWhiteSpace(Patient.BloodType))
                    {
                        var btPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
                        btPanel.Children.Add(new TextBlock
                        {
                            Text = "Tipo de sangre:  ",
                            FontWeight = FontWeights.SemiBold,
                            Foreground = medSlate,
                            FontSize = 10,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                        btPanel.Children.Add(new Border
                        {
                            Background = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                            CornerRadius = new CornerRadius(10),
                            Padding = new Thickness(10, 2, 10, 2),
                            Child = new TextBlock
                            {
                                Text = Patient.BloodType,
                                Foreground = Brushes.White,
                                FontWeight = FontWeights.Bold,
                                FontSize = 10
                            }
                        });
                        histStack.Children.Add(btPanel);
                    }

                    // Condiciones (chips)
                    if (conditions.Count > 0)
                    {
                        var condPanel = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
                        condPanel.Children.Add(new TextBlock
                        {
                            Text = "Condiciones:  ",
                            FontWeight = FontWeights.SemiBold,
                            Foreground = medSlate,
                            FontSize = 10,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 4, 4)
                        });
                        foreach (var c in conditions)
                        {
                            condPanel.Children.Add(new Border
                            {
                                Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)),
                                BorderBrush = new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                                BorderThickness = new Thickness(1),
                                CornerRadius = new CornerRadius(10),
                                Padding = new Thickness(8, 2, 8, 2),
                                Margin = new Thickness(0, 0, 4, 4),
                                Child = new TextBlock
                                {
                                    Text = c,
                                    Foreground = new SolidColorBrush(Color.FromRgb(120, 53, 15)),
                                    FontSize = 9,
                                    FontWeight = FontWeights.SemiBold
                                }
                            });
                        }
                        histStack.Children.Add(condPanel);
                    }

                    // Alergias (destacado en rojo)
                    if (Patient.HasAllergies)
                    {
                        var allergyBorder = new Border
                        {
                            Background = new SolidColorBrush(Color.FromRgb(254, 226, 226)),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(6),
                            Padding = new Thickness(10, 6, 10, 6),
                            Margin = new Thickness(0, 4, 0, 8)
                        };
                        var allergyStack = new StackPanel();
                        allergyStack.Children.Add(new TextBlock
                        {
                            Text = "⚠  ALERGIAS",
                            Foreground = new SolidColorBrush(Color.FromRgb(153, 27, 27)),
                            FontWeight = FontWeights.Bold,
                            FontSize = 10
                        });
                        allergyStack.Children.Add(new TextBlock
                        {
                            Text = string.IsNullOrWhiteSpace(Patient.AllergiesDetails) ? "No especificadas" : Patient.AllergiesDetails,
                            Foreground = new SolidColorBrush(Color.FromRgb(127, 29, 29)),
                            FontSize = 10,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 2, 0, 0)
                        });
                        allergyBorder.Child = allergyStack;
                        histStack.Children.Add(allergyBorder);
                    }

                    // Contacto de emergencia
                    if (!string.IsNullOrWhiteSpace(Patient.EmergencyContactName) || !string.IsNullOrWhiteSpace(Patient.EmergencyContactPhone))
                    {
                        var ecText = new TextBlock
                        {
                            FontSize = 10,
                            Foreground = medSlate,
                            Margin = new Thickness(0, 0, 0, 6),
                            TextWrapping = TextWrapping.Wrap
                        };
                        ecText.Inlines.Add(new Run("Contacto de emergencia:  ") { FontWeight = FontWeights.SemiBold });
                        var parts = new List<string>();
                        if (!string.IsNullOrWhiteSpace(Patient.EmergencyContactName)) parts.Add(Patient.EmergencyContactName);
                        if (!string.IsNullOrWhiteSpace(Patient.EmergencyContactPhone)) parts.Add(Patient.EmergencyContactPhone);
                        ecText.Inlines.Add(new Run(string.Join("  •  ", parts)));
                        histStack.Children.Add(ecText);
                    }

                    // Notas médicas
                    if (!string.IsNullOrWhiteSpace(Patient.MedicalNotes))
                    {
                        var notesText = new TextBlock
                        {
                            FontSize = 10,
                            Foreground = medSlate,
                            TextWrapping = TextWrapping.Wrap
                        };
                        notesText.Inlines.Add(new Run("Notas médicas:  ") { FontWeight = FontWeights.SemiBold });
                        notesText.Inlines.Add(new Run(Patient.MedicalNotes));
                        histStack.Children.Add(notesText);
                    }

                    histBorder.Child = histStack;
                    doc.Blocks.Add(new BlockUIContainer(histBorder));
                }

                // ═══════════════ 3. ODONTOGRAMA ═══════════════
                doc.Blocks.Add(SectionHeader("ODONTOGRAMA", tealBrush, darkSlate));

                var odoContainer = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = borderGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(14, 12, 14, 12),
                    Margin = new Thickness(0, 0, 0, 12)
                };
                var odoStack = new StackPanel();
                odoStack.Children.Add(BuildToothQuadrantRow("SUPERIOR DERECHO", "SUPERIOR IZQUIERDO",
                    TeethUpperRight, TeethUpperLeft, false, lightSlate));
                odoStack.Children.Add(new Border
                {
                    Height = 1,
                    Background = borderGray,
                    Margin = new Thickness(0, 8, 0, 8)
                });
                odoStack.Children.Add(BuildToothQuadrantRow("INFERIOR DERECHO", "INFERIOR IZQUIERDO",
                    TeethLowerRight, TeethLowerLeft, true, lightSlate));
                odoContainer.Child = odoStack;
                doc.Blocks.Add(new BlockUIContainer(odoContainer));

                doc.Blocks.Add(new BlockUIContainer(BuildLegend(medSlate, borderGray)));

                // ═══════════════ 4. HALLAZGOS ═══════════════
                var allTeeth = TeethUpperRight.Concat(TeethUpperLeft)
                                              .Concat(TeethLowerRight)
                                              .Concat(TeethLowerLeft)
                                              .Where(t => t.Condition != "Sano")
                                              .ToList();

                if (allTeeth.Count > 0)
                {
                    doc.Blocks.Add(SectionHeader("DETALLE DE HALLAZGOS", tealBrush, darkSlate));

                    var detailTable = new Table
                    {
                        CellSpacing = 0,
                        Margin = new Thickness(0, 0, 0, 12),
                        BorderBrush = borderGray,
                        BorderThickness = new Thickness(1)
                    };
                    detailTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
                    detailTable.Columns.Add(new TableColumn { Width = new GridLength(140) });
                    detailTable.Columns.Add(new TableColumn { Width = new GridLength(470) });

                    var detailGroup = new TableRowGroup();
                    detailTable.RowGroups.Add(detailGroup);

                    var headRow = new TableRow { Background = tealBrush };
                    headRow.Cells.Add(new TableCell(new Paragraph(new Run("DIENTE"))
                        { Margin = new Thickness(0), FontWeight = FontWeights.Bold, Foreground = Brushes.White })
                        { Padding = new Thickness(12, 6, 10, 6) });
                    headRow.Cells.Add(new TableCell(new Paragraph(new Run("ESTADO"))
                        { Margin = new Thickness(0), FontWeight = FontWeights.Bold, Foreground = Brushes.White })
                        { Padding = new Thickness(12, 6, 10, 6) });
                    headRow.Cells.Add(new TableCell(new Paragraph(new Run("NOTAS"))
                        { Margin = new Thickness(0), FontWeight = FontWeights.Bold, Foreground = Brushes.White })
                        { Padding = new Thickness(12, 6, 10, 6) });
                    detailGroup.Rows.Add(headRow);

                    int i = 0;
                    foreach (var t in allTeeth)
                    {
                        var r = new TableRow();
                        if (i % 2 == 1) r.Background = bgSoft;
                        r.Cells.Add(new TableCell(new Paragraph(new Run(t.ToothNumber.ToString()))
                            { Margin = new Thickness(0), FontWeight = FontWeights.SemiBold, Foreground = darkSlate })
                            { Padding = new Thickness(12, 5, 10, 5) });
                        r.Cells.Add(new TableCell(new Paragraph(new Run(t.Condition))
                            { Margin = new Thickness(0), Foreground = tealDark })
                            { Padding = new Thickness(12, 5, 10, 5) });
                        var notes = string.IsNullOrWhiteSpace(t.Record.Notes) ? "—" : t.Record.Notes;
                        r.Cells.Add(new TableCell(new Paragraph(new Run(notes))
                            { Margin = new Thickness(0), Foreground = medSlate })
                            { Padding = new Thickness(12, 5, 10, 5) });
                        detailGroup.Rows.Add(r);
                        i++;
                    }
                    doc.Blocks.Add(detailTable);
                }
                else
                {
                    var emptyBorder = new Border
                    {
                        Background = bgTealSoft,
                        BorderBrush = borderGray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(20, 14, 20, 14),
                        Margin = new Thickness(0, 0, 0, 12)
                    };
                    var emptyStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                    emptyStack.Children.Add(new TextBlock
                    {
                        Text = "✓  Sin hallazgos registrados",
                        Foreground = tealDark,
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                    emptyStack.Children.Add(new TextBlock
                    {
                        Text = "Todos los dientes se encuentran en estado sano.",
                        Foreground = medSlate,
                        FontSize = 10,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 0)
                    });
                    emptyBorder.Child = emptyStack;
                    doc.Blocks.Add(new BlockUIContainer(emptyBorder));
                }

                // ═══════════════ 5. FECHA DE EMISIÓN ═══════════════
                var datePara = new Paragraph
                {
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 4, 0, 0),
                    FontSize = 9,
                    Foreground = lightSlate
                };
                datePara.Inlines.Add(new Run($"Emitido el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}"));
                doc.Blocks.Add(datePara);

                // ═══════════════ 6. FIRMA CENTRADA ═══════════════
                var sigContainer = new Border { Margin = new Thickness(0, 10, 0, 0) };
                var sigStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Width = 260 };
                sigStack.Children.Add(new Border
                {
                    Height = 1,
                    Background = darkSlate,
                    Margin = new Thickness(0, 0, 0, 6)
                });
                sigStack.Children.Add(new TextBlock
                {
                    Text = "Firma del Odontólogo",
                    Foreground = medSlate,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                sigStack.Children.Add(new TextBlock
                {
                    Text = "Cédula Profesional: ________________",
                    Foreground = lightSlate,
                    FontSize = 9,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0)
                });
                sigContainer.Child = sigStack;
                doc.Blocks.Add(new BlockUIContainer(sigContainer));

                // ═══════════════ 7. FOOTER ═══════════════
                var footerPara = new Paragraph
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 7.5,
                    Foreground = lightSlate
                };
                footerPara.Inlines.Add(new Run("Clínica Dental  •  Documento confidencial  •  Generado automáticamente"));
                doc.Blocks.Add(footerPara);
            return doc;
        }

        private void PrintRecord()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() != true) return;

                var doc = BuildExpedienteDocument();
                printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator,
                    "Expediente Clínico - " + Patient.FullName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir el expediente: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadExpedientePdf()
        {
            var safeName = string.Join("_", Patient.FullName.Split(
                System.IO.Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries));

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
                MessageBox.Show(
                    $"Expediente guardado correctamente:\n\n{sfd.FileName}",
                    "PDF generado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al generar el PDF:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ═══════════════ HELPERS ═══════════════

        private Paragraph SectionHeader(string title, Brush accent, Brush text)
        {
            var p = new Paragraph { Margin = new Thickness(0, 10, 0, 8) };
            p.Inlines.Add(new Run("▌ ") { Foreground = accent, FontSize = 14, FontWeight = FontWeights.Bold });
            p.Inlines.Add(new Run(title) { Foreground = text, FontSize = 12, FontWeight = FontWeights.Bold });
            return p;
        }

        private FrameworkElement BuildToothQuadrantRow(string leftLabel, string rightLabel,
            IEnumerable<ToothViewModel> leftTeeth, IEnumerable<ToothViewModel> rightTeeth,
            bool invert, Brush labelColor)
        {
            var outerGrid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };
            leftStack.Children.Add(new TextBlock
            {
                Text = leftLabel, FontSize = 8, FontWeight = FontWeights.SemiBold,
                Foreground = labelColor, HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });
            leftStack.Children.Add(BuildToothGrid(leftTeeth, invert));
            Grid.SetColumn(leftStack, 0);
            outerGrid.Children.Add(leftStack);

            var centerLine = new Border
            {
                Width = 1,
                Background = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 12)
            };
            Grid.SetColumn(centerLine, 1);
            outerGrid.Children.Add(centerLine);

            var rightStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Left };
            rightStack.Children.Add(new TextBlock
            {
                Text = rightLabel, FontSize = 8, FontWeight = FontWeights.SemiBold,
                Foreground = labelColor, HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });
            rightStack.Children.Add(BuildToothGrid(rightTeeth, invert));
            Grid.SetColumn(rightStack, 2);
            outerGrid.Children.Add(rightStack);

            return outerGrid;
        }

        private FrameworkElement BuildToothGrid(IEnumerable<ToothViewModel> teeth, bool invert)
        {
            var panel = new UniformGrid { Columns = 8 };
            foreach (var tooth in teeth)
            {
                var grid = new Grid { Margin = new Thickness(1), Width = 24, Height = 32 };
                var path = new System.Windows.Shapes.Path
                {
                    Data = Geometry.Parse("M 15,5 C 25,0 35,0 45,5 C 50,15 50,30 45,40 L 40,60 C 40,65 35,65 35,60 L 30,45 L 30,60 C 30,65 25,65 25,60 L 20,45 L 20,60 C 20,65 15,65 15,60 L 10,40 C 5,30 5,15 10,5 Z"),
                    Stretch = Stretch.Uniform,
                    Fill = tooth.BackgroundColor,
                    Stroke = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                    StrokeThickness = 0.6,
                    Margin = new Thickness(2)
                };
                if (invert)
                {
                    path.RenderTransformOrigin = new Point(0.5, 0.5);
                    path.RenderTransform = new RotateTransform(180);
                }
                var text = new TextBlock
                {
                    Text = tooth.ToothNumber.ToString(),
                    Foreground = tooth.TextColor,
                    FontWeight = FontWeights.Bold,
                    FontSize = 6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = invert ? new Thickness(0, 4, 0, 0) : new Thickness(0, 0, 0, 4)
                };
                grid.Children.Add(path);
                grid.Children.Add(text);
                panel.Children.Add(grid);
            }
            return panel;
        }

        private FrameworkElement BuildLegend(Brush textColor, Brush borderColor)
        {
            var legendBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = borderColor,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(0, 0, 0, 12)
            };
            var legendStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            legendStack.Children.Add(new TextBlock
            {
                Text = "LEYENDA",
                FontSize = 9, FontWeight = FontWeights.Bold,
                Foreground = textColor,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            });

            var items = new (string name, Color color)[]
            {
                ("Sano",      Color.FromRgb(255, 255, 255)),
                ("Caries",    Color.FromRgb(239,  68,  68)),
                ("Resina",    Color.FromRgb( 59, 130, 246)),
                ("Endodoncia",Color.FromRgb(139,  92, 246)),
                ("Corona",    Color.FromRgb(245, 158,  11)),
                ("Extraído",  Color.FromRgb(148, 163, 184)),
                ("Implante",  Color.FromRgb( 16, 185, 129))
            };

            for (int idx = 0; idx < items.Length; idx++)
            {
                var (name, color) = items[idx];
                var item = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, idx < items.Length - 1 ? 12 : 0, 0)
                };
                item.Children.Add(new Border
                {
                    Width = 10, Height = 10,
                    Background = new SolidColorBrush(color),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                    BorderThickness = new Thickness(0.5),
                    CornerRadius = new CornerRadius(2),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                });
                item.Children.Add(new TextBlock
                {
                    Text = name, FontSize = 8,
                    Foreground = textColor,
                    VerticalAlignment = VerticalAlignment.Center
                });
                legendStack.Children.Add(item);
            }

            legendBorder.Child = legendStack;
            return legendBorder;
        }
    }
}



















