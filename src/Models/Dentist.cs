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
        [RegularExpression(@"^[\d\s\+\-\(\)]{7,}$", ErrorMessage = "Phone Number must contain at least 7 digits.")]
        public string PhoneNumber { get; set; } = "+52";

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

        public string FullName => $"{FirstName} {LastName}";
        public string DisplayInfo => $"{LastName}, {Specialization}";
    }
}