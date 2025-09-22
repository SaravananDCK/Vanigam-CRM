using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class Opportunity : BaseClass
{
    public Guid? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public Lead? Lead { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Source { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedValue { get; set; }

    [Range(0, 100)]
    public int Probability { get; set; } = 50;

    // Notes & Tracking
    [StringLength(2000)]
    public string? Comments { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public DateTimeOffset? ExpectedCloseDate { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}