using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Contract : BaseClass
    {
        public string Title { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Terms { get; set; }
    }
}
