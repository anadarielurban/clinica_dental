using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.TreatmentPlan;
namespace DentalClinicWPF.Views.TreatmentPlan {
    public partial class TreatmentPlanView : UserControl {
        public TreatmentPlanView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e) { if (DataContext is TreatmentPlanViewModel vm) vm.SearchText = string.Empty; }
    }
}
