using System;
using System.Collections.Generic;
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

        public DateTime CreationDate { get; set; } = DateTime.Now;
        
        public string Status { get; set; } = "Borrador";

        // Propiedad mantenida por compatibilidad con la base de datos SQLite antigua
        public string? Details { get; set; } // Borrador, Enviado, Aprobado, Rechazado
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }
        
        public string? Observations { get; set; }

        public virtual ICollection<TreatmentPlanItem> Items { get; set; } = new List<TreatmentPlanItem>();
    }
}

