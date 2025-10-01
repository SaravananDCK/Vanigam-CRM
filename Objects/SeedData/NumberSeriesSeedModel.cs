namespace Vanigam.CRM.Objects.SeedData;

public class NumberSeriesSeedModel
{
    public string EntityType { get; set; } = string.Empty;
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public long StartNo { get; set; } = 1;
    public long CurrentNo { get; set; } = 1;
    public int PaddingLength { get; set; } = 4;
    public bool IsActive { get; set; } = true;
}
