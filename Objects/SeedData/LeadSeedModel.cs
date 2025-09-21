namespace Vanigam.CRM.Objects.SeedData
{
    public class LeadSeedModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? SecondaryPhone { get; set; }
        public string? Organization { get; set; }
        public string? JobTitle { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? ProductOfInterest { get; set; }
        public decimal? EstimatedBudget { get; set; }
        public string? Timeline { get; set; }
        public int LeadScore { get; set; }
        public string? Source { get; set; }
        public string? CampaignSource { get; set; }
        public string? ReferredBy { get; set; }
        public string? LinkedInProfile { get; set; }
        public string Status { get; set; } = "New";
        public string? Comments { get; set; }
        public string? LastContactDate { get; set; }
        public string? NextFollowUpDate { get; set; }
    }
}