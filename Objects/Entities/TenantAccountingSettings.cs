using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Stores tenant-specific accounting configuration and default ledger accounts.
    /// This allows each tenant to have their own chart of accounts and posting rules.
    /// </summary>
    public class TenantAccountingSettings : BaseClass
    {
        // Sales/Revenue Accounts
        [Required]
        public Guid DefaultSalesAccountId { get; set; }
        [ForeignKey(nameof(DefaultSalesAccountId))]
        public LedgerAccount DefaultSalesAccount { get; set; }

        public Guid? SalesReturnAccountId { get; set; }
        [ForeignKey(nameof(SalesReturnAccountId))]
        public LedgerAccount? SalesReturnAccount { get; set; }

        public Guid? SalesDiscountAccountId { get; set; }
        [ForeignKey(nameof(SalesDiscountAccountId))]
        public LedgerAccount? SalesDiscountAccount { get; set; }

        // Purchase/Expense Accounts
        [Required]
        public Guid DefaultPurchasesAccountId { get; set; }
        [ForeignKey(nameof(DefaultPurchasesAccountId))]
        public LedgerAccount DefaultPurchasesAccount { get; set; }

        public Guid? PurchaseReturnAccountId { get; set; }
        [ForeignKey(nameof(PurchaseReturnAccountId))]
        public LedgerAccount? PurchaseReturnAccount { get; set; }

        public Guid? PurchaseDiscountAccountId { get; set; }
        [ForeignKey(nameof(PurchaseDiscountAccountId))]
        public LedgerAccount? PurchaseDiscountAccount { get; set; }

        // Tax Accounts
        [Required]
        public Guid DefaultTaxPayableAccountId { get; set; }
        [ForeignKey(nameof(DefaultTaxPayableAccountId))]
        public LedgerAccount DefaultTaxPayableAccount { get; set; }

        [Required]
        public Guid DefaultTaxInputAccountId { get; set; }
        [ForeignKey(nameof(DefaultTaxInputAccountId))]
        public LedgerAccount DefaultTaxInputAccount { get; set; }

        // Inventory Accounts
        public Guid? DefaultInventoryAccountId { get; set; }
        [ForeignKey(nameof(DefaultInventoryAccountId))]
        public LedgerAccount? DefaultInventoryAccount { get; set; }

        public Guid? CostOfGoodsSoldAccountId { get; set; }
        [ForeignKey(nameof(CostOfGoodsSoldAccountId))]
        public LedgerAccount? CostOfGoodsSoldAccount { get; set; }

        // Work in Progress (for job costing)
        public Guid? WorkInProgressAccountId { get; set; }
        [ForeignKey(nameof(WorkInProgressAccountId))]
        public LedgerAccount? WorkInProgressAccount { get; set; }

        // Receivables/Payables
        public Guid? DefaultReceivableAccountId { get; set; }
        [ForeignKey(nameof(DefaultReceivableAccountId))]
        public LedgerAccount? DefaultReceivableAccount { get; set; }

        public Guid? DefaultPayableAccountId { get; set; }
        [ForeignKey(nameof(DefaultPayableAccountId))]
        public LedgerAccount? DefaultPayableAccount { get; set; }

        // Cash/Bank Accounts
        public Guid? DefaultCashAccountId { get; set; }
        [ForeignKey(nameof(DefaultCashAccountId))]
        public LedgerAccount? DefaultCashAccount { get; set; }

        public Guid? DefaultBankAccountId { get; set; }
        [ForeignKey(nameof(DefaultBankAccountId))]
        public BankAccount? DefaultBankAccount { get; set; }

        // Rounding and Exchange Accounts
        public Guid? RoundingAccountId { get; set; }
        [ForeignKey(nameof(RoundingAccountId))]
        public LedgerAccount? RoundingAccount { get; set; }

        public Guid? ExchangeGainLossAccountId { get; set; }
        [ForeignKey(nameof(ExchangeGainLossAccountId))]
        public LedgerAccount? ExchangeGainLossAccount { get; set; }

        // Accounting Preferences
        [StringLength(10)]
        public string? FiscalYearStartMonth { get; set; } = "01"; // January

        public bool UseJobCosting { get; set; } = true;
        public bool UseInventoryAccounting { get; set; } = true;
        public bool RequireBalancedEntries { get; set; } = true;
        public bool AutoPostOnApproval { get; set; } = false;
        public bool AllowNegativeInventory { get; set; } = false;

        [StringLength(50)]
        public string? DefaultCurrency { get; set; } = "USD";

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
