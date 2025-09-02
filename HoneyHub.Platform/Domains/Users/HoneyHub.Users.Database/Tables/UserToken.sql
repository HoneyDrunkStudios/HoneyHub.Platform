CREATE TABLE [dbo].[UserToken]
(
    [UserId]        INT NOT NULL,
    [LoginProvider] NVARCHAR(128) NOT NULL,
    [Name]          NVARCHAR(128) NOT NULL,
    [Value]         NVARCHAR(MAX) NULL,

    CONSTRAINT [PK_UserToken] PRIMARY KEY CLUSTERED ([UserId], [LoginProvider], [Name]),

    CONSTRAINT [FK_UserToken_User]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id]) ON DELETE CASCADE
);
GO