using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Payment : BaseClass
    {
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime PaidAt { get; set; }
        public string? ProviderReference { get; set; }
    }
}
