using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(100)]
        public string Position { get; set; } // Example: "Dental Assistant", "Receptionist"

        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        public int? DentistID { get; set; }
        public virtual Dentist Dentist { get; set; }

        public string DisplayInfo => $"{FirstName} {LastName} - {Position}";
    }
}