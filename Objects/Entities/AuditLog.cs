using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class AuditLog : BaseClass
    {
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Data { get; set; }
    }
}
