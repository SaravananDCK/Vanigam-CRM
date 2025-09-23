namespace Vanigam.CRM.Objects.SeedData;

public class ProductSeedModel
{
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string Type { get; set; } = "Product";
    public decimal UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public string? CreatedAtUtc { get; set; }
}