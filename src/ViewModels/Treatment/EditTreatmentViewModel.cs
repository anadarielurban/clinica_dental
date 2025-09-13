using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Treatment
{
    public class EditTreatmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        public Models.Treatment OriginalTreatment { get; } // Reference to the original object
        public Models.Treatment EditingTreatment { get; set; } // A copy for editing

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditTreatmentViewModel(DentalClinicContext context, Models.Treatment treatment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OriginalTreatment = treatment ?? throw new ArgumentNullException(nameof(treatment));

            // Create a copy of the treatment for editing
            EditingTreatment = new Models.Treatment
            {
                TreatmentID = treatment.TreatmentID,
                Name = treatment.Name,
                Description = treatment.Description,
                Price = treatment.Price,
                Duration = treatment.Duration
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
        }

        private bool CanDelete() => OriginalTreatment != null;

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(EditingTreatment.Name) || EditingTreatment.Price <= 0 || EditingTreatment.Duration <= TimeSpan.Zero)
            {
                MessageBox.Show("Please fill all fields correctly!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Apply changes to the original treatment
            OriginalTreatment.Name = EditingTreatment.Name;
            OriginalTreatment.Description = EditingTreatment.Description;
            OriginalTreatment.Price = EditingTreatment.Price;
            OriginalTreatment.Duration = EditingTreatment.Duration;

            try
            {
                _context.Treatment.Update(OriginalTreatment);
                _context.SaveChanges();

                MessageBox.Show("Treatment updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving treatment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this treatment? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Treatment.Remove(OriginalTreatment);
                    _context.SaveChanges();

                    MessageBox.Show("Treatment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while deleting treatment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
    }
}
