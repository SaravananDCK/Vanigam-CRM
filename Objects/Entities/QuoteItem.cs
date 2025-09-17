using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class QuoteItem : BaseClass
    {
        [Required]
        public Guid QuoteId { get; set; }

        [ForeignKey(nameof(QuoteId))]
        public Quote Quote { get; set; } = null!;

        public Guid? InventoryItemId { get; set; }

        [ForeignKey(nameof(InventoryItemId))]
        public InventoryItem? InventoryItem { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}
