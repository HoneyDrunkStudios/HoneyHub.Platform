CREATE TABLE [dbo].[User]
(
    -- Primary key
    [Id]                   INT IDENTITY(1,1) NOT NULL
        CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED,

    -- Public identifier
    [PublicId]             UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_User_PublicId] DEFAULT NEWSEQUENTIALID(),

    -- Identity core
    [UserName]             NVARCHAR(256) NOT NULL,
    [NormalizedUserName]   NVARCHAR(256) NOT NULL,
    [Email]                NVARCHAR(256) NOT NULL,
    [NormalizedEmail]      NVARCHAR(256) NOT NULL,
    [EmailConfirmed]       BIT NOT NULL CONSTRAINT [DF_User_EmailConfirmed] DEFAULT (0),

    [PasswordHash]         NVARCHAR(512) NULL,
    [SecurityStamp]        NVARCHAR(40)  NOT NULL CONSTRAINT [DF_User_SecurityStamp] DEFAULT (CONVERT(NVARCHAR(40), NEWID())),
    [ConcurrencyStamp]     NVARCHAR(40)  NOT NULL CONSTRAINT [DF_User_ConcurrencyStamp] DEFAULT (CONVERT(NVARCHAR(40), NEWID())),

    [PhoneNumber]          NVARCHAR(32)  NULL,
    [PhoneNumberConfirmed] BIT NOT NULL CONSTRAINT [DF_User_PhoneNumberConfirmed] DEFAULT (0),

    [TwoFactorEnabled]     BIT NOT NULL CONSTRAINT [DF_User_TwoFactorEnabled] DEFAULT (0),
    [LockoutEnd]           DATETIMEOFFSET NULL,
    [LockoutEnabled]       BIT NOT NULL CONSTRAINT [DF_User_LockoutEnabled] DEFAULT (1),
    [AccessFailedCount]    INT NOT NULL CONSTRAINT [DF_User_AccessFailedCount] DEFAULT (0),

    -- Domain flags
    [IsActive]             BIT NOT NULL CONSTRAINT [DF_User_IsActive] DEFAULT (1),
    [IsDeleted]            BIT NOT NULL CONSTRAINT [DF_User_IsDeleted] DEFAULT (0),

    -- Subscription
    [SubscriptionPlanId]   INT NOT NULL CONSTRAINT [DF_User_SubscriptionPlanId] DEFAULT (1),

    -- Audit
    [CreatedAt]            DATETIME2 NOT NULL CONSTRAINT [DF_User_CreatedAt] DEFAULT SYSUTCDATETIME(),
    [CreatedBy]            NVARCHAR(100) NOT NULL CONSTRAINT [DF_User_CreatedBy] DEFAULT (N'System'),
    [UpdatedAt]            DATETIME2 NULL,
    [UpdatedBy]            NVARCHAR(100) NULL,
    [LastLoginAt]          DATETIME2 NULL,

    -- Constraints / indexes
    CONSTRAINT [UK_User_PublicId] UNIQUE NONCLUSTERED ([PublicId]),
    CONSTRAINT [UK_User_NormalizedUserName] UNIQUE NONCLUSTERED ([NormalizedUserName]),
    CONSTRAINT [UK_User_NormalizedEmail] UNIQUE NONCLUSTERED ([NormalizedEmail]),

    CONSTRAINT [FK_User_SubscriptionPlan]
        FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [dbo].[SubscriptionPlan]([Id]) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX [IX_User_SubscriptionPlanId] ON [dbo].[User]([SubscriptionPlanId]);
GO