using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class Activity : BaseClass
{
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTimeOffset ActivityDate { get; set; } = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

    [Required]
    public ActivityStatus Status { get; set; } = ActivityStatus.Pending;

    [Required]
    public ActivityType Type { get; set; } = ActivityType.Task;

    [StringLength(2000)]
    public string Notes { get; set; } = string.Empty;

    public Guid? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public Lead? Lead { get; set; }

    public Guid? OpportunityId { get; set; }

    [ForeignKey(nameof(OpportunityId))]
    public Opportunity? Opportunity { get; set; }
}