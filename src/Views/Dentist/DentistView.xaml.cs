using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Dentist;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DentalClinicWPF.Views.Dentist
{
    public partial class DentistView : UserControl
    {
        public DentistView()
        {
            InitializeComponent();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Models.Dentist dentist)
            {
                if (DataContext is DentistViewModel viewModel)
                {
                    viewModel.SelectedDentist = dentist;
                    viewModel.OpenDentistDetailCommand.Execute(null);
                }
            }
        }
    }
}