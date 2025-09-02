CREATE TABLE [dbo].[UserClaim]
(
    [Id]         INT IDENTITY(1,1) NOT NULL
        CONSTRAINT [PK_UserClaim] PRIMARY KEY CLUSTERED,

    [UserId]     INT NOT NULL,
    [ClaimType]  NVARCHAR(256) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,

    CONSTRAINT [FK_UserClaim_User]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_UserClaim_UserId] ON [dbo].[UserClaim]([UserId]);
GO