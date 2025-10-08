-- =============================================
-- Add Contract Duration Field (SQL Server)
-- Date: 2025-10-07
-- Description: Adds Duration enum field to Contract table and removes EndDate column
-- =============================================

-- Add Duration column (stored as integer representing months)
ALTER TABLE [Contracts]
ADD [Duration] INT NOT NULL DEFAULT 12; -- Default to Annual (12 months)

-- Optional: Update existing contracts to have Annual duration if needed
-- This ensures data consistency for existing records
UPDATE [Contracts]
SET [Duration] = 12
WHERE [Duration] IS NULL OR [Duration] = 0;

-- Remove EndDate column as it's now computed
-- Note: If you want to preserve existing EndDate data, you may want to:
-- 1. First calculate Duration from existing StartDate and EndDate before dropping
-- 2. Or keep EndDate temporarily for data migration

-- Uncomment the following line after you've migrated any necessary data:
-- ALTER TABLE [Contracts] DROP COLUMN [EndDate];

-- For SQL Server: Add extended property to document the Duration values
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Contract duration in months: 1=Monthly, 3=Quarterly, 6=SemiAnnual, 12=Annual, 24=Biennial, 36=Triennial',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Contracts',
    @level2type = N'COLUMN', @level2name = N'Duration';

-- =============================================
-- Migration Notes:
-- =============================================
-- 1. EndDate is now a computed property in the application (not stored in DB)
-- 2. EndDate is calculated as: StartDate + Duration months
-- 3. Duration values map to ContractDuration enum:
--    - Monthly = 1
--    - Quarterly = 3
--    - SemiAnnual = 6
--    - Annual = 12
--    - Biennial = 24
--    - Triennial = 36
-- =============================================
