using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Invoice
{
    public class InvoicePreviewViewModel : BaseViewModel
    {
        public Models.Invoice Invoice { get; }
        public string BankNumber { get; }

        public InvoicePreviewViewModel(Models.Invoice invoice, string bankNumber)
        {
            Invoice = invoice;
            BankNumber = bankNumber;
        }
        
    }
}