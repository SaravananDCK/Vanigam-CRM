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
}
