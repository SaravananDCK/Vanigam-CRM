using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class Opportunity : BaseClass
{
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = default!;

    public string Title { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public DateTime ExpectedCloseDate { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}