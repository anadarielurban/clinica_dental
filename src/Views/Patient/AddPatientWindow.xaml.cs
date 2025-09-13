using System.Windows;

namespace DentalClinicWPF.Views.Patient
{
    public partial class AddPatientWindow : Window
    {
        public AddPatientWindow()
        {
            InitializeComponent();
        }
        
        private void OnSaveSuccess()
        {
            this.DialogResult = true;
        }
        
    }
}