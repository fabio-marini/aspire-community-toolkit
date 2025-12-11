# CommunityToolkit.Hosting.Sftp

## Overview

This .NET Aspire Integration runs the [atmoz SFTP server](https://hub.docker.com/r/atmoz/sftp/) in a container.


## Usage

The atmoz SFTP integration exposes a connection string with the format `endpoint=sftp://<host>:<port>`.
This connection string can be used to with a DbConnectionStringBuilder to get the sftp endpoint.

### Example 1: Add atmoz SFTP with generated ports

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sftp = builder.AddSftp("sftp");

var xyz = builder.AddProject<Xyz>("application")
    .WithReference(sftp)
    .WaitFor(sftp);

builder.Build().Run();
```

### Example 2: Add atmoz SFTP with user-defined ports

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sftp = builder.AddSftp("sftp", 80, 25);

var xyz = builder.AddProject<Xyz>("application")
    .WithReference(sftp)
    .WaitFor(sftp);

builder.Build().Run();
```

### Example 3: Get URI from connection-string using DbConnectionStringBuilder

```csharp
string? sftpConnectionString = builder.Configuration.GetConnectionString("sftp");
DbConnectionStringBuilder connectionBuilder = new()
{
    ConnectionString = sftpConnectionString 
};

Uri endpoint = new(connectionBuilder["Endpoint"].ToString()!, UriKind.Absolute);
builder.Services.AddScoped(_ => new SmtpClient(endpoint.Host, endpoint.Port));
```
