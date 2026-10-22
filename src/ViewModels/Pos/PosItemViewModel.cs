using CommunityToolkit.Mvvm.ComponentModel;

namespace DentalClinicWPF.ViewModels.Pos
{
    public class PosItemViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsInventory { get; set; } // true = producto, false = tratamiento
        public decimal UnitPrice { get; set; }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => Quantity * UnitPrice;
    }
}
