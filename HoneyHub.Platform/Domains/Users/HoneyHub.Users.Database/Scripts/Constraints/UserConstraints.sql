-- =================================================================
-- Business Rule Constraints
-- =================================================================

-- Email format validation (basic pattern matching)
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_EmailFormat]
CHECK ([Email] LIKE '%_@_%.__%');
GO

-- Normalized email format validation
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_NormalizedEmailFormat]
CHECK ([NormalizedEmail] LIKE '%_@_%.__%');
GO

-- Username length validation (minimum 3 characters)
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_UserNameLength]
CHECK (LEN([UserName]) >= 3);
GO

-- Normalized username length validation
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_NormalizedUserNameLength]
CHECK (LEN([NormalizedUserName]) >= 3);
GO

-- Email and normalized email consistency (both must be provided)
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_EmailConsistency]
CHECK ([Email] IS NOT NULL AND [NormalizedEmail] IS NOT NULL);
GO

-- Username and normalized username consistency (both must be provided)
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_UserNameConsistency]
CHECK ([UserName] IS NOT NULL AND [NormalizedUserName] IS NOT NULL);
GO

-- Ensure active users are not deleted
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_ActiveNotDeleted]
CHECK (([IsActive] = 0) OR ([IsActive] = 1 AND [IsDeleted] = 0));
GO

-- Subscription plan ID validation (must be positive)
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_SubscriptionPlanIdPositive]
CHECK ([SubscriptionPlanId] > 0);
GO

-- Access failed count validation (non-negative)
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_AccessFailedCountNonNegative]
CHECK ([AccessFailedCount] >= 0);
GO

-- Phone number format validation (basic - allows null)
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_PhoneNumberFormat]
CHECK ([PhoneNumber] IS NULL OR LEN([PhoneNumber]) >= 10);
GO

-- Security stamp length validation
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_SecurityStampLength]
CHECK (LEN([SecurityStamp]) >= 10);
GO

-- Concurrency stamp length validation
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [CK_User_ConcurrencyStampLength]
CHECK (LEN([ConcurrencyStamp]) >= 10);
GO