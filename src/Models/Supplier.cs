using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClinicWPF.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required, MaxLength(100)]
        public string CompanyName { get; set; }

        [MaxLength(100)]
        public string ContactName { get; set; }

        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; }
    }
}