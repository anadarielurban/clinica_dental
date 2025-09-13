using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DentalClinicWPF.Views.Invoice
{
    public partial class InvoicePreviewWindow : Window
    {
        public InvoicePreviewWindow(Models.Invoice invoice, string bankNumber)
        {
            InitializeComponent();
            BuildFlowDocument(invoice, bankNumber);
        }

        private void BuildFlowDocument(Models.Invoice invoice, string bankNumber)
        {
            // Create FlowDocument
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(30),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 14
            };

            // (2) Header with clinic name
            var headerParagraph = new Paragraph
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            headerParagraph.Inlines.Add("Dental Clinic XYZ\n");
            headerParagraph.Inlines.Add(new Run("123 Main Street, City, Country\n")
            {
                FontWeight = FontWeights.Normal, 
                FontSize = 12
            });
            headerParagraph.Inlines.Add(new Run("Phone: +48 123 456 789\n")
            {
                FontWeight = FontWeights.Normal, 
                FontSize = 12
            });
            headerParagraph.Inlines.Add(new Run("Email: info@dentalclinicxyz.com")
            {
                FontWeight = FontWeights.Normal, 
                FontSize = 12
            });
            doc.Blocks.Add(headerParagraph);

            // (3) Invoice title
            var titleParagraph = new Paragraph
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 20)
            };
            titleParagraph.Inlines.Add("Invoice Preview");
            doc.Blocks.Add(titleParagraph);

            // (4) Invoice details
            var infoParagraph = new Paragraph();
            infoParagraph.Inlines.Add(new Run($"Invoice ID: {invoice.InvoiceID}\n"));
            infoParagraph.Inlines.Add(new Run($"Patient: {invoice.Appointment.Patient.DisplayInfo}\n"));
            infoParagraph.Inlines.Add(new Run($"Dentist: {invoice.Appointment.Dentist.DisplayInfo}\n"));
            infoParagraph.Inlines.Add(new Run($"Treatment: {invoice.Appointment.Treatment.Name}\n"));
            infoParagraph.Inlines.Add(new Run($"Appointment Date: {invoice.Appointment.AppointmentDate:yyyy-MM-dd}\n"));
            infoParagraph.Inlines.Add(new Run($"Total Amount: {invoice.TotalAmount:C}\n"));
            infoParagraph.Inlines.Add(new Run($"Paid: {(invoice.IsPaid ? "Yes" : "No")}\n"));
            infoParagraph.Inlines.Add(new Run($"Bank Number: {bankNumber}\n"));
            doc.Blocks.Add(infoParagraph);

            // (5) Signatures (clinic + client) – lines
            var signaturesParagraph = new Paragraph
            {
                Margin = new Thickness(0, 30, 0, 0),
                FontWeight = FontWeights.Bold
            };
            signaturesParagraph.Inlines.Add("Signature (Clinic):\n");
            signaturesParagraph.Inlines.Add("___________________________\n\n");  // Simple lines

            signaturesParagraph.Inlines.Add("Signature (Client):\n");
            signaturesParagraph.Inlines.Add("___________________________\n");
            doc.Blocks.Add(signaturesParagraph);

            // (6) Disclaimer
            var disclaimerParagraph = new Paragraph
            {
                Margin = new Thickness(0, 20, 0, 0),
                FontStyle = FontStyles.Italic
            };
            disclaimerParagraph.Inlines.Add(
                "Please review the invoice details carefully. Contact the clinic if you have any questions."
            );
            doc.Blocks.Add(disclaimerParagraph);

            // Set FlowDocument in the FlowDocumentReader control
            DocViewer.Document = doc;
        }
    }
}
