using System;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }

        [Required]
        public int PatientID { get; set; }
        public virtual Patient Patient { get; set; }

        [Required]
        public int DentistID { get; set; }
        public virtual Dentist Dentist { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }

        [MaxLength(200)]
        public string Medication { get; set; }

        [MaxLength(100)]
        public string Dosage { get; set; }

        public string Instructions { get; set; }
    }
}