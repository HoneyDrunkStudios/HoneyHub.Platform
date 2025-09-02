CREATE TABLE [dbo].[UserLogin]
(
    [LoginProvider]      NVARCHAR(128) NOT NULL,
    [ProviderKey]        NVARCHAR(256) NOT NULL,
    [ProviderDisplayName] NVARCHAR(256) NULL,
    [UserId]             INT NOT NULL,

    CONSTRAINT [PK_UserLogin] PRIMARY KEY CLUSTERED ([LoginProvider], [ProviderKey]),

    CONSTRAINT [FK_UserLogin_User]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_UserLogin_UserId] ON [dbo].[UserLogin]([UserId]);
GO