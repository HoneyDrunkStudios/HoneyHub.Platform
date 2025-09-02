CREATE TABLE [dbo].[UserRole]
(
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,

    CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED ([UserId], [RoleId]),

    CONSTRAINT [FK_UserRole_User]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_UserRole_Role]
        FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role]([Id]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_UserRole_RoleId] ON [dbo].[UserRole]([RoleId]);
GO