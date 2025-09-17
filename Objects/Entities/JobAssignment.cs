using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class JobAssignment : BaseClass
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;

        public Guid? TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        public DateTime? AssignedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}
