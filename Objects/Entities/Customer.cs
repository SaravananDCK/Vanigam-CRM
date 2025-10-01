using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Customer : LedgerAccount
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CustomerType Type { get; set; } = CustomerType.Company;

        [StringLength(100)]
        public string? Industry { get; set; }

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

        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
