namespace Vanigam.CRM.Objects.Helpers;

public class VanigamAccountingOptions
{
    public const string LiveConfiguration = "Live";
    public string Configuration { get; set; }
    public int? AutoLogoutMinutes { get; set; }
}
