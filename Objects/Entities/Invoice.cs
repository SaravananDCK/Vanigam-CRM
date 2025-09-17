using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Invoice : BaseClass
    {
        public string Number { get; set; } = string.Empty;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public Guid? JobId { get; set; }
        public Job? Job { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
