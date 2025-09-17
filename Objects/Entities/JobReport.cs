using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class JobReport : BaseClass
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;
        public string? Notes { get; set; }
        public string? SignatureBase64 { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
