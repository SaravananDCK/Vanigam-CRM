-- =============================================
-- Migration: Add Warranty and AMC Contract Support
-- Description: Adds TPH for Contract entity with WarrantyContract, AmcContract, GuaranteeContract, ServiceContract
--              Adds warranty/AMC fields to Items (Product/InventoryItem)
--              Enhances ContractCoverageRule for invoice item tracking
-- Date: 2025-01-08
-- =============================================

-- Step 1: Add ContractType enum values (discriminator for TPH)
-- Note: EF Core will handle discriminator column automatically

-- Step 2: Add new fields to Contracts table (base class fields)
ALTER TABLE "Contracts"
ADD COLUMN IF NOT EXISTS "ContractType" TEXT NOT NULL DEFAULT 'AMC',
ADD COLUMN IF NOT EXISTS "InvoiceId" UUID NULL,
ADD COLUMN IF NOT EXISTS "ContractValue" DECIMAL(18,2) NULL,
ADD COLUMN IF NOT EXISTS "AutoRenewEnabled" BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN IF NOT EXISTS "AutoRenewNoticeDays" INTEGER NULL DEFAULT 30,
ADD COLUMN IF NOT EXISTS "ParentContractId" UUID NULL;

-- Step 3: Add foreign key constraints for new Contract fields
ALTER TABLE "Contracts"
ADD CONSTRAINT "FK_Contracts_Invoices_InvoiceId"
FOREIGN KEY ("InvoiceId") REFERENCES "Invoices"("Oid") ON DELETE RESTRICT;

ALTER TABLE "Contracts"
ADD CONSTRAINT "FK_Contracts_Contracts_ParentContractId"
FOREIGN KEY ("ParentContractId") REFERENCES "Contracts"("Oid") ON DELETE RESTRICT;

-- Step 4: Add WarrantyContract-specific fields
ALTER TABLE "Contracts"
ADD COLUMN IF NOT EXISTS "WarrantyNumber" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "RequiresRegistration" BOOLEAN NULL,
ADD COLUMN IF NOT EXISTS "RegistrationDate" TIMESTAMP WITH TIME ZONE NULL,
ADD COLUMN IF NOT EXISTS "IsTransferable" BOOLEAN NULL;

-- Step 5: Add AmcContract-specific fields
ALTER TABLE "Contracts"
ADD COLUMN IF NOT EXISTS "AmcNumber" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "ScheduledVisitsPerYear" INTEGER NULL,
ADD COLUMN IF NOT EXISTS "ResponseTimeHours" INTEGER NULL,
ADD COLUMN IF NOT EXISTS "ResolutionTimeHours" INTEGER NULL,
ADD COLUMN IF NOT EXISTS "Includes24x7Support" BOOLEAN NULL,
ADD COLUMN IF NOT EXISTS "IncludesRemoteSupport" BOOLEAN NULL,
ADD COLUMN IF NOT EXISTS "IncludesOnsiteSupport" BOOLEAN NULL,
ADD COLUMN IF NOT EXISTS "RenewalDiscountPercent" DECIMAL(5,2) NULL;

-- Step 6: Add GuaranteeContract-specific fields
ALTER TABLE "Contracts"
ADD COLUMN IF NOT EXISTS "GuaranteeNumber" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "IsMoneyBackGuarantee" BOOLEAN NULL,
ADD COLUMN IF NOT EXISTS "GuaranteeConditions" VARCHAR(2000) NULL;

-- Step 7: Add ServiceContract-specific fields
ALTER TABLE "Contracts"
ADD COLUMN IF NOT EXISTS "ServiceContractNumber" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "ServiceType" VARCHAR(500) NULL,
ADD COLUMN IF NOT EXISTS "IncludedServiceHours" INTEGER NULL,
ADD COLUMN IF NOT EXISTS "AdditionalHourlyRate" DECIMAL(18,2) NULL;

-- Step 8: Add warranty/AMC fields to Items table (Product/InventoryItem base class)
ALTER TABLE "Items"
ADD COLUMN IF NOT EXISTS "WarrantyPeriodMonths" INTEGER NULL,
ADD COLUMN IF NOT EXISTS "GuaranteePeriodMonths" INTEGER NULL,
ADD COLUMN IF NOT EXISTS "IsAmcEligible" BOOLEAN NOT NULL DEFAULT true,
ADD COLUMN IF NOT EXISTS "AmcAnnualRate" DECIMAL(18,2) NULL,
ADD COLUMN IF NOT EXISTS "RequiresPurchaseProof" BOOLEAN NOT NULL DEFAULT true,
ADD COLUMN IF NOT EXISTS "Manufacturer" VARCHAR(200) NULL,
ADD COLUMN IF NOT EXISTS "ModelNumber" VARCHAR(100) NULL;

-- Step 9: Add invoice item tracking fields to ContractCoverageRules table
ALTER TABLE "ContractCoverageRules"
ADD COLUMN IF NOT EXISTS "InvoiceItemId" UUID NULL,
ADD COLUMN IF NOT EXISTS "SerialNumber" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "ItemWarrantyEndDate" TIMESTAMP WITH TIME ZONE NULL,
ADD COLUMN IF NOT EXISTS "CoveredQuantity" DOUBLE PRECISION NULL;

-- Step 10: Add foreign key for InvoiceItemId in ContractCoverageRules
ALTER TABLE "ContractCoverageRules"
ADD CONSTRAINT "FK_ContractCoverageRules_InvoiceItems_InvoiceItemId"
FOREIGN KEY ("InvoiceItemId") REFERENCES "VoucherLines"("Oid") ON DELETE RESTRICT;

-- Step 11: Add foreign key for CategoryId in ContractCoverageRules (if ItemCategories table exists)
ALTER TABLE "ContractCoverageRules"
ADD CONSTRAINT "FK_ContractCoverageRules_ItemCategories_InventoryItemCategoryId"
FOREIGN KEY ("InventoryItemCategoryId") REFERENCES "ItemCategories"("Oid") ON DELETE RESTRICT;

-- Step 12: Create indexes for performance
CREATE INDEX IF NOT EXISTS "IX_Contracts_ContractType" ON "Contracts"("ContractType");
CREATE INDEX IF NOT EXISTS "IX_Contracts_InvoiceId" ON "Contracts"("InvoiceId");
CREATE INDEX IF NOT EXISTS "IX_Contracts_ParentContractId" ON "Contracts"("ParentContractId");
CREATE INDEX IF NOT EXISTS "IX_Contracts_StartDate_EndDate" ON "Contracts"("StartDate", "Duration"); -- For expiry queries
CREATE INDEX IF NOT EXISTS "IX_Contracts_CustomerId_IsActive" ON "Contracts"("CustomerId", "IsActive");
CREATE INDEX IF NOT EXISTS "IX_Items_IsAmcEligible" ON "Items"("IsAmcEligible");
CREATE INDEX IF NOT EXISTS "IX_Items_WarrantyPeriodMonths" ON "Items"("WarrantyPeriodMonths") WHERE "WarrantyPeriodMonths" IS NOT NULL;
CREATE INDEX IF NOT EXISTS "IX_ContractCoverageRules_InvoiceItemId" ON "ContractCoverageRules"("InvoiceItemId");
CREATE INDEX IF NOT EXISTS "IX_ContractCoverageRules_ItemWarrantyEndDate" ON "ContractCoverageRules"("ItemWarrantyEndDate") WHERE "ItemWarrantyEndDate" IS NOT NULL;

-- Step 13: Update existing Contract records to set default ContractType if needed
UPDATE "Contracts"
SET "ContractType" = 'AMC'
WHERE "ContractType" IS NULL OR "ContractType" = '';

-- Step 14: Create view for warranty expiry tracking
CREATE OR REPLACE VIEW "V_ExpiringWarranties" AS
SELECT
    c."Oid" AS "ContractId",
    c."Title" AS "ContractTitle",
    c."CustomerId",
    cu."Name" AS "CustomerName",
    c."StartDate",
    c."StartDate" + (c."Duration" || ' months')::INTERVAL AS "EndDate",
    c."Duration",
    ccr."InvoiceItemId",
    ccr."InventoryItemId",
    i."Name" AS "ProductName",
    i."SKU" AS "ProductSKU",
    ccr."ItemWarrantyEndDate",
    ccr."SerialNumber",
    i."IsAmcEligible",
    i."AmcAnnualRate",
    EXTRACT(DAY FROM (c."StartDate" + (c."Duration" || ' months')::INTERVAL) - CURRENT_TIMESTAMP) AS "DaysUntilExpiry"
FROM "Contracts" c
INNER JOIN "Customers" cu ON c."CustomerId" = cu."Oid"
INNER JOIN "ContractCoverageRules" ccr ON c."Oid" = ccr."ContractId"
LEFT JOIN "Items" i ON ccr."InventoryItemId" = i."Oid"
WHERE c."ContractType" = 'Warranty'
  AND c."IsActive" = true
  AND c."ParentContractId" IS NULL  -- Not yet converted to AMC
  AND (c."StartDate" + (c."Duration" || ' months')::INTERVAL) BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '90 days';

-- Step 15: Create view for AMC renewal tracking
CREATE OR REPLACE VIEW "V_AmcRenewals" AS
SELECT
    c."Oid" AS "ContractId",
    c."Title" AS "ContractTitle",
    c."AmcNumber",
    c."CustomerId",
    cu."Name" AS "CustomerName",
    c."StartDate",
    c."StartDate" + (c."Duration" || ' months')::INTERVAL AS "EndDate",
    c."Duration",
    c."ContractValue",
    c."AutoRenewEnabled",
    c."AutoRenewNoticeDays",
    c."RenewalDiscountPercent",
    EXTRACT(DAY FROM (c."StartDate" + (c."Duration" || ' months')::INTERVAL) - CURRENT_TIMESTAMP) AS "DaysUntilExpiry",
    COUNT(ccr."Oid") AS "CoveredItemCount"
FROM "Contracts" c
INNER JOIN "Customers" cu ON c."CustomerId" = cu."Oid"
LEFT JOIN "ContractCoverageRules" ccr ON c."Oid" = ccr."ContractId"
WHERE c."ContractType" = 'AMC'
  AND c."IsActive" = true
  AND (c."StartDate" + (c."Duration" || ' months')::INTERVAL) BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '90 days'
GROUP BY c."Oid", c."Title", c."AmcNumber", c."CustomerId", cu."Name", c."StartDate", c."Duration", c."ContractValue",
         c."AutoRenewEnabled", c."AutoRenewNoticeDays", c."RenewalDiscountPercent";

-- Step 16: Create function to get AMC-eligible products for customer
CREATE OR REPLACE FUNCTION get_amc_eligible_products(p_customer_id UUID)
RETURNS TABLE (
    "ProductId" UUID,
    "ProductName" TEXT,
    "SKU" TEXT,
    "AmcAnnualRate" DECIMAL(18,2),
    "WarrantyContractId" UUID,
    "WarrantyEndDate" TIMESTAMP WITH TIME ZONE,
    "InvoiceNumber" TEXT,
    "InvoiceDate" TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT
        i."Oid" AS "ProductId",
        i."Name" AS "ProductName",
        i."SKU",
        i."AmcAnnualRate",
        c."Oid" AS "WarrantyContractId",
        (c."StartDate" + (c."Duration" || ' months')::INTERVAL) AS "WarrantyEndDate",
        inv."InvoiceNumber",
        inv."InvoiceDate"
    FROM "Contracts" c
    INNER JOIN "ContractCoverageRules" ccr ON c."Oid" = ccr."ContractId"
    INNER JOIN "Items" i ON ccr."InventoryItemId" = i."Oid"
    LEFT JOIN "Invoices" inv ON c."InvoiceId" = inv."Oid"
    WHERE c."CustomerId" = p_customer_id
      AND c."ContractType" = 'Warranty'
      AND c."IsActive" = true
      AND c."ParentContractId" IS NULL  -- Not converted to AMC yet
      AND i."IsAmcEligible" = true
      AND (c."StartDate" + (c."Duration" || ' months')::INTERVAL) <= CURRENT_TIMESTAMP + INTERVAL '30 days'; -- Expiring soon
END;
$$ LANGUAGE plpgsql;

-- Step 17: Add comments to tables and columns for documentation
COMMENT ON COLUMN "Contracts"."ContractType" IS 'TPH discriminator: Warranty, AMC, Guarantee, ServiceContract';
COMMENT ON COLUMN "Contracts"."InvoiceId" IS 'Link to original invoice (for warranty contracts auto-created from sales)';
COMMENT ON COLUMN "Contracts"."ContractValue" IS 'Contract value in currency (null for free warranty contracts)';
COMMENT ON COLUMN "Contracts"."ParentContractId" IS 'Parent contract reference (e.g., AMC references original warranty)';
COMMENT ON COLUMN "Items"."WarrantyPeriodMonths" IS 'Standard warranty period offered with this product';
COMMENT ON COLUMN "Items"."IsAmcEligible" IS 'Whether this product can be covered under AMC (false for software licenses, consumables)';
COMMENT ON COLUMN "Items"."AmcAnnualRate" IS 'Annual AMC rate for this product';
COMMENT ON COLUMN "ContractCoverageRules"."InvoiceItemId" IS 'Link to specific invoice item (for warranty tracking)';
COMMENT ON COLUMN "ContractCoverageRules"."ItemWarrantyEndDate" IS 'Item-specific warranty end date (can override contract end date)';

-- Migration completed successfully
-- Run this script against your PostgreSQL database
-- For SQL Server, adjust data types: TEXT -> NVARCHAR, TIMESTAMP WITH TIME ZONE -> DATETIMEOFFSET, UUID -> UNIQUEIDENTIFIER
