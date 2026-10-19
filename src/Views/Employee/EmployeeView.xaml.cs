using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.Employee;
namespace DentalClinicWPF.Views.Employee {
    public partial class EmployeeView : UserControl {
        public EmployeeView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e) { if (DataContext is EmployeeViewModel vm) vm.SearchText = string.Empty; }
    }
}
