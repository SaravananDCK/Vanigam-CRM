using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects;

[Table("AspNetTenants")]
public partial class ApplicationTenant: IName
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public ICollection<ApplicationUser>? Users { get; set; }

    public ICollection<ApplicationRole>? Roles { get; set; }

    public string? Name { get; set; }

    public string? Hosts { get; set; }
    public ICollection<ApplicationTenantUser> ApplicationTenantUsers { get; } = [];
}
