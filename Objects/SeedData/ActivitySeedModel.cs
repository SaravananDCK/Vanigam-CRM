namespace Vanigam.CRM.Objects.SeedData;

public class ActivitySeedModel
{
    public string Type { get; set; } = "PhoneCall";
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ActivityDate { get; set; }
    public int? Duration { get; set; }
    public string Status { get; set; } = "Pending";
    public string Priority { get; set; } = "Medium";
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
    public string? CreatedAtUtc { get; set; }
}