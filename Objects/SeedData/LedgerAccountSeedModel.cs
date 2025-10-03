namespace Vanigam.CRM.Objects.SeedData;

public class LedgerAccountSeedModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "LedgerAccount";
    public string GroupCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal CreditLimit { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
