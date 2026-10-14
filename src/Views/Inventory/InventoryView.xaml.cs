using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Inventory;

namespace DentalClinicWPF.Views.Inventory
{
    public partial class InventoryView : UserControl
    {
        public InventoryView() { InitializeComponent(); }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InventoryViewModel vm) vm.SearchText = string.Empty;
        }
    }
}
