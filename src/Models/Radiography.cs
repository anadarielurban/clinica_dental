using System;
using System.ComponentModel.DataAnnotations;
namespace DentalClinicWPF.Models {
    public class Radiography {
        [Key] public int Id { get; set; }
        public int PatientID { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}