using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
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

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }

        private void BuildFlowDocument(Models.Prescription prescription)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                ColumnWidth = double.PositiveInfinity,
                Background = Brushes.White
            };

            // 1. Banner (Teal)
            var banner = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 184, 166)),
                Padding = new Thickness(20),
                Margin = new Thickness(-50, -50, -50, 30)
            };
            var bannerText = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            bannerText.Children.Add(new TextBlock { Text = "Clínica Dental", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center });
            bannerText.Children.Add(new TextBlock { Text = "Receta Médica", FontSize = 14, Foreground = Brushes.White, Margin = new Thickness(0, 4, 0, 0), HorizontalAlignment = HorizontalAlignment.Center });
            banner.Child = bannerText;
            doc.Blocks.Add(new BlockUIContainer(banner));

            // 2. Doctor Info
            var docInfo = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 0, 0, 20) };
            docInfo.Inlines.Add(new Run($"Dr(a). {prescription.Dentist?.FullName ?? "Médico Tratante"}\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
            docInfo.Inlines.Add(new Run($"{prescription.Dentist?.Specialization ?? "Odontología"}") { Foreground = Brushes.Gray });
            doc.Blocks.Add(docInfo);

            // 3. Patient Info Box
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

            // 4. Rx Symbol
            var rxPara = new Paragraph { Margin = new Thickness(0, 0, 0, 15) };
            rxPara.Inlines.Add(new Run("Rx") { FontFamily = new FontFamily("Times New Roman"), FontSize = 40, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(20, 184, 166)) });
            doc.Blocks.Add(rxPara);

            // 5. Details
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

            var instText = new Paragraph { Margin = new Thickness(0, 0, 0, 40) };
            instText.Inlines.Add(new Run(prescription.Instructions ?? "-") { FontSize = 13 });
            doc.Blocks.Add(instText);

            // 6. Signature
            var sigGrid = new Grid { Width = 200, HorizontalAlignment = HorizontalAlignment.Center };
            var sigStack = new StackPanel();
            sigStack.Children.Add(new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 0, 0, 5), Width = 200 });
            sigStack.Children.Add(new TextBlock { Text = "Firma y Sello", FontSize = 11, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center });
            sigGrid.Children.Add(sigStack);
            doc.Blocks.Add(new BlockUIContainer(sigGrid));

            DocViewer.Document = doc;
        }
    }
}
