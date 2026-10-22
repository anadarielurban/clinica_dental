using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace DentalClinicWPF.ViewModels.Patient.PatientDetail
{
    public partial class RadiographyViewModel : ObservableObject
    {
        private readonly DentalClinicContext _context;
        private readonly int _patientId;
        private readonly string _imagesFolder;

        public ObservableCollection<Radiography> Radiographies { get; } = new();

        public RadiographyViewModel(DentalClinicContext context, int patientId)
        {
            _context = context;
            _patientId = patientId;
            
            _imagesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "DentalClinic_Radiografias");
            if (!Directory.Exists(_imagesFolder)) Directory.CreateDirectory(_imagesFolder);
            
            LoadData();
        }

        private void LoadData()
        {
            Radiographies.Clear();
            var list = _context.Radiographies.Where(r => r.PatientID == _patientId).ToList();
            foreach (var r in list) Radiographies.Add(r);
        }

        [RelayCommand]
        private void UploadImage()
        {
            var ofd = new OpenFileDialog { Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp", Title = "Seleccionar Radiografía" };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    string fileName = $"{_patientId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(ofd.FileName)}";
                    string destPath = Path.Combine(_imagesFolder, fileName);
                    File.Copy(ofd.FileName, destPath, true);

                    var radio = new Radiography { PatientID = _patientId, FilePath = destPath, Description = Path.GetFileName(ofd.FileName) };
                    _context.Radiographies.Add(radio);
                    _context.SaveChanges();
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show($"Error al subir imagen: {ex.Message}"); }
            }
        }

        [RelayCommand]
        private void OpenImage(Radiography radio)
        {
            if (radio == null || string.IsNullOrEmpty(radio.FilePath)) return;
            
            if (File.Exists(radio.FilePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(radio.FilePath) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("El archivo de imagen ya no se encuentra en tu computadora.", "Archivo no encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}