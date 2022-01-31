CREATE TABLE [dbo].[TimeLog] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [UserId]       INT           NOT NULL,
    [PunchInTime]  DATETIME2 (7) NOT NULL,
    [PunchOutTime] DATETIME2 (7) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimeLog_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserAccount] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);

