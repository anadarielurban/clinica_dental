using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Dentist;

namespace DentalClinicWPF.Views.Dentist
{
    public partial class DentistView : UserControl
    {
        public DentistView() { InitializeComponent(); }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DentistViewModel vm) vm.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is DentistViewModel vm && vm.OpenDentistDetailCommand.CanExecute(null))
                vm.OpenDentistDetailCommand.Execute(null);
        }
    }
}
