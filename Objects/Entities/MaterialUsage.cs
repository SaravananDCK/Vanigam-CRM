using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class MaterialUsage : BaseClass
    {
        [Required]
        public Guid JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; } = null!;

        [Required]
        public Guid InventoryItemId { get; set; }

        [ForeignKey(nameof(InventoryItemId))]
        public InventoryItem InventoryItem { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
