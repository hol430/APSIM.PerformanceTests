
USE [APSIM.PerformanceTests]
GO

---------------------------------------------------------------------------
--  DROP STORED PROCEDURES
---------------------------------------------------------------------------
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_PredictedObservedDataInsert]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[usp_PredictedObservedDataInsert]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_PredictedObservedDataTwoInsert]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[usp_PredictedObservedDataTwoInsert]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_PredictedObservedDataThreeInsert]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[usp_PredictedObservedDataThreeInsert]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_SimulationsInsert]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[usp_SimulationsInsert]
GO

PRINT 'STORED PROCEDURES DROPPED'
GO

---------------------------------------------------------------------------
--  DROP USER-DEFINED TABLE TYPES
---------------------------------------------------------------------------
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'PredictedObservedDataTableType' AND ss.name = N'dbo')
	DROP TYPE [dbo].[PredictedObservedDataTableType]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'PredictedObservedDataTwoTableType' AND ss.name = N'dbo')
	DROP TYPE [dbo].[PredictedObservedDataTwoTableType]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'PredictedObservedDataThreeTableType' AND ss.name = N'dbo')
	DROP TYPE [dbo].[PredictedObservedDataThreeTableType]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'SimulationDataTableType' AND ss.name = N'dbo')
	DROP TYPE [dbo].[SimulationDataTableType]
GO
	
PRINT 'USER-DEFINED TABLE TYPES DROPPED'
GO

---------------------------------------------------------------------------
--  DROP ADDITIONAL INDEXES 
---------------------------------------------------------------------------
IF EXISTS (SELECT name FROM sys.indexes 
			WHERE name = N'IX_PredictedObservedValues_PredictedObservedDetailsID_ID')  
	DROP INDEX [IX_PredictedObservedValues_PredictedObservedDetailsID_ID] ON [dbo].[PredictedObservedValues];
GO

IF EXISTS (SELECT name FROM sys.indexes
			WHERE name = N'IX_PredictedObservedDetails_SimulationsID_ID')  
	DROP INDEX [IX_PredictedObservedDetails_SimulationsID_ID] ON [dbo].[PredictedObservedDetails];
GO

IF EXISTS (SELECT name FROM sys.indexes 
			WHERE name = N'IX_Simulations_ApsimFilesID_ID')  
	DROP INDEX [IX_Simulations_ApsimFilesID_ID] ON [dbo].[Simulations];
GO
	
PRINT 'INDEXES DROPPED'
GO


---------------------------------------------------------------------------
--  DROP FOREIGN KEY RELATIONSHIPS 
---------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.foreign_keys 
			WHERE object_id = OBJECT_ID(N'dbo.FK_PredictedObservedValues_PredictedObservedDetails') 
			AND parent_object_id = OBJECT_ID(N'dbo.PredictedObservedValues'))
	ALTER TABLE [dbo].[PredictedObservedValues] DROP CONSTRAINT [FK_PredictedObservedValues_PredictedObservedDetails];
GO

IF EXISTS (SELECT * FROM sys.foreign_keys 
			WHERE object_id = OBJECT_ID(N'dbo.FK_PredictedObservedDetails_Simulations') 
			AND parent_object_id = OBJECT_ID(N'dbo.PredictedObservedDetails'))
	ALTER TABLE [dbo].[PredictedObservedDetails] DROP CONSTRAINT [FK_PredictedObservedDetails_Simulations];
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_Simulations_ApsimFiles') 
			AND parent_object_id = OBJECT_ID(N'dbo.Simulations'))
	ALTER TABLE [dbo].[Simulations] DROP CONSTRAINT [FK_Simulations_ApsimFiles];
GO

PRINT 'FOREIGN KEY RELATIONSHIPS DROPPED'
GO

---------------------------------------------------------------------------
--  DROP TABLES  
---------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PredictedObservedValues]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[PredictedObservedValues];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PredictedObservedDetails]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[PredictedObservedDetails];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Simulations]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[Simulations];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApsimFiles]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[ApsimFiles];
GO
	
PRINT 'TABLES DROPPED'
GO

PRINT '--------------------------------------------------------------------------'
GO
PRINT '--------------------------------------------------------------------------'
GO



---------------------------------------------------------------------------
--TO CREATE ALL TABLES, INDEXES AND FOREIGN KEY RELATIONSHIPS
---------------------------------------------------------------------------
USE [APSIM.PerformanceTests]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

---------------------------------------------------------------------------
CREATE TABLE [dbo].[ApsimFiles](
	[ID] [int] IDENTITY(1,1),
	[PullRequestId] [int] NOT NULL,
	[FileName] [nvarchar](50) NOT NULL,
	[FullFileName] [nvarchar](200) NOT NULL,
	[RunDate] [datetime] NOT NULL,
	[IsReleased] [bit] NULL,
 CONSTRAINT [PK_ApsimFiles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

PRINT '[ApsimFiles] TABLES CREATED'
GO

---------------------------------------------------------------------------
CREATE TABLE [dbo].[Simulations](
	[ID] [int] IDENTITY(1,1),
	[ApsimFilesID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[OriginalSimulationID] [int] NOT NULL,
 CONSTRAINT [PK_Simulations] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO
--Foreign Key Relationship
ALTER TABLE [dbo].[Simulations]  WITH NOCHECK ADD  CONSTRAINT [FK_Simulations_ApsimFiles] FOREIGN KEY([ApsimFilesID])
REFERENCES [dbo].[ApsimFiles] ([ID])
GO
ALTER TABLE [dbo].[Simulations] CHECK CONSTRAINT [FK_Simulations_ApsimFiles]
GO
--Index on concatinated keys
CREATE UNIQUE NONCLUSTERED INDEX [IX_Simulations_ApsimFilesID_ID] ON [dbo].[Simulations]
(
	[ApsimFilesID] ASC,
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

PRINT '[Simulations] TABLES CREATED'
GO

---------------------------------------------------------------------------
CREATE TABLE [dbo].[PredictedObservedDetails](
	[ID] [int] IDENTITY(1,1),
	[ApsimFilesID] [int] NOT NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[PredictedTableName] [nvarchar](100) NOT NULL,
	[ObservedTableName] [nvarchar](100) NOT NULL,
	[FieldNameUsedForMatch] [nvarchar] (100),
	[FieldName2UsedForMatch] [nvarchar] (100),
	[FieldName3UsedForMatch] [nvarchar] (100),
	
 CONSTRAINT [PK_PredictedObservedDetails] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

--Foreign Key Relationship
ALTER TABLE [dbo].[PredictedObservedDetails]  WITH NOCHECK ADD  CONSTRAINT [FK_PredictedObservedDetails_ApsimFiles] FOREIGN KEY([ApsimFilesID])
REFERENCES [dbo].[ApsimFiles] ([ID])
GO
ALTER TABLE [dbo].[PredictedObservedDetails] CHECK CONSTRAINT [FK_PredictedObservedDetails_ApsimFiles]
GO
--Index on concatinated keys
CREATE UNIQUE NONCLUSTERED INDEX [IX_PredictedObservedDetails_ApsimFilesID_ID] ON [dbo].[PredictedObservedDetails]
(
	[ApsimFilesID] ASC,
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

PRINT '[PredictedObservedDetails] TABLES CREATED'
GO

---------------------------------------------------------------------------
CREATE TABLE [dbo].[PredictedObservedValues](
	[ID] [int] IDENTITY(1,1),
	[PredictedObservedDetailsID] [int] NOT NULL,
	[SimulationsID] [int] NOT NULL,
	[MatchName] [nvarchar](100) NOT NULL,
	[MatchValue] [nvarchar](100) NOT NULL,
	[MatchName2] [nvarchar](100),
	[MatchValue2] [nvarchar](100),
	[MatchName3] [nvarchar](100),
	[MatchValue3] [nvarchar](100),
	[ValueName] [nvarchar](100) NOT NULL,
	[PredictedValue] float,
	[ObservedValue] float,
 CONSTRAINT [PK_PredictedObservedValues] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

--Foreign Key Relationship
ALTER TABLE [dbo].[PredictedObservedValues]  WITH NOCHECK ADD  CONSTRAINT [FK_PredictedObservedValues_PredictedObservedDetails] FOREIGN KEY([PredictedObservedDetailsID])
REFERENCES [dbo].[PredictedObservedDetails] ([ID])
GO
ALTER TABLE [dbo].[PredictedObservedValues] CHECK CONSTRAINT [FK_PredictedObservedValues_PredictedObservedDetails]
GO

ALTER TABLE [dbo].[PredictedObservedValues]  WITH NOCHECK ADD  CONSTRAINT [FK_PredictedObservedValues_Simulations] FOREIGN KEY([SimulationsID])
REFERENCES [dbo].[Simulations] ([ID])
GO
ALTER TABLE [dbo].[PredictedObservedValues] CHECK CONSTRAINT [FK_PredictedObservedValues_Simulations]
GO

--Index on concatinated keys
CREATE UNIQUE NONCLUSTERED INDEX [IX_PredictedObservedValues_PredictedObservedDetailsID_SimulationsID_ID] ON [dbo].[PredictedObservedValues]
(
	[PredictedObservedDetailsID] ASC,
	[SimulationsID] ASC,
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

PRINT '[PredictedObservedValues] TABLES CREATED'
GO

PRINT '--------------------------------------------------------------------------'
GO
CREATE TYPE [dbo].[SimulationDataTableType] AS TABLE(
	[ID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL
)
GO



PRINT '[SimulationDataTableType] TABLE VALUE TYPE CREATED'
GO

---------------------------------------------------------------------------
CREATE TYPE [dbo].[PredictedObservedDataTableType] AS TABLE(
	[SimulationID] [int] NOT NULL,
	[MatchValue] [nvarchar](100) NOT NULL,
	[PredictedValue] float,
	[ObservedValue] float
)
GO

CREATE TYPE [dbo].[PredictedObservedDataTwoTableType] AS TABLE(
	[SimulationID] [int] NOT NULL,
	[MatchValue] [nvarchar](100) NOT NULL,
	[MatchValue2] [nvarchar](100),
	[PredictedValue] float,
	[ObservedValue] float
)
GO

CREATE TYPE [dbo].[PredictedObservedDataThreeTableType] AS TABLE(
	[SimulationID] [int] NOT NULL,
	[MatchValue] [nvarchar](100) NOT NULL,
	[MatchValue2] [nvarchar](100),
	[MatchValue3] [nvarchar](100),
	[PredictedValue] float,
	[ObservedValue] float
)
GO

PRINT '[PredictedObservedDataTableType] TABLE VALUE TYPES CREATED'
GO
PRINT '--------------------------------------------------------------------------'
GO

CREATE PROCEDURE [dbo].[usp_SimulationsInsert]
(
	@ApsimID as [int],
	@Simulations As [dbo].[SimulationDataTableType] Readonly
)
AS
BEGIN
	INSERT INTO [dbo].[Simulations] ([ApsimFilesID], [Name], [OriginalSimulationID])
	SELECT @ApsimID, Name, [ID] From @Simulations; 
	
END
GO	

PRINT '[usp_SimulationsInsert] STORED PROCEDURE CREATED'
GO


---------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[usp_PredictedObservedDataInsert]
(
	@PredictedObservedID as [int],
	@ApsimFilesID as [int],
	@ValueName as [nvarchar] (100),
	@MatchName as [nvarchar] (100),
	@PredictedOabservedData As [dbo].[PredictedObservedDataTableType] Readonly
)
AS
BEGIN

	--need to get back the simulation data so that we can match it with this data to get new Simulation ID
	INSERT INTO [dbo].[PredictedObservedValues] ([PredictedObservedDetailsID], [SimulationsID], 
		[MatchName], [MatchValue], [ValueName],
		[PredictedValue], [ObservedValue])
	SELECT @PredictedObservedID, s.[ID], @MatchName, p.[MatchValue], @ValueName, p.[PredictedValue], p.[ObservedValue]
	  FROM @PredictedOabservedData p INNER JOIN [dbo].[Simulations] s 
	    ON s.[OriginalSimulationID] = p.[SimulationID]
	 WHERE s.[ApsimFilesID] = @ApsimFilesID
	   and p.[ObservedValue] IS NOT NULL; 
	
END
GO		
CREATE PROCEDURE [dbo].[usp_PredictedObservedDataTwoInsert]
(
	@PredictedObservedID as [int],
	@ApsimFilesID as [int],
	@ValueName as [nvarchar] (100),
	@MatchName as [nvarchar] (100),
	@MatchName2 as [nvarchar] (100),
	@PredictedOabservedData As [dbo].[PredictedObservedDataTwoTableType] Readonly
)
AS
BEGIN

	--need to get back the simulation data so that we can match it with this data to get new Simulation ID
	INSERT INTO [dbo].[PredictedObservedValues] ([PredictedObservedDetailsID], [SimulationsID], 
		[MatchName], [MatchValue], [MatchName2], [MatchValue2], [ValueName],
		[PredictedValue], [ObservedValue])
	SELECT @PredictedObservedID, s.[ID], @MatchName, p.[MatchValue], @MatchName2, p.[MatchValue2], @ValueName, p.[PredictedValue], p.[ObservedValue]
	  FROM @PredictedOabservedData p INNER JOIN [dbo].[Simulations] s 
	    ON s.[OriginalSimulationID] = p.[SimulationID]
	 WHERE s.[ApsimFilesID] = @ApsimFilesID
	   and p.[ObservedValue] IS NOT NULL; 
	
END
GO		
CREATE PROCEDURE [dbo].[usp_PredictedObservedDataThreeInsert]
(
	@PredictedObservedID as [int],
	@ApsimFilesID as [int],
	@ValueName as [nvarchar] (100),
	@MatchName as [nvarchar] (100),
	@MatchName2 as [nvarchar] (100),
	@MatchName3 as [nvarchar] (100),
	@PredictedOabservedData As [dbo].[PredictedObservedDataThreeTableType] Readonly
)
AS
BEGIN

	--need to get back the simulation data so that we can match it with this data to get new Simulation ID
	INSERT INTO [dbo].[PredictedObservedValues] ([PredictedObservedDetailsID], [SimulationsID], 
		[MatchName], [MatchValue], [MatchName2], [MatchValue2], [MatchName3], [MatchValue3], [ValueName], 
		[PredictedValue], [ObservedValue])
	SELECT @PredictedObservedID, s.[ID], @MatchName, p.[MatchValue], @MatchName2, p.[MatchValue2], @MatchName3, p.[MatchValue3], @ValueName,
		p.[PredictedValue], p.[ObservedValue]
	  FROM @PredictedOabservedData p INNER JOIN [dbo].[Simulations] s 
	    ON s.[OriginalSimulationID] = p.[SimulationID]
	 WHERE s.[ApsimFilesID] = @ApsimFilesID
	   and p.[ObservedValue] IS NOT NULL; 
	
END
GO		
PRINT '[usp_PredictedObservedDataInsert] STORED PROCEDURES CREATED'
GO

PRINT '--------------------------------------------------------------------------'



--THIS IS REQUIRED TO ALLOW SV-EXTERNAL EXECUTE RIGHTS ON THE TABLE TYPES
USE [APSIM.PerformanceTests]
GO
GRANT EXECUTE ON TYPE::[dbo].[SimulationDataTableType] TO [sv-login-external]
GO
GRANT EXECUTE ON TYPE::[dbo].[PredictedObservedDataTableType] TO [sv-login-external]
GO
GRANT EXECUTE ON TYPE::[dbo].[PredictedObservedDataTwoTableType] TO [sv-login-external]
GO
GRANT EXECUTE ON TYPE::[dbo].[PredictedObservedDataThreeTableType] TO [sv-login-external]
GO

PRINT 'PERMISSIONS GRANTED TO USER sv-login-external FOR TABLE TYPES'
PRINT '--------------------------------------------------------------------------'


--THIS IS REQUIRED TO ALLOW SV-EXTERNAL EXECUTE RIGHTS ON THE STORED PROCEDURES
USE [APSIM.PerformanceTests]
GO
GRANT EXECUTE ON [dbo].[usp_SimulationsInsert] TO [sv-login-external]
GO
GRANT EXECUTE ON [dbo].[usp_PredictedObservedDataInsert] TO [sv-login-external]
GO
GRANT EXECUTE ON [dbo].[usp_PredictedObservedDataTwoInsert] TO [sv-login-external]
GO
GRANT EXECUTE ON [dbo].[usp_PredictedObservedDataThreeInsert] TO [sv-login-external]
GO

PRINT 'PERMISSIONS GRANTED TO USER sv-login-external FOR STORED PROCEDURES'
PRINT '--------------------------------------------------------------------------'





SELECT * FROM [APSIM.PerformanceTests].[dbo].[ApsimFiles]
GO
SELECT * FROM [APSIM.PerformanceTests].[dbo].[Simulations]
GO
SELECT * FROM [APSIM.PerformanceTests].[dbo].[PredictedObservedDetails]
GO
SELECT * FROM [APSIM.PerformanceTests].[dbo].[PredictedObservedValues]
GO


--INSERT INTO ApsimFiles (PullRequestId, FileName, FullFileName, RunDate, IsReleased) OUTPUT INSERTED.ID Values (1,	'Tomato Soup', 'Groceries', '2016-11-01', 0)
--INSERT INTO ApsimFiles (PullRequestId, FileName, FullFileName, RunDate, IsReleased) OUTPUT INSERTED.ID Values (2,	'Yo-yo', 'Toys', '2016-11-02', 0)
--INSERT INTO ApsimFiles (PullRequestId, FileName, FullFileName, RunDate, IsReleased) OUTPUT INSERTED.ID Values (3,	'Hammer', 'Hardware', '2016-11-03', 0)

/*
DELETE FROM [APSIM.PerformanceTests].[dbo].[PredictedObservedValues] 
DELETE FROM [APSIM.PerformanceTests].[dbo].[PredictedObservedDetails] 
DELETE FROM [APSIM.PerformanceTests].[dbo].[Simulations] 
DELETE FROM [APSIM.PerformanceTests].[dbo].[ApsimFiles] WHERE [PullRequestId] = 4444
*/