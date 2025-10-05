# Ledger Posting Implementation Guide

## Overview

This guide explains the ledger posting system for the Vanigam CRM application. The system implements **double-entry bookkeeping** where every financial transaction creates balanced ledger entries (Debits = Credits).

## Architecture

### EF Core Service Layer (Recommended Approach)

The system uses **EF Core services** rather than database triggers for ledger posting. This provides:

- ✅ Business logic visibility in C# code
- ✅ Automatic tenant isolation
- ✅ Type safety and compile-time validation
- ✅ Easy unit testing
- ✅ Better debugging and error handling
- ✅ Transaction safety via EF Core's SaveChanges

### Database Constraints (Safety Layer)

PostgreSQL triggers and constraints ensure data integrity:

- ✅ Validates posted vouchers have ledger entries
- ✅ Ensures ledger entries balance (debits = credits)
- ✅ Prevents deletion of posted vouchers with entries
- ✅ Validates account references

## Components

### 1. LedgerPostingService

**Location**: `Server/Services/LedgerPostingService.cs`

**Key Methods**:
- `PostInvoiceToLedger(Invoice)` - Posts sales invoices
- `PostPaymentToLedger(Payment)` - Posts customer payments
- `PostPurchaseInvoiceToLedger(PurchaseInvoice)` - Posts supplier invoices
- `ReverseVoucherEntries(voucherId, reason)` - Reverses posted entries
- `ValidateVoucherEntriesBalance(voucherId)` - Validates balance

**Double-Entry Logic**:

```
Invoice Posting:
  Debit:  Customer Account (Accounts Receivable) - Asset increases
  Credit: Sales Account (Revenue)                - Income increases
  Credit: Tax Payable Account                    - Liability increases

Payment Posting:
  Debit:  Bank Account (Cash)                    - Asset increases
  Credit: Customer Account (Accounts Receivable) - Asset decreases

Purchase Invoice Posting:
  Debit:  Purchases Account (Expense)            - Expense increases
  Debit:  Tax Input Account                      - Asset increases
  Credit: Vendor Account (Accounts Payable)      - Liability increases
```

### 2. TenantAccountingSettings

**Location**: `Objects/Entities/TenantAccountingSettings.cs`

Stores tenant-specific default ledger accounts:
- Sales Account
- Purchases Account
- Tax Payable/Input Accounts
- Work in Progress Account
- Cash/Bank Accounts
- Rounding & Exchange Accounts

Each tenant can have their own chart of accounts and posting rules.

### 3. LedgerEntry Entity

**Location**: `Objects/Entities/LedgerEntry.cs`

**New Computed Columns**:
- `DebitAmount` - PostgreSQL generated column: `CASE WHEN EntryType = 'Debit' THEN Amount ELSE 0 END`
- `CreditAmount` - PostgreSQL generated column: `CASE WHEN EntryType = 'Credit' THEN Amount ELSE 0 END`

These columns are stored in the database and improve reporting performance.

### 4. Updated Voucher Services

**Updated Services**:
- `InvoiceService` - Auto-posts when status = Posted
- `PaymentService` - Auto-posts on create
- `PurchaseInvoiceService` - Auto-posts when status = Posted

**Transaction Flow**:
1. Create/Update voucher
2. If status is Posted, call LedgerPostingService
3. Validate entries balance
4. Commit transaction or rollback on error

## Usage Examples

### Creating an Invoice with Ledger Posting

```csharp
var invoice = new Invoice
{
    Number = "INV-001",
    PartyId = customerId,
    Status = InvoiceStatus.Draft, // No posting yet
    SubTotal = 1000,
    TaxAmount = 180,
    TotalAmount = 1180
};

// Create invoice (no ledger entries yet)
invoice = await invoiceService.CreateAsync(invoice);

// Update to Posted status (triggers ledger posting)
invoice.Status = InvoiceStatus.Posted;
invoice = await invoiceService.UpdateAsync(invoice);

// Ledger entries are automatically created:
// Debit:  Customer Account    $1180
// Credit: Sales Account       $1000
// Credit: Tax Payable         $180
```

### Recording a Payment

```csharp
var payment = new Payment
{
    ReferenceNumber = "PMT-001",
    CustomerId = customerId,
    BankAccountId = bankAccountId,
    Amount = 1180,
    PaymentDate = DateTime.UtcNow
};

// Create payment (automatically posts to ledger)
payment = await paymentService.CreateAsync(payment);

// Ledger entries are automatically created:
// Debit:  Bank Account        $1180
// Credit: Customer Account    $1180
```

### Reversing a Posted Invoice

```csharp
// Get posted invoice
var invoice = await invoiceService.GetByIdAsync(invoiceId);

// Change status from Posted to Draft (triggers reversal)
invoice.Status = InvoiceStatus.Draft;
invoice = await invoiceService.UpdateAsync(invoice);

// Reversal entries are automatically created:
// Credit: Customer Account    $1180 (reverses original debit)
// Debit:  Sales Account       $1000 (reverses original credit)
// Debit:  Tax Payable         $180 (reverses original credit)
```

## Database Setup

### 1. Apply Database Constraints (Automatic)

**All SQL files in `Objects/Database/` folder are automatically executed** during database initialization via `SeedInitialData()`.

The system:
- ✅ Embeds all `*.sql` files as resources
- ✅ Auto-detects PostgreSQL vs SQL Server scripts by filename
- ✅ Executes scripts in order during first database creation
- ✅ Logs execution progress to console

**Naming Convention for SQL Files**:
- PostgreSQL scripts: Include "PostgreSQL" in filename (e.g., `PostgreSQL_VoucherLedgerConstraints.sql`)
- SQL Server scripts: Include "MSSQL" or "SqlServer" in filename (e.g., `MSSQL_VoucherLedgerConstraints.sql`)
- Generic scripts: No provider prefix (executed for all providers)

**What Gets Created**:
- Validation triggers for posted vouchers
- Deletion prevention for posted vouchers
- Account validation triggers
- Performance indexes
- Reporting views

### 2. Manual Execution (If Needed)

If automatic execution fails, run the SQL script manually:

```bash
# PostgreSQL
psql -U postgres -d vanigam_crm -f Objects/Database/PostgreSQL_VoucherLedgerConstraints.sql

# SQL Server
sqlcmd -S localhost -d vanigam_crm -i Objects/Database/MSSQL_VoucherLedgerConstraints.sql
```

### 2. Seed Tenant Accounting Settings

Create default accounting settings for each tenant:

```csharp
var settings = new TenantAccountingSettings
{
    TenantId = tenantId,
    DefaultSalesAccountId = salesAccountId,
    DefaultPurchasesAccountId = purchasesAccountId,
    DefaultTaxPayableAccountId = taxPayableAccountId,
    DefaultTaxInputAccountId = taxInputAccountId,
    UseJobCosting = true,
    RequireBalancedEntries = true
};

await context.TenantAccountingSettings.AddAsync(settings);
await context.SaveChangesAsync();
```

## Reporting Queries

### Voucher Ledger Summary

```sql
SELECT * FROM vw_voucher_ledger_summary
WHERE "VoucherType" = 'Invoice'
AND "IsBalanced" = true
ORDER BY "VoucherDate" DESC;
```

### Account Ledger Report

```sql
SELECT * FROM vw_account_ledger_report
WHERE "AccountCode" = 'SALES'
AND "EntryDate" BETWEEN '2024-01-01' AND '2024-12-31'
ORDER BY "EntryDate";
```

### Trial Balance

```sql
SELECT
    la."Code",
    la."Name",
    SUM(le."DebitAmount") AS "TotalDebits",
    SUM(le."CreditAmount") AS "TotalCredits",
    SUM(le."DebitAmount") - SUM(le."CreditAmount") AS "Balance"
FROM "LedgerAccounts" la
LEFT JOIN "LedgerEntries" le ON la."Oid" = le."AccountId"
WHERE la."TenantId" = 1
AND la."IsNotDeleted" = true
AND le."IsNotDeleted" = true
GROUP BY la."Code", la."Name"
ORDER BY la."Code";
```

### Income Statement

```sql
SELECT
    ag."Name" AS "AccountGroup",
    la."Code",
    la."Name",
    SUM(le."CreditAmount") - SUM(le."DebitAmount") AS "Amount"
FROM "LedgerAccounts" la
JOIN "AccountGroups" ag ON la."AccountGroupId" = ag."Oid"
LEFT JOIN "LedgerEntries" le ON la."Oid" = le."AccountId"
WHERE la."TenantId" = 1
AND ag."Nature" IN ('Income', 'Expense')
AND le."EntryDate" BETWEEN '2024-01-01' AND '2024-12-31'
GROUP BY ag."Name", la."Code", la."Name"
ORDER BY ag."Name", la."Code";
```

## Best Practices

### 1. Always Use Transactions

```csharp
await using var transaction = await Context.Database.BeginTransactionAsync();
try
{
    // Create voucher
    // Post to ledger
    // Validate balance
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 2. Validate Entries Balance

```csharp
var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(voucherId);
if (!isBalanced)
{
    throw new InvalidOperationException("Ledger entries are not balanced");
}
```

### 3. Use Reversal Instead of Deletion

Instead of deleting ledger entries, create reversal entries:

```csharp
await ledgerPostingService.ReverseVoucherEntries(voucherId, "Reason for reversal");
```

### 4. Configure Tenant Settings

Ensure each tenant has accounting settings configured before posting:

```csharp
var settings = await context.TenantAccountingSettings
    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

if (settings == null)
{
    throw new InvalidOperationException("Tenant accounting settings not configured");
}
```

## Troubleshooting

### Error: "Cannot post voucher without ledger entries"

**Cause**: Voucher status changed to Posted but LedgerPostingService was not called.

**Solution**: Ensure service methods call `PostXxxToLedger()` before changing status.

### Error: "Ledger entries are not balanced"

**Cause**: Debits ≠ Credits in posted entries.

**Solution**: Review posting logic in LedgerPostingService. Ensure all debits have matching credits.

### Error: "Account not configured for tenant"

**Cause**: TenantAccountingSettings not set up or account references are missing.

**Solution**: Create TenantAccountingSettings record with all required account references.

## Migration Path

If you have existing data without ledger entries:

1. **Backup database**
2. **Create migration script** to generate ledger entries for existing posted vouchers
3. **Run validation queries** to ensure all posted vouchers have balanced entries
4. **Apply database constraints** after validation passes

```sql
-- Example migration script
INSERT INTO "LedgerEntries" ("Oid", "VoucherId", "AccountId", "EntryType", "Amount", ...)
SELECT
    gen_random_uuid(),
    i."Oid",
    i."PartyId",
    'Debit',
    i."TotalAmount",
    ...
FROM "Vouchers" i
WHERE i."VoucherType" = 'Invoice'
AND i."Status" = 'Posted'
AND NOT EXISTS (
    SELECT 1 FROM "LedgerEntries" WHERE "VoucherId" = i."Oid"
);
```

## Future Enhancements

- [ ] Multi-currency support with exchange gain/loss tracking
- [ ] Budget vs actual reporting
- [ ] Financial period closing
- [ ] Audit trail for entry modifications
- [ ] Bank reconciliation automation
- [ ] Cost center/department allocation
- [ ] Job costing integration
- [ ] Automated tax calculations per jurisdiction

## Support

For questions or issues, contact the development team or refer to the codebase documentation.
