using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class MaterialUsage : BaseClass
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;

        public Guid InventoryItemId { get; set; }
        public InventoryItem InventoryItem { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
