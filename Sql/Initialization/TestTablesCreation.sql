USE [NuScien5]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/* Customers */
CREATE TABLE [dbo].[testcustomers](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[address] [nvarchar](255) NULL,
	[phone] [nvarchar](20) NULL,
	[site] [nvarchar](80) NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
 CONSTRAINT [PK_testcustomers] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_testcustomers
ON [dbo].[testcustomers] ([name], [state], [lastupdate] DESC, [site])


/* Goods */
CREATE TABLE [dbo].[testgoods](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[image] [nvarchar](255) NULL,
	[site] [nvarchar](80) NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
 CONSTRAINT [PK_testgoods] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_testgoods
ON [dbo].[testgoods] ([name], [state], [lastupdate] DESC, [site])

