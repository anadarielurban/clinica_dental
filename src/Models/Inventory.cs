using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        [Required]
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Price { get; set; }

        public int ReorderLevel { get; set; }

        public int? SupplierID { get; set; }
        [ForeignKey(nameof(SupplierID))]
        public virtual Supplier? Supplier { get; set; }

        public int? RoomID { get; set; }
        [ForeignKey(nameof(RoomID))]
        public virtual Room? Room { get; set; }
    }
}
