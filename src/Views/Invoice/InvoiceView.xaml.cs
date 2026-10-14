using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Invoice;

namespace DentalClinicWPF.Views.Invoice
{
    public partial class InvoiceView : UserControl
    {
        public InvoiceView() { InitializeComponent(); }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InvoiceViewModel vm) vm.SearchText = string.Empty;
        }
    }
}
