using System;
using System.Linq;
using System.Windows;
using DentalClinicWPF.Data;

namespace DentalClinicWPF.Views.Inventory
{
    public partial class AddStockWindow : Window
    {
        private readonly DentalClinicContext _context;

        public AddStockWindow(DentalClinicContext context)
        {
            InitializeComponent();
            _context = context;
            LoadProducts();
        }

        private void LoadProducts()
        {
            var products = _context.Inventory.OrderBy(i => i.ProductName).ToList();
            ProductCombo.ItemsSource = products;
            if (products.Count > 0)
                ProductCombo.SelectedIndex = 0;
        }

        private void ProductCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProductCombo.SelectedItem is Models.Inventory inv)
            {
                CurrentQtyText.Text = $"📊 Cantidad actual: {inv.Quantity} unidades";
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCombo.SelectedItem is not Models.Inventory inv)
            {
                MessageBox.Show("Selecciona un producto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida (mayor a 0).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                inv.Quantity += cantidad;
                _context.SaveChanges();

                MessageBox.Show(
                    $"✅ +{cantidad} unidades a '{inv.ProductName}'.\nNuevo total: {inv.Quantity} unidades",
                    "Stock actualizado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar stock:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
