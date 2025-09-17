using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class RecurringJob : BaseClass
    {
        public string Name { get; set; } = string.Empty;
        public Guid ContractId { get; set; }
        public Contract Contract { get; set; } = null!;
        public string CronExpression { get; set; } = string.Empty;
    }
}
