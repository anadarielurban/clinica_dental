using System.Windows;
using DentalClinicWPF.ViewModels;

namespace DentalClinicWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    
        private void BtnPresupuestos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var context = new DentalClinicWPF.Data.DentalClinicContext();
                var budgetVM = new DentalClinicWPF.ViewModels.Budget.BudgetViewModel(context);
                var budgetView = new DentalClinicWPF.Views.Budget.BudgetView { DataContext = budgetVM };

                var window = new Window
                {
                    Title = "Nexup Dental - Presupuestos",
                    Content = budgetView,
                    Width = 1100,
                    Height = 750,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = System.Windows.Media.Brushes.White
                };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al abrir presupuestos: " + ex.Message, "Error");
            }
        }
    }
}