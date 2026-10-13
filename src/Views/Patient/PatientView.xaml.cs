using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Patient;

namespace DentalClinicWPF.Views.Patient
{
    public partial class PatientView : UserControl
    {
        public PatientView()
        {
            InitializeComponent();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientViewModel vm)
            {
                // Solo actualizamos el texto; el timer interno se encarga del resto
                vm.SearchText = string.Empty;
            }
        }
    }
}
