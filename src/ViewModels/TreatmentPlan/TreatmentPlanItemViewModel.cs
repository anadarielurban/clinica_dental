using CommunityToolkit.Mvvm.ComponentModel;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.ViewModels.TreatmentPlan
{
    public class TreatmentPlanItemViewModel : ObservableObject
    {
        public Models.Treatment Treatment { get; }
        
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Subtotal)); // Actualiza el dinero visualmente
            }
        }

        public decimal UnitPrice => Treatment.Price;
        public decimal Subtotal => Quantity * UnitPrice;
        public string TreatmentName => Treatment.Name;

        public TreatmentPlanItemViewModel(Models.Treatment treatment, int quantity)
        {
            Treatment = treatment;
            _quantity = quantity;
        }

        public Models.TreatmentPlanItem ToEntity()
        {
            return new Models.TreatmentPlanItem
            {
                TreatmentID = Treatment.TreatmentID,
                Quantity = Quantity,
                UnitPrice = UnitPrice
            };
        }
    }
}
