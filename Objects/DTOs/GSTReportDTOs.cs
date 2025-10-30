using System.ComponentModel.DataAnnotations;

namespace Vanigam.CRM.Objects.DTOs;

#region GSTR-1 DTOs

/// <summary>
/// Main GSTR-1 Return structure
/// </summary>
public class GSTR1Report
{
    public string GSTIN { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ReturnPeriod { get; set; } = string.Empty; // MMYYYY format
    public List<B2BInvoice> B2BInvoices { get; set; } = new();
    public List<B2CLInvoice> B2CLInvoices { get; set; } = new();
    public List<B2CSInvoice> B2CSInvoices { get; set; } = new();
    public List<ExportInvoice> ExportInvoices { get; set; } = new();
    public List<CreditDebitNote> CreditNotes { get; set; } = new();
    public List<CreditDebitNote> DebitNotes { get; set; } = new();
    public List<HSNSummary> HSNSummary { get; set; } = new();
    public List<DocumentIssued> DocumentsIssued { get; set; } = new();
}

/// <summary>
/// B2B (Business to Business) Invoice - Turnover > 2.5 lakh
/// </summary>
public class B2BInvoice
{
    public string GSTIN { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal InvoiceValue { get; set; }
    public string PlaceOfSupply { get; set; } = string.Empty; // State code
    public bool ReverseCharge { get; set; } = false;
    public string InvoiceType { get; set; } = "Regular"; // Regular, SEZ, Deemed Export
    public decimal TaxableValue { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
}

/// <summary>
/// B2CL (Business to Consumer Large) - Invoice value > 2.5 lakh
/// </summary>
public class B2CLInvoice
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal InvoiceValue { get; set; }
    public string PlaceOfSupply { get; set; } = string.Empty;
    public decimal TaxableValue { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
}

/// <summary>
/// B2CS (Business to Consumer Small) - Invoice value <= 2.5 lakh - State-wise summary
/// </summary>
public class B2CSInvoice
{
    public string PlaceOfSupply { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
}

/// <summary>
/// Export Invoice
/// </summary>
public class ExportInvoice
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal InvoiceValue { get; set; }
    public string ExportType { get; set; } = "WPAY"; // WPAY (with payment), WOPAY (without payment)
    public string ShippingPortCode { get; set; } = string.Empty;
    public string ShippingBillNumber { get; set; } = string.Empty;
    public DateTime? ShippingBillDate { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
}

/// <summary>
/// Credit/Debit Notes
/// </summary>
public class CreditDebitNote
{
    public string GSTIN { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string NoteNumber { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    public string NoteType { get; set; } = "C"; // C = Credit, D = Debit
    public string PlaceOfSupply { get; set; } = string.Empty;
    public bool ReverseCharge { get; set; } = false;
    public string OriginalInvoiceNumber { get; set; } = string.Empty;
    public DateTime OriginalInvoiceDate { get; set; }
    public decimal NoteValue { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
}

/// <summary>
/// Documents Issued summary
/// </summary>
public class DocumentIssued
{
    public string DocumentType { get; set; } = string.Empty; // Invoice, Credit Note, Debit Note
    public string SeriesCode { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public string ToNumber { get; set; } = string.Empty;
    public int TotalNumber { get; set; }
    public int Cancelled { get; set; }
    public int NetIssued { get; set; }
}

#endregion

#region HSN Summary DTOs

/// <summary>
/// HSN/SAC Summary for GSTR-1
/// </summary>
public class HSNSummary
{
    public string HSNCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UQC { get; set; } = string.Empty; // Unit Quantity Code
    public decimal TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
    public decimal TaxRate { get; set; }
}

#endregion

#region GSTR-3B DTOs

/// <summary>
/// Main GSTR-3B Return structure (Summary return)
/// </summary>
public class GSTR3BReport
{
    public string GSTIN { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string ReturnPeriod { get; set; } = string.Empty; // MMYYYY format

    // Table 3.1 - Outward supplies
    public OutwardSupplies OutwardSupplies { get; set; } = new();

    // Table 3.2 - Inter-state supplies
    public List<InterStateSupply> InterStateSupplies { get; set; } = new();

    // Table 4 - ITC Available
    public ITCAvailable ITCAvailable { get; set; } = new();

    // Table 5 - Values of exempt, nil rated and non-GST supplies
    public ExemptSupplies ExemptSupplies { get; set; } = new();

    // Table 6.1 - Tax payable
    public TaxPayable TaxPayable { get; set; } = new();
}

public class OutwardSupplies
{
    public decimal TaxableValue { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
}

public class InterStateSupply
{
    public string PlaceOfSupply { get; set; } = string.Empty;
    public decimal TaxableValue { get; set; }
    public decimal IGSTAmount { get; set; }
}

public class ITCAvailable
{
    // Import of goods
    public TaxAmounts ImportOfGoods { get; set; } = new();

    // Import of services
    public TaxAmounts ImportOfServices { get; set; } = new();

    // Inward supplies from ISD
    public TaxAmounts InwardFromISD { get; set; } = new();

    // All other ITC
    public TaxAmounts AllOtherITC { get; set; } = new();

    // Total ITC available
    public TaxAmounts TotalITC { get; set; } = new();

    // ITC Reversed
    public TaxAmounts ITCReversed { get; set; } = new();

    // Net ITC Available
    public TaxAmounts NetITC { get; set; } = new();

    // Ineligible ITC
    public TaxAmounts IneligibleITC { get; set; } = new();
}

public class TaxAmounts
{
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
}

public class ExemptSupplies
{
    public decimal NilRated { get; set; }
    public decimal Exempted { get; set; }
    public decimal NonGST { get; set; }
}

public class TaxPayable
{
    public TaxAmounts TaxPayable { get; set; } = new();
    public TaxAmounts TaxPaidThroughITC { get; set; } = new();
    public TaxAmounts TaxPaidInCash { get; set; } = new();
    public decimal Interest { get; set; }
    public decimal LateFee { get; set; }
}

#endregion

#region Tax Liability Report DTOs

/// <summary>
/// Tax Liability Summary Report
/// </summary>
public class TaxLiabilitySummary
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public decimal TaxableSales { get; set; }
    public decimal ExemptSales { get; set; }
    public decimal CGSTPayable { get; set; }
    public decimal SGSTPayable { get; set; }
    public decimal IGSTPayable { get; set; }
    public decimal CessPayable { get; set; }
    public decimal TotalTaxPayable { get; set; }
    public List<TaxLiabilityDetail> Details { get; set; } = new();
}

public class TaxLiabilityDetail
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string GSTIN { get; set; } = string.Empty;
    public decimal TaxableValue { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
    public decimal TotalTax { get; set; }
}

#endregion

#region ITC Report DTOs

/// <summary>
/// Input Tax Credit Summary Report
/// </summary>
public class ITCReportSummary
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public decimal TaxablePurchases { get; set; }
    public decimal CGSTInput { get; set; }
    public decimal SGSTInput { get; set; }
    public decimal IGSTInput { get; set; }
    public decimal CessInput { get; set; }
    public decimal TotalITCAvailable { get; set; }
    public decimal ITCUtilized { get; set; }
    public decimal ITCBalance { get; set; }
    public List<ITCDetail> Details { get; set; } = new();
}

public class ITCDetail
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string GSTIN { get; set; } = string.Empty;
    public decimal TaxableValue { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CessAmount { get; set; }
    public decimal TotalITC { get; set; }
    public bool IsEligible { get; set; } = true;
}

#endregion
