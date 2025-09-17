using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class TimeSheet : BaseClass
    {
        public Guid TechnicianId { get; set; }
        public Technician Technician { get; set; } = null!;

        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public decimal Hours => (decimal)(EndAt - StartAt).TotalHours;
    }
}
