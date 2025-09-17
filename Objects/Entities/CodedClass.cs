using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Attributes;

namespace Vanigam.CRM.Objects.Entities;

public interface IName
{
    string? Name { get; set; }
}
[NonTenantObject]
public abstract class CodedClass : BaseClass, IName
{
    [JsonIgnore]
    public string DisplayName => $"{Code}-{Name}";

    [Required]
    public string? Code { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? Description { get; set; }
}
