namespace Vanigam.CRM.Objects.SeedData;

public class InventoryItemSeedModel
{
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string Type { get; set; } = "InventoryItem";
    public decimal UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public int QuantityOnHand { get; set; }
    public string? CreatedAtUtc { get; set; }
}