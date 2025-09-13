using System;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentID { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Model { get; set; }

        [MaxLength(50)]
        public string SerialNumber { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public DateTime? LastServiceDate { get; set; }
        
        public int? RoomID { get; set; }
        public virtual Room Room { get; set; }
    }
}