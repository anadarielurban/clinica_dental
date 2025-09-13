using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class TreatmentPlan
    {
        [Key]
        public int TreatmentPlanID { get; set; }

        [Required]
        public int PatientID { get; set; }
        [ForeignKey(nameof(PatientID))]
        public virtual Patient Patient { get; set; }

        [Required]
        public int DentistID { get; set; }
        [ForeignKey(nameof(DentistID))]
        public virtual Dentist Dentist { get; set; }

        public DateTime CreationDate { get; set; }
        public string Details { get; set; }
    }
}