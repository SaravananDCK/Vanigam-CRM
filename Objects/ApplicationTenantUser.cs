using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects;

[Table("AspNetTenantUsers")]
public partial class ApplicationTenantUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? TenantId { get; set; }

    [ForeignKey(nameof(TenantId))]
    public ApplicationTenant? ApplicationTenant { get; set; }
    
    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? ApplicationUser { get; set; }
}
