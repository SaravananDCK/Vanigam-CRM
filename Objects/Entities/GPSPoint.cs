using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class GPSPoint : BaseClass
    {
        public Guid? TechnicianId { get; set; }

        [ForeignKey(nameof(TechnicianId))]
        public Technician? Technician { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,8)")]
        public double Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(11,8)")]
        public double Longitude { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        [Column(TypeName = "decimal(5,2)")]
        public double? Speed { get; set; }
    }
}
