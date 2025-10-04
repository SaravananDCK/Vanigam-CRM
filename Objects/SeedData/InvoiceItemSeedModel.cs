namespace Vanigam.CRM.Objects.SeedData
{
    public class InvoiceItemSeedModel
    {
        public int LineNumber { get; set; }
        public string? Description { get; set; }
        public string? ItemCode { get; set; } // Optional - to lookup Item by Code
        public double Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxPercent { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal LineTotal { get; set; }
        public string? Unit { get; set; }
    }
}
