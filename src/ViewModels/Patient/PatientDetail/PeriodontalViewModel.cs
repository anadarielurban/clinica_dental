using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DentalClinicWPF.ViewModels.Patient.PatientDetail
{
    public partial class PeriodontalViewModel : ObservableObject
    {
        private readonly DentalClinicContext _context;
        private readonly int _patientId;
        public ObservableCollection<PeriodontalRecord> Records { get; } = new();

        public PeriodontalViewModel(DentalClinicContext context, int patientId)
        {
            _context = context;
            _patientId = patientId;
            LoadData();
        }

        [RelayCommand]
        public void LoadData()
        {
            try
            {
                // Verificar si la tabla existe, si no, crearla
                try
                {
                    _context.Database.ExecuteSqlRaw("SELECT Id FROM PeriodontalRecords LIMIT 1");
                }
                catch
                {
                    _context.Database.ExecuteSqlRaw("CREATE TABLE PeriodontalRecords (Id INTEGER PRIMARY KEY AUTOINCREMENT, PatientID INTEGER NOT NULL, ToothNumber INTEGER NOT NULL, ProbingDepths TEXT, Bleeding INTEGER NOT NULL, Mobility INTEGER NOT NULL, Notes TEXT);");
                }

                var existing = _context.PeriodontalRecords.Where(r => r.PatientID == _patientId).ToList();
                
                if (existing.Count == 0)
                {
                    int[] teeth = { 18,17,16,15,14,13,12,11,21,22,23,24,25,26,27,28,48,47,46,45,44,43,42,41,31,32,33,34,35,36,37,38 };
                    foreach (var t in teeth)
                    {
                        _context.PeriodontalRecords.Add(new PeriodontalRecord { PatientID = _patientId, ToothNumber = t });
                    }
                    _context.SaveChanges();
                    existing = _context.PeriodontalRecords.Where(r => r.PatientID == _patientId).ToList();
                }
                
                Records.Clear();
                foreach (var r in existing.OrderBy(x => x.ToothNumber)) Records.Add(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el periodontograma: {ex.Message}", "Error de BD");
            }
        }
    }
}