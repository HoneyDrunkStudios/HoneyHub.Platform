-- Scripts\Post-Deployment.sql  (SQLCMD mode)
-- This file is compiled into the DACPAC and executed after schema publish.

IF '$(SeedData)' = '1'
BEGIN
    PRINT 'Seeding SubscriptionPlan...';
    :r ./Scripts/Data/SubscriptionPlanDataSeeding.sql
END
ELSE
BEGIN
    PRINT 'SeedData disabled; skipping data seeds.';
END
