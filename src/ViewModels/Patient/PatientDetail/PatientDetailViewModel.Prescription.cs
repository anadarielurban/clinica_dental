using System;
using System.Windows;
using DentalClinicWPF.Data;

namespace DentalClinicWPF.ViewModels.Patient.PatientDetail
{
    // Esta clase se fusiona automáticamente con PatientDetailViewModel.cs
    public partial class PatientDetailViewModel
    {
        public void OpenPrescription(Models.Prescription? prescription)
        {
            if (prescription == null) return;

            try
            {
                // Publicar evento para que el módulo de recetas navegue
                // y seleccione esta receta.
                DentalClinicWPF.Events.PrescriptionSelectedEvent.Instance.Publish(prescription);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la receta: {ex.Message}", "Error");
            }
        }
    }
}