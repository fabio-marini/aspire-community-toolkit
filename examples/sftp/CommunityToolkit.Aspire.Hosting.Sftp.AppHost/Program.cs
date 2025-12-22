using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

#region [Known Issues]

// TODO: how to use rsa host key file - if ed25519 not supplied, server will create one and use it...

#endregion

// 1a. pass users as args
builder.AddSftp("sftp-1a", port: 55011).WithArgs($"foo:pass:::uploads");

// 1b. pass users as environment variable
builder.AddSftp("sftp-1b", port: 55012).WithEnvironment("SFTP_USERS", "foo:pass:::uploads");

// 1c. use encrypted passwords (generated with: `wsl mkpasswd -m sha-512`)
builder.AddSftp("sftp-1c", port: 55013).WithEnvironment("SFTP_USERS", "foo:$5$t9qxNlrcFqVBNnad$U27ZrjbKNjv4JkRWvi6MjX4x6KXNQGr8NTIySOcDgi4:e:::uploads");

// 2. store users in config
builder.AddSftp("sftp-2", port: 55020).WithUsersFile(".\\etc\\sftp\\users.conf");

// 3. use own host key
builder.AddSftp("sftp-3", port: 55030).WithUsersFile(".\\etc\\sftp\\users.conf").WithHostKeyFile(".\\etc\\ssh\\ssh_host_ed25519_key", KeyType.Ed25519);

// 4a. log in with RSA private/public key pair (no pwd in users string)
builder.AddSftp("sftp-4a", port: 55041).WithArgs("foo::::uploads").WithUserKeyFile("foo", ".\\home\\foo\\.ssh\\keys\\id_rsa.pub", KeyType.Rsa);

// 4b. log in with Ed25519 private/public key pair (no pwd in users string)
builder.AddSftp("sftp-4b", port: 55042).WithArgs("foo::::uploads").WithUserKeyFile("foo", ".\\home\\foo\\.ssh\\keys\\id_ed25519.pub", KeyType.Ed25519);

builder.Build().Run();
