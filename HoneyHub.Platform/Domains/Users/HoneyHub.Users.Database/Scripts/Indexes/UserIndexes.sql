-- =================================================================
-- Indexes for Performance Optimization
-- =================================================================

-- Index for email lookups (authentication) - using NormalizedEmail for case-insensitive lookups
CREATE NONCLUSTERED INDEX [IX_User_NormalizedEmail_Active]
ON [dbo].[User] ([NormalizedEmail])
WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Index for username lookups - using NormalizedUserName for case-insensitive lookups
CREATE NONCLUSTERED INDEX [IX_User_NormalizedUserName_Active]
ON [dbo].[User] ([NormalizedUserName])
WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Index for public ID lookups (API queries)
CREATE NONCLUSTERED INDEX [IX_User_PublicId_Active]
ON [dbo].[User] ([PublicId])
WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Index for last login tracking and analytics
CREATE NONCLUSTERED INDEX [IX_User_LastLoginAt]
ON [dbo].[User] ([LastLoginAt])
WHERE [LastLoginAt] IS NOT NULL AND [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Index for subscription plan queries with filtered coverage for active users
-- Note: Base IX_User_SubscriptionPlanId already exists from User.sql
-- This provides additional filtered coverage with useful INCLUDE columns
CREATE NONCLUSTERED INDEX [IX_User_SubscriptionPlanId_Active]
ON [dbo].[User] ([SubscriptionPlanId])
INCLUDE ([PublicId], [UserName], [Email], [IsActive])
WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO