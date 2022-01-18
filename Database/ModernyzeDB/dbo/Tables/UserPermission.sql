CREATE TABLE [dbo].[UserPermission] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [PermissionId] INT NOT NULL,
    [UserId]       INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserPermission_PermissionID] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_UserPermission_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserAccount] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);

