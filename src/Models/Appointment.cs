using System;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }

        [Required]
        public int PatientID { get; set; }
        public virtual Patient Patient { get; set; }

        [Required]
        public int DentistID { get; set; }
        public virtual Dentist Dentist { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public int TreatmentID { get; set; }
        public virtual Treatment Treatment { get; set; }

        public string Notes { get; set; }

        public virtual Invoice Invoice { get; set; }

        public string DisplayInfo =>
            $"{AppointmentDate:yyyy-MM-dd} {StartTime:hh\\:mm} - {Patient?.FirstName} {Patient?.LastName}, " +
            $"Treatment: {Treatment?.Name}, Cost: {Treatment?.Price:C}, Dentist: {Dentist?.LastName}";
    }
}