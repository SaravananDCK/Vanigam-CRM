using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class Opportunity : BaseClass
{
    [Required]
    public Guid LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public Lead Lead { get; set; } = default!;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedValue { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public DateTime ExpectedCloseDate { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}