using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        public int PatientID { get; set; }
        [ForeignKey(nameof(PatientID))]
        public virtual Patient Patient { get; set; }

        // Puede venir de una Cita o de un Presupuesto (Opcionales)
        public int? AppointmentID { get; set; }
        public virtual Appointment Appointment { get; set; }

        public int? TreatmentPlanID { get; set; }
        public virtual TreatmentPlan TreatmentPlan { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal AmountPaid { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal Change { get; set; }

        public string PaymentMethod { get; set; } = "Efectivo"; // Efectivo, Tarjeta, Transferencia

        [NotMapped]
        public bool IsPaid => AmountPaid >= TotalAmount;

        [NotMapped]
        public decimal BalanceDue => TotalAmount - AmountPaid;
    }
}

