using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class TimeSheet : BaseClass
    {
        [Required]
        public Guid TechnicianId { get; set; }

        [ForeignKey(nameof(TechnicianId))]
        public Technician Technician { get; set; } = null!;

        [Required]
        public Guid JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; } = null!;

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal Hours => (decimal)(EndAt - StartAt).TotalHours;
    }
}
