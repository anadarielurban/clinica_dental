using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.TreatmentPlan
{
    public class EditTreatmentPlanViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.TreatmentPlan TreatmentPlan { get; private set; }
        public Models.TreatmentPlan OriginalTreatmentPlan { get; private set; }

        public List<Models.Patient> Patients { get; }
        public List<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditTreatmentPlanViewModel(DentalClinicContext context, Models.TreatmentPlan treatmentPlan)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OriginalTreatmentPlan = treatmentPlan ?? throw new ArgumentNullException(nameof(treatmentPlan));
            
            TreatmentPlan = new Models.TreatmentPlan
            {
                TreatmentPlanID = treatmentPlan.TreatmentPlanID,
                PatientID = treatmentPlan.PatientID,
                DentistID = treatmentPlan.DentistID,
                CreationDate = treatmentPlan.CreationDate,
                Details = treatmentPlan.Details
            };

            Patients = _context.Patient.ToList();
            Dentists = _context.Dentist.ToList();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
        }

        private bool CanDelete() => OriginalTreatmentPlan != null;

        private void Save()
        {
            try
            {
                if (TreatmentPlan.PatientID == 0 || TreatmentPlan.DentistID == 0)
                {
                    MessageBox.Show("Please select both a patient and a dentist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Aktualizowanie oryginalnego obiektu
                OriginalTreatmentPlan.PatientID = TreatmentPlan.PatientID;
                OriginalTreatmentPlan.DentistID = TreatmentPlan.DentistID;
                OriginalTreatmentPlan.Details = TreatmentPlan.Details;

                _context.TreatmentPlan.Update(OriginalTreatmentPlan);
                _context.SaveChanges();

                MessageBox.Show("Treatment Plan updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving treatment plan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this treatment plan? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.TreatmentPlan.Remove(OriginalTreatmentPlan);
                    _context.SaveChanges();

                    MessageBox.Show("Treatment Plan deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while deleting treatment plan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Cancel()
        {
            // Zamknięcie okna bez zapisywania zmian
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult = false)
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
    }
}
