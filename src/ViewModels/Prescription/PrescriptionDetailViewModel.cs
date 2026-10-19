using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.Prescription;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace DentalClinicWPF.ViewModels.Prescription
{
    public class PrescriptionDetailViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Prescription Prescription { get; }

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

        private void PrintPrescription()
        {
            try
            {
                FlowDocument doc = BuildFlowDocument(Prescription);
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    IDocumentPaginatorSource idpSource = doc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Imprimiendo Receta");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al imprimir:\n{ex.Message}", "Error de Impresión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPrescriptionAsPdf()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Receta_{Prescription.PrescriptionID}",
                    DefaultExt = ".pdf",
                    Filter = "Documentos PDF (.pdf)|*.pdf"
                };

                if (saveFileDialog.ShowDialog() != true) return;
                string fileName = saveFileDialog.FileName;

                using (var document = new PdfDocument())
                {
                    document.Info.Title = $"Receta {Prescription.PrescriptionID}";
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    // Colors & Fonts
                    XColor teal = XColor.FromArgb(20, 184, 166);
                    XColor darkGray = XColor.FromArgb(51, 65, 85);
                    XColor lightGray = XColor.FromArgb(226, 232, 240);
                    
                    XFont fontTitle = new XFont("Arial", 22, XFontStyle.Bold);
                    XFont fontSubtitle = new XFont("Arial", 14, XFontStyle.Bold);
                    XFont fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
                    XFont fontBody = new XFont("Arial", 11, XFontStyle.Regular);
                    XFont fontSmall = new XFont("Arial", 9, XFontStyle.Regular);
                    XFont fontRx = new XFont("Times New Roman", 36, XFontStyle.Bold);

                    double margin = 40;
                    double width = page.Width - (2 * margin);
                    double y = margin;

                    // 1. Header Banner (Teal)
                    gfx.DrawRectangle(new XSolidBrush(teal), 0, 0, page.Width, 90);
                    gfx.DrawString("Clínica Dental", fontTitle, XBrushes.White, new XRect(0, 25, page.Width, 30), XStringFormats.TopCenter);
                    gfx.DrawString("Receta Médica", fontSubtitle, XBrushes.White, new XRect(0, 55, page.Width, 20), XStringFormats.TopCenter);
                    
                    y += 110; // Start below banner

                    // 2. Doctor Info (Top Right)
                    gfx.DrawString("Dr(a). " + (Prescription.Dentist?.FullName ?? "Médico Tratante"), fontHeader, new XSolidBrush(darkGray), page.Width - margin - 200, y, XStringFormats.TopRight);
                    gfx.DrawString(Prescription.Dentist?.Specialization ?? "Odontología", fontBody, XBrushes.Gray, page.Width - margin - 200, y + 15, XStringFormats.TopRight);
                    
                    y += 50;

                    // 3. Patient Info Box (Light Gray Box with Border)
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 250, 252)), margin, y, width, 60);
                    gfx.DrawRectangle(new XPen(lightGray, 1), margin, y, width, 60);
                    
                    gfx.DrawString("PACIENTE", fontSmall, XBrushes.Gray, margin + 10, y + 10);
                    gfx.DrawString(Prescription.Patient?.FullName ?? "Paciente General", fontHeader, new XSolidBrush(darkGray), margin + 10, y + 25);
                    
                    gfx.DrawString("FECHA", fontSmall, XBrushes.Gray, margin + width - 200, y + 10);
                    gfx.DrawString(Prescription.DateIssued.ToString("dd/MM/yyyy"), fontBody, new XSolidBrush(darkGray), margin + width - 200, y + 25);
                    
                    gfx.DrawString("EDAD", fontSmall, XBrushes.Gray, margin + width - 100, y + 10);
                    gfx.DrawString($"{CalculateAge(Prescription.Patient?.DateOfBirth ?? DateTime.Today)} años", fontBody, new XSolidBrush(darkGray), margin + width - 100, y + 25);

                    y += 90;

                    // 4. Rx Symbol
                    gfx.DrawString("Rx", fontRx, new XSolidBrush(teal), margin, y);
                    y += 50;

                    // 5. Prescription Details
                    gfx.DrawString("MEDICAMENTO", fontSmall, XBrushes.Gray, margin, y);
                    gfx.DrawString(Prescription.Medication ?? "-", fontSubtitle, new XSolidBrush(darkGray), margin, y + 15);
                    y += 50;

                    gfx.DrawString("DOSIS", fontSmall, XBrushes.Gray, margin, y);
                    gfx.DrawString(Prescription.Dosage ?? "-", fontBody, new XSolidBrush(darkGray), margin, y + 15);
                    y += 50;

                    gfx.DrawString("INSTRUCCIONES", fontSmall, XBrushes.Gray, margin, y);
                    XRect instRect = new XRect(margin, y + 15, width, 150);
                    gfx.DrawString(Prescription.Instructions ?? "-", fontBody, new XSolidBrush(darkGray), instRect, XStringFormats.TopLeft);

                    // 6. Footer Signature
                    double sigY = page.Height - 100;
                    gfx.DrawLine(new XPen(XBrushes.Gray, 1), margin + (width/2) - 100, sigY, margin + (width/2) + 100, sigY);
                    gfx.DrawString("Firma y Sello", fontBody, XBrushes.Gray, new XRect(margin + (width/2) - 100, sigY + 10, 200, 20), XStringFormats.TopCenter);

                    // Bottom line
                    gfx.DrawLine(new XPen(lightGray, 1), margin, page.Height - 40, page.Width - margin, page.Height - 40);
                    gfx.DrawString("Documento generado por Clínica Dental", fontSmall, XBrushes.LightGray, new XRect(margin, page.Height - 35, width, 20), XStringFormats.TopCenter);

                    document.Save(fileName);
                }

                MessageBox.Show($"PDF exportado correctamente:\n{fileName}", "Exportar a PDF", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al exportar a PDF:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }

        private void PreviewPrescription()
        {
            var previewWindow = new PrescriptionPreviewWindow(Prescription);
            previewWindow.ShowDialog();
        }

        private FlowDocument BuildFlowDocument(Models.Prescription prescription)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                ColumnWidth = double.PositiveInfinity
            };

            // Teal Header Banner
            var banner = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 184, 166)), // Teal
                Padding = new Thickness(20),
                Margin = new Thickness(-50, -50, -50, 30)
            };
            var bannerText = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            bannerText.Children.Add(new TextBlock { Text = "Clínica Dental", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center });
            bannerText.Children.Add(new TextBlock { Text = "Receta Médica", FontSize = 14, Foreground = Brushes.White, Margin = new Thickness(0, 4, 0, 0), HorizontalAlignment = HorizontalAlignment.Center });
            banner.Child = bannerText;
            doc.Blocks.Add(new BlockUIContainer(banner));

            // Doctor Info
            var docInfo = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 0, 0, 20) };
            docInfo.Inlines.Add(new Run($"Dr(a). {prescription.Dentist?.FullName ?? "Médico Tratante"}\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
            docInfo.Inlines.Add(new Run($"{prescription.Dentist?.Specialization ?? "Odontología"}") { Foreground = Brushes.Gray });
            doc.Blocks.Add(docInfo);

            // Patient Info Box
            var patientBox = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 30)
            };
            var patientGrid = new Grid();
            patientGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            patientGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            patientGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var patientStack = new StackPanel();
            patientStack.Children.Add(new TextBlock { Text = "PACIENTE", FontSize = 10, Foreground = Brushes.Gray });
            patientStack.Children.Add(new TextBlock { Text = prescription.Patient?.FullName ?? "Paciente", FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 2, 0, 0) });

            var ageStack = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            ageStack.Children.Add(new TextBlock { Text = "EDAD", FontSize = 10, Foreground = Brushes.Gray });
            ageStack.Children.Add(new TextBlock { Text = $"{CalculateAge(prescription.Patient?.DateOfBirth ?? DateTime.Today)} años", FontSize = 14, Margin = new Thickness(0, 2, 0, 0) });

            var dateStack = new StackPanel();
            dateStack.Children.Add(new TextBlock { Text = "FECHA", FontSize = 10, Foreground = Brushes.Gray });
            dateStack.Children.Add(new TextBlock { Text = prescription.DateIssued.ToString("dd/MM/yyyy"), FontSize = 14, Margin = new Thickness(0, 2, 0, 0) });

            Grid.SetColumn(patientStack, 0);
            Grid.SetColumn(ageStack, 1);
            Grid.SetColumn(dateStack, 2);
            patientGrid.Children.Add(patientStack);
            patientGrid.Children.Add(ageStack);
            patientGrid.Children.Add(dateStack);

            patientBox.Child = patientGrid;
            doc.Blocks.Add(new BlockUIContainer(patientBox));

            // Rx Symbol
            var rxPara = new Paragraph { Margin = new Thickness(0, 0, 0, 15) };
            rxPara.Inlines.Add(new Run("Rx") { FontFamily = new FontFamily("Times New Roman"), FontSize = 40, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(20, 184, 166)) });
            doc.Blocks.Add(rxPara);

            // Prescription details
            var medLabel = new Paragraph { Margin = new Thickness(0, 0, 0, 2) };
            medLabel.Inlines.Add(new Run("MEDICAMENTO") { FontSize = 10, Foreground = Brushes.Gray, FontWeight = FontWeights.Bold });
            doc.Blocks.Add(medLabel);

            var medText = new Paragraph { Margin = new Thickness(0, 0, 0, 20) };
            medText.Inlines.Add(new Run(prescription.Medication ?? "-") { FontSize = 16, FontWeight = FontWeights.Bold });
            doc.Blocks.Add(medText);

            var doseLabel = new Paragraph { Margin = new Thickness(0, 0, 0, 2) };
            doseLabel.Inlines.Add(new Run("DOSIS") { FontSize = 10, Foreground = Brushes.Gray, FontWeight = FontWeights.Bold });
            doc.Blocks.Add(doseLabel);

            var doseText = new Paragraph { Margin = new Thickness(0, 0, 0, 20) };
            doseText.Inlines.Add(new Run(prescription.Dosage ?? "-") { FontSize = 13 });
            doc.Blocks.Add(doseText);

            var instLabel = new Paragraph { Margin = new Thickness(0, 0, 0, 2) };
            instLabel.Inlines.Add(new Run("INSTRUCCIONES") { FontSize = 10, Foreground = Brushes.Gray, FontWeight = FontWeights.Bold });
            doc.Blocks.Add(instLabel);

            var instText = new Paragraph { Margin = new Thickness(0, 0, 0, 60) };
            instText.Inlines.Add(new Run(prescription.Instructions ?? "-") { FontSize = 13 });
            doc.Blocks.Add(instText);

            // Signature
            var sigGrid = new Grid { Width = 200, HorizontalAlignment = HorizontalAlignment.Center };
            var sigStack = new StackPanel();
            sigStack.Children.Add(new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 0, 0, 5), Width = 200 });
            sigStack.Children.Add(new TextBlock { Text = "Firma y Sello", FontSize = 11, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center });
            sigGrid.Children.Add(sigStack);
            doc.Blocks.Add(new BlockUIContainer(sigGrid));

            return doc;
        }
    }
}
