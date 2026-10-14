using DentalClinicWPF.ViewModels.Base;
using System;

namespace DentalClinicWPF.ViewModels.Appointment
{
    public class WeekDayCard : BaseViewModel
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public string DayNumber { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public bool IsToday { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
