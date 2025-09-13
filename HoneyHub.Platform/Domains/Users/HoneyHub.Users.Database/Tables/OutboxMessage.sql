CREATE TABLE [dbo].[OutboxMessage]
(
    [Id]                BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT [PK_OutboxMessage] PRIMARY KEY CLUSTERED,

    [PublicId]          UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_OutboxMessage_PublicId] DEFAULT NEWSEQUENTIALID(),

    [AggregateType]     NVARCHAR(100) NOT NULL,     -- e.g. 'User'
    [AggregatePublicId] UNIQUEIDENTIFIER NULL,      -- points to [User].[PublicId] (no FK on purpose)
    [EventType]         NVARCHAR(200) NOT NULL,
    [CorrelationId]     UNIQUEIDENTIFIER NULL,
    [CausationId]       UNIQUEIDENTIFIER NULL,

    [Payload]           NVARCHAR(MAX) NOT NULL,     -- JSON
    [Headers]           NVARCHAR(MAX) NULL,         -- JSON

    [Status]            TINYINT NOT NULL CONSTRAINT [DF_OutboxMessage_Status] DEFAULT (0),
    [Attempts]          INT NOT NULL    CONSTRAINT [DF_OutboxMessage_Attempts] DEFAULT (0),

    [OccurredAt]        DATETIME2 NOT NULL CONSTRAINT [DF_OutboxMessage_OccurredAt]  DEFAULT SYSUTCDATETIME(),
    [AvailableAt]       DATETIME2 NOT NULL CONSTRAINT [DF_OutboxMessage_AvailableAt] DEFAULT SYSUTCDATETIME(),
    [ProcessedAt]       DATETIME2 NULL,

    [LastError]         NVARCHAR(2000) NULL,

    [CreatedBy]         NVARCHAR(100) NOT NULL CONSTRAINT [DF_OutboxMessage_CreatedBy] DEFAULT (N'System'),
    [UpdatedAt]         DATETIME2 NULL,
    [UpdatedBy]         NVARCHAR(100) NULL,
    [IsDeleted]         BIT NOT NULL CONSTRAINT [DF_OutboxMessage_IsDeleted] DEFAULT (0)
);
GO
-- Unique
ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [UK_OutboxMessage_PublicId] UNIQUE NONCLUSTERED ([PublicId]);
GO

-- Status domain: 0=Pending, 1=InFlight, 2=Succeeded, 3=DeadLettered
ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [CHK_OutboxMessage_Status] CHECK ([Status] IN (0,1,2,3));
GO

-- JSON checks
ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [CHK_OutboxMessage_Payload_IsJson] CHECK (ISJSON([Payload]) = 1);
GO

ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [CHK_OutboxMessage_Headers_IsJson] CHECK ([Headers] IS NULL OR ISJSON([Headers]) = 1);
GO