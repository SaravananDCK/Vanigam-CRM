using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class Activity : BaseClass
{
    public string Type { get; set; } = string.Empty; // Call, Email, Meeting
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
}