using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DentalClinicWPF.ViewModels.Patient.PatientDetail;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Views.Patient.PatientDetail
{
    public partial class PatientDetailView : UserControl
    {
        public PatientDetailView()
        {
            InitializeComponent();
        }
    
        private void OpenPrescription_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is Models.Prescription prescription)
            {
                if (DataContext is ViewModels.Patient.PatientDetail.PatientDetailViewModel vm)
                {
                    vm.OpenPrescription(prescription);
                }
                else
                {
                    System.Windows.MessageBox.Show("Error: El DataContext no es el ViewModel esperado.", "Error");
                }
            }
        }}

        }