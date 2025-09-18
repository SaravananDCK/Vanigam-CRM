using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class JobAssignment : BaseClass
    {
        [Required]
        public Guid JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; } = null!;

        public Guid? TechnicianId { get; set; }

        [ForeignKey(nameof(TechnicianId))]
        public Technician? Technician { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        public DateTime? AssignedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}
