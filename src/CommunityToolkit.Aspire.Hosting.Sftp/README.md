# CommunityToolkit.Hosting.Sftp

## Overview

This .NET Aspire Integration runs the [atmoz SFTP server](https://hub.docker.com/r/atmoz/sftp/) in a container.


## Usage

The SFTP integration exposes a connection string with the format `endpoint=sftp://<host>:<port>`.
This connection string can be used to with a DbConnectionStringBuilder to get the sftp endpoint.

### Example 1: Add SFTP container with user credentials as arguments

```csharp
builder.AddSftp("sftp-1", port: 55010, args: "foo:pass:::uploads");
```

### Example 2: Add SFTP container with user credentials in config file

```csharp
builder.AddSftp("sftp-2", port: 55020)
    .WithBindMount(".\\etc\\sftp\\users.conf", "/etc/sftp/users.conf", isReadOnly: true);
```

### Example 3: Add SFTP container with own host key, as opposed to auto-generated

```csharp
builder.AddSftp("sftp-3", port: 55030)
    .WithBindMount(".\\etc\\sftp\\users.conf", "/etc/sftp/users.conf", isReadOnly: true)
    .WithBindMount(".\\etc\\ssh\\ssh_host_ed25519_key", "/etc/ssh/ssh_host_ed25519_key")
    .WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key", "/etc/ssh/ssh_host_rsa_key");
```

### Example 4: Add SFTP container with user logging with public/private SSH key pair

```csharp
builder.AddSftp("sftp-4", port: 55040, args: "foo::::uploads")
    .WithBindMount(".\\etc\\ssh\\ssh_host_ed25519_key", "/etc/ssh/ssh_host_ed25519_key")
    .WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key", "/etc/ssh/ssh_host_rsa_key")
    .WithBindMount(".\\home\\foo\\.ssh\\keys\\id_rsa.pub", "/home/foo/.ssh/keys/id_rsa.pub", isReadOnly: true);
```
