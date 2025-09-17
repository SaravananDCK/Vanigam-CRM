using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class InventoryItem : BaseClass
    {
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public int QuantityOnHand { get; set; }
        public Guid? LocationId { get; set; }
    }
}
