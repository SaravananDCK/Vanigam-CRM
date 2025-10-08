-- =============================================
-- Add Contract Duration Field
-- Date: 2025-10-07
-- Description: Adds Duration enum field to Contract table and removes EndDate column
-- =============================================

-- Add Duration column (stored as integer representing months)
ALTER TABLE "Contracts"
ADD COLUMN "Duration" INTEGER NOT NULL DEFAULT 12; -- Default to Annual (12 months)

-- Optional: Update existing contracts to have Annual duration if they don't have StartDate or EndDate
-- This ensures data consistency for existing records
UPDATE "Contracts"
SET "Duration" = 12
WHERE "Duration" IS NULL OR "Duration" = 0;

-- Remove EndDate column as it's now computed
-- Note: If you want to preserve existing EndDate data, you may want to:
-- 1. First calculate Duration from existing StartDate and EndDate before dropping
-- 2. Or keep EndDate temporarily for data migration

-- Uncomment the following lines after you've migrated any necessary data:
-- ALTER TABLE "Contracts" DROP COLUMN "EndDate";

-- For PostgreSQL: Add a comment to document the Duration values
COMMENT ON COLUMN "Contracts"."Duration" IS 'Contract duration in months: 1=Monthly, 3=Quarterly, 6=SemiAnnual, 12=Annual, 24=Biennial, 36=Triennial';

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
