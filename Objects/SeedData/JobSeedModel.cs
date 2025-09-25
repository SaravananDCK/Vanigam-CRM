namespace Vanigam.CRM.Objects.SeedData;

public class JobSeedModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string Priority { get; set; } = "Medium";
    public string? CreatedAtUtc { get; set; }
    public string? CustomerName { get; set; }
}