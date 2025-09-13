using System.Windows;
using System.Windows.Documents;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Views.Prescription
{
    public partial class PrescriptionPreviewWindow : Window
    {
        public PrescriptionPreviewWindow(Models.Prescription prescription)
        {
            InitializeComponent();
            BuildFlowDocument(prescription);
        }

        private void BuildFlowDocument(Models.Prescription prescription)
        {
            // Create a FlowDocument
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(30),
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 14
            };

            // Title
            var titleParagraph = new Paragraph
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            titleParagraph.Inlines.Add("Prescription Preview");
            doc.Blocks.Add(titleParagraph);

            // Basic info paragraph
            var infoParagraph = new Paragraph();
            infoParagraph.Inlines.Add(new Run($"Prescription ID: {prescription.PrescriptionID}\n"));
            infoParagraph.Inlines.Add(new Run($"Date Issued: {prescription.DateIssued:yyyy-MM-dd}\n"));
            infoParagraph.Inlines.Add(new Run($"Patient: {prescription?.Patient?.DisplayInfo}\n"));
            infoParagraph.Inlines.Add(new Run($"Dentist: {prescription?.Dentist?.DisplayInfo}\n"));
            infoParagraph.Inlines.Add(new Run($"Medication: {prescription?.Medication}\n"));
            infoParagraph.Inlines.Add(new Run($"Dosage: {prescription?.Dosage}\n"));
            infoParagraph.Inlines.Add(new Run($"Instructions: {prescription?.Instructions}\n"));
            doc.Blocks.Add(infoParagraph);

            // Optionally, add disclaimers, signature lines, etc.
            var disclaimerParagraph = new Paragraph
            {
                Margin = new Thickness(0, 20, 0, 0)
            };
            disclaimerParagraph.Inlines.Add("**Disclaimer**: Please follow dosage instructions carefully and consult with your dentist if any side effects occur.");
            doc.Blocks.Add(disclaimerParagraph);

            // Set the FlowDocument to the viewer
            DocViewer.Document = doc;
        }
    }
}
