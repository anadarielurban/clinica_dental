using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Supplier;

namespace DentalClinicWPF.Views.Supplier
{
    public partial class SupplierView : UserControl
    {
        public SupplierView() { InitializeComponent(); }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SupplierViewModel vm) vm.SearchText = string.Empty;
        }
    }
}
