using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Schedule;

namespace DentalClinicWPF.Views.Schedule
{
    public partial class ScheduleView : UserControl
    {
        public ScheduleView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ScheduleViewModel vm) vm.SearchText = string.Empty;
        }
    }
}
