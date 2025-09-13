using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class Treatment
    {
        [Key]
        public int TreatmentID { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")] 
        public decimal Price { get; set; }

        [NotMapped] 
        public string PriceFormatted => $"{Price} PLN";

        [Required]
        public TimeSpan Duration { get; set; }
        
        public virtual ICollection<Appointment> Appointments { get; set; }
        
        public string DisplayInfo => $"{Name} - {Price:C} - {Duration.TotalMinutes} min";
    }
}