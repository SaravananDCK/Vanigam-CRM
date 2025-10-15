namespace Vanigam.CRM.Objects.SeedData
{
    public class TaxCodeSeedModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxType { get; set; } = string.Empty;
        public double TaxRate { get; set; }
        public double CGSTRate { get; set; }
        public double SGSTRate { get; set; }
        public double IGSTRate { get; set; }
        public double UTGSTRate { get; set; }
        public double CessRate { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompoundTax { get; set; }
    }
}
