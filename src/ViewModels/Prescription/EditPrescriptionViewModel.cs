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
    public class EditPrescriptionViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Prescription OriginalPrescription { get; }
        public Models.Prescription EditingPrescription { get; set; }

        public List<Models.Patient> Patients { get; }
        public List<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditPrescriptionViewModel(Models.Prescription prescription, DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OriginalPrescription = prescription ?? throw new ArgumentNullException(nameof(prescription));

            EditingPrescription = new Models.Prescription
            {
                PrescriptionID = prescription.PrescriptionID,
                PatientID = prescription.PatientID,
                DentistID = prescription.DentistID,
                DateIssued = prescription.DateIssued,
                Medication = prescription.Medication,
                Dosage = prescription.Dosage,
                Instructions = prescription.Instructions
            };

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
                
                OriginalPrescription.PatientID = EditingPrescription.PatientID;
                OriginalPrescription.DentistID = EditingPrescription.DentistID;
                OriginalPrescription.DateIssued = EditingPrescription.DateIssued;
                OriginalPrescription.Medication = EditingPrescription.Medication;
                OriginalPrescription.Dosage = EditingPrescription.Dosage;
                OriginalPrescription.Instructions = EditingPrescription.Instructions;
                
                _context.Prescription.Update(OriginalPrescription);
                _context.SaveChanges();

                MessageBox.Show("Prescription updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving prescription: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (EditingPrescription.PatientID == 0)
            {
                MessageBox.Show("Please select a patient.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditingPrescription.DentistID == 0)
            {
                MessageBox.Show("Please select a dentist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditingPrescription.DateIssued == default)
            {
                MessageBox.Show("Please select a valid date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditingPrescription.Medication))
            {
                MessageBox.Show("Medication is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditingPrescription.Dosage))
            {
                MessageBox.Show("Dosage is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (EditingPrescription.Medication.Length > 200)
            {
                MessageBox.Show("Medication cannot exceed 200 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EditingPrescription.Dosage.Length > 100)
            {
                MessageBox.Show("Dosage cannot exceed 100 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
