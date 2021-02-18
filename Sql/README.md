# Transact-SQL

This is the SSMS (Microsoft SQL Server Managment Studio) solution folder.
It includes the Transact-SQL files and the project to create (or drop) the tables needed for the framework and the sample.

## Create a table in database

Following is a template to create a customized table in the database.
You need replace the placeholder `{THE_TABLE_NAME}` to the real name of the table.

```tsql
/*
  Create a table {THE_TABLE_NAME} and its index
  */
CREATE TABLE [dbo].[{THE_TABLE_NAME}](
	[id] [varchar](80) NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[state] [int] NOT NULL,
	[creation] [datetime2](7) NOT NULL,
	[lastupdate] [datetime2](7) NOT NULL,
	[revision] [varchar](80) NOT NULL,
	/* Other columns (includes information about name and type), */
 CONSTRAINT [PK_testgoods] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE INDEX IX_{THE_TABLE_NAME}
ON [dbo].[{THE_TABLE_NAME}] ([name], [state], [lastupdate] DESC /*, Other column names */)

GO
```

This table will be mapped to the entity.
The above columns defined are required and cannot be modified.
