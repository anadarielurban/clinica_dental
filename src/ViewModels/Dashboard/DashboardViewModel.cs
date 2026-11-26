using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Helpers;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

// Aliases para evitar conflicto entre el namespace y la entidad Appointment
using AppointmentEntity = DentalClinicWPF.Models.Appointment;
using AppointmentViewModelAlias = DentalClinicWPF.ViewModels.Appointment.AppointmentViewModel;
using PatientViewModelAlias = DentalClinicWPF.ViewModels.Patient.PatientViewModel;

namespace DentalClinicWPF.ViewModels.Dashboard
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Action<string, BaseViewModel> _openTab;

        // --- PROPIEDADES DE LOS KPIs ---
        public decimal TotalIncome { get; set; }
        public int PendingInvoices { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
        public int ActiveDentistsCount { get; set; }

        // --- LISTAS PARA LAS TABLAS ---
        public ObservableCollection<AppointmentEntity> UpcomingAppointments { get; set; }
        public ObservableCollection<DentistIncome> IncomeByDentist { get; set; }

        // --- COMANDOS ---
        public ICommand AddAppointmentCommand { get; }
        public ICommand AddPatientCommand { get; }

        public DashboardViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTab)
        {
            _context = context;
            _openTab = openTab;

            UpcomingAppointments = new ObservableCollection<AppointmentEntity>();
            IncomeByDentist = new ObservableCollection<DentistIncome>();

            AddAppointmentCommand = new RelayCommand(() => _openTab("Citas", new AppointmentViewModelAlias(_context)));
            AddPatientCommand = new RelayCommand(() => _openTab("Pacientes", new PatientViewModelAlias(_context, _openTab)));

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // 1 & 2. Calcular Ingresos y Facturas Pendientes
                // Traemos las facturas a memoria con .ToList() para evitar el error de traducción de EF
                var todasLasFacturas = _context.Invoice.ToList();
                
                TotalIncome = todasLasFacturas.Where(i => i.IsPaid).Sum(i => i.TotalAmount);
                OnPropertyChanged(nameof(TotalIncome));

                PendingInvoices = todasLasFacturas.Count(i => !i.IsPaid);
                OnPropertyChanged(nameof(PendingInvoices));

                // 3. Proximas citas (las de hoy en adelante)
                var today = DateTime.Today;
                var citas = _context.Appointment
                    .Include(a => a.Patient)
                    .Include(a => a.Dentist)
                    .Where(a => a.AppointmentDate >= today)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(10)
                    .ToList();

                UpcomingAppointmentsCount = citas.Count;
                OnPropertyChanged(nameof(UpcomingAppointmentsCount));

                foreach (var c in citas) UpcomingAppointments.Add(c);

                // 4. Dentistas activos
                ActiveDentistsCount = _context.Dentist.Count();
                OnPropertyChanged(nameof(ActiveDentistsCount));

                // 5. Ingresos por dentista
                // Hacemos lo mismo, traemos a memoria para agrupar sin problemas
                var facturasPagadas = _context.Invoice
                    .Include(i => i.Appointment)
                    .ThenInclude(a => a.Dentist)
                    .ToList()
                    .Where(i => i.IsPaid && i.Appointment != null && i.Appointment.Dentist != null);

                var incomes = facturasPagadas
                    .GroupBy(i => i.Appointment.Dentist.FirstName + " " + i.Appointment.Dentist.LastName)
                    .Select(g => new DentistIncome { DentistName = g.Key, TotalIncome = g.Sum(i => i.TotalAmount) })
                    .ToList();

                foreach (var inc in incomes) IncomeByDentist.Add(inc);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al cargar el Dashboard: {ex.Message}\n\nDetalles: {ex.InnerException?.Message}", "Error");
            }
        }
    }

    // Clase auxiliar para la tabla de ingresos
    public class DentistIncome
    {
        public string DentistName { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
    }
}
