-- PostgreSQL Views and Functions for Contract Component Usage Tracking
-- These provide real-time usage data without storing duplicate information

-- =====================================================
-- View: Contract Component Usage Summary
-- Aggregates material usage by contract, item, and year
-- =====================================================
CREATE OR REPLACE VIEW "ContractComponentUsageSummary" AS
SELECT
    c."Oid" AS "ContractId",
    c."Title" AS "ContractTitle",
    c."CustomerId",
    mu."ItemId" AS "InventoryItemId",
    i."Name" AS "ItemName",
    EXTRACT(YEAR FROM v."CreatedAtUtc") AS "UsageYear",
    SUM(mu."Quantity") AS "TotalQuantityUsed",
    SUM(CAST(mu."Quantity" AS DECIMAL) * mu."UnitPrice") AS "TotalValueUsed",
    SUM(mu."ChargedAmount") AS "TotalChargedAmount",
    SUM(mu."WaivedAmount") AS "TotalWaivedAmount",
    COUNT(*) AS "UsageCount",
    MIN(v."CreatedAtUtc") AS "FirstUsageDate",
    MAX(v."CreatedAtUtc") AS "LastUsageDate"
FROM "Contracts" c
INNER JOIN "Jobs" j ON j."PartyId" = c."CustomerId"
    AND j."CreatedAtUtc" BETWEEN c."StartDate" AND c."EndDate"
    AND j."IsNotDeleted" = true
INNER JOIN "MaterialUsages" mu ON mu."VoucherId" = j."Oid"
    AND mu."IsNotDeleted" = true
INNER JOIN "Vouchers" v ON v."Oid" = j."Oid"
INNER JOIN "Items" i ON i."Oid" = mu."ItemId"
WHERE c."IsNotDeleted" = true
    AND c."IsActive" = true
GROUP BY
    c."Oid",
    c."Title",
    c."CustomerId",
    mu."ItemId",
    i."Name",
    EXTRACT(YEAR FROM v."CreatedAtUtc");

-- =====================================================
-- Function: Get Contract Item Usage for Specific Year
-- Returns quantity and value used for a contract + item combination
-- =====================================================
CREATE OR REPLACE FUNCTION "GetContractItemUsage"(
    p_contract_id UUID,
    p_inventory_item_id UUID,
    p_year INT DEFAULT EXTRACT(YEAR FROM CURRENT_DATE)::INT
)
RETURNS TABLE (
    "QuantityUsed" NUMERIC,
    "ValueUsed" NUMERIC,
    "ChargedAmount" NUMERIC,
    "WaivedAmount" NUMERIC,
    "UsageCount" INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COALESCE(SUM(mu."Quantity"), 0)::NUMERIC AS "QuantityUsed",
        COALESCE(SUM(CAST(mu."Quantity" AS DECIMAL) * mu."UnitPrice"), 0)::NUMERIC AS "ValueUsed",
        COALESCE(SUM(mu."ChargedAmount"), 0)::NUMERIC AS "ChargedAmount",
        COALESCE(SUM(mu."WaivedAmount"), 0)::NUMERIC AS "WaivedAmount",
        COALESCE(COUNT(*)::INT, 0) AS "UsageCount"
    FROM "Contracts" c
    INNER JOIN "Jobs" j ON j."PartyId" = c."CustomerId"
        AND EXTRACT(YEAR FROM j."CreatedAtUtc")::INT = p_year
        AND j."CreatedAtUtc" BETWEEN c."StartDate" AND c."EndDate"
        AND j."IsNotDeleted" = true
    INNER JOIN "MaterialUsages" mu ON mu."VoucherId" = j."Oid"
        AND mu."ItemId" = p_inventory_item_id
        AND mu."IsNotDeleted" = true
    WHERE c."Oid" = p_contract_id
        AND c."IsNotDeleted" = true
        AND c."IsActive" = true;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- Function: Get Total Contract Usage (All Items) for Year
-- Returns total value used across all items for a contract
-- =====================================================
CREATE OR REPLACE FUNCTION "GetContractTotalUsage"(
    p_contract_id UUID,
    p_year INT DEFAULT EXTRACT(YEAR FROM CURRENT_DATE)::INT
)
RETURNS TABLE (
    "TotalValueUsed" NUMERIC,
    "TotalChargedAmount" NUMERIC,
    "TotalWaivedAmount" NUMERIC,
    "ItemCount" INT,
    "UsageCount" INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COALESCE(SUM(CAST(mu."Quantity" AS DECIMAL) * mu."UnitPrice"), 0)::NUMERIC AS "TotalValueUsed",
        COALESCE(SUM(mu."ChargedAmount"), 0)::NUMERIC AS "TotalChargedAmount",
        COALESCE(SUM(mu."WaivedAmount"), 0)::NUMERIC AS "TotalWaivedAmount",
        COALESCE(COUNT(DISTINCT mu."ItemId")::INT, 0) AS "ItemCount",
        COALESCE(COUNT(*)::INT, 0) AS "UsageCount"
    FROM "Contracts" c
    INNER JOIN "Jobs" j ON j."PartyId" = c."CustomerId"
        AND EXTRACT(YEAR FROM j."CreatedAtUtc")::INT = p_year
        AND j."CreatedAtUtc" BETWEEN c."StartDate" AND c."EndDate"
        AND j."IsNotDeleted" = true
    INNER JOIN "MaterialUsages" mu ON mu."VoucherId" = j."Oid"
        AND mu."IsNotDeleted" = true
    WHERE c."Oid" = p_contract_id
        AND c."IsNotDeleted" = true
        AND c."IsActive" = true;
END;
$$ LANGUAGE plpgsql;
