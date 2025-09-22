namespace Vanigam.CRM.Objects.SeedData;

public class OpportunitySeedModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal EstimatedValue { get; set; }
    public string? ExpectedCloseDate { get; set; }
    public string Stage { get; set; } = "Prospecting";
    public int Probability { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public string? CreatedAtUtc { get; set; }
}