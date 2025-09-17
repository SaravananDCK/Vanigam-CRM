using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class Activity : BaseClass
{
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // Call, Email, Meeting

    [Required]
    [StringLength(2000)]
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public Guid? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public Lead? Lead { get; set; }

    public Guid? OpportunityId { get; set; }

    [ForeignKey(nameof(OpportunityId))]
    public Opportunity? Opportunity { get; set; }
}