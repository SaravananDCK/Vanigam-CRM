namespace Vanigam.CRM.Objects.SeedData
{
    public class TaxCodeSeedModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxType { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal UTGSTRate { get; set; }
        public decimal CessRate { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompoundTax { get; set; }
    }
}
