using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Invoice;
using System.Windows.Controls;

namespace DentalClinicWPF.Views.Invoice
{
    public partial class InvoiceDetailView : UserControl
    {
        // Konstruktor bezparametrowy
        public InvoiceDetailView()
        {
            InitializeComponent();
            // Opcjonalnie ustaw DataContext na null lub domyślny ViewModel
            // this.DataContext = new InvoiceDetailViewModel(new DentalClinicContext(), new Invoice());
        }

        // Konstruktor z parametrami
        public InvoiceDetailView(DentalClinicContext context, Models.Invoice invoice)
        {
            InitializeComponent();
            this.DataContext = new InvoiceDetailViewModel(context, invoice);
        }
    }
}