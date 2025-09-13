using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Dentist
    {
        [Key]
        public int DentistID { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Specialization { get; set; }

        [Required, MaxLength(15)]
        [RegularExpression(@"^\+48\d{9}$", ErrorMessage = "Phone Number must start with +48 and contain 9 digits.")]
        public string PhoneNumber { get; set; } = "+48";

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public int RoomID { get; set; }
        public virtual Room Room { get; set; }

        public virtual ICollection<Employee> DentalAssistants { get; set; } = new List<Employee>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        public string DisplayInfo => $"{LastName}, {Specialization}";
    }
}