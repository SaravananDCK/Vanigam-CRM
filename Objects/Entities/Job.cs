using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Job : BaseClass
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }
        public JobStatus Status { get; set; } = JobStatus.New;
        public Priority Priority { get; set; } = Priority.Normal;
        public Guid? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public Guid? ContactId { get; set; }

        [ForeignKey(nameof(ContactId))]
        public Contact? Contact { get; set; }

        public ICollection<JobAssignment> Assignments { get; set; } = new List<JobAssignment>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
