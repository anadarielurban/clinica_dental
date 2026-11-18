using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DentalClinicWPF.Services
{
    public static class PrescriptionPrintService
    {
        public static void ShowPreview(DentalClinicWPF.Models.Prescription prescription)
        {
            if (prescription == null) return;
            var win = new DentalClinicWPF.Views.Prescription.PrescriptionPreviewWindow(prescription);
            win.ShowDialog();
        }

        public static void Print(DentalClinicWPF.Models.Prescription prescription)
        {
            if (prescription == null) return;

            var printView = new DentalClinicWPF.Views.Prescription.PrescriptionPrintView();
            printView.SetData(prescription);

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Viewbox para que escale sin cortar el texto ni el logo
                var viewbox = new Viewbox
                {
                    Child = printView,
                    Stretch = Stretch.Uniform
                };

                // Border que ocupa EXACTAMENTE la mitad superior de la hoja
                var halfPageBorder = new Border
                {
                    Child = viewbox,
                    Height = printDialog.PrintableAreaHeight / 2, // Mitad de la hoja
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(40) // Margen blanco
                };

                var page = new Grid
                {
                    Width = printDialog.PrintableAreaWidth,
                    Height = printDialog.PrintableAreaHeight,
                    Background = Brushes.White
                };
                
                page.Children.Add(halfPageBorder);

                page.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                page.Arrange(new Rect(new Point(0, 0), new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight)));
                page.UpdateLayout();

                printDialog.PrintVisual(page, "Receta");
            }
        }
    }
}