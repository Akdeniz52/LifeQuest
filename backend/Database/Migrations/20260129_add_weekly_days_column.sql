-- Add WeeklyDays column to QuestDefinitions table
-- This column stores comma-separated day numbers (0=Sunday, 1=Monday, ..., 6=Saturday)
-- Example: "1,3,5" for Monday, Wednesday, Friday

ALTER TABLE "QuestDefinitions"
ADD COLUMN "WeeklyDays" TEXT NULL;

-- Add comment for documentation
COMMENT ON COLUMN "QuestDefinitions"."WeeklyDays" IS 'Comma-separated day numbers for weekly quests (0=Sunday, 1=Monday, ..., 6=Saturday)';
