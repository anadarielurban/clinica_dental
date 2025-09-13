using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordID { get; set; }

        [Required]
        public int PatientID { get; set; }
        [ForeignKey(nameof(PatientID))]
        public virtual Patient Patient { get; set; }

        [Required]
        public int DentistID { get; set; }
        [ForeignKey(nameof(DentistID))]
        public virtual Dentist Dentist { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public byte[] Attachments { get; set; } = Array.Empty<byte>();
    }

}