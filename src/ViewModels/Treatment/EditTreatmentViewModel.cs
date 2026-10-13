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
                MessageBox.Show("Completa todos los campos correctamente.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                MessageBox.Show("¡Tratamiento actualizado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el tratamiento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                "¿Seguro que quieres eliminar este tratamiento? Esta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Treatment.Remove(OriginalTreatment);
                    _context.SaveChanges();

                    MessageBox.Show("¡Tratamiento eliminado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el tratamiento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
