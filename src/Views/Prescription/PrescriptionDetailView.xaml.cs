using System.Windows;
using System.Windows.Controls;

namespace DentalClinicWPF.Views.Prescription
{
    public partial class PrescriptionDetailView : UserControl
    {
        public PrescriptionDetailView()
        {
            InitializeComponent();
        }

        private DentalClinicWPF.Models.Prescription ObtenerReceta()
        {
            if (DataContext == null) return null;
            var prop = DataContext.GetType().GetProperty("Prescription");
            if (prop != null) return prop.GetValue(DataContext, null) as DentalClinicWPF.Models.Prescription;
            if (DataContext is DentalClinicWPF.Models.Prescription recetaDirecta) return recetaDirecta;
            return null;
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            var receta = ObtenerReceta();
            if (receta != null) DentalClinicWPF.Services.PrescriptionPrintService.Print(receta);
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var receta = ObtenerReceta();
            if (receta != null) 
            {
                MessageBox.Show("Selecciona 'Microsoft Print to PDF' en la ventana que aparecerá para guardar tu PDF.", "Exportar PDF");
                DentalClinicWPF.Services.PrescriptionPrintService.Print(receta); 
            }
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            var receta = ObtenerReceta();
            if (receta != null) DentalClinicWPF.Services.PrescriptionPrintService.ShowPreview(receta);
        }
    }
}