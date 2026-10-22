using System.ComponentModel.DataAnnotations;
namespace DentalClinicWPF.Models {
    public class PeriodontalRecord {
        [Key] public int Id { get; set; }
        public int PatientID { get; set; }
        public int ToothNumber { get; set; }
        public string? ProbingDepths { get; set; }
        public bool Bleeding { get; set; }
        public int Mobility { get; set; }
        public string? Notes { get; set; }
    }
}