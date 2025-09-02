CREATE TABLE [dbo].[RefreshToken]
(
    [Id]                   INT IDENTITY(1,1) NOT NULL
        CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED,

    [UserId]               INT NOT NULL,
    [TokenHash]            NVARCHAR(512) NOT NULL,
    [ReplacedByTokenHash]  NVARCHAR(512) NULL,

    [ExpiresAt]            DATETIME2 NOT NULL,
    [CreatedAt]            DATETIME2 NOT NULL CONSTRAINT [DF_RefreshToken_CreatedAt] DEFAULT SYSUTCDATETIME(),
    [CreatedByIp]          NVARCHAR(45) NULL,

    [RevokedAt]            DATETIME2 NULL,
    [RevokedByIp]          NVARCHAR(45) NULL,
    [ReasonRevoked]        NVARCHAR(256) NULL,

    CONSTRAINT [FK_RefreshToken_User]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id]) ON DELETE CASCADE
);
GO

-- Ensure single storage per hash and speed lookups
CREATE UNIQUE NONCLUSTERED INDEX [UK_RefreshToken_TokenHash] ON [dbo].[RefreshToken]([TokenHash]);
GO
CREATE NONCLUSTERED INDEX [IX_RefreshToken_UserId] ON [dbo].[RefreshToken]([UserId]);
GO