using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class GPSPoint : BaseClass
    {
        public Guid? TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public double? Speed { get; set; }
    }
}
