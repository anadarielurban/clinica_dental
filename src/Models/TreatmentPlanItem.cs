using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinicWPF.Models
{
    public class TreatmentPlanItem
    {
        [Key]
        public int TreatmentPlanItemID { get; set; }

        public int TreatmentPlanID { get; set; }
        [ForeignKey(nameof(TreatmentPlanID))]
        public virtual TreatmentPlan TreatmentPlan { get; set; }

        public int TreatmentID { get; set; }
        [ForeignKey(nameof(TreatmentID))]
        public virtual Treatment Treatment { get; set; }

        public int Quantity { get; set; } = 1;
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
