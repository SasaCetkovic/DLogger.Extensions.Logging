# DLogger.Extensions.Logging
Database logger for ASP.NET Core (an implementation of common logging abstractions)


### To do:
- [x] Filtering
- [x] Logging scopes
- [x] Options for caching and bulk writing
- [x] Make multiple storage implementations possible
- [x] `ILogWriter` implementation for SQL Server&reg;
- [ ] `ILogWriter` implementation for SQLite
- [ ] `ILogWriter` implementation for PostgreSQL
- [ ] `ILogWriter` implementation for MySQL

**You can implement `DLogger.Extensions.Logging.Contracts.ILogWriter` interface for working with any database, permanent storage, or actually anything.**


### Configuration

You need to call the `AddDLogger()` extension methond on loggerFactory in the `Configure()` method of your Startup class:
```csharp
var logWriter = new SqlServerLogWriter(Configuration.GetConnectionString("Logging"));
loggerFactory.AddDLogger(Configuration.GetSection("Logging"), logWriter);
```

Relevant appsettings.json section:
```json
"Logging": {
  "IncludeScopes": true,
  "BulkWrite": true,
  "BulkWriteCacheSize": 1000,
  "LogLevel": {
    "Default": "Debug",
    "System": "Warning",
    "Microsoft": "Warning"
  }
},

"ConnectionStrings": {
  "Logging": "Data Source=(local);Initial Catalog=MyDatabaseWithLoggingTable;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
}
```

You can take a look at some example usages in the `TestAspNetApplication` project.


### Database Requirements for `SqlServerLogWriter`

The `SqlServerLogWriter` requires the following table and stored procedure in the database:
```sql
CREATE TABLE dbo.Logging
(
    [ID]        int identity(1, 1)  NOT NULL,
    [LogLevel]  varchar(24)         NOT NULL,
    [Category]  varchar(256)        NULL,
    [LogTime]   datetime            NOT NULL,
    [EventID]   int                 NULL,
    [EventName] varchar(256)        NULL,
    [Scope]     varchar(4000)       NULL,
    [Message]   varchar(4000)       NOT NULL,
    [Exception] varchar(max)        NULL
    

    CONSTRAINT PK_dboLogging PRIMARY KEY CLUSTERED (ID ASC)
        WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = ON, IGNORE_DUP_KEY = OFF, 
            ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
) ON [PRIMARY];


CREATE PROCEDURE dbo.LogRecordInsert
(
    @eventID    int = NULL,
    @eventName  varchar(256) = NULL,
    @logLevel   varchar(24),
    @category   varchar(256) = NULL,
    @scope      varchar(4000) = NULL,
    @message    varchar(4000),
    @logTime    datetime,
    @exception  varchar(4000) = NULL
)
AS
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Logging]
    (
        [EventID],  
        [EventName],    
        [LogLevel], 
        [Category], 
        [LogTime],  
        [Scope],  
        [Message],  
        [Exception]         
    )
    VALUES
    (
        @eventID,
        @eventName,
        @logLevel,
        @category,
        @logTime,
        @scope,
        @message,
        @exception
    );
```
