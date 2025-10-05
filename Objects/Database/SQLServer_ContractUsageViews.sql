-- SQL Server Views and Functions for Contract Component Usage Tracking
-- These provide real-time usage data without storing duplicate information

-- =====================================================
-- View: Contract Component Usage Summary
-- Aggregates material usage by contract, item, and year
-- =====================================================
CREATE OR ALTER VIEW [ContractComponentUsageSummary] AS
SELECT
    c.[Oid] AS [ContractId],
    c.[Title] AS [ContractTitle],
    c.[CustomerId],
    mu.[ItemId] AS [InventoryItemId],
    i.[Name] AS [ItemName],
    YEAR(v.[CreatedAtUtc]) AS [UsageYear],
    SUM(mu.[Quantity]) AS [TotalQuantityUsed],
    SUM(CAST(mu.[Quantity] AS DECIMAL) * mu.[UnitPrice]) AS [TotalValueUsed],
    SUM(mu.[ChargedAmount]) AS [TotalChargedAmount],
    SUM(mu.[WaivedAmount]) AS [TotalWaivedAmount],
    COUNT(*) AS [UsageCount],
    MIN(v.[CreatedAtUtc]) AS [FirstUsageDate],
    MAX(v.[CreatedAtUtc]) AS [LastUsageDate]
FROM [Contracts] c
INNER JOIN [Jobs] j ON j.[PartyId] = c.[CustomerId]
    AND j.[CreatedAtUtc] BETWEEN c.[StartDate] AND c.[EndDate]
    AND j.[IsNotDeleted] = 1
INNER JOIN [MaterialUsages] mu ON mu.[VoucherId] = j.[Oid]
    AND mu.[IsNotDeleted] = 1
INNER JOIN [Vouchers] v ON v.[Oid] = j.[Oid]
INNER JOIN [Items] i ON i.[Oid] = mu.[ItemId]
WHERE c.[IsNotDeleted] = 1
    AND c.[IsActive] = 1
GROUP BY
    c.[Oid],
    c.[Title],
    c.[CustomerId],
    mu.[ItemId],
    i.[Name],
    YEAR(v.[CreatedAtUtc]);
GO

-- =====================================================
-- Function: Get Contract Item Usage for Specific Year
-- Returns quantity and value used for a contract + item combination
-- =====================================================
CREATE OR ALTER FUNCTION [dbo].[GetContractItemUsage](
    @ContractId UNIQUEIDENTIFIER,
    @InventoryItemId UNIQUEIDENTIFIER,
    @Year INT = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        COALESCE(SUM(mu.[Quantity]), 0) AS [QuantityUsed],
        COALESCE(SUM(CAST(mu.[Quantity] AS DECIMAL) * mu.[UnitPrice]), 0) AS [ValueUsed],
        COALESCE(SUM(mu.[ChargedAmount]), 0) AS [ChargedAmount],
        COALESCE(SUM(mu.[WaivedAmount]), 0) AS [WaivedAmount],
        COALESCE(COUNT(*), 0) AS [UsageCount]
    FROM [Contracts] c
    INNER JOIN [Jobs] j ON j.[PartyId] = c.[CustomerId]
        AND YEAR(j.[CreatedAtUtc]) = COALESCE(@Year, YEAR(GETUTCDATE()))
        AND j.[CreatedAtUtc] BETWEEN c.[StartDate] AND c.[EndDate]
        AND j.[IsNotDeleted] = 1
    INNER JOIN [MaterialUsages] mu ON mu.[VoucherId] = j.[Oid]
        AND mu.[ItemId] = @InventoryItemId
        AND mu.[IsNotDeleted] = 1
    WHERE c.[Oid] = @ContractId
        AND c.[IsNotDeleted] = 1
        AND c.[IsActive] = 1
);
GO

-- =====================================================
-- Function: Get Total Contract Usage (All Items) for Year
-- Returns total value used across all items for a contract
-- =====================================================
CREATE OR ALTER FUNCTION [dbo].[GetContractTotalUsage](
    @ContractId UNIQUEIDENTIFIER,
    @Year INT = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        COALESCE(SUM(CAST(mu.[Quantity] AS DECIMAL) * mu.[UnitPrice]), 0) AS [TotalValueUsed],
        COALESCE(SUM(mu.[ChargedAmount]), 0) AS [TotalChargedAmount],
        COALESCE(SUM(mu.[WaivedAmount]), 0) AS [TotalWaivedAmount],
        COALESCE(COUNT(DISTINCT mu.[ItemId]), 0) AS [ItemCount],
        COALESCE(COUNT(*), 0) AS [UsageCount]
    FROM [Contracts] c
    INNER JOIN [Jobs] j ON j.[PartyId] = c.[CustomerId]
        AND YEAR(j.[CreatedAtUtc]) = COALESCE(@Year, YEAR(GETUTCDATE()))
        AND j.[CreatedAtUtc] BETWEEN c.[StartDate] AND c.[EndDate]
        AND j.[IsNotDeleted] = 1
    INNER JOIN [MaterialUsages] mu ON mu.[VoucherId] = j.[Oid]
        AND mu.[IsNotDeleted] = 1
    WHERE c.[Oid] = @ContractId
        AND c.[IsNotDeleted] = 1
        AND c.[IsActive] = 1
);
GO
