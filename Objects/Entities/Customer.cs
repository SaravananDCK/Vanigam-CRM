using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Customer : BaseClass
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CustomerType Type { get; set; } = CustomerType.Company;

        [StringLength(100)]
        public string? Industry { get; set; }

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

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(500)]
        [Url]
        public string? Website { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AnnualRevenue { get; set; }

        public int? EmployeeCount { get; set; }

        public DateTimeOffset? CustomerSince { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

        [StringLength(10)]
        public string? Rating { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
