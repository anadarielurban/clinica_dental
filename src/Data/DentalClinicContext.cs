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
        public DbSet<Models.TreatmentPlanItem> TreatmentPlanItem { get; set; }
        public DbSet<MedicalRecord> MedicalRecord { get; set; }
        public DbSet<Prescription> Prescription { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<LabOrder> LabOrder { get; set; }
        public DbSet<ToothRecord> ToothRecord { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=clinica-dental.db");
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
    /// <summary>
    /// Agrega las columnas de historia clínica a la tabla Patient si no existen.
    /// No destruye datos existentes. Llamar después de EnsureCreated().
    /// </summary>
    public void EnsureMedicalHistorySchema()
    {
        try
        {
            var conn = Database.GetDbConnection();
            var wasClosed = conn.State != System.Data.ConnectionState.Open;
            if (wasClosed) conn.Open();

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(Patient);";
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) existing.Add(reader.GetString(1));
            }

            var columns = new (string Name, string Def)[]
            {
                ("BloodType", "TEXT NULL"),
                ("HasDiabetes", "INTEGER NOT NULL DEFAULT 0"),
                ("HasHypertension", "INTEGER NOT NULL DEFAULT 0"),
                ("HasAllergies", "INTEGER NOT NULL DEFAULT 0"),
                ("AllergiesDetails", "TEXT NULL"),
                ("IsPregnant", "INTEGER NOT NULL DEFAULT 0"),
                ("HasHeartConditions", "INTEGER NOT NULL DEFAULT 0"),
                ("HasCoagulationIssues", "INTEGER NOT NULL DEFAULT 0"),
                ("TakesAnticoagulants", "INTEGER NOT NULL DEFAULT 0"),
                ("IsSmoker", "INTEGER NOT NULL DEFAULT 0"),
                ("MedicalNotes", "TEXT NULL"),
                ("EmergencyContactName", "TEXT NULL"),
                ("EmergencyContactPhone", "TEXT NULL"),
            };

            foreach (var (name, def) in columns)
            {
                if (existing.Contains(name)) continue;
                using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE Patient ADD COLUMN {name} {def};";
                alter.ExecuteNonQuery();
            }

            if (wasClosed) conn.Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Migration] Error: {ex.Message}");
            throw;
        }
    }
    
    public void EnsureTreatmentPlanSchema()
    {
        try
        {
            var conn = Database.GetDbConnection();
            var wasClosed = conn.State != System.Data.ConnectionState.Open;
            if (wasClosed) conn.Open();

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(TreatmentPlan);";
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) existing.Add(reader.GetString(1));
            }

            var cols = new (string Name, string Def)[]
            {
                ("Status", "TEXT NOT NULL DEFAULT 'Borrador'"),
                ("TotalAmount", "TEXT NOT NULL DEFAULT '0.0'"),
                ("Observations", "TEXT NULL")
            };

            foreach (var (name, def) in cols)
            {
                if (!existing.Contains(name))
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = $"ALTER TABLE TreatmentPlan ADD COLUMN {name} {def};";
                    alter.ExecuteNonQuery();
                }
            }

            // Crear tabla TreatmentPlanItem
            using (var createCmd = conn.CreateCommand())
            {
                createCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS TreatmentPlanItem (
                        TreatmentPlanItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                        TreatmentPlanID INTEGER NOT NULL,
                        TreatmentID INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice TEXT NOT NULL,
                        FOREIGN KEY(TreatmentPlanID) REFERENCES TreatmentPlan(TreatmentPlanID) ON DELETE CASCADE,
                        FOREIGN KEY(TreatmentID) REFERENCES Treatment(TreatmentID) ON DELETE CASCADE
                    );";
                createCmd.ExecuteNonQuery();
            }

            if (wasClosed) conn.Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Migration] Error: {ex.Message}");
            throw;
        }
    }}


    }





