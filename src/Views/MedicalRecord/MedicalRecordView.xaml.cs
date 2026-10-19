using System.Windows;
using System.Windows.Controls;
using DentalClinicWPF.ViewModels.MedicalRecord;
namespace DentalClinicWPF.Views.MedicalRecord {
    public partial class MedicalRecordView : UserControl {
        public MedicalRecordView() { InitializeComponent(); }
        private void ClearSearch_Click(object sender, RoutedEventArgs e) { if (DataContext is MedicalRecordViewModel vm) vm.SearchText = string.Empty; }
    }
}
