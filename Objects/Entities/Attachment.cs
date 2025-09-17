using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Attachment : BaseClass
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string? Url { get; set; }
        public Guid? JobReportId { get; set; }
        public JobReport? JobReport { get; set; }
    }
}
