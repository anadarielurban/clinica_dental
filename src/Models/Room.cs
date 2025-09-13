using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Room
    {
        [Key]
        public int RoomID { get; set; }

        [Required, MaxLength(10)]
        public string RoomNumber { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
        public virtual ICollection<Dentist> Dentists { get; set; } = new List<Dentist>();
    }
}