using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Treatment;

namespace DentalClinicWPF.Views.Treatment
{
    public partial class TreatmentView : UserControl
    {
        public TreatmentView() { InitializeComponent(); }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TreatmentViewModel vm) vm.SearchText = string.Empty;
        }
    }
}
