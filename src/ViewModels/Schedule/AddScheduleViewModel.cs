using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Schedule
{
    public class AddScheduleViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        private Models.Schedule _schedule;
        public Models.Schedule Schedule
        {
            get => _schedule;
            set
            {
                _schedule = value;
                OnPropertyChanged();
            }
        }

        public List<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddScheduleViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Schedule = new Models.Schedule
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(4), // Default to a 5-day week
                IsDayOff = false
                // StartTime and EndTime are set to default values in the model
            };

            // Retrieve the list of dentists
            Dentists = _context.Dentist.ToList();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            // Validate that work dates are Monday to Friday
            if (Schedule.StartDate.DayOfWeek == DayOfWeek.Saturday || Schedule.StartDate.DayOfWeek == DayOfWeek.Sunday ||
                Schedule.EndDate.DayOfWeek == DayOfWeek.Saturday || Schedule.EndDate.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("Employees do not work on weekends.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ensure the date range is logical
            if (Schedule.StartDate > Schedule.EndDate)
            {
                MessageBox.Show("Start date must be earlier than end date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if a schedule for this dentist and date range already exists
            var overlappingSchedule = _context.Schedule
                .Where(s => s.DentistID == Schedule.DentistID)
                .Any(s => s.StartDate <= Schedule.EndDate && s.EndDate >= Schedule.StartDate);

            if (overlappingSchedule)
            {
                MessageBox.Show("A schedule for this dentist and date range already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate the selected dentist
            var dentist = Dentists.FirstOrDefault(d => d.DentistID == Schedule.DentistID);
            if (dentist == null)
            {
                MessageBox.Show("Selected dentist does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _context.Schedule.Add(Schedule);
            _context.SaveChanges();

            // Set DialogResult to true to indicate success
            SetDialogResult(true);
        }

        private void Cancel()
        {
            // Set DialogResult to false to indicate cancellation
            SetDialogResult(false);
        }

        private void SetDialogResult(bool result)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = result;
                    window.Close();
                    break;
                }
            }
        }
    }
}
