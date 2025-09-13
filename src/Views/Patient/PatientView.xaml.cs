// Views/Patient/PatientView.xaml.cs
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

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = DataContext as PatientViewModel;
            if (viewModel?.OpenPatientDetailCommand.CanExecute(null) == true)
            {
                viewModel.OpenPatientDetailCommand.Execute(null);
            }
        }
    }
}