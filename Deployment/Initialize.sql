-- Run once against a new ProjectSunrise database on SQL Server 2008 R2 Express.
SET XACT_ABORT ON;
IF DB_NAME() IN ('master','model','msdb','tempdb') OR EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped=0)
BEGIN
    RAISERROR('Select an empty application database before running this script.', 16, 1);
    RETURN;
END;
BEGIN TRANSACTION;
create table [dbo].[AspNetUsers] (
    [Id] [nvarchar](128) not null,
    [Nickname] [nvarchar](30) not null,
    [Bio] [nvarchar](500) null,
    [Avatar] [varbinary](max) null,
    [Joined] [datetime] not null,
    [Email] [nvarchar](256) null,
    [EmailConfirmed] [bit] not null,
    [PasswordHash] [nvarchar](max) null,
    [SecurityStamp] [nvarchar](max) null,
    [PhoneNumber] [nvarchar](max) null,
    [PhoneNumberConfirmed] [bit] not null,
    [TwoFactorEnabled] [bit] not null,
    [LockoutEndDateUtc] [datetime] null,
    [LockoutEnabled] [bit] not null,
    [AccessFailedCount] [int] not null,
    [UserName] [nvarchar](256) not null,
    primary key ([Id])
);
create table [dbo].[Categories] (
    [Id] [int] not null identity,
    [Name] [nvarchar](40) not null,
    [Description] [nvarchar](max) null,
    primary key ([Id])
);
create table [dbo].[Conversations] (
    [Id] [int] not null identity,
    [FirstId] [nvarchar](128) not null,
    [SecondId] [nvarchar](128) not null,
    [Updated] [datetime] not null,
    primary key ([Id])
);
create table [dbo].[AspNetRoles] (
    [Id] [nvarchar](128) not null,
    [Name] [nvarchar](256) not null,
    primary key ([Id])
);
create table [dbo].[AspNetUserClaims] (
    [Id] [int] not null identity,
    [UserId] [nvarchar](128) not null,
    [ClaimType] [nvarchar](max) null,
    [ClaimValue] [nvarchar](max) null,
    primary key ([Id])
);
create table [dbo].[AspNetUserLogins] (
    [LoginProvider] [nvarchar](128) not null,
    [ProviderKey] [nvarchar](128) not null,
    [UserId] [nvarchar](128) not null,
    primary key ([LoginProvider], [ProviderKey], [UserId])
);
create table [dbo].[AspNetUserRoles] (
    [UserId] [nvarchar](128) not null,
    [RoleId] [nvarchar](128) not null,
    primary key ([UserId], [RoleId])
);
create table [dbo].[Letters] (
    [Id] [int] not null identity,
    [ConversationId] [int] not null,
    [SenderId] [nvarchar](128) not null,
    [Body] [nvarchar](max) not null,
    [Created] [datetime] not null,
    primary key ([Id])
);
create table [dbo].[Notices] (
    [Id] [int] not null identity,
    [RecipientId] [nvarchar](128) not null,
    [ActorId] [nvarchar](128) not null,
    [RoomId] [int] null,
    [PostId] [int] null,
    [LetterId] [int] null,
    [Kind] [nvarchar](20) not null,
    [Body] [nvarchar](255) not null,
    [IsRead] [bit] not null,
    [Created] [datetime] not null,
    primary key ([Id])
);
create table [dbo].[Reactions] (
    [PostId] [int] not null,
    [UserId] [nvarchar](128) not null,
    [Kind] [nvarchar](10) not null,
    primary key ([PostId], [UserId], [Kind])
);
create table [dbo].[Rooms] (
    [Id] [int] not null identity,
    [Name] [nvarchar](100) not null,
    [Description] [nvarchar](500) not null,
    [OwnerId] [nvarchar](128) not null,
    [CategoryId] [int] null,
    [Visibility] [nvarchar](20) not null,
    [Posting] [nvarchar](20) not null,
    [Joining] [nvarchar](20) not null,
    primary key ([Id])
);
create table [dbo].[RoomPosts] (
    [Id] [int] not null identity,
    [RoomId] [int] not null,
    [SenderId] [nvarchar](128) not null,
    [Body] [nvarchar](max) not null,
    [Created] [datetime] not null,
    primary key ([Id])
);
create table [dbo].[RoomSeats] (
    [RoomId] [int] not null,
    [UserId] [nvarchar](128) not null,
    [Pending] [bit] not null,
    [Moderator] [bit] not null,
    primary key ([RoomId], [UserId])
);
create table [dbo].[Tags] (
    [Id] [int] not null identity,
    [Name] [nvarchar](30) not null,
    primary key ([Id])
);
create table [dbo].[TagRooms] (
    [Tag_Id] [int] not null,
    [Room_Id] [int] not null,
    primary key ([Tag_Id], [Room_Id])
);
alter table [dbo].[AspNetUserClaims] add constraint [ApplicationUser_Claims] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[AspNetUserLogins] add constraint [ApplicationUser_Logins] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[AspNetUserRoles] add constraint [ApplicationUser_Roles] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Conversations] add constraint [Conversation_First] foreign key ([FirstId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Conversations] add constraint [Conversation_Second] foreign key ([SecondId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[AspNetUserRoles] add constraint [IdentityRole_Users] foreign key ([RoleId]) references [dbo].[AspNetRoles]([Id]);
alter table [dbo].[Letters] add constraint [Letter_Conversation] foreign key ([ConversationId]) references [dbo].[Conversations]([Id]) on delete cascade;
alter table [dbo].[Letters] add constraint [Letter_Sender] foreign key ([SenderId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Notices] add constraint [Notice_Actor] foreign key ([ActorId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Notices] add constraint [Notice_Letter] foreign key ([LetterId]) references [dbo].[Letters]([Id]);
alter table [dbo].[Notices] add constraint [Notice_Post] foreign key ([PostId]) references [dbo].[RoomPosts]([Id]);
alter table [dbo].[Notices] add constraint [Notice_Recipient] foreign key ([RecipientId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Notices] add constraint [Notice_Room] foreign key ([RoomId]) references [dbo].[Rooms]([Id]);
alter table [dbo].[Reactions] add constraint [Reaction_Post] foreign key ([PostId]) references [dbo].[RoomPosts]([Id]) on delete cascade;
alter table [dbo].[Reactions] add constraint [Reaction_User] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Rooms] add constraint [Room_Category] foreign key ([CategoryId]) references [dbo].[Categories]([Id]);
alter table [dbo].[Rooms] add constraint [Room_Owner] foreign key ([OwnerId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[RoomPosts] add constraint [RoomPost_Room] foreign key ([RoomId]) references [dbo].[Rooms]([Id]) on delete cascade;
alter table [dbo].[RoomPosts] add constraint [RoomPost_Sender] foreign key ([SenderId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[RoomSeats] add constraint [RoomSeat_Room] foreign key ([RoomId]) references [dbo].[Rooms]([Id]) on delete cascade;
alter table [dbo].[RoomSeats] add constraint [RoomSeat_User] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[TagRooms] add constraint [Tag_Rooms_Source] foreign key ([Tag_Id]) references [dbo].[Tags]([Id]) on delete cascade;
alter table [dbo].[TagRooms] add constraint [Tag_Rooms_Target] foreign key ([Room_Id]) references [dbo].[Rooms]([Id]) on delete cascade;

CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers(UserName);
CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles(Name);
CREATE UNIQUE INDEX IX_Email ON dbo.AspNetUsers(Email);
CREATE UNIQUE INDEX IX_Nickname ON dbo.AspNetUsers(Nickname);
CREATE UNIQUE INDEX IX_Pair ON dbo.Conversations(FirstId,SecondId);
CREATE UNIQUE INDEX IX_CategoryName ON dbo.Categories(Name);
CREATE UNIQUE INDEX IX_TagName ON dbo.Tags(Name);
CREATE INDEX IX_RoomHistory ON dbo.RoomPosts(RoomId,Id);
CREATE INDEX IX_DirectHistory ON dbo.Letters(ConversationId,Id);
CREATE INDEX IX_Inbox ON dbo.Notices(RecipientId,Id);
CREATE INDEX IX_Membership ON dbo.RoomSeats(UserId,RoomId);
INSERT INTO dbo.AspNetRoles(Id,Name) VALUES
    (CONVERT(nvarchar(128),NEWID()),'Staff'),
    (CONVERT(nvarchar(128),NEWID()),'Administrators'),
    (CONVERT(nvarchar(128),NEWID()),'Moderators'),
    (CONVERT(nvarchar(128),NEWID()),'Room Managers');
COMMIT TRANSACTION;
