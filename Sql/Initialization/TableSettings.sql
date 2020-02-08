USE [NuScien5]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[nssettings](
	[id] [char](80) NOT NULL,
	[name] [nchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[update] [datetime2](7) NOT NULL,
	[revision] [char](80) NOT NULL,
	[config] [nvarchar](max) NULL,
	[site] [nchar](80) NULL,
 CONSTRAINT [PK_nssettings] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


