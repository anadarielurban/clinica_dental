using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Appointment;

namespace DentalClinicWPF.Views.Appointment
{
    public partial class AppointmentView : UserControl
    {
        public AppointmentView() { InitializeComponent(); }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AppointmentViewModel vm) vm.SearchText = string.Empty;
        }
    }
}
