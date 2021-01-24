USE [NuScien5]
GO

IF OBJECT_ID('nssettings') IS NOT NULL 
	DROP TABLE [dbo].[nssettings]
GO

IF OBJECT_ID('nsclients') IS NOT NULL 
	DROP TABLE [dbo].[nsclients]
GO

IF OBJECT_ID('nsusers') IS NOT NULL 
	DROP TABLE [dbo].[nsusers]
GO

IF OBJECT_ID('nsusergroups') IS NOT NULL 
	DROP TABLE [dbo].[nsusergroups]
GO

IF OBJECT_ID('nsusergrouprelas') IS NOT NULL 
	DROP TABLE [dbo].[nsusergrouprelas]
GO

IF OBJECT_ID('nstokens') IS NOT NULL 
	DROP TABLE [dbo].[nstokens]
GO

IF OBJECT_ID('nsauthcodes') IS NOT NULL 
	DROP TABLE [dbo].[nsauthcodes]
GO

IF OBJECT_ID('nsclientperms') IS NOT NULL 
	DROP TABLE [dbo].[nsclientperms]
GO

IF OBJECT_ID('nsuserperms') IS NOT NULL 
	DROP TABLE [dbo].[nsuserperms]
GO

IF OBJECT_ID('nsusergroupperms') IS NOT NULL 
	DROP TABLE [dbo].[nsusergroupperms]
GO

IF OBJECT_ID('nscontents') IS NOT NULL 
	DROP TABLE [dbo].[nscontents]
GO

IF OBJECT_ID('nscontrev') IS NOT NULL 
	DROP TABLE [dbo].[nscontrev]
GO

IF OBJECT_ID('nstemplates') IS NOT NULL 
	DROP TABLE [dbo].[nstemplates]
GO

IF OBJECT_ID('nstemplrev') IS NOT NULL 
	DROP TABLE [dbo].[nstemplrev]
GO

IF OBJECT_ID('nscontcomments') IS NOT NULL 
	DROP TABLE [dbo].[nscontcomments]
GO
