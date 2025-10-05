-- =====================================================
-- PostgreSQL Database Constraints and Triggers for
-- Voucher and Ledger Integrity
-- =====================================================

-- ======================
-- FUNCTION: Check if posted vouchers have ledger entries
-- ======================
CREATE OR REPLACE FUNCTION check_voucher_has_ledger_entries()
RETURNS TRIGGER AS $$
BEGIN
    -- Only check if voucher is being set to Posted status
    IF NEW."Status" IN ('Posted', 'Paid') THEN
        -- Check if there are any ledger entries for this voucher
        IF NOT EXISTS (
            SELECT 1
            FROM "LedgerEntries"
            WHERE "VoucherId" = NEW."Oid"
            AND "IsNotDeleted" = true
        ) THEN
            RAISE EXCEPTION 'Cannot set voucher status to Posted/Paid without ledger entries. Voucher: %', NEW."Number";
        END IF;

        -- Validate that ledger entries balance (debits = credits)
        DECLARE
            total_debits DECIMAL(18,2);
            total_credits DECIMAL(18,2);
        BEGIN
            SELECT
                COALESCE(SUM(CASE WHEN "EntryType" = 'Debit' THEN "Amount" ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN "EntryType" = 'Credit' THEN "Amount" ELSE 0 END), 0)
            INTO total_debits, total_credits
            FROM "LedgerEntries"
            WHERE "VoucherId" = NEW."Oid"
            AND "IsNotDeleted" = true;

            IF total_debits != total_credits THEN
                RAISE EXCEPTION 'Ledger entries for voucher % are not balanced. Debits: %, Credits: %',
                    NEW."Number", total_debits, total_credits;
            END IF;
        END;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ======================
-- TRIGGER: Validate Invoice ledger entries
-- ======================
DROP TRIGGER IF EXISTS trg_validate_invoice_ledger ON "Vouchers";
CREATE TRIGGER trg_validate_invoice_ledger
    BEFORE UPDATE ON "Vouchers"
    FOR EACH ROW
    WHEN (NEW."VoucherType" = 'Invoice')
    EXECUTE FUNCTION check_voucher_has_ledger_entries();

-- ======================
-- TRIGGER: Validate PurchaseInvoice ledger entries
-- ======================
DROP TRIGGER IF EXISTS trg_validate_purchase_invoice_ledger ON "Vouchers";
CREATE TRIGGER trg_validate_purchase_invoice_ledger
    BEFORE UPDATE ON "Vouchers"
    FOR EACH ROW
    WHEN (NEW."VoucherType" = 'PurchaseInvoice')
    EXECUTE FUNCTION check_voucher_has_ledger_entries();

-- ======================
-- FUNCTION: Prevent deletion of posted vouchers with ledger entries
-- ======================
CREATE OR REPLACE FUNCTION prevent_posted_voucher_deletion()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD."Status" IN ('Posted', 'Paid') THEN
        -- Check if there are unbalanced ledger entries
        DECLARE
            entry_count INTEGER;
        BEGIN
            SELECT COUNT(*)
            INTO entry_count
            FROM "LedgerEntries"
            WHERE "VoucherId" = OLD."Oid"
            AND "IsNotDeleted" = true;

            IF entry_count > 0 THEN
                RAISE EXCEPTION 'Cannot delete posted voucher % with existing ledger entries. Reverse entries first.', OLD."Number";
            END IF;
        END;
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

-- ======================
-- TRIGGER: Prevent deletion of posted vouchers
-- ======================
DROP TRIGGER IF EXISTS trg_prevent_posted_voucher_deletion ON "Vouchers";
CREATE TRIGGER trg_prevent_posted_voucher_deletion
    BEFORE DELETE ON "Vouchers"
    FOR EACH ROW
    EXECUTE FUNCTION prevent_posted_voucher_deletion();

-- ======================
-- FUNCTION: Validate ledger entry has valid account
-- ======================
CREATE OR REPLACE FUNCTION validate_ledger_entry_account()
RETURNS TRIGGER AS $$
BEGIN
    -- Ensure the account exists and is active
    IF NOT EXISTS (
        SELECT 1
        FROM "LedgerAccounts"
        WHERE "Oid" = NEW."AccountId"
        AND "IsActive" = true
        AND "IsNotDeleted" = true
    ) THEN
        RAISE EXCEPTION 'Cannot create ledger entry with inactive or deleted account';
    END IF;

    -- Ensure the account belongs to the same tenant
    IF NEW."TenantId" IS NOT NULL THEN
        IF NOT EXISTS (
            SELECT 1
            FROM "LedgerAccounts"
            WHERE "Oid" = NEW."AccountId"
            AND "TenantId" = NEW."TenantId"
        ) THEN
            RAISE EXCEPTION 'Ledger entry and account must belong to the same tenant';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ======================
-- TRIGGER: Validate ledger entry account
-- ======================
DROP TRIGGER IF EXISTS trg_validate_ledger_entry_account ON "LedgerEntries";
CREATE TRIGGER trg_validate_ledger_entry_account
    BEFORE INSERT OR UPDATE ON "LedgerEntries"
    FOR EACH ROW
    EXECUTE FUNCTION validate_ledger_entry_account();

-- ======================
-- FUNCTION: Auto-calculate running balance (optional)
-- ======================
CREATE OR REPLACE FUNCTION calculate_account_running_balance()
RETURNS TRIGGER AS $$
DECLARE
    account_nature TEXT;
    previous_balance DECIMAL(18,2);
    new_balance DECIMAL(18,2);
BEGIN
    -- Get account nature (Asset, Liability, Income, Expense)
    SELECT ag."Nature"
    INTO account_nature
    FROM "LedgerAccounts" la
    JOIN "AccountGroups" ag ON la."AccountGroupId" = ag."Oid"
    WHERE la."Oid" = NEW."AccountId";

    -- Get previous balance
    SELECT COALESCE(la."Balance", 0)
    INTO previous_balance
    FROM "LedgerAccounts" la
    WHERE la."Oid" = NEW."AccountId";

    -- Calculate new balance based on entry type and account nature
    -- Asset and Expense accounts increase with Debits
    -- Liability and Income accounts increase with Credits
    IF account_nature IN ('Asset', 'Expense') THEN
        IF NEW."EntryType" = 'Debit' THEN
            new_balance := previous_balance + NEW."Amount";
        ELSE
            new_balance := previous_balance - NEW."Amount";
        END IF;
    ELSE -- Liability, Income
        IF NEW."EntryType" = 'Credit' THEN
            new_balance := previous_balance + NEW."Amount";
        ELSE
            new_balance := previous_balance - NEW."Amount";
        END IF;
    END IF;

    -- Update account balance
    UPDATE "LedgerAccounts"
    SET "Balance" = new_balance,
        "UpdatedAtUtc" = NOW()
    WHERE "Oid" = NEW."AccountId";

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ======================
-- TRIGGER: Auto-update account balance (optional - enable if needed)
-- This trigger automatically updates account balances when ledger entries are created
-- Disable this if you prefer to calculate balances on-demand via queries
-- ======================
-- DROP TRIGGER IF EXISTS trg_update_account_balance ON "LedgerEntries";
-- CREATE TRIGGER trg_update_account_balance
--     AFTER INSERT ON "LedgerEntries"
--     FOR EACH ROW
--     EXECUTE FUNCTION calculate_account_running_balance();

-- ======================
-- INDEX: Improve ledger query performance
-- ======================
CREATE INDEX IF NOT EXISTS idx_ledger_entries_voucher ON "LedgerEntries"("VoucherId") WHERE "IsNotDeleted" = true;
CREATE INDEX IF NOT EXISTS idx_ledger_entries_account ON "LedgerEntries"("AccountId") WHERE "IsNotDeleted" = true;
CREATE INDEX IF NOT EXISTS idx_ledger_entries_date ON "LedgerEntries"("EntryDate") WHERE "IsNotDeleted" = true;
CREATE INDEX IF NOT EXISTS idx_ledger_entries_tenant ON "LedgerEntries"("TenantId") WHERE "IsNotDeleted" = true;
CREATE INDEX IF NOT EXISTS idx_ledger_entries_reconciled ON "LedgerEntries"("IsReconciled") WHERE "IsNotDeleted" = true;

-- ======================
-- INDEX: Improve voucher query performance
-- ======================
CREATE INDEX IF NOT EXISTS idx_vouchers_status ON "Vouchers"("Status") WHERE "IsNotDeleted" = true;
CREATE INDEX IF NOT EXISTS idx_vouchers_type ON "Vouchers"("VoucherType") WHERE "IsNotDeleted" = true;
CREATE INDEX IF NOT EXISTS idx_vouchers_party ON "Vouchers"("PartyId") WHERE "IsNotDeleted" = true;
CREATE INDEX IF NOT EXISTS idx_vouchers_date ON "Vouchers"("VoucherDate") WHERE "IsNotDeleted" = true;

-- ======================
-- VIEW: Voucher Ledger Summary
-- ======================
CREATE OR REPLACE VIEW vw_voucher_ledger_summary AS
SELECT
    v."Oid" AS "VoucherId",
    v."Number" AS "VoucherNumber",
    v."VoucherType",
    v."VoucherDate",
    v."Status",
    v."TotalAmount" AS "VoucherAmount",
    COUNT(le."Oid") AS "EntryCount",
    SUM(le."DebitAmount") AS "TotalDebits",
    SUM(le."CreditAmount") AS "TotalCredits",
    CASE
        WHEN SUM(le."DebitAmount") = SUM(le."CreditAmount") THEN true
        ELSE false
    END AS "IsBalanced"
FROM "Vouchers" v
LEFT JOIN "LedgerEntries" le ON v."Oid" = le."VoucherId" AND le."IsNotDeleted" = true
WHERE v."IsNotDeleted" = true
GROUP BY v."Oid", v."Number", v."VoucherType", v."VoucherDate", v."Status", v."TotalAmount";

-- ======================
-- VIEW: Account Ledger Report
-- ======================
CREATE OR REPLACE VIEW vw_account_ledger_report AS
SELECT
    la."Oid" AS "AccountId",
    la."Code" AS "AccountCode",
    la."Name" AS "AccountName",
    ag."Nature" AS "AccountNature",
    le."EntryDate",
    le."EntryNumber",
    le."Description",
    le."DebitAmount",
    le."CreditAmount",
    v."Number" AS "VoucherNumber",
    v."VoucherType",
    le."IsReconciled"
FROM "LedgerAccounts" la
LEFT JOIN "LedgerEntries" le ON la."Oid" = le."AccountId" AND le."IsNotDeleted" = true
LEFT JOIN "AccountGroups" ag ON la."AccountGroupId" = ag."Oid"
LEFT JOIN "Vouchers" v ON le."VoucherId" = v."Oid"
WHERE la."IsNotDeleted" = true
ORDER BY la."Code", le."EntryDate", le."EntryNumber";

-- ======================
-- COMMENT: Usage Notes
-- ======================
COMMENT ON FUNCTION check_voucher_has_ledger_entries() IS
'Validates that vouchers marked as Posted have corresponding ledger entries and that the entries balance';

COMMENT ON FUNCTION prevent_posted_voucher_deletion() IS
'Prevents deletion of vouchers that have been posted and have ledger entries';

COMMENT ON VIEW vw_voucher_ledger_summary IS
'Summary view showing voucher details with ledger entry totals and balance status';

COMMENT ON VIEW vw_account_ledger_report IS
'Detailed ledger report showing all transactions for each account';
