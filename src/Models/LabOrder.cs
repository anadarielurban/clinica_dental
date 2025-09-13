using System;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class LabOrder
    {
        [Key]
        public int LabOrderID { get; set; }

        [Required]
        public int PatientID { get; set; }
        public virtual Patient Patient { get; set; }

        [Required]
        public int DentistID { get; set; }
        public virtual Dentist Dentist { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public string Description { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }
}