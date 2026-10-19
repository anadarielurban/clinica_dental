using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Room;

namespace DentalClinicWPF.Views.Room
{
    public partial class RoomView : UserControl
    {
        public RoomView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RoomViewModel vm) vm.SearchText = string.Empty;
        }
    }
}
