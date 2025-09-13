using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = "N/A";

        [Required(ErrorMessage = "Last Name is required.")]
        [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string LastName { get; set; } = "N/A";

        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
        public string Gender { get; set; } = "Male"; 


        [Required(ErrorMessage = "Phone Number is required.")]
        [MaxLength(15, ErrorMessage = "Phone Number cannot exceed 15 characters.")]
        [RegularExpression(@"^\+48\d{9}$", ErrorMessage = "Phone Number must start with +48 and contain 9 digits.")]
        public string PhoneNumber { get; set; } = "+48";

        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; }

        [MaxLength(1000, ErrorMessage = "Medical History cannot exceed 1000 characters.")]
        public string MedicalHistory { get; set; } = "No history available";

        // Navigation Properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
        
        public string FullName => $"{FirstName} {LastName}";
        public string DisplayInfo => $"{FirstName} {LastName} (DOB: {DateOfBirth:yyyy-MM-dd})";
    }
}
