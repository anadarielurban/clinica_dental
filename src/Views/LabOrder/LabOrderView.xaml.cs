using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.LabOrder;
namespace DentalClinicWPF.Views.LabOrder {
    public partial class LabOrderView : UserControl {
        public LabOrderView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e) { if (DataContext is LabOrderViewModel vm) vm.SearchText = string.Empty; }
    }
}
