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
                var viewbox = new Viewbox { Child = printView, Stretch = Stretch.Uniform };
                // Margen de 50px para que no toque los bordes de la hoja
                var border = new Border { Child = viewbox, Padding = new Thickness(50) }; 
                
                var container = new Grid
                {
                    Width = printDialog.PrintableAreaWidth,
                    Height = printDialog.PrintableAreaHeight,
                    Background = Brushes.White
                };
                container.Children.Add(border);

                container.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                container.Arrange(new Rect(new Point(0, 0), container.DesiredSize));
                container.UpdateLayout();

                printDialog.PrintVisual(container, "Receta");
            }
        }
    }
}