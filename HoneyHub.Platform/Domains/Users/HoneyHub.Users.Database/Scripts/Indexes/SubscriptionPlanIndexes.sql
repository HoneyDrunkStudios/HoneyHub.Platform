-- =================================================================
-- Indexes for Performance Optimization (Idempotent)
-- =================================================================

-- Index for active public plans (common query)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SubscriptionPlan_Active_Public' AND object_id = OBJECT_ID('[dbo].[SubscriptionPlan]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubscriptionPlan_Active_Public
    ON [dbo].[SubscriptionPlan] ([IsActive], [IsPublic], [SortOrder])
    INCLUDE ([Id], [Name], [DisplayName], [Price], [Currency], [BillingCycle]);
    
    PRINT 'Index IX_SubscriptionPlan_Active_Public created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_SubscriptionPlan_Active_Public already exists.';
END

-- Index for default plan lookup
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SubscriptionPlan_Default' AND object_id = OBJECT_ID('[dbo].[SubscriptionPlan]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubscriptionPlan_Default
    ON [dbo].[SubscriptionPlan] ([IsDefault])
    WHERE [IsDefault] = 1 AND [IsActive] = 1;
    
    PRINT 'Index IX_SubscriptionPlan_Default created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_SubscriptionPlan_Default already exists.';
END

-- Index for pricing queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SubscriptionPlan_Price_Currency' AND object_id = OBJECT_ID('[dbo].[SubscriptionPlan]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubscriptionPlan_Price_Currency
    ON [dbo].[SubscriptionPlan] ([Currency], [Price])
    WHERE [IsActive] = 1 AND [IsPublic] = 1;
    
    PRINT 'Index IX_SubscriptionPlan_Price_Currency created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_SubscriptionPlan_Price_Currency already exists.';
END

PRINT 'SubscriptionPlan indexes script completed successfully.';