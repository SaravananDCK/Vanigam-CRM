using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Job : Voucher
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JobStatus Status { get; set; } = JobStatus.Pending;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Priority Priority { get; set; } = Priority.Normal;

        public Guid? ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public Contact? Contact { get; set; }

        public ICollection<JobAssignment> Assignments { get; set; } = new List<JobAssignment>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<JobReport> JobReports { get; set; } = new List<JobReport>();
        public ICollection<MaterialUsage> MaterialUsages { get; set; } = new List<MaterialUsage>();
    }
}
