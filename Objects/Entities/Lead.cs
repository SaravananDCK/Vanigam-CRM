using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Lead : BaseClass
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;
        [StringLength(200)] public string? Email { get; set; }
        [StringLength(20)] public string? Phone { get; set; }
        [StringLength(100)] public string? Source { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeadStatus Status { get; set; } = LeadStatus.New;
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}
