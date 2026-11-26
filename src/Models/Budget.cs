using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DentalClinicWPF.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }
        public string Folio { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        public string Status { get; set; } = "Prospecto";
        public decimal ExchangeRate { get; set; } = 18.00m;
        public string Observations { get; set; } = "";
        
        // --- CAMPOS DE TOTALES Y PAGOS ---
        public decimal Deposit { get; set; } // Anticipo
        public decimal TotalMXN { get; set; }
        public decimal TotalUSD { get; set; }
        public int NumberOfPayments { get; set; } = 1;

        public virtual ObservableCollection<BudgetItem> Items { get; set; } = new ObservableCollection<BudgetItem>();
    }

    public class BudgetItem : INotifyPropertyChanged
    {
        [Key]
        public int BudgetItemId { get; set; }
        public int BudgetId { get; set; }
        [ForeignKey("BudgetId")]
        public virtual Budget Budget { get; set; }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalMXN)); OnPropertyChanged(nameof(TotalUSD)); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private decimal _unitPriceMXN;
        public decimal UnitPriceMXN
        {
            get => _unitPriceMXN;
            set { _unitPriceMXN = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalMXN)); OnPropertyChanged(nameof(TotalUSD)); }
        }

        private decimal _discountPercentage;
        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            set { _discountPercentage = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalMXN)); OnPropertyChanged(nameof(TotalUSD)); }
        }
        
        [NotMapped]
        public decimal TotalMXN => (Quantity * UnitPriceMXN) * (1 - (DiscountPercentage / 100));
        [NotMapped]
        public decimal UnitPriceUSD => Budget != null && Budget.ExchangeRate > 0 ? UnitPriceMXN / Budget.ExchangeRate : 0;
        [NotMapped]
        public decimal TotalUSD => Budget != null && Budget.ExchangeRate > 0 ? TotalMXN / Budget.ExchangeRate : 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
