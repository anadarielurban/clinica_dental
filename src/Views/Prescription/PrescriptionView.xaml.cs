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
    }
}
