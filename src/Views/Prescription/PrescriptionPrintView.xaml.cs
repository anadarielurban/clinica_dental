using System.Windows.Controls;

namespace DentalClinicWPF.Views.Prescription
{
    public partial class PrescriptionPrintView : UserControl
    {
        public PrescriptionPrintView()
        {
            InitializeComponent();
        }

        // Usamos el nombre completo para evitar el conflicto con el namespace "Prescription"
        public void SetData(DentalClinicWPF.Models.Prescription p)
        {
            if (p == null) return;
            TxtPatientName.Text = p.Patient != null ? $"{p.Patient.FirstName} {p.Patient.LastName}" : "Paciente Desconocido";
            TxtMedication.Text = p.Medication;
            TxtDosage.Text = p.Dosage;
            TxtInstructions.Text = p.Instructions;
        }
    }
}