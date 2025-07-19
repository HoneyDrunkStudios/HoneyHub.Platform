-- =================================================================
-- Business Rule Constraints (Idempotent)
-- =================================================================

PRINT 'Starting User constraints creation...';

-- Ensure either password or provider authentication
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_User_AuthenticationMethod' AND parent_object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    ALTER TABLE [dbo].[User]
    ADD CONSTRAINT CK_Users_User_AuthenticationMethod
    CHECK (
        ([PasswordHash] IS NOT NULL) OR 
        ([Provider] IS NOT NULL AND [ProviderId] IS NOT NULL)
    );
    
    PRINT 'Constraint CK_Users_User_AuthenticationMethod created successfully.';
END
ELSE
BEGIN
    PRINT 'Constraint CK_Users_User_AuthenticationMethod already exists.';
END

-- Ensure provider and provider ID are both present or both null
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_User_ProviderConsistency' AND parent_object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    ALTER TABLE [dbo].[User]
    ADD CONSTRAINT CK_Users_User_ProviderConsistency
    CHECK (
        ([Provider] IS NULL AND [ProviderId] IS NULL) OR
        ([Provider] IS NOT NULL AND [ProviderId] IS NOT NULL)
    );
    
    PRINT 'Constraint CK_Users_User_ProviderConsistency created successfully.';
END
ELSE
BEGIN
    PRINT 'Constraint CK_Users_User_ProviderConsistency already exists.';
END

-- Email format validation
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_User_EmailFormat' AND parent_object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    ALTER TABLE [dbo].[User]
    ADD CONSTRAINT CK_Users_User_EmailFormat
    CHECK ([Email] LIKE '%_@_%.__%');
    
    PRINT 'Constraint CK_Users_User_EmailFormat created successfully.';
END
ELSE
BEGIN
    PRINT 'Constraint CK_Users_User_EmailFormat already exists.';
END

-- Username length validation
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_User_UsernameLength' AND parent_object_id = OBJECT_ID('[dbo].[User]'))
BEGIN
    ALTER TABLE [dbo].[User]
    ADD CONSTRAINT CK_Users_User_UsernameLength
    CHECK (LEN([Username]) >= 3);
    
    PRINT 'Constraint CK_Users_User_UsernameLength created successfully.';
END
ELSE
BEGIN
    PRINT 'Constraint CK_Users_User_UsernameLength already exists.';
END

PRINT 'User constraints script completed successfully.';