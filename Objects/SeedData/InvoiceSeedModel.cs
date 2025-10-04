namespace Vanigam.CRM.Objects.SeedData
{
    public class InvoiceSeedModel
    {
        public string Number { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<InvoiceItemSeedModel> Items { get; set; } = new List<InvoiceItemSeedModel>();
    }
}
