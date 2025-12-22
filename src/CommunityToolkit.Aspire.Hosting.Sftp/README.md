# CommunityToolkit.Hosting.Sftp

## Overview

This Aspire integration is a wrapper for the [atmoz SFTP server](https://hub.docker.com/r/atmoz/sftp/) image.


## Usage

The SFTP integration exposes a connection string with the format `endpoint=sftp://<host>:<port>`.


### Example 1: Add SFTP container with users as an argument

Define users in (1) command arguments or (2) `SFTP_USERS` environment variable (syntax: `user:pass[:e][:uid[:gid[:dir1[,dir2]...]]]`, see below for examples)

Add `:e` behind password to mark it as encrypted. On Windows, use `wsl mkpasswd -m sha-512` to generate encrypted passwords.

```csharp
builder.AddSftp("sftp-1a", port: 55010).WithArgs($"foo:pass:::uploads");

builder.AddSftp("sftp-1b", port: 55012).WithEnvironment("SFTP_USERS", "foo:pass:::uploads");

builder.AddSftp("sftp-1c", port: 55013).WithEnvironment("SFTP_USERS", "foo:$5$t9qxNlrcFqVBNnad$U27ZrjbKNjv4JkRWvi6MjX4x6KXNQGr8NTIySOcDgi4:e:::uploads");
```

### Example 2: Add SFTP container with users in config file

Define users in file mounted as `/etc/sftp/users.conf` using the `WithUsersFile()` extension method.

```csharp
builder.AddSftp("sftp-2", port: 55020).WithUsersFile(".\\etc\\sftp\\users.conf");
```

### Example 3: Add SFTP container with own host key

This container will generate new SSH host keys at first run. To avoid that your users get a MITM warning when you recreate your container (and the host keys changes), you can mount your own host keys with the `WithHostKeyFile()` extension method.

```csharp
builder.AddSftp("sftp-3", port: 55030).WithUsersFile(".\\etc\\sftp\\users.conf").WithHostKeyFile(".\\etc\\ssh\\ssh_host_ed25519_key", KeyType.Ed25519);
```

### Example 4: Add SFTP container with user public key

Mount **public** keys in the user's `.ssh/keys/` directory using the `WithUserKeyFile()` extension method. All keys are automatically appended to `.ssh/authorized_keys` (you can't mount this file directly, because OpenSSH requires limited file permissions). In this example, we do not provide any password, so the user `foo` can only login with the corresponding **private** key.

```csharp
builder.AddSftp("sftp-4a", port: 55041).WithArgs("foo::::uploads").WithUserKeyFile("foo", ".\\home\\foo\\.ssh\\keys\\id_rsa.pub", KeyType.Rsa);

builder.AddSftp("sftp-4b", port: 55042).WithArgs("foo::::uploads").WithUserKeyFile("foo", ".\\home\\foo\\.ssh\\keys\\id_ed25519.pub", KeyType.Ed25519);
```
