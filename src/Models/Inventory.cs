using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        [Required, MaxLength(100)]
        public string ProductName { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int ReorderLevel { get; set; }

        [Required]
        public int SupplierID { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int? RoomID { get; set; }
        public virtual Room Room { get; set; }
    }
}