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
INNER JOIN "LedgerAccounts" cu ON c."CustomerId" = cu."Oid"
INNER JOIN "ContractCoverageRules" ccr ON c."Oid" = ccr."ContractId"
LEFT JOIN "Items" i ON ccr."InventoryItemId" = i."Oid"
WHERE c."ContractType" = 0--'Warranty'
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
INNER JOIN "LedgerAccounts" cu ON c."CustomerId" = cu."Oid"
LEFT JOIN "ContractCoverageRules" ccr ON c."Oid" = ccr."ContractId"
WHERE c."ContractType" =1--'AMC'
  AND c."IsActive" = true
  AND (c."StartDate" + (c."Duration" || ' months')::INTERVAL) BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '90 days'
GROUP BY c."Oid", c."Title", c."AmcNumber", c."CustomerId", cu."Name", c."StartDate", c."Duration", c."ContractValue",
         c."AutoRenewEnabled", c."AutoRenewNoticeDays", c."RenewalDiscountPercent";

-- Step 16: Create function to get AMC-eligible products for customer
CREATE OR REPLACE FUNCTION get_amc_eligible_products(p_customer_id UUID)
RETURNS TABLE (
    "ProductId" UUID,
    "ProductName" VARCHAR(200),
    "SKU" VARCHAR(50),
    "AmcAnnualRate" DECIMAL(18,2),
    "WarrantyContractId" UUID,
    "WarrantyEndDate" TIMESTAMP WITH TIME ZONE,
    "InvoiceNumber" VARCHAR(50),
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
        inv."Number" as "InvoiceNumber",
        inv."VoucherDate" as "InvoiceDate"
    FROM "Contracts" c
    INNER JOIN "ContractCoverageRules" ccr ON c."Oid" = ccr."ContractId"
    INNER JOIN "Items" i ON ccr."InventoryItemId" = i."Oid"
    LEFT JOIN "Vouchers" inv ON c."InvoiceId" = inv."Oid"
    WHERE c."CustomerId" = p_customer_id
      AND c."ContractType" = 0--'Warranty'
      AND c."IsActive" = true
      AND c."ParentContractId" IS NULL  -- Not converted to AMC yet
      AND i."IsAmcEligible" = true
      AND (c."StartDate" + (c."Duration" || ' months')::INTERVAL) <= CURRENT_TIMESTAMP + INTERVAL '30 days'; -- Expiring soon
END;
$$ LANGUAGE plpgsql;