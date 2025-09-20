using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Lead : BaseClass
    {
        // Basic Contact Information
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? SecondaryPhone { get; set; }

        // Organization Information
        [StringLength(200)]
        public string? Organization { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(50)]
        public string? CompanySize { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        // Address Information
        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        // Business Context
        [StringLength(200)]
        public string? ProductOfInterest { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedBudget { get; set; }

        [StringLength(100)]
        public string? Timeline { get; set; }

        public int LeadScore { get; set; } = 0;

        // Assignment & Ownership
        public Guid? AssignedToId { get; set; }

        [ForeignKey(nameof(AssignedToId))]
        public ApplicationUser? AssignedTo { get; set; }

        // Lead Source & Marketing
        [StringLength(100)]
        public string? Source { get; set; }

        [StringLength(100)]
        public string? CampaignSource { get; set; }

        [StringLength(200)]
        public string? ReferredBy { get; set; }

        // Social & Professional
        [StringLength(200)]
        public string? LinkedInProfile { get; set; }

        // Status & Classification
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeadStatus Status { get; set; } = LeadStatus.New;

        // Notes & Tracking
        [StringLength(2000)]
        public string? Comments { get; set; }

        public DateTime? LastContactDate { get; set; }

        public DateTime? NextFollowUpDate { get; set; }

        // Navigation Properties
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}
