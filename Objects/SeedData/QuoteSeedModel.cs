namespace Vanigam.CRM.Objects.SeedData;

public class QuoteSeedModel
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string? CustomerName { get; set; }
    public string? JobTitle { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CreatedAtUtc { get; set; }
    public List<QuoteItemSeedModel> Items { get; set; } = new();
}

public class QuoteItemSeedModel
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}