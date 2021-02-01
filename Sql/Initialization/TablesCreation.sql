USE [NuScien5]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/* Settings */
CREATE TABLE [dbo].[nssettings](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[site] [nvarchar](80) NULL,
	[config] [nvarchar](max) NULL,
 CONSTRAINT [PK_nssettings] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nssettings
ON [dbo].[nssettings] ([name], [state], [lastupdate] DESC, [site])


/* Clients */
CREATE TABLE [dbo].[nsclients](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[nickname] [nvarchar](250) NULL,
	[avatar] [nvarchar](250) NULL,
	[credential] [varchar](512) NULL,
	[group] [varchar](80) NULL,
 CONSTRAINT [PK_nsclients] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nsclients
ON [dbo].[nsclients] ([name], [state], [lastupdate] DESC, [group])


/* Users */
CREATE TABLE [dbo].[nsusers](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[nickname] [nvarchar](250) NULL,
	[avatar] [nvarchar](250) NULL,
	[password] [varchar](256) NULL,
	[gender] [int] NOT NULL,
	[birthday] [datetime2](7) NULL,
	[market] [varchar](20) NULL,
 CONSTRAINT [PK_nsusers] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nsusers
ON [dbo].[nsusers] ([name], [state], [lastupdate] DESC, [nickname])


/* User groups */
CREATE TABLE [dbo].[nsusergroups](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[nickname] [nvarchar](250) NULL,
	[avatar] [nvarchar](250) NULL,
	[site] [nvarchar](80) NULL,
	[membership] [int] NOT NULL,
	[visible] [int] NOT NULL,
 CONSTRAINT [PK_nsusergroups] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nsusergroups
ON [dbo].[nsusergroups] ([name], [state], [lastupdate] DESC, [nickname], [site])


/* User group relatonships */
CREATE TABLE [dbo].[nsusergrouprelas](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[owner] [varchar](80) NOT NULL,
	[res] [varchar](80) NOT NULL,
	[role] [int] NOT NULL,
 CONSTRAINT [PK_nsusergrouprelas] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nsusergrouprelas
ON [dbo].[nsusergrouprelas] ([name], [state], [lastupdate] DESC, [owner])


/* Access tokens */
CREATE TABLE [dbo].[nstokens](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[refreshtoken] [nvarchar](250) NULL,
	[expiration] [datetime2](7) NOT NULL,
	[user] [varchar](80) NULL,
	[client] [varchar](80) NULL,
	[granttype] [varchar](80) NULL,
	[scope] [nvarchar](max) NULL,
 CONSTRAINT [PK_nstokens] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nstokens
ON [dbo].[nstokens] ([name], [state], [lastupdate] DESC, [user])


/* Autherization codes */
CREATE TABLE [dbo].[nsauthcodes](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[owner] [varchar](80) NOT NULL,
	[kind] [int] NOT NULL,
	[avatar] [nvarchar](250) NULL,
	[code] [nvarchar](250) NULL,
	[provider] [nvarchar](250) NULL,
 CONSTRAINT [PK_nsauthcodes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE INDEX IX_nsauthcodes
ON [dbo].[nsauthcodes] ([name], [state], [lastupdate] DESC, [owner])


/* Client permissions */
CREATE TABLE [dbo].[nsclientperms](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[site] [nvarchar](80) NULL,
	[target] [varchar](80) NOT NULL,
	[permissions] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsclientperms] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nsclientperms
ON [dbo].[nsclientperms] ([name], [state], [lastupdate] DESC, [site])


/* User permissions */
CREATE TABLE [dbo].[nsuserperms](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[site] [nvarchar](80) NULL,
	[target] [varchar](80) NOT NULL,
	[permissions] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsuserperms] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nsuserperms
ON [dbo].[nsuserperms] ([name], [state], [lastupdate] DESC, [site])


/* User group permissions */
CREATE TABLE [dbo].[nsusergroupperms](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[site] [nvarchar](80) NULL,
	[target] [varchar](80) NOT NULL,
	[permissions] [nvarchar](max) NULL,
 CONSTRAINT [PK_nsusergroupperms] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nsusergroupperms
ON [dbo].[nsusergroupperms] ([name], [state], [lastupdate] DESC, [site])


/* Contents */
CREATE TABLE [dbo].[nscontents](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[site] [nvarchar](80) NULL,
	[intro] [varchar](250) NULL,
	[parent] [varchar](80) NULL,
	[publisher] [varchar](80) NOT NULL,
	[thumb] [varchar](250) NULL,
	[templ] [nvarchar](80) NULL,
	[keywords] [nvarchar](250) NULL,
	[content] [nvarchar](max) NULL,
	[templc] [nvarchar](max) NULL,
	[creator] [varchar](80) NOT NULL,
 CONSTRAINT [PK_nscontents] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nscontents
ON [dbo].[nscontents] ([name], [state], [lastupdate] DESC, [site], [parent], [intro], [publisher], [keywords])


/* Content revisions */
CREATE TABLE [dbo].[nscontrev](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[site] [nvarchar](80) NULL,
	[owner] [varchar](80) NOT NULL,
	[intro] [varchar](250) NULL,
	[parent] [varchar](80) NULL,
	[publisher] [varchar](80) NOT NULL,
	[thumb] [varchar](250) NULL,
	[templ] [nvarchar](80) NULL,
	[keywords] [nvarchar](250) NULL,
	[content] [nvarchar](max) NULL,
	[templc] [nvarchar](max) NULL,
	[message] [varchar](250) NULL,
 CONSTRAINT [PK_nscontrev] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nscontrev
ON [dbo].[nscontrev] ([name], [state], [lastupdate] DESC, [site], [owner], [publisher])


/* Content templates */
CREATE TABLE [dbo].[nstemplates](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[site] [nvarchar](80) NULL,
	[intro] [varchar](250) NULL,
	[publisher] [varchar](80) NOT NULL,
	[thumb] [varchar](250) NULL,
	[content] [nvarchar](max) NULL,
	[creator] [varchar](80) NOT NULL,
 CONSTRAINT [PK_nstemplates] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nstemplates
ON [dbo].[nstemplates] ([name], [state], [lastupdate] DESC, [site], [intro], [publisher])


/* Content template revisions */
CREATE TABLE [dbo].[nstemplrev](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[site] [nvarchar](80) NULL,
	[owner] [varchar](80) NOT NULL,
	[intro] [varchar](250) NULL,
	[publisher] [varchar](80) NOT NULL,
	[thumb] [varchar](250) NULL,
	[content] [nvarchar](max) NULL,
	[message] [varchar](250) NULL,
CONSTRAINT [PK_nstemplrev] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nstemplrev
ON [dbo].[nstemplrev] ([name], [state], [lastupdate] DESC, [site], [owner], [publisher])


/* Content comments */
CREATE TABLE [dbo].[nscontcomments](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[owner] [varchar](80) NOT NULL,
	[publisher] [varchar](80) NOT NULL,
	[parent] [varchar](80) NULL,
	[ancestor] [varchar](80) NULL,
	[content] [nvarchar](max) NULL,
 CONSTRAINT [PK_nscontcomments] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_nscontcomments
ON [dbo].[nscontcomments] ([name], [state], [lastupdate] DESC, [parent], [owner], [publisher])
