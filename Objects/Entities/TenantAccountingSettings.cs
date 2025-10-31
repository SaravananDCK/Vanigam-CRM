using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

/// <summary>
/// Stores tenant-specific accounting configuration and default ledger accounts.
/// Used by LedgerPostingService to automatically select correct accounts for transactions.
/// This allows each tenant to have their own chart of accounts and posting rules.
/// </summary>
public class TenantAccountingSettings : BaseClass
{
    #region Sales & Revenue Accounts

    public Guid? DefaultSalesAccountId { get; set; }

    [ForeignKey(nameof(DefaultSalesAccountId))]
    public LedgerAccount? DefaultSalesAccount { get; set; }

    public Guid? SalesReturnAccountId { get; set; }

    [ForeignKey(nameof(SalesReturnAccountId))]
    public LedgerAccount? SalesReturnAccount { get; set; }

    public Guid? SalesDiscountAccountId { get; set; }

    [ForeignKey(nameof(SalesDiscountAccountId))]
    public LedgerAccount? SalesDiscountAccount { get; set; }

    #endregion

    #region Purchase & Expense Accounts

    public Guid? DefaultPurchasesAccountId { get; set; }

    [ForeignKey(nameof(DefaultPurchasesAccountId))]
    public LedgerAccount? DefaultPurchasesAccount { get; set; }

    public Guid? PurchaseReturnAccountId { get; set; }

    [ForeignKey(nameof(PurchaseReturnAccountId))]
    public LedgerAccount? PurchaseReturnAccount { get; set; }

    public Guid? PurchaseDiscountAccountId { get; set; }

    [ForeignKey(nameof(PurchaseDiscountAccountId))]
    public LedgerAccount? PurchaseDiscountAccount { get; set; }

    #endregion

    #region Tax Payable Accounts (Output Tax - Sales)

    public Guid? DefaultTaxPayableAccountId { get; set; }

    [ForeignKey(nameof(DefaultTaxPayableAccountId))]
    public LedgerAccount? DefaultTaxPayableAccount { get; set; }

    public Guid? DefaultSGSTPayableAccountId { get; set; }

    [ForeignKey(nameof(DefaultSGSTPayableAccountId))]
    public LedgerAccount? DefaultSGSTPayableAccount { get; set; }

    public Guid? DefaultCGSTPayableAccountId { get; set; }

    [ForeignKey(nameof(DefaultCGSTPayableAccountId))]
    public LedgerAccount? DefaultCGSTPayableAccount { get; set; }

    public Guid? DefaultIGSTPayableAccountId { get; set; }

    [ForeignKey(nameof(DefaultIGSTPayableAccountId))]
    public LedgerAccount? DefaultIGSTPayableAccount { get; set; }

    #endregion

    #region Tax Input Accounts (Input Tax - Purchase)

    public Guid? DefaultTaxInputAccountId { get; set; }

    [ForeignKey(nameof(DefaultTaxInputAccountId))]
    public LedgerAccount? DefaultTaxInputAccount { get; set; }

    public Guid? DefaultSGSTInputAccountId { get; set; }

    [ForeignKey(nameof(DefaultSGSTInputAccountId))]
    public LedgerAccount? DefaultSGSTInputAccount { get; set; }

    public Guid? DefaultCGSTInputAccountId { get; set; }

    [ForeignKey(nameof(DefaultCGSTInputAccountId))]
    public LedgerAccount? DefaultCGSTInputAccount { get; set; }

    public Guid? DefaultIGSTInputAccountId { get; set; }

    [ForeignKey(nameof(DefaultIGSTInputAccountId))]
    public LedgerAccount? DefaultIGSTInputAccount { get; set; }

    #endregion

    #region GST Cess Accounts

    public Guid? CessPayableAccountId { get; set; }

    [ForeignKey(nameof(CessPayableAccountId))]
    public LedgerAccount? CessPayableAccount { get; set; }

    public Guid? CessInputAccountId { get; set; }

    [ForeignKey(nameof(CessInputAccountId))]
    public LedgerAccount? CessInputAccount { get; set; }

    #endregion

    #region TDS/TCS Accounts

    public Guid? DefaultTDSPayableAccountId { get; set; }

    [ForeignKey(nameof(DefaultTDSPayableAccountId))]
    public LedgerAccount? DefaultTDSPayableAccount { get; set; }

    public Guid? DefaultTCSPayableAccountId { get; set; }

    [ForeignKey(nameof(DefaultTCSPayableAccountId))]
    public LedgerAccount? DefaultTCSPayableAccount { get; set; }

    public Guid? TDSReceivableAccountId { get; set; }

    [ForeignKey(nameof(TDSReceivableAccountId))]
    public LedgerAccount? TDSReceivableAccount { get; set; }

    #endregion

    #region Payment Method Accounts

    public Guid? DefaultCashAccountId { get; set; }

    [ForeignKey(nameof(DefaultCashAccountId))]
    public LedgerAccount? DefaultCashAccount { get; set; }

    public Guid? DefaultBankAccountId { get; set; }

    [ForeignKey(nameof(DefaultBankAccountId))]
    public LedgerAccount? DefaultBankAccount { get; set; }

    public Guid? DefaultCardAccountId { get; set; }

    [ForeignKey(nameof(DefaultCardAccountId))]
    public LedgerAccount? DefaultCardAccount { get; set; }

    public Guid? DefaultUpiAccountId { get; set; }

    [ForeignKey(nameof(DefaultUpiAccountId))]
    public LedgerAccount? DefaultUpiAccount { get; set; }

    #endregion

    #region Receivables & Payables Accounts

    public Guid? DefaultReceivableAccountId { get; set; }

    [ForeignKey(nameof(DefaultReceivableAccountId))]
    public LedgerAccount? DefaultReceivableAccount { get; set; }

    public Guid? DefaultPayableAccountId { get; set; }

    [ForeignKey(nameof(DefaultPayableAccountId))]
    public LedgerAccount? DefaultPayableAccount { get; set; }

    #endregion

    #region Advance & Deposit Accounts

    public Guid? AdvanceReceivedAccountId { get; set; }

    [ForeignKey(nameof(AdvanceReceivedAccountId))]
    public LedgerAccount? AdvanceReceivedAccount { get; set; }

    public Guid? AdvancePaidAccountId { get; set; }

    [ForeignKey(nameof(AdvancePaidAccountId))]
    public LedgerAccount? AdvancePaidAccount { get; set; }

    public Guid? SecurityDepositReceivableAccountId { get; set; }

    [ForeignKey(nameof(SecurityDepositReceivableAccountId))]
    public LedgerAccount? SecurityDepositReceivableAccount { get; set; }

    public Guid? SecurityDepositPayableAccountId { get; set; }

    [ForeignKey(nameof(SecurityDepositPayableAccountId))]
    public LedgerAccount? SecurityDepositPayableAccount { get; set; }

    #endregion

    #region Bad Debt & Write-off Accounts

    public Guid? BadDebtAccountId { get; set; }

    [ForeignKey(nameof(BadDebtAccountId))]
    public LedgerAccount? BadDebtAccount { get; set; }

    public Guid? ProvisionForBadDebtAccountId { get; set; }

    [ForeignKey(nameof(ProvisionForBadDebtAccountId))]
    public LedgerAccount? ProvisionForBadDebtAccount { get; set; }

    public Guid? WriteOffAccountId { get; set; }

    [ForeignKey(nameof(WriteOffAccountId))]
    public LedgerAccount? WriteOffAccount { get; set; }

    #endregion

    #region Income & Expense Accounts

    public Guid? InterestIncomeAccountId { get; set; }

    [ForeignKey(nameof(InterestIncomeAccountId))]
    public LedgerAccount? InterestIncomeAccount { get; set; }

    public Guid? InterestExpenseAccountId { get; set; }

    [ForeignKey(nameof(InterestExpenseAccountId))]
    public LedgerAccount? InterestExpenseAccount { get; set; }

    public Guid? LateFeeIncomeAccountId { get; set; }

    [ForeignKey(nameof(LateFeeIncomeAccountId))]
    public LedgerAccount? LateFeeIncomeAccount { get; set; }

    public Guid? LateFeeExpenseAccountId { get; set; }

    [ForeignKey(nameof(LateFeeExpenseAccountId))]
    public LedgerAccount? LateFeeExpenseAccount { get; set; }

    #endregion

    #region Bank & Financial Accounts

    public Guid? BankChargesAccountId { get; set; }

    [ForeignKey(nameof(BankChargesAccountId))]
    public LedgerAccount? BankChargesAccount { get; set; }

    public Guid? PaymentGatewayChargesAccountId { get; set; }

    [ForeignKey(nameof(PaymentGatewayChargesAccountId))]
    public LedgerAccount? PaymentGatewayChargesAccount { get; set; }

    public Guid? UndepositedFundsAccountId { get; set; }

    [ForeignKey(nameof(UndepositedFundsAccountId))]
    public LedgerAccount? UndepositedFundsAccount { get; set; }

    #endregion

    #region Inventory & WIP Accounts

    public Guid? DefaultInventoryAccountId { get; set; }

    [ForeignKey(nameof(DefaultInventoryAccountId))]
    public LedgerAccount? DefaultInventoryAccount { get; set; }

    public Guid? WorkInProgressAccountId { get; set; }

    [ForeignKey(nameof(WorkInProgressAccountId))]
    public LedgerAccount? WorkInProgressAccount { get; set; }

    public Guid? CostOfGoodsSoldAccountId { get; set; }

    [ForeignKey(nameof(CostOfGoodsSoldAccountId))]
    public LedgerAccount? CostOfGoodsSoldAccount { get; set; }

    #endregion

    #region Additional Accounts

    public Guid? RoundingAccountId { get; set; }

    [ForeignKey(nameof(RoundingAccountId))]
    public LedgerAccount? RoundingAccount { get; set; }

    public Guid? ExchangeGainLossAccountId { get; set; }

    [ForeignKey(nameof(ExchangeGainLossAccountId))]
    public LedgerAccount? ExchangeGainLossAccount { get; set; }

    public Guid? FreightChargesAccountId { get; set; }

    [ForeignKey(nameof(FreightChargesAccountId))]
    public LedgerAccount? FreightChargesAccount { get; set; }

    public Guid? PackingChargesAccountId { get; set; }

    [ForeignKey(nameof(PackingChargesAccountId))]
    public LedgerAccount? PackingChargesAccount { get; set; }

    #endregion

    #region Company Information

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(500)]
    public string? CompanyAddress { get; set; }

    [StringLength(100)]
    public string? CompanyCity { get; set; }

    [StringLength(100)]
    public string? CompanyState { get; set; }

    [StringLength(20)]
    public string? CompanyPostalCode { get; set; }

    [StringLength(100)]
    public string? CompanyCountry { get; set; }

    [StringLength(50)]
    public string? CompanyPhone { get; set; }

    [StringLength(100)]
    public string? CompanyEmail { get; set; }

    [StringLength(200)]
    public string? CompanyWebsite { get; set; }

    [StringLength(50)]
    public string? CompanyTaxId { get; set; }

    [StringLength(50)]
    public string? CompanyRegistrationNumber { get; set; }

    public byte[]? CompanyLogo { get; set; }

    #endregion

    #region GST Registration Information

    /// <summary>
    /// GST Identification Number (15-character alphanumeric)
    /// Format: 22AAAAA0000A1Z5 (State Code + PAN + Entity Number + Z + Checksum)
    /// </summary>
    [StringLength(15)]
    public string? GSTIN { get; set; }

    /// <summary>
    /// Permanent Account Number (10-character alphanumeric)
    /// Format: AAAAA9999A
    /// </summary>
    [StringLength(10)]
    public string? PAN { get; set; }

    /// <summary>
    /// Date when GST registration was obtained
    /// </summary>
    public DateTimeOffset? GSTRegistrationDate { get; set; }

    /// <summary>
    /// Type of GST registration
    /// </summary>
    [StringLength(50)]
    public string? GSTRegistrationType { get; set; } // Regular, Composition, Casual, Non-Resident

    /// <summary>
    /// State Code for GST (2-digit code as per GST rules)
    /// </summary>
    [StringLength(2)]
    public string? StateCode { get; set; }

    /// <summary>
    /// Current status of GSTIN
    /// </summary>
    [StringLength(20)]
    public string? GSTINStatus { get; set; } // Active, Cancelled, Suspended

    #endregion

    #region Configuration Settings

    public bool IsActive { get; set; } = true;

    public bool UseJobCosting { get; set; } = true;

    public bool UseInventoryAccounting { get; set; } = true;

    public bool RequireBalancedEntries { get; set; } = true;

    public bool AutoPostInvoices { get; set; } = true;

    public bool AutoPostPayments { get; set; } = true;

    public bool AutoPostPurchaseInvoices { get; set; } = true;

    public bool AutoPostOnApproval { get; set; } = false;

    public bool AllowNegativeInventory { get; set; } = false;

    public bool EnableMultiCurrency { get; set; } = false;

    [StringLength(10)]
    public string? FiscalYearStartMonth { get; set; } = "01"; // January

    [StringLength(50)]
    public string? DefaultCurrency { get; set; } = "INR";

    [StringLength(500)]
    public string? Notes { get; set; }

    #endregion

    #region Document Numbering Configuration

    [StringLength(20)]
    public string? InvoiceNumberPrefix { get; set; } = "INV";

    public int InvoiceNumberSeriesStart { get; set; } = 1;

    [StringLength(20)]
    public string? QuoteNumberPrefix { get; set; } = "QT";

    public int QuoteNumberSeriesStart { get; set; } = 1;

    [StringLength(20)]
    public string? PurchaseOrderNumberPrefix { get; set; } = "PO";

    public int PurchaseOrderNumberSeriesStart { get; set; } = 1;

    [StringLength(20)]
    public string? PaymentReceiptNumberPrefix { get; set; } = "RCP";

    public int PaymentReceiptNumberSeriesStart { get; set; } = 1;

    [StringLength(20)]
    public string? CreditNoteNumberPrefix { get; set; } = "CN";

    public int CreditNoteNumberSeriesStart { get; set; } = 1;

    [StringLength(20)]
    public string? DebitNoteNumberPrefix { get; set; } = "DN";

    public int DebitNoteNumberSeriesStart { get; set; } = 1;

    #endregion

    #region Terms & Policies

    /// <summary>
    /// Default payment terms (e.g., Net 30, Net 60, Due on Receipt)
    /// </summary>
    [StringLength(100)]
    public string? DefaultPaymentTerms { get; set; } = "Net 30";

    /// <summary>
    /// Default credit period in days
    /// </summary>
    public int DefaultCreditDays { get; set; } = 30;

    /// <summary>
    /// Default terms and conditions text for invoices
    /// </summary>
    [StringLength(2000)]
    public string? InvoiceTermsAndConditions { get; set; }

    /// <summary>
    /// Default validity period for quotes in days
    /// </summary>
    public int QuoteValidityDays { get; set; } = 30;

    /// <summary>
    /// Default terms and conditions text for quotes
    /// </summary>
    [StringLength(2000)]
    public string? QuoteTermsAndConditions { get; set; }

    /// <summary>
    /// Late payment fee percentage
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? LatePaymentFeePercentage { get; set; }

    /// <summary>
    /// Grace period for late payment fees in days
    /// </summary>
    public int LatePaymentGraceDays { get; set; } = 0;

    #endregion

    #region GST Configuration

    /// <summary>
    /// Enable reverse charge mechanism for applicable transactions
    /// </summary>
    public bool EnableReverseCharge { get; set; } = false;

    /// <summary>
    /// Enable e-invoicing for applicable businesses (turnover > 5 Cr)
    /// </summary>
    public bool EnableEInvoicing { get; set; } = false;

    /// <summary>
    /// E-Invoice API endpoint URL
    /// </summary>
    [StringLength(500)]
    public string? EInvoiceApiUrl { get; set; }

    /// <summary>
    /// E-Invoice API username
    /// </summary>
    [StringLength(100)]
    public string? EInvoiceApiUsername { get; set; }

    /// <summary>
    /// E-Invoice API password (should be encrypted)
    /// </summary>
    [StringLength(500)]
    public string? EInvoiceApiPassword { get; set; }

    /// <summary>
    /// GST return filing frequency
    /// </summary>
    [StringLength(20)]
    public string? GSTFilingFrequency { get; set; } = "Monthly"; // Monthly, Quarterly

    /// <summary>
    /// Enable GST composition scheme
    /// </summary>
    public bool IsCompositionScheme { get; set; } = false;

    /// <summary>
    /// Composition scheme tax rate percentage
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? CompositionTaxRate { get; set; }

    /// <summary>
    /// Enable automatic GST calculation based on supply type
    /// </summary>
    public bool AutoCalculateGST { get; set; } = true;

    /// <summary>
    /// Default HSN/SAC code for services
    /// </summary>
    [StringLength(20)]
    public string? DefaultHSNCode { get; set; }

    #endregion
}
