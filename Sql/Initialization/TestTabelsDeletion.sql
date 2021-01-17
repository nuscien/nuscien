USE [NuScien5]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[testcustomers]') AND type in (N'U'))
DROP TABLE [dbo].[testcustomers]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[testgoods]') AND type in (N'U'))
DROP TABLE [dbo].testgoods
GO
