using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Notification : BaseClass
    {
        public string Type { get; set; } = string.Empty;
        public string? Message { get; set; }
        public Guid? RecipientUserId { get; set; }
        public bool Sent { get; set; } = false;
    }
}
