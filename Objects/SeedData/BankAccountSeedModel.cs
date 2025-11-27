namespace Vanigam.CRM.Objects.SeedData;

public class BankAccountSeedModel
{
    public string Code { get; set; } = string.Empty;
    public string AccountType { get; set; } = "BankAccount";
    public string AccountGroupCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Currency { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? IFSCCode { get; set; }
    public string? BranchName { get; set; }
    public string? SwiftCode { get; set; }
}
