# Database Scripts Folder

## Overview

This folder contains SQL scripts that are **automatically executed** during database initialization. All `.sql` files in this folder and subfolders are embedded as resources and executed via `VanigamAccountingDbContext.SeedInitialData()`.

## How It Works

### 1. Embedded Resources

All SQL files are marked as embedded resources in `Vanigam.CRM.Objects.csproj`:

```xml
<ItemGroup>
    <EmbeddedResource Include="Database\**\*.sql" />
</ItemGroup>
```

### 2. Automatic Execution

When `SeedInitialData()` runs (on first database creation):

1. ✅ Detects database provider (PostgreSQL or SQL Server)
2. ✅ Scans for all embedded SQL resources in the Database folder
3. ✅ Filters scripts based on database provider and filename
4. ✅ Executes each script using `Database.ExecuteSqlRawAsync()`
5. ✅ Logs execution progress and errors to console

### 3. Provider-Specific Scripts

**Naming Convention**:
- **PostgreSQL**: Include `PostgreSQL` in filename
  - Example: `PostgreSQL_VoucherLedgerConstraints.sql`
- **SQL Server**: Include `MSSQL` or `SqlServer` in filename
  - Example: `MSSQL_VoucherLedgerConstraints.sql`
- **Generic**: No provider prefix (runs on all providers)
  - Example: `CommonIndexes.sql`

**Filtering Logic**:
- PostgreSQL database → Executes PostgreSQL scripts, skips MSSQL scripts
- SQL Server database → Executes MSSQL scripts, skips PostgreSQL scripts
- Both execute generic scripts (no provider prefix)

## File Organization

```
Database/
├── PostgreSQL_VoucherLedgerConstraints.sql    # PostgreSQL triggers/constraints
├── MSSQL_VoucherLedgerConstraints.sql         # SQL Server equivalent (if needed)
├── PostgreSQL_Functions/                      # Subfolder for functions
│   └── fn_CalculateBalance.sql
└── PostgreSQL_Views/                          # Subfolder for views
    └── vw_TrialBalance.sql
```

## Current Scripts

### PostgreSQL_VoucherLedgerConstraints.sql

Creates ledger posting integrity constraints:

**Functions**:
- `check_voucher_has_ledger_entries()` - Validates posted vouchers have entries
- `prevent_posted_voucher_deletion()` - Prevents deletion of posted vouchers
- `validate_ledger_entry_account()` - Validates account references
- `calculate_account_running_balance()` - Optional auto-balance updates

**Triggers**:
- `trg_validate_invoice_ledger` - Invoice posting validation
- `trg_validate_purchase_invoice_ledger` - Purchase invoice validation
- `trg_prevent_posted_voucher_deletion` - Deletion protection
- `trg_validate_ledger_entry_account` - Account validation

**Indexes**:
- Performance indexes on LedgerEntries and Vouchers tables

**Views**:
- `vw_voucher_ledger_summary` - Voucher summary with entry totals
- `vw_account_ledger_report` - Detailed account transactions

## Adding New Scripts

### Step 1: Create SQL File

Create a new `.sql` file in the `Database/` folder:

```sql
-- Database/PostgreSQL_MyNewFeature.sql
-- =====================================================
-- Description of what this script does
-- =====================================================

CREATE OR REPLACE FUNCTION my_function()
RETURNS VOID AS $$
BEGIN
    -- Function logic here
END;
$$ LANGUAGE plpgsql;
```

### Step 2: Build Project

The file is automatically included as an embedded resource (no manual configuration needed).

### Step 3: Test Execution

Run the application with a fresh database to test:

```bash
# Delete existing database
dropdb vanigam_crm

# Run application - scripts execute automatically during SeedInitialData()
dotnet run --project Server
```

Check console output for execution confirmation:
```
Executing database scripts for PostgreSQL...
Found 1 SQL script(s) to execute
Executing: Vanigam.CRM.Objects.Database.PostgreSQL_MyNewFeature.sql
Successfully executed: Vanigam.CRM.Objects.Database.PostgreSQL_MyNewFeature.sql
Database scripts execution completed
```

## Best Practices

### 1. Idempotent Scripts

Always use `CREATE OR REPLACE` or `DROP IF EXISTS`:

```sql
-- Good: Idempotent
CREATE OR REPLACE FUNCTION my_func() ...
DROP TRIGGER IF EXISTS my_trigger ON my_table;

-- Bad: Fails on re-execution
CREATE FUNCTION my_func() ...
CREATE TRIGGER my_trigger ...
```

### 2. Error Handling

Scripts should handle errors gracefully:

```sql
DO $$
BEGIN
    -- Try to create something
    CREATE INDEX idx_my_index ON my_table(my_column);
EXCEPTION
    WHEN duplicate_table THEN
        NULL; -- Index already exists, ignore
END $$;
```

### 3. Comments and Documentation

Add clear comments explaining what each script does:

```sql
-- =====================================================
-- Script Name: PostgreSQL_AccountingViews.sql
-- Purpose: Creates reporting views for trial balance
-- Dependencies: LedgerAccounts, LedgerEntries tables
-- =====================================================

-- Create trial balance view
CREATE OR REPLACE VIEW vw_trial_balance AS ...
```

### 4. Execution Order

If scripts depend on each other, use filename prefixes:

```
Database/
├── 01_PostgreSQL_CoreFunctions.sql      # Executed first
├── 02_PostgreSQL_Triggers.sql           # Uses functions from step 1
└── 03_PostgreSQL_Views.sql              # Uses triggers from step 2
```

## Troubleshooting

### Script Not Executing

**Symptoms**: Script file exists but doesn't run

**Solutions**:
1. Verify file is in `Database/` folder
2. Check file extension is `.sql`
3. Rebuild project to embed resource
4. Check console output for execution logs

### Provider Mismatch

**Symptoms**: PostgreSQL script runs on SQL Server (or vice versa)

**Solutions**:
1. Ensure filename includes provider name
2. Check spelling: `PostgreSQL` (not `Postgres`)
3. Check provider detection in `ExecuteDatabaseConstraints()`

### Execution Error

**Symptoms**: Script fails during execution

**Solutions**:
1. Check console output for detailed error message
2. Test script manually: `psql -f your_script.sql`
3. Verify script syntax and dependencies
4. Check if script is idempotent (can run multiple times)

### Resource Not Found

**Symptoms**: "Could not load resource" error

**Solutions**:
1. Clean and rebuild solution
2. Check `.csproj` has `<EmbeddedResource Include="Database\**\*.sql" />`
3. Verify file build action is set to "Embedded Resource"

## Manual Execution

If automatic execution fails, you can run scripts manually:

### PostgreSQL
```bash
psql -U postgres -d vanigam_crm -f Objects/Database/PostgreSQL_VoucherLedgerConstraints.sql
```

### SQL Server
```bash
sqlcmd -S localhost -d vanigam_crm -i Objects\Database\MSSQL_VoucherLedgerConstraints.sql
```

## Testing Scripts

### Verify Execution

Check if functions/triggers were created:

```sql
-- PostgreSQL
SELECT routine_name
FROM information_schema.routines
WHERE routine_schema = 'public'
AND routine_name LIKE 'check_voucher%';

SELECT trigger_name
FROM information_schema.triggers
WHERE trigger_schema = 'public';

-- SQL Server
SELECT name FROM sys.objects WHERE type IN ('FN', 'IF', 'TF');  -- Functions
SELECT name FROM sys.triggers;  -- Triggers
```

### Test Constraints

Try operations that should be blocked:

```sql
-- Should fail: Posting voucher without ledger entries
UPDATE "Vouchers"
SET "Status" = 'Posted'
WHERE "Number" = 'INV-001';
-- Expected: ERROR: Cannot set voucher status to Posted/Paid without ledger entries
```

## Migration from Old System

If you previously used manual SQL execution:

1. Move SQL files to `Objects/Database/` folder
2. Follow naming convention (include provider name)
3. Make scripts idempotent (use `CREATE OR REPLACE`)
4. Rebuild project
5. Test with fresh database

## Future Enhancements

Planned improvements:

- [ ] Execution order control via numbered prefixes
- [ ] Schema versioning to track applied scripts
- [ ] Rollback scripts for reversing changes
- [ ] Dry-run mode to preview scripts without executing
- [ ] Script dependency graph validation
