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
    }
}