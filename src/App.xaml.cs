using System.Windows;
using DentalClinicWPF.Data;

namespace DentalClinicWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var context = new DentalClinicContext();
            context.Database.EnsureCreated();
        }
    }
}
