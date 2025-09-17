using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Attributes;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

[NonTenantObject]
public abstract class NamedClass : BaseClass, IName
{
    [Required]
    public string? Name { get; set; }
    public string? Description { get; set; }
}
