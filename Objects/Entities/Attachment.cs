using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Attachment : BaseClass
    {
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Url { get; set; }
        public Guid? JobReportId { get; set; }

        [ForeignKey(nameof(JobReportId))]
        public JobReport? JobReport { get; set; }
    }
}
