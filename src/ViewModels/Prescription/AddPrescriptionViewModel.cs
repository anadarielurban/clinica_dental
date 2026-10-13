using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Prescription
{
    public class AddPrescriptionViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Prescription Prescription { get; set; }
        public List<Models.Patient> Patients { get; }
        public List<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddPrescriptionViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Prescription = new Models.Prescription { DateIssued = DateTime.Now };

            Patients = _context.Patient
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToList();

            Dentists = _context.Dentist
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToList();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }


        private void Save()
        {
            try
            {
                if (!IsPrescriptionDataValid())
                {
                    return;
                }

                _context.Prescription.Add(Prescription);
                _context.SaveChanges();

                MessageBox.Show("Receta agregada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la receta: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }

        private bool IsPrescriptionDataValid()
        {
            if (Prescription.PatientID == 0)
            {
                MessageBox.Show("Selecciona un paciente.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Prescription.DentistID == 0)
            {
                MessageBox.Show("Selecciona un dentista.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Prescription.DateIssued == default)
            {
                MessageBox.Show("Selecciona una fecha válida.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Prescription.Medication))
            {
                MessageBox.Show("El medicamento es obligatorio.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Prescription.Dosage))
            {
                MessageBox.Show("La dosis es obligatoria.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (Prescription.Medication.Length > 200)
            {
                MessageBox.Show("El medicamento no puede superar 200 caracteres.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Prescription.Dosage.Length > 100)
            {
                MessageBox.Show("La dosis no puede superar 100 caracteres.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
