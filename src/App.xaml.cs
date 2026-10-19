using System;
using System.Windows;
using System.Windows.Threading;
using DentalClinicWPF.Data;

namespace DentalClinicWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Captura cualquier excepción no controlada y la muestra
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(
                    "EXCEPCIÓN NO CONTROLADA:\n\n" +
                    args.Exception.GetType().FullName + "\n\n" +
                    args.Exception.Message + "\n\n" +
                    "STACK TRACE:\n" +
                    args.Exception.StackTrace,
                    "Error en la aplicación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };

            try
            {
                using var context = new DentalClinicContext();
                context.Database.EnsureCreated();
        context.EnsureMedicalHistorySchema();
            context.EnsureTreatmentPlanSchema();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al inicializar la base de datos:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}



