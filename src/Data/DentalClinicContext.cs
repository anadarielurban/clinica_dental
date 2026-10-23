using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Data
{
    public partial class DentalClinicContext : DbContext
    {
        public DbSet<Models.Patient> Patient { get; set; }
        public DbSet<Models.Dentist> Dentist { get; set; }
        public DbSet<Models.Appointment> Appointment { get; set; }
        public DbSet<Models.Treatment> Treatment { get; set; }
        public DbSet<Models.TreatmentPlan> TreatmentPlan { get; set; }
        public DbSet<Models.TreatmentPlanItem> TreatmentPlanItem { get; set; }
        public DbSet<Models.Prescription> Prescription { get; set; }
        public DbSet<Models.Invoice> Invoice { get; set; }
        public DbSet<Models.MedicalRecord> MedicalRecord { get; set; }
        public DbSet<Models.LabOrder> LabOrder { get; set; }
        public DbSet<Models.Inventory> Inventory { get; set; }
        public DbSet<Models.Supplier> Supplier { get; set; }
        public DbSet<Models.Room> Room { get; set; }
        public DbSet<Models.Equipment> Equipment { get; set; }
        public DbSet<Models.Employee> Employee { get; set; }
        public DbSet<Models.Schedule> Schedule { get; set; }
        public DbSet<Models.ToothRecord> ToothRecord { get; set; }
        public DbSet<PeriodontalRecord> PeriodontalRecords { get; set; }
        public DbSet<Radiography> Radiographies { get; set; }

    public DbSet<Prescription> Prescriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "DentalClinic.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        // Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â MIGRACIÃƒâ€œN 1: HISTORIA CLÃƒÂNICA Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
        public void EnsureMedicalHistorySchema()
        {
            try
            {
                var conn = Database.GetDbConnection();
                var wasClosed = conn.State != ConnectionState.Open;
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
                    ("MedicalHistory", "TEXT NULL"),
                    ("Gender", "TEXT NULL")
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

        // Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â MIGRACIÃƒâ€œN 2: PRESUPUESTOS Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
        public void EnsureTreatmentPlanSchema()
        {
            try
            {
                var conn = Database.GetDbConnection();
                var wasClosed = conn.State != ConnectionState.Open;
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
                    ("Observations", "TEXT NULL"),
                    ("Details", "TEXT NULL")
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
        }

        // Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â MIGRACIÃƒâ€œN 3: POS / FACTURACIÃƒâ€œN Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
        public void EnsureInvoicePosSchema()
        {
            try
            {
                var conn = Database.GetDbConnection();
                var wasClosed = conn.State != ConnectionState.Open;
                if (wasClosed) conn.Open();

                var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(Invoice);";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read()) existing.Add(reader.GetString(1));
                }

                var cols = new (string Name, string Def)[]
                {
                    ("PatientID", "INTEGER NOT NULL DEFAULT 1"),
                    ("AmountPaid", "TEXT NOT NULL DEFAULT '0.0'"),
                    ("PaymentMethod", "TEXT NOT NULL DEFAULT 'Efectivo'"),
                    ("TreatmentPlanID", "INTEGER NULL")
                };

                foreach (var (name, def) in cols)
                {
                    if (!existing.Contains(name))
                    {
                        using var alter = conn.CreateCommand();
                        alter.CommandText = $"ALTER TABLE Invoice ADD COLUMN {name} {def};";
                        alter.ExecuteNonQuery();
                    }
                }
                if (wasClosed) conn.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Migration] Error: {ex.Message}");
                throw;
            }
        }

        // Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â MIGRACIÃƒâ€œN 4: EFECTIVO Y CAMBIO Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
        public void EnsureInvoiceCashSchema()
        {
            try
            {
                var conn = Database.GetDbConnection();
                var wasClosed = conn.State != ConnectionState.Open;
                if (wasClosed) conn.Open();
                var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(Invoice);";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read()) existing.Add(reader.GetString(1));
                }

                if (!existing.Contains("AmountReceived"))
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE Invoice ADD COLUMN AmountReceived TEXT NOT NULL DEFAULT '0.0';";
                    alter.ExecuteNonQuery();
                }
                if (!existing.Contains("Change"))
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE Invoice ADD COLUMN Change TEXT NOT NULL DEFAULT '0.0';";
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
    
        public void EnsureRoomPhoneSchema()
        {
            try
            {
                Database.ExecuteSqlRaw("SELECT Phone FROM Room LIMIT 1");
            }
            catch
            {
                Database.ExecuteSqlRaw("ALTER TABLE Room ADD COLUMN Phone TEXT;");
            }
        }
        public void EnsureClinicalSchema()
        {
            try { Database.ExecuteSqlRaw("SELECT Id FROM PeriodontalRecords LIMIT 1"); } catch { Database.ExecuteSqlRaw("CREATE TABLE PeriodontalRecords (Id INTEGER PRIMARY KEY AUTOINCREMENT, PatientID INTEGER NOT NULL, ToothNumber INTEGER NOT NULL, ProbingDepths TEXT, Bleeding INTEGER NOT NULL, Mobility INTEGER NOT NULL, Notes TEXT);"); }
            try { Database.ExecuteSqlRaw("SELECT Id FROM Radiographies LIMIT 1"); } catch { Database.ExecuteSqlRaw("CREATE TABLE Radiographies (Id INTEGER PRIMARY KEY AUTOINCREMENT, PatientID INTEGER NOT NULL, Description TEXT, FilePath TEXT, Date TEXT NOT NULL);"); }
        }}

        }
