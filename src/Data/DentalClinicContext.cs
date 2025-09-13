using Microsoft.EntityFrameworkCore;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Data
{
    public class DentalClinicContext : DbContext
    {
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Dentist> Dentist { get; set; }
        public DbSet<Treatment> Treatment { get; set; }
        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<TreatmentPlan> TreatmentPlan { get; set; }
        public DbSet<MedicalRecord> MedicalRecord { get; set; }
        public DbSet<Prescription> Prescription { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<LabOrder> LabOrder { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=ADRIANPC;Database=DentalClinicDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja tabel i relacji
            
            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.InvoiceID);

            // Relacja Patient -> Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientID)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relacja Dentist -> Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Dentist)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DentistID)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja Treatment -> Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Treatment)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TreatmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja Appointment -> Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Appointment)
                .WithOne(a => a.Invoice)
                .HasForeignKey<Invoice>(i => i.AppointmentID)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relacja LabOrder -> Patient
            modelBuilder.Entity<LabOrder>()
                .HasOne(lo => lo.Patient)
                .WithMany(p => p.LabOrders)
                .HasForeignKey(lo => lo.PatientID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja TreatmentPlan -> Patient
            modelBuilder.Entity<TreatmentPlan>()
                .HasOne(tp => tp.Patient)
                .WithMany(p => p.TreatmentPlans)
                .HasForeignKey(tp => tp.PatientID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Dentist)
                .WithMany(d => d.DentalAssistants)
                .HasForeignKey(e => e.DentistID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja TreatmentPlan -> Dentist
            modelBuilder.Entity<TreatmentPlan>()
                .HasOne(tp => tp.Dentist)
                .WithMany(d => d.TreatmentPlans)
                .HasForeignKey(tp => tp.DentistID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Room>()
                .HasMany(r => r.Equipments)
                .WithOne(e => e.Room)
                .HasForeignKey(e => e.RoomID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
