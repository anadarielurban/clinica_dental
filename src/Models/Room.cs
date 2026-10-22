using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Room
    {
        [Key]
        public int RoomID { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? Phone { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Propiedades de navegación
        public virtual ICollection<Dentist> Dentists { get; set; } = new List<Dentist>();
        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}