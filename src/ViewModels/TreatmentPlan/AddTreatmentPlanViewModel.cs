using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.TreatmentPlan
{
    public class AddTreatmentPlanViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.TreatmentPlan TreatmentPlan { get; set; }
        public List<Models.Patient> Patients { get; }
        public List<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddTreatmentPlanViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            TreatmentPlan = new Models.TreatmentPlan { CreationDate = DateTime.Now };

            Patients = _context.Patient.ToList();
            Dentists = _context.Dentist.ToList();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                if (TreatmentPlan.PatientID == 0 || TreatmentPlan.DentistID == 0)
                {
                    MessageBox.Show("Please select both a patient and a dentist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.TreatmentPlan.Add(TreatmentPlan);
                _context.SaveChanges();
                MessageBox.Show("Treatment Plan added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow();

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = false;
                    window.Close();
                    break;
                }
            }
        }
    }
}
