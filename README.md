# SolidElements.Extensions.Logging
Database logger for ASP.NET Core (an implementation of common logging abstractions)


### To do:
- [x] Logging to SQL Server&reg;
- [x] Filtering
- [x] Bulk write to SQL Server&reg;
- [ ] Logging scopes
- [ ] Full configurability
- [ ] Multiple database providers


### Configuration

You need to call the `AddDatabaseLogger()` extension methond on loggerFactory in the `Configure()` method of your Startup class:
```csharp
loggerFactory
    .AddDatabaseLogger(Configuration.GetSection("Logging"),
                       Configuration.GetConnectionString("Logging"));
```

Relevant appsettings.json section:
```json
"Logging": {
  "IncludeScopes": false,
  "BulkWrite": false,
  "BulkWriteCacheSize": 1000,
  "LogLevel": {
    "Default": "Debug",
    "System": "Warning",
    "Microsoft": "Warning"
  }
},

"ConnectionStrings": {
  "Logging": "Data Source=(local);Initial Catalog=Yahtzee;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
}
```


### Database Requirements

The logger currently requires the following table and stored procedure in the database:
```sql
CREATE TABLE dbo.Logging
(
    [ID]        int identity(1, 1)  NOT NULL,
    [LogLevel]  varchar(24)         NOT NULL,
    [Category]  varchar(256)        NULL,
    [LogTime]   datetime            NOT NULL,
    [EventID]   int                 NULL,
    [EventName] varchar(256)        NULL,
    [Message]   varchar(4000)       NOT NULL,
    [Exception] varchar(max)        NULL
    

    CONSTRAINT PK_dboLogging PRIMARY KEY CLUSTERED (ID ASC)
        WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = ON, IGNORE_DUP_KEY = OFF, 
            ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
) ON [PRIMARY];


CREATE PROCEDURE dbo.LogRecordInsert (
    @eventID    int = NULL,
    @eventName  varchar(256) = NULL,
    @logLevel   varchar(24),
    @category   varchar(256) = NULL,
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
        @message,
        @exception
    );
```

#### The information provided here may change in the future, due to implementing new features and/or improving the architecture!
