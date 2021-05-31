USE [NuScien5]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/* Contacts */
CREATE TABLE [dbo].[nscontacts](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[owner] [nvarchar](80) NOT NULL,
	[res] [nvarchar](80) NULL,
	[config] [nvarchar](max) NULL,
 CONSTRAINT [PK_nscontacts] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nscontacts
ON [dbo].[nscontacts] ([name], [state], [lastupdate] DESC, [owner], [res])


/* Blogs */
CREATE TABLE [dbo].[nsblogs](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[kind] [int] NOT NULL,
	[owner] [nvarchar](80) NOT NULL,
	[publisher] [nvarchar](80) NULL,
	[intro] [nvarchar](250) NULL,
	[category] [nvarchar](200) NULL,
	[keywords] [nvarchar](250) NULL,
	[thumb] [nvarchar](250) NULL,
	[content] [nvarchar](max) NULL,
	[config] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsblogs] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nsblogs
ON [dbo].[nsblogs] ([name], [state], [lastupdate] DESC, [kind], [owner], [publisher], [category], [keywords])


/* Blog comments */
CREATE TABLE [dbo].[nsblogcomments](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[owner] [nvarchar](80) NOT NULL,
	[publisher] [nvarchar](80) NULL,
	[parent] [nvarchar](80) NULL,
	[ancestor] [nvarchar](80) NULL,
	[config] [nvarchar](max) NULL,
	[content] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsblogcomments] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nsblogcomments
ON [dbo].[nsblogcomments] ([name], [state], [lastupdate] DESC, [owner], [publisher], [parent], [ancestor])


/* User activities */
CREATE TABLE [dbo].[nsuseractivities](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[owner] [nvarchar](80) NOT NULL,
	[publisher] [nvarchar](80) NULL,
	[parent] [nvarchar](80) NULL,
	[ancestor] [nvarchar](80) NULL,
	[config] [nvarchar](max) NULL,
	[content] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsuseractivities] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nsuseractivities
ON [dbo].[nsuseractivities] ([name], [state], [lastupdate] DESC, [owner], [publisher], [parent], [ancestor])


/* User group activities */
CREATE TABLE [dbo].[nsgroupactivities](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[owner] [nvarchar](80) NOT NULL,
	[publisher] [nvarchar](80) NULL,
	[parent] [nvarchar](80) NULL,
	[ancestor] [nvarchar](80) NULL,
	[config] [nvarchar](max) NULL,
	[content] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsgroupactivities] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nsgroupactivities
ON [dbo].[nsgroupactivities] ([name], [state], [lastupdate] DESC, [owner], [publisher], [parent], [ancestor])


/* Sent mails */
CREATE TABLE [dbo].[nsmails1](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[kind] [int] NOT NULL,
	[owner] [nvarchar](80) NOT NULL,
	[thread] [nvarchar](80) NULL,
	[folder] [nvarchar](250) NULL,
	[send] [datetime2](7) NOT NULL,
	[sender] [nvarchar](250) NULL,
	[addr] [nvarchar](250) NULL,
	[app] [nvarchar](250) NULL,
	[priority] [int] NOT NULL,
	[config] [nvarchar](max) NULL,
	[content] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsmails1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nsmails1
ON [dbo].[nsmails1] ([name], [state], [lastupdate] DESC, [kind], [owner], [send], [priority], [thread], [folder])


/* Received mails */
CREATE TABLE [dbo].[nsmails2](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[kind] [int] NOT NULL,
	[owner] [nvarchar](80) NOT NULL,
	[res] [nvarchar](80) NULL,
	[flag] [int] NOT NULL,
	[send] [datetime2](7) NOT NULL,
	[sender] [nvarchar](200) NULL,
	[addr] [nvarchar](250) NULL,
	[thread] [nvarchar](80) NULL,
	[folder] [nvarchar](250) NULL,
	[priority] [int] NOT NULL,
	[config] [nvarchar](max) NULL,
	[content] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsmails2] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nsmails2
ON [dbo].[nsmails2] ([name], [state], [lastupdate] DESC, [kind], [owner], [send], [sender], [addr], [priority], [thread], [folder])

