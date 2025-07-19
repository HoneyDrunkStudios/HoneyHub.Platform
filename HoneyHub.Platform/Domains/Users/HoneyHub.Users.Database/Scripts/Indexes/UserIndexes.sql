-- =================================================================
-- Indexes for Performance Optimization (Idempotent)
-- =================================================================

-- Index for email lookups (authentication)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_User_Email_Active' AND object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_User_Email_Active
    ON [dbo].[User] ([Email])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
    
    PRINT 'Index IX_Users_User_Email_Active created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Users_User_Email_Active already exists.';
END

-- Index for username lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_User_Username_Active' AND object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_User_Username_Active
    ON [dbo].[User] ([Username])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
    
    PRINT 'Index IX_Users_User_Username_Active created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Users_User_Username_Active already exists.';
END

-- Index for provider authentication
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_User_Provider_ProviderId' AND object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_User_Provider_ProviderId
    ON [dbo].[User] ([Provider], [ProviderId])
    WHERE [Provider] IS NOT NULL AND [IsActive] = 1 AND [IsDeleted] = 0;
    
    PRINT 'Index IX_Users_User_Provider_ProviderId created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Users_User_Provider_ProviderId already exists.';
END

-- Index for subscription plan queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_User_SubscriptionPlanId' AND object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_User_SubscriptionPlanId
    ON [dbo].[User] ([SubscriptionPlanId])
    INCLUDE ([PublicId], [Username], [Email], [IsActive]);
    
    PRINT 'Index IX_Users_User_SubscriptionPlanId created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Users_User_SubscriptionPlanId already exists.';
END

PRINT 'User indexes script completed successfully.';