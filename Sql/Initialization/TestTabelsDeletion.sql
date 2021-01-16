USE [NuScien5]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[testcustomer]') AND type in (N'U'))
DROP TABLE [dbo].[testcustomer]
GO
