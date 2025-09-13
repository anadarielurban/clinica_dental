using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.Win32;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DentalClinicWPF.ViewModels.Invoice
{
    public class InvoiceDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        public Models.Invoice Invoice { get; }

        public string BankNumber { get; } = "00 1234 5678 9012 3456 7890 1234";

        // Commands
        public ICommand PrintCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand PreviewCommand { get; }

        public InvoiceDetailViewModel(DentalClinicContext context, Models.Invoice invoice)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));

            PrintCommand = new RelayCommand(PrintInvoice);
            ExportPdfCommand = new RelayCommand(ExportInvoiceAsPdf);
            PreviewCommand = new RelayCommand(PreviewInvoice);
        }

        private void PrintInvoice()
        {
            try
            {
                // Create FlowDocument just like in the preview
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
                infoParagraph.Inlines.Add(new Run($"Invoice ID: {Invoice.InvoiceID}\n"));
                infoParagraph.Inlines.Add(new Run($"Patient: {Invoice.Appointment.Patient.DisplayInfo}\n"));
                infoParagraph.Inlines.Add(new Run($"Dentist: {Invoice.Appointment.Dentist.DisplayInfo}\n"));
                infoParagraph.Inlines.Add(new Run($"Treatment: {Invoice.Appointment.Treatment.Name}\n"));
                infoParagraph.Inlines.Add(new Run($"Appointment Date: {Invoice.Appointment.AppointmentDate:yyyy-MM-dd}\n"));
                infoParagraph.Inlines.Add(new Run($"Total Amount: {Invoice.TotalAmount:C}\n"));
                infoParagraph.Inlines.Add(new Run($"Paid: {(Invoice.IsPaid ? "Yes" : "No")}\n"));
                infoParagraph.Inlines.Add(new Run($"Bank Number: {BankNumber}\n"));
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

                // Printing FlowDocument
                PrintDialog printDialog = new PrintDialog();
                bool? printResult = printDialog.ShowDialog();
                if (printResult == true)
                {
                    IDocumentPaginatorSource idpSource = doc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Printing Invoice");
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

        private void ExportInvoiceAsPdf()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Invoice_{Invoice.InvoiceID}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Documents (.pdf)|*.pdf"
                };

                bool? result = saveFileDialog.ShowDialog();
                if (result != true) return;

                string fileName = saveFileDialog.FileName;

                using (var pdfDocument = new PdfDocument())
                {
                    pdfDocument.Info.Title = "Invoice Export";
                    var page = pdfDocument.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);

                    // Fonts
                    var fontRegular = new XFont("Segoe UI", 12, XFontStyle.Regular);
                    var fontBold = new XFont("Segoe UI", 12, XFontStyle.Bold);
                    var fontTitle = new XFont("Segoe UI", 16, XFontStyle.Bold);

                    double xPos = 50;
                    double yPos = 50;
                    double lineSpacing = 25;

                    // ----------------------------------
                    // (2) Clinic data
                    // ----------------------------------
                    gfx.DrawString("Dental Clinic XYZ", fontBold, XBrushes.Black, new XPoint(xPos, yPos));
                    gfx.DrawString("123 Main Street, City, Country", fontRegular, XBrushes.Black, new XPoint(xPos, yPos + 20));
                    gfx.DrawString("Phone: +48 123 456 789", fontRegular, XBrushes.Black, new XPoint(xPos, yPos + 40));

                    yPos += 70;

                    // ----------------------------------
                    // (3) Invoice title
                    // ----------------------------------
                    gfx.DrawString("Invoice Details", fontTitle, XBrushes.Black,
                        new XRect(xPos, yPos, page.Width - 2 * xPos, 40),
                        PdfSharpCore.Drawing.XStringFormats.TopCenter);
                    yPos += 50;

                    // ----------------------------------
                    // (4) Invoice details
                    // ----------------------------------
                    gfx.DrawString("Invoice ID:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Invoice.InvoiceID}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    gfx.DrawString("Patient:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Invoice.Appointment.Patient.DisplayInfo}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    gfx.DrawString("Dentist:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Invoice.Appointment.Dentist.DisplayInfo}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    gfx.DrawString("Treatment:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Invoice.Appointment.Treatment.Name}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    gfx.DrawString("Total Amount:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{Invoice.TotalAmount:C}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    gfx.DrawString("Bank Number:", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString($"{BankNumber}", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += (lineSpacing * 2);

                    // ----------------------------------
                    // (5) Signatures as lines
                    // ----------------------------------
                    gfx.DrawString("Signature (Clinic):", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString("___________________________", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    gfx.DrawString("Signature (Client):", fontBold, XBrushes.Black, xPos, yPos);
                    gfx.DrawString("___________________________", fontRegular, XBrushes.Black, xPos + 150, yPos);
                    yPos += lineSpacing;

                    // Save PDF
                    try
                    {
                        pdfDocument.Save(fileName);
                        MessageBox.Show($"PDF exported successfully:\n{fileName}",
                                        "Export to PDF",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving PDF:\n{ex.Message}",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting to PDF:\n{ex.Message}",
                                "Export PDF Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void PreviewInvoice()
        {
            try
            {
                var previewWindow = new Views.Invoice.InvoicePreviewWindow(Invoice, BankNumber);
                previewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the preview window:\n{ex.Message}",
                                "Preview Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
