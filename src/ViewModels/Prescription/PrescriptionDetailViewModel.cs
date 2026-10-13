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
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Imprimiendo Receta");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al imprimir:\n{ex.Message}",
                                "Error de Impresión",
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
                    Filter = "Documentos PDF (.pdf)|*.pdf"
                };

                bool? result = saveFileDialog.ShowDialog();

                if (result != true)
                    return; // User canceled

                string fileName = saveFileDialog.FileName;

                using (var document = new PdfDocument())
                {
                    document.Info.Title = "Exportar Receta";

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
                    gfx.DrawString("Detalles de la Receta",
                                   fontTitle,
                                   XBrushes.Black,
                                   new XRect(xPos, yPos, page.Width - 2 * xPos, 40),
                                   XStringFormats.TopCenter);
                    yPos += 50;

                    // Prescription ID
                    gfx.DrawString("ID de Receta:", fontBold, XBrushes.Black, xPos, yPos);
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
                    gfx.DrawString("Fecha de Emisión:", fontBold, XBrushes.Black, xPos, yPos);
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

                MessageBox.Show($"PDF exportado correctamente:\n{fileName}",
                                "Exportar a PDF",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al exportar a PDF:\n{ex.Message}",
                                "Error al Exportar PDF",
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
            titlePara.Inlines.Add("Detalles de la Receta");
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
