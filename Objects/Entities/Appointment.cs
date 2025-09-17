using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Appointment : BaseClass
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;

        public Guid? TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    }
}
