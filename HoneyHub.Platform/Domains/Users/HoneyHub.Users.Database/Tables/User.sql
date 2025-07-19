CREATE TABLE [dbo].[User]
(
	 -- Primary Identity
    [Id]                    INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Users_User PRIMARY KEY CLUSTERED,
    
    -- Public-facing identifier (non-enumerable)
    [PublicId]              UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Users_User_PublicId DEFAULT NEWSEQUENTIALID()
        CONSTRAINT UK_Users_User_PublicId UNIQUE NONCLUSTERED,
    
    -- Core Identity Fields
    [Username]              NVARCHAR(50) NOT NULL
        CONSTRAINT UK_Users_User_Username UNIQUE NONCLUSTERED,
    
    [Email]                 NVARCHAR(256) NOT NULL
        CONSTRAINT UK_Users_User_Email UNIQUE NONCLUSTERED,
    
    -- Authentication Fields
    [PasswordHash]          NVARCHAR(512) NULL,        -- NULL for OAuth-only accounts
    [Provider]              NVARCHAR(50) NULL,         -- OAuth provider (Google, Microsoft, etc.)
    [ProviderId]            NVARCHAR(256) NULL,        -- External provider user ID
    [RefreshTokenHash]      NVARCHAR(MAX) NULL,        -- Hashed JWT refresh token storage
    
    -- Account Status Fields
    [EmailVerified]         BIT NOT NULL
        CONSTRAINT DF_Users_User_EmailVerified DEFAULT 0,
    
    [IsActive]              BIT NOT NULL
        CONSTRAINT DF_Users_User_IsActive DEFAULT 1,
    
    [IsDeleted]             BIT NOT NULL
        CONSTRAINT DF_Users_User_IsDeleted DEFAULT 0,
    
    -- Subscription Management
    [SubscriptionPlanId]    INT NOT NULL
        CONSTRAINT DF_Users_User_SubscriptionPlanId DEFAULT 1,
    
    -- Audit Fields
    [CreatedAt]             DATETIME2 NOT NULL
        CONSTRAINT DF_Users_User_CreatedAt DEFAULT SYSUTCDATETIME(),
    
    [CreatedBy]             NVARCHAR(100) NOT NULL
        CONSTRAINT DF_Users_User_CreatedBy DEFAULT 'System',
    
    [UpdatedAt]             DATETIME2 NULL,
    [UpdatedBy]             NVARCHAR(100) NULL,
    [LastLoginAt]           DATETIME2 NULL,
    
    -- Foreign Key Constraints
    CONSTRAINT FK_Users_User_SubscriptionPlan 
        FOREIGN KEY ([SubscriptionPlanId]) 
        REFERENCES [dbo].[SubscriptionPlan]([Id])
);
