namespace Vanigam.CRM.Objects.SeedData;

public class AccountGroupSeedModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Nature { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public bool IsActive { get; set; } = true;
}
