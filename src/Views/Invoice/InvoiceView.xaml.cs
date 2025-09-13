using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Invoice;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DentalClinicWPF.Views.Invoice
{
    public partial class InvoiceView : UserControl
    {
        public InvoiceView()
        {
            InitializeComponent();
        }

        // Opcjonalnie możesz dodać obsługę podwójnego kliknięcia na wierszu, aby automatycznie otwierać szczegóły:
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Models.Invoice invoice)
            {
                if (DataContext is InvoiceViewModel viewModel)
                {
                    viewModel.SelectedInvoice = invoice;
                    viewModel.OpenInvoiceDetailCommand.Execute(null);
                }
            }
        }
    }
}