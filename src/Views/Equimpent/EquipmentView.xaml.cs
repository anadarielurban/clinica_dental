using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Equipment;
namespace DentalClinicWPF.Views.Equipment {
    public partial class EquipmentView : UserControl {
        public EquipmentView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e) { if (DataContext is EquipmentViewModel vm) vm.SearchText = string.Empty; }
    }
}
