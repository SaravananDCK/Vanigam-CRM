using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Attributes;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

[NonTenantObject]
public abstract class NamedClass : BaseClass, IName
{
    [Required]
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
}
