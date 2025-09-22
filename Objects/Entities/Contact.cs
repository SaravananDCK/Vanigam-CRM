using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Contact : BaseClass
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Mobile { get; set; }

        [StringLength(500)]
        [Url]
        public string? LinkedInProfile { get; set; }

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

        public bool IsPrimary { get; set; } = false;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContactStatus Status { get; set; } = ContactStatus.Active;

        [StringLength(2000)]
        public string? Notes { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContactType Type { get; set; } = ContactType.Individual;

        public Guid? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }
    }
}
