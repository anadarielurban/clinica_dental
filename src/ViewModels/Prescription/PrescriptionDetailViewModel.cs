using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.Prescription; // For PrescriptionPreviewWindow
using System.Printing;
using System.Windows.Controls;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace DentalClinicWPF.ViewModels.Prescription
{
    public class PrescriptionDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Prescription Prescription { get; }

        // Commands
        public ICommand PrintCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand PreviewCommand { get; }

        public PrescriptionDetailViewModel(DentalClinicContext context, Models.Prescription prescription)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Prescription = prescription ?? throw new ArgumentNullException(nameof(prescription));

            PrintCommand = new RelayCommand(PrintPrescription);
            ExportPdfCommand = new RelayCommand(ExportPrescriptionAsPdf);
            PreviewCommand = new RelayCommand(PreviewPrescription);
        }

        // 1) Print Logic
        private void PrintPrescription()
        {
            try
            {
                // Build FlowDocument for preview
                FlowDocument doc = BuildFlowDocument(Prescription);

                // Initialize PrintDialog
                PrintDialog printDialog = new PrintDialog();

                // Optionally: Set printer settings
                // printDialog.PrintQueue = new PrintQueue(new PrintServer(), "YourPrinterName");

                // Show print dialog
                bool? result = printDialog.ShowDialog();
                if (result == true)
                {
                    // Print FlowDocument
                    IDocumentPaginatorSource idpSource = doc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Printing Prescription");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while printing:\n{ex.Message}",
                                "Print Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // 2) Export to PDF Logic
        private void ExportPrescriptionAsPdf()
        {
            try
            {
                // Initialize SaveFileDialog
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Prescription_{Prescription.PrescriptionID}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Documents (.pdf)|*.pdf"
                };

                bool? result = saveFileDialog.ShowDialog();

                if (result != true)
                    return; // User canceled

                string fileName = saveFileDialog.FileName;

                using (var document = new PdfDocument())
                {
                    document.Info.Title = "Prescription Export";

                    // Create a page
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    // Fonts
                    XFont fontRegular = new XFont("Segoe UI", 12, XFontStyle.Regular);
                    XFont fontBold = new XFont("Segoe UI", 12, XFontStyle.Bold);
                    XFont fontTitle = new XFont("Segoe UI", 16, XFontStyle.Bold);

                    double xPos = 50;
                    double yPos = 50;
                    double lineSpacing = 25;

                    // Title
                    gfx.DrawString("Prescription Details",
                                   fontTitle,
                                   XBrushes.Black,
                                   new XRect(xPos, yPos, page.Width - 2 * xPos, 40),
                                   XStringFormats.TopCenter);
                    yPos += 50;

                    // Prescription ID
                    gfx.DrawString("Prescription ID:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Prescription.PrescriptionID}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    // Patient
                    gfx.DrawString("Patient:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Prescription.Patient?.DisplayInfo}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    // Dentist
                    gfx.DrawString("Dentist:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Prescription.Dentist?.DisplayInfo}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    // Date Issued
                    gfx.DrawString("Date Issued:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Prescription.DateIssued:yyyy-MM-dd}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    // Medication
                    gfx.DrawString("Medication:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Prescription.Medication}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    // Dosage
                    gfx.DrawString("Dosage:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Prescription.Dosage}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    // Instructions
                    gfx.DrawString("Instructions:", fontBold, XBrushes.Black, xPos, yPos);
                    yPos += lineSpacing;
                    XRect instructionsRect = new XRect(xPos + 150, yPos, page.Width - (xPos + 200), page.Height - yPos - 50);
                    gfx.DrawString($"{Prescription.Instructions}", fontRegular, XBrushes.Black, instructionsRect, XStringFormats.TopLeft);

                    // Save the document
                    document.Save(fileName);
                }

                MessageBox.Show($"PDF exported successfully:\n{fileName}",
                                "Export to PDF",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting to PDF:\n{ex.Message}",
                                "Export PDF Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // 3) Preview Logic
        private void PreviewPrescription()
        {
            var previewWindow = new PrescriptionPreviewWindow(Prescription);
            previewWindow.ShowDialog();
        }

        // Helper method to build a FlowDocument for printing or preview
        private FlowDocument BuildFlowDocument(Models.Prescription prescription)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 14
            };

            // Title
            var titlePara = new Paragraph
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            titlePara.Inlines.Add("Prescription Details");
            doc.Blocks.Add(titlePara);

            // Information Paragraph
            var info = new Paragraph();
            info.Inlines.Add(new Run($"Prescription ID: {prescription.PrescriptionID}\n"));
            info.Inlines.Add(new Run($"Patient: {prescription.Patient?.DisplayInfo}\n"));
            info.Inlines.Add(new Run($"Dentist: {prescription.Dentist?.DisplayInfo}\n"));
            info.Inlines.Add(new Run($"Date Issued: {prescription.DateIssued:yyyy-MM-dd}\n"));
            info.Inlines.Add(new Run($"Medication: {prescription.Medication}\n"));
            info.Inlines.Add(new Run($"Dosage: {prescription.Dosage}\n"));
            info.Inlines.Add(new Run($"Instructions: {prescription.Instructions}\n"));
            doc.Blocks.Add(info);

            return doc;
        }
    }
}
