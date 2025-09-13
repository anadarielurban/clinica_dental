using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class Schedule
    {
        [Key]
        public int ScheduleID { get; set; }

        [Required]
        public int DentistID { get; set; }

        [ForeignKey("DentistID")]
        public virtual Dentist Dentist { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Fixed working hours
        public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0); // 8:00 AM
        public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);  // 5:00 PM

        public bool IsDayOff { get; set; }
    }
}