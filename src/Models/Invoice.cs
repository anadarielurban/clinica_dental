using System;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        public int AppointmentID { get; set; }
        public virtual Appointment Appointment { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public bool IsPaid { get; set; }
    }
}