USE [NuScien5]
GO

IF OBJECT_ID('nscontacts') IS NOT NULL 
	DROP TABLE [dbo].[nscontacts]
GO

IF OBJECT_ID('nsblogs') IS NOT NULL 
	DROP TABLE [dbo].[nsblogs]
GO

IF OBJECT_ID('nsblogcomments') IS NOT NULL 
	DROP TABLE [dbo].[nsblogcomments]
GO

IF OBJECT_ID('nsuseractivities') IS NOT NULL 
	DROP TABLE [dbo].[nsuseractivities]
GO

IF OBJECT_ID('nsgroupactivities') IS NOT NULL 
	DROP TABLE [dbo].[nsgroupactivities]
GO

IF OBJECT_ID('nsmails1') IS NOT NULL 
	DROP TABLE [dbo].[nsmails1]
GO

IF OBJECT_ID('nsmails2') IS NOT NULL 
	DROP TABLE [dbo].[nsmails2]
GO
