using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DentalClinicWPF.ViewModels.Dentist;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Views.Dentist
{
    public partial class DentistDetailView : UserControl
    {
        public DentistDetailView(DentistDetailViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        public DentistDetailView()
        {
            InitializeComponent();
        }

        private void Appointment_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DentistDetailViewModel vm)
            {
                if (((FrameworkElement)e.OriginalSource).DataContext is Models.Appointment appointment)
                {
                    vm.OpenAppointmentDetailsCommand.Execute(appointment);
                }
            }
        }
    }
}