using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Product : Item
    {

    }
    public class InventoryItem : Item
    {
        public int QuantityOnHand { get; set; }
    }
    public class ServiceItem : Item
    {
        public decimal? HourlyRate { get; set; }
    }

    [JsonDerivedType(typeof(InventoryItem), nameof(ItemType.InventoryItem))]
    [JsonDerivedType(typeof(Product), nameof(ItemType.Product))]
    [JsonDerivedType(typeof(ServiceItem), nameof(ItemType.ServiceItem))]
    public abstract class Item : BaseClass
    {
        protected Item() { }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? SKU { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ItemType Type { get; set; }
        
        public decimal UnitPrice { get; set; }  // price charged to customer
        public decimal? Cost { get; set; }
        
        public Guid? LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }
    }
}
