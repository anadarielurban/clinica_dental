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
        public ICommand ShowPatientsCommand { get; }
        public ICommand ShowAppointmentsCommand { get; }
        public ICommand ShowDentistsCommand { get; }
        public ICommand ShowTreatmentsCommand { get; }
        public ICommand ShowInvoicesCommand { get; }
        public ICommand ShowEquipmentCommand { get; }
        public ICommand ShowTreatmentPlansCommand { get; }
        public ICommand ShowMedicalRecordsCommand { get; }
        public ICommand ShowPrescriptionsCommand { get; }
        public ICommand ShowSchedulesCommand { get; }
        public ICommand ShowInventoryCommand { get; }
        public ICommand ShowSuppliersCommand { get; }
        public ICommand ShowLabOrdersCommand { get; }
        public ICommand ShowEmployeesCommand { get; }
        public ICommand ShowRoomsCommand { get; }

        public MainViewModel()
        {
            // Initialize context
            _context = new DentalClinicContext();

            // Define navigation commands with OpenTab action
            ShowDashboardCommand = new RelayCommand(() => OpenTab("Dashboard", new DashboardViewModel(_context, OpenTab)));
            ShowPatientsCommand = new RelayCommand(() => OpenTab("Patients", new PatientViewModel(_context, OpenTab)));
            ShowAppointmentsCommand = new RelayCommand(() => OpenTab("Appointments", new AppointmentViewModel(_context)));
            ShowDentistsCommand = new RelayCommand(() => OpenTab("Dentists", new DentistViewModel(_context, OpenTab)));
            ShowTreatmentsCommand = new RelayCommand(() => OpenTab("Treatments", new TreatmentViewModel(_context)));
            ShowInvoicesCommand = new RelayCommand(() => OpenTab("Invoices", new InvoiceViewModel(_context, OpenTab)));
            ShowTreatmentPlansCommand = new RelayCommand(() => OpenTab("Treatment Plans", new TreatmentPlanViewModel(_context)));
            ShowMedicalRecordsCommand = new RelayCommand(() => OpenTab("Medical Records", new MedicalRecordViewModel(_context)));
            ShowPrescriptionsCommand = new RelayCommand(() => OpenTab("Prescriptions", new PrescriptionViewModel(_context, OpenTab)));
            ShowSchedulesCommand = new RelayCommand(() => OpenTab("Schedules", new ScheduleViewModel(_context)));
            ShowInventoryCommand = new RelayCommand(() => OpenTab("Inventory", new InventoryViewModel(_context)));
            ShowSuppliersCommand = new RelayCommand(() => OpenTab("Suppliers", new SupplierViewModel(_context)));
            ShowLabOrdersCommand = new RelayCommand(() => OpenTab("Lab Orders", new LabOrderViewModel(_context)));
            ShowEmployeesCommand = new RelayCommand(() => OpenTab("Employees", new EmployeeViewModel(_context)));
            ShowRoomsCommand = new RelayCommand(() => OpenTab("Rooms", new RoomViewModel(_context)));
            ShowEquipmentCommand = new RelayCommand(() => OpenTab("Equipment", new EquipmentViewModel(_context)));

            // Open default tab (Dashboard)
            OpenTab("Dashboard", new DashboardViewModel(_context, OpenTab));
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
    }
}
