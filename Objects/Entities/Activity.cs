using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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

    public int? Duration { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityStatus Status { get; set; } = ActivityStatus.Pending;

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityType Type { get; set; } = ActivityType.Task;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Priority Priority { get; set; } = Priority.Normal;

    [StringLength(2000)]
    public string? Outcome { get; set; }

    [StringLength(2000)]
    public string Notes { get; set; } = string.Empty;

    public Guid? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public Lead? Lead { get; set; }

    public Guid? OpportunityId { get; set; }

    [ForeignKey(nameof(OpportunityId))]
    public Opportunity? Opportunity { get; set; }
}