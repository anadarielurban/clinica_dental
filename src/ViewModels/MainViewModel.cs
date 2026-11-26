using DentalClinicWPF.ViewModels.Budget;
using DentalClinicWPF.ViewModels.Pos;
// ViewModels/MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Helpers;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.ViewModels.Dashboard;
using DentalClinicWPF.ViewModels.Patient;
using DentalClinicWPF.ViewModels.Appointment;
using DentalClinicWPF.ViewModels.Dentist;
using DentalClinicWPF.ViewModels.Treatment;
using DentalClinicWPF.ViewModels.Invoice;
using DentalClinicWPF.ViewModels.TreatmentPlan;
using DentalClinicWPF.ViewModels.MedicalRecord;
using DentalClinicWPF.ViewModels.Prescription;
using DentalClinicWPF.ViewModels.Schedule;
using DentalClinicWPF.ViewModels.Inventory;
using DentalClinicWPF.ViewModels.Supplier;
using DentalClinicWPF.ViewModels.LabOrder;
using DentalClinicWPF.ViewModels.Employee;
using DentalClinicWPF.ViewModels.Room;
using DentalClinicWPF.ViewModels.Equipment;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

namespace DentalClinicWPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<TabItemViewModel> OpenTabs { get; set; } = new();

        private TabItemViewModel? _selectedTab;
        public TabItemViewModel? SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
        }

        // Sidebar navigation commands
        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowCorteCajaCommand { get; }
        public ICommand ShowPatientsCommand { get; }
        public ICommand ShowAppointmentsCommand { get; }
        public ICommand ShowDentistsCommand { get; }
        public ICommand ShowTreatmentsCommand { get; }
        public ICommand ShowInvoicesCommand { get; }
        public ICommand ShowPosCommand { get; }
        public ICommand ShowEquipmentCommand { get; }
        public ICommand ShowTreatmentPlansCommand { get; }
        public ICommand ShowMedicalRecordsCommand { get; }
        public ICommand ShowPrescriptionsCommand { get; }
        public ICommand ShowSchedulesCommand { get; }
        public ICommand ShowBudgetCommand { get; }
    
        public ICommand ShowInventoryCommand { get; }
        public ICommand ShowSuppliersCommand { get; }
        public ICommand ShowLabOrdersCommand { get; }
        public ICommand ShowEmployeesCommand { get; }
        public ICommand ShowRoomsCommand { get; }

        public MainViewModel()
        {
            DentalClinicWPF.Events.PrescriptionSelectedEvent.Instance.Subscribe(OnPrescriptionSelectedFromPatientDetail);
            // Initialize context
            _context = new DentalClinicContext();

            // Define navigation commands with OpenTab action
            ShowDashboardCommand = new RelayCommand(() => OpenTab("Inicio", new DashboardViewModel(_context, OpenTab)));
            ShowCorteCajaCommand = new RelayCommand(() => OpenTab("Corte de Caja", new ViewModels.Corte.CorteCajaViewModel(_context)));
            ShowPatientsCommand = new RelayCommand(() => OpenTab("Pacientes", new PatientViewModel(_context, OpenTab)));
            ShowAppointmentsCommand = new RelayCommand(() => OpenTab("Citas", new AppointmentViewModel(_context)));
            ShowDentistsCommand = new RelayCommand(() => OpenTab("Dentistas", new DentistViewModel(_context, OpenTab)));
            ShowTreatmentsCommand = new RelayCommand(() => OpenTab("Tratamientos", new TreatmentViewModel(_context)));
            ShowInvoicesCommand = new RelayCommand(() => OpenTab("Facturas", new InvoiceViewModel(_context)));
            ShowBudgetCommand = new RelayCommand(() => OpenTab("Presupuestos", new BudgetViewModel(_context)));
            ShowPosCommand = new RelayCommand(() => OpenTab("Punto de Venta", new PosViewModel(_context)));
            ShowTreatmentPlansCommand = new RelayCommand(() => OpenTab("Planes de Tratamiento", new TreatmentPlanViewModel(_context)));
            ShowMedicalRecordsCommand = new RelayCommand(() => OpenTab("Expedientes", new MedicalRecordViewModel(_context)));
            ShowPrescriptionsCommand = new RelayCommand(() => OpenTab("Recetas", new PrescriptionViewModel(_context, OpenTab)));
            // ShowSchedulesCommand = new RelayCommand(() => OpenTab("Horarios", new ScheduleViewModel(_context))); // MÃ³dulo deshabilitado
            ShowInventoryCommand = new RelayCommand(() => OpenTab("Inventario", new InventoryViewModel(_context)));
            ShowSuppliersCommand = new RelayCommand(() => OpenTab("Proveedores", new SupplierViewModel(_context)));
            ShowLabOrdersCommand = new RelayCommand(() => OpenTab("Ã“rdenes de Laboratorio", new LabOrderViewModel(_context)));
            ShowEmployeesCommand = new RelayCommand(() => OpenTab("Empleados", new EmployeeViewModel(_context)));
            ShowRoomsCommand = new RelayCommand(() => OpenTab("Consultorios", new RoomViewModel(_context)));
            ShowEquipmentCommand = new RelayCommand(() => OpenTab("Equipos", new EquipmentViewModel(_context)));

            // Open default tab (Dashboard)
            OpenTab("Inicio", new DashboardViewModel(_context, OpenTab));
        }

        private void OpenTab(string header, BaseViewModel content)
        {
            // Check if the tab already exists
            var existingTab = OpenTabs.FirstOrDefault(t => t.Header == header && t.Content.GetType() == content.GetType());
            if (existingTab != null)
            {
                SelectedTab = existingTab;
                return;
            }

            var newTab = new TabItemViewModel(
                header,
                content,
                CloseTab // Pass the CloseTab method as the action
            );

            OpenTabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void CloseTab(TabItemViewModel tab)
        {
            OpenTabs.Remove(tab);
        }
    
        private void OnPrescriptionSelectedFromPatientDetail(DentalClinicWPF.Models.Prescription prescription)
        {
            if (prescription == null) return;

            // AQUI DEBES LLAMAR A TU MÃ‰TODO QUE ABRE LA PESTAÃ‘A DE RECETAS.
            // Como no conozco el nombre exacto, asumo que tienes un comando o mÃ©todo para abrir pestaÃ±as.
            // Ejemplo: OpenTab("Recetas", new PrescriptionViewModel(_context));
            
            // TODO: Descomentar y ajustar la siguiente lÃ­nea segÃºn tu cÃ³digo real:
            // AbrirPestanaRecetas();
            
            // 1. Intentar ejecutar el comando que abre el mÃ³dulo de recetas
                var cmdProp = this.GetType().GetProperty("ShowPrescriptionsCommand") 
                           ?? this.GetType().GetProperty("OpenPrescriptionsCommand") 
                           ?? this.GetType().GetProperty("ShowRecetasCommand")
                           ?? this.GetType().GetProperty("OpenRecetasCommand");
                
                if (cmdProp != null)
                {
                    try 
                    {
                        var cmd = cmdProp.GetValue(this) as System.Windows.Input.ICommand;
                        if (cmd != null) 
                        {
                            // Ejecuta el comando para cambiar de pestaÃ±a
                            cmd.Execute(null); 
                            return;
                        }
                    }
                    catch { } // Si el comando falla, ignoramos el error y mostramos el mensaje de abajo
                }

                // 2. Si no se encuentra el comando exacto, avisamos pero la receta ya estÃ¡ seleccionada en el ViewModel
                System.Windows.MessageBox.Show("Receta lista. Por favor haz clic en la pestaÃ±a 'Recetas' en el menÃº lateral.", "NavegaciÃ³n");
        }
    }
}











