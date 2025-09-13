using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Prescription;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DentalClinicWPF.Views.Prescription
{
    public partial class PrescriptionView : UserControl
    {
        public PrescriptionView()
        {
            InitializeComponent();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Models.Prescription prescription)
            {
                if (DataContext is PrescriptionViewModel viewModel)
                {
                    viewModel.SelectedPrescription = prescription;
                    viewModel.OpenPrescriptionDetailCommand.Execute(null);
                }
            }
        }
    }
}