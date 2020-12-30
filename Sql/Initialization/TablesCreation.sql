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
	[update] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[site] [nvarchar](80) NULL,
 CONSTRAINT [PK_nssettings] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/* Clients */
CREATE TABLE [dbo].[nsclients](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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

/* Users */
CREATE TABLE [dbo].[nsusers](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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

/* User groups */
CREATE TABLE [dbo].[nsusergroups](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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


/* User group relatonships */
CREATE TABLE [dbo].[nsusergrouprelas](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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

/* Access tokens */
CREATE TABLE [dbo].[nstokens](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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

/* Autherization codes */
CREATE TABLE [dbo].[nsauthcodes](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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

/* Client permissions */
CREATE TABLE [dbo].[nsclientperms](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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

/* User permissions */
CREATE TABLE [dbo].[nsuserperms](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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

/* User group permissions */
CREATE TABLE [dbo].[nsusergroupperms](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
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


