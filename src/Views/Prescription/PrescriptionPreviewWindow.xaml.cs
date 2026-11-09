using System.Windows;

namespace DentalClinicWPF.Views.Prescription
{
    public partial class PrescriptionPreviewWindow : Window
    {
        // Usamos el nombre completo para evitar el error CS0118
        private DentalClinicWPF.Models.Prescription _prescription;

        public PrescriptionPreviewWindow(DentalClinicWPF.Models.Prescription p)
        {
            InitializeComponent();
            _prescription = p;
            PrintView.SetData(p);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            DentalClinicWPF.Services.PrescriptionPrintService.Print(_prescription);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}