using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Quote : BaseClass
    {
        public string Title { get; set; } = string.Empty;
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
        public Guid? JobId { get; set; }
        public Job? Job { get; set; }
        public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
        public decimal TotalAmount { get; set; }
    }
}
