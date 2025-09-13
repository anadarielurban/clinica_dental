using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Dentist
{
    public class DentistDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        public Models.Dentist Dentist { get; }

        // Collection of days in the current week (Mon–Fri).
        public ObservableCollection<DayViewModel> WeekDays { get; set; }
            = new ObservableCollection<DayViewModel>();

        // Current Monday as the start of the week.
        private DateTime _currentWeekStart;
        public DateTime CurrentWeekStart
        {
            get => _currentWeekStart;
            set
            {
                if (SetProperty(ref _currentWeekStart, value))
                {
                    LoadAppointments();
                    OnPropertyChanged(nameof(CurrentWeekRange));
                }
            }
        }

        // Displayed range: e.g., "03.02 - 07.02".
        public string CurrentWeekRange
            => $"{CurrentWeekStart:dd.MM} - {CurrentWeekStart.AddDays(4):dd.MM}";

        // Commands for navigating weeks.
        public ICommand NextWeekCommand { get; }
        public ICommand PreviousWeekCommand { get; }

        // Command to display appointment details.
        public ICommand OpenAppointmentDetailsCommand { get; }

        // Indicators for the current week (not the month).
        private int _totalAppointmentsThisWeek;
        public int TotalAppointmentsThisWeek
        {
            get => _totalAppointmentsThisWeek;
            set => SetProperty(ref _totalAppointmentsThisWeek, value);
        }

        private string _mostFrequentProcedureThisWeek;
        public string MostFrequentProcedureThisWeek
        {
            get => _mostFrequentProcedureThisWeek ?? "N/A";
            set => SetProperty(ref _mostFrequentProcedureThisWeek, value);
        }

        public DentistDetailViewModel(DentalClinicContext context, Models.Dentist dentist)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Dentist = dentist ?? throw new ArgumentNullException(nameof(dentist));

            NextWeekCommand = new RelayCommand(NextWeek);
            PreviousWeekCommand = new RelayCommand(PreviousWeek);
            OpenAppointmentDetailsCommand = new RelayCommand<Models.Appointment>(OpenAppointmentDetails);

            // Set the current week based on today's date.
            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            CurrentWeekStart = today.AddDays(-diff).Date;
        }

        private void NextWeek()
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(7);
        }

        private void PreviousWeek()
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        }

        private void LoadAppointments()
        {
            try
            {
                // 1) Clear and create WeekDays (Mon–Fri).
                WeekDays.Clear();
                for (int i = 0; i < 5; i++)
                {
                    var date = CurrentWeekStart.AddDays(i);
                    WeekDays.Add(new DayViewModel(date));
                }

                // 2) Fetch appointments for the current week.
                var startDate = CurrentWeekStart;
                var endDate = CurrentWeekStart.AddDays(5); // Up to Friday inclusive (exclusive).

                var weekAppointments = _context.Appointment
                    .Include(a => a.Patient)
                    .Include(a => a.Treatment)
                    .Where(a => a.DentistID == Dentist.DentistID
                                && a.AppointmentDate >= startDate
                                && a.AppointmentDate < endDate)
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.StartTime)
                    .ToList();

                // 3) Assign each appointment to the appropriate day.
                foreach (var appt in weekAppointments)
                {
                    var dayView = WeekDays
                        .FirstOrDefault(d => d.Date.Date == appt.AppointmentDate.Date);
                    if (dayView != null)
                    {
                        dayView.Appointments.Add(appt);
                    }
                }

                // 4) Update weekly indicators (instead of monthly).
                UpdateKeyPerformance(weekAppointments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateKeyPerformance(System.Collections.Generic.List<Models.Appointment> appointments)
        {
            // Number of appointments in the given week.
            TotalAppointmentsThisWeek = appointments.Count;

            // Most frequently performed procedure this week.
            MostFrequentProcedureThisWeek = appointments
                .GroupBy(a => a.Treatment.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key
                ?? "N/A";
        }

        private void OpenAppointmentDetails(Models.Appointment appointment)
        {
            if (appointment == null) return;

            string msg = $"Appointment details:\n" +
                         $"Patient: {appointment.Patient?.FullName}\n" +
                         $"Procedure: {appointment.Treatment?.Name}\n" +
                         $"Date: {appointment.AppointmentDate:dd/MM/yyyy}\n" +
                         $"Time: {appointment.StartTime:HH\\:mm} - {appointment.EndTime:HH\\:mm}\n" +
                         $"Notes: {appointment.Notes}\n";

            MessageBox.Show(msg, "Appointment Details",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}


