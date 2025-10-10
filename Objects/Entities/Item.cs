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

    [JsonDerivedType(typeof(InventoryItem), "#Vanigam.CRM.Objects.Entities.InventoryItem")]
    [JsonDerivedType(typeof(Product), "#Vanigam.CRM.Objects.Entities.Product")]
    [JsonDerivedType(typeof(ServiceItem), "#Vanigam.CRM.Objects.Entities.ServiceItem")]
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization, TypeDiscriminatorPropertyName = "@odata.type")]
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

        public Guid? TaxCodeId { get; set; }

        [ForeignKey(nameof(TaxCodeId))]
        public TaxCode? TaxCode { get; set; }

        public decimal UnitPrice { get; set; }  // price charged to customer
        public decimal? Cost { get; set; }

        public Guid? CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public ItemCategory? Category { get; set; }

        public Guid? LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        /// <summary>
        /// Standard warranty period offered with this product (in months)
        /// </summary>
        public int? WarrantyPeriodMonths { get; set; }

        /// <summary>
        /// Extended guarantee period (in months)
        /// </summary>
        public int? GuaranteePeriodMonths { get; set; }

        /// <summary>
        /// Can this product be covered under AMC?
        /// False for: Software licenses, consumables, cloud services
        /// </summary>
        public bool IsAmcEligible { get; set; } = true;

        /// <summary>
        /// Annual AMC rate for this product (if eligible)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmcAnnualRate { get; set; }

        /// <summary>
        /// Require purchase proof (invoice) for warranty claims
        /// </summary>
        public bool RequiresPurchaseProof { get; set; } = true;

        /// <summary>
        /// Manufacturer/Brand name
        /// </summary>
        [StringLength(200)]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Model number/code
        /// </summary>
        [StringLength(100)]
        public string? ModelNumber { get; set; }
    }
}
