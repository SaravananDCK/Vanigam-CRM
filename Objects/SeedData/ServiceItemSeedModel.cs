namespace Vanigam.CRM.Objects.SeedData;

public class ServiceItemSeedModel
{
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string Type { get; set; } = "ServiceItem";
    public decimal UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? CreatedAtUtc { get; set; }
}