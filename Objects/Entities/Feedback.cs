using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Feedback : BaseClass
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comments { get; set; }
    }
}
