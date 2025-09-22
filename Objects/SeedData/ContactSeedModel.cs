namespace Vanigam.CRM.Objects.SeedData;

public class ContactSeedModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsPrimary { get; set; } = false;
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
    public string? CreatedAtUtc { get; set; }
}