CREATE TABLE [dbo].[Users] (
    [UserId]   VARCHAR (50) NOT NULL,
    [Password] VARCHAR (50) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

