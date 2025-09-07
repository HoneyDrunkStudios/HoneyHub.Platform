-- =====================================================================
-- Indexes for Performance Optimization
-- =====================================================================

-- Index for active public plans (common query for plan listings)
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlan_Active_Public]
ON [dbo].[SubscriptionPlan] ([IsActive], [IsPublic], [SortOrder])
INCLUDE ([Id], [Name], [DisplayName], [Price], [Currency], [BillingCycle]);
GO

-- Index for default plan lookup (should be unique but filtered)
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlan_Default]
ON [dbo].[SubscriptionPlan] ([IsDefault])
WHERE [IsDefault] = 1 AND [IsActive] = 1;
GO

-- Index for pricing queries and filtering
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlan_Price_Currency]
ON [dbo].[SubscriptionPlan] ([Currency], [Price])
WHERE [IsActive] = 1 AND [IsPublic] = 1;
GO

-- Index for plan name lookups (for API queries by name)
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlan_Name_Active]
ON [dbo].[SubscriptionPlan] ([Name])
WHERE [IsActive] = 1;
GO