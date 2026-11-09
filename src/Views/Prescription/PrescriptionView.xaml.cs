using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Prescription;

namespace DentalClinicWPF.Views.Prescription
{
    public partial class PrescriptionView : UserControl
    {
        public PrescriptionView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PrescriptionViewModel vm) vm.SearchText = string.Empty;
        }
    
        private void BtnImprimir_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var receta = button?.DataContext as DentalClinicWPF.Models.Prescription;

            if (receta != null)
            {
                if (DataContext is DentalClinicWPF.ViewModels.Prescription.PrescriptionViewModel vm)
                {
                    vm.ImprimirReceta(receta);
                }
                else
                {
                    DentalClinicWPF.Services.PrescriptionPrintService.Print(receta);
                }
            }
        }
    }
}
