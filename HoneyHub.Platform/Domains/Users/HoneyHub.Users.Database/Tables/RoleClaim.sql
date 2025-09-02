CREATE TABLE [dbo].[RoleClaim]
(
    [Id]         INT IDENTITY(1,1) NOT NULL
        CONSTRAINT [PK_RoleClaim] PRIMARY KEY CLUSTERED,

    [RoleId]     INT NOT NULL,
    [ClaimType]  NVARCHAR(256) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,

    CONSTRAINT [FK_RoleClaim_Role]
        FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role]([Id]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_RoleClaim_RoleId] ON [dbo].[RoleClaim]([RoleId]);
GO