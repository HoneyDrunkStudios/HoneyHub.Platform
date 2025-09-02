CREATE TABLE [dbo].[Role]
(
    [Id]               INT IDENTITY(1,1) NOT NULL
        CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED,

    [Name]             NVARCHAR(256) NOT NULL,
    [NormalizedName]   NVARCHAR(256) NOT NULL,
    [ConcurrencyStamp] NVARCHAR(40)  NOT NULL CONSTRAINT [DF_Role_ConcurrencyStamp] DEFAULT (CONVERT(NVARCHAR(40), NEWID())),

    CONSTRAINT [UK_Role_NormalizedName] UNIQUE NONCLUSTERED ([NormalizedName])
);
GO