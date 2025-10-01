using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class NumberSeries : BaseClass
{
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Prefix { get; set; }

    [StringLength(20)]
    public string? Suffix { get; set; }

    public long StartNo { get; set; } = 1;

    public long CurrentNo { get; set; } = 1;

    public int PaddingLength { get; set; } = 4;

    public bool IsActive { get; set; } = true;
}
