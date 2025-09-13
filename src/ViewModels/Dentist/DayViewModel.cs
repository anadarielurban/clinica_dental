using System;
using System.Collections.ObjectModel;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Dentist
{
    public class DayViewModel : BaseViewModel
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public string DisplayDate => Date.ToString("dd.MM");

        public ObservableCollection<Models.Appointment> Appointments { get; set; } = new ObservableCollection<Models.Appointment>();

        public DayViewModel(DateTime date)
        {
            Date = date;
        }
    }
}