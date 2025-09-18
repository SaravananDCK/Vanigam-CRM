using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects;

public class ApplicationRole : IdentityRole<Guid>
{
    public const string SuperUserRole = nameof(SuperUserRole);
    public const string AdminRole = nameof(AdminRole);

    public static string[] AdministratorRoles =>
        new[]
        {
            AdminRole,
            SuperUserRole
        };
   

    [JsonIgnore] 
    public ICollection<ApplicationUser>? Users { get; set; }
    
    public int? TenantId { get; set; }

    [ForeignKey(nameof(TenantId))]
    public ApplicationTenant? ApplicationTenant { get; set; }
}
