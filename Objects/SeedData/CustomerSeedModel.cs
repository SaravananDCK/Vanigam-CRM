namespace Vanigam.CRM.Objects.SeedData;

public class CustomerSeedModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Company";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? EmployeeCount { get; set; }
    public string? CustomerSince { get; set; }
    public string Status { get; set; } = "Active";
    public string? Rating { get; set; }
    public string? Description { get; set; }
}