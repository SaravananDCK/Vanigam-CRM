using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class QuoteItem : BaseClass
    {
        public Guid QuoteId { get; set; }
        public Quote Quote { get; set; } = null!;
        public Guid? InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
