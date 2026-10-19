using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class ToothRecord
    {
        [Key]
        public int ToothRecordID { get; set; }

        [Required]
        public int PatientID { get; set; }

        [Required]
        public int ToothNumber { get; set; }

        [MaxLength(50)]
        public string Condition { get; set; } = "Sano";

        [MaxLength(200)]
        public string? Notes { get; set; } = string.Empty;

        // Volvemos a agregar esta propiedad para que la BD no marque error
        public bool IsPrimaryTooth { get; set; } = false;
    }
}
