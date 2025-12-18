using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// 1. pass users as args
builder.AddSftp("sftp-1", args: "foo:pass:::uploads");

// 2. store users in config
builder.AddSftp("sftp-2", port: 55020)
    .WithBindMount(".\\etc\\sftp\\users.conf", "/etc/sftp/users.conf", isReadOnly: true)
    ;

// 3. use own host key
builder.AddSftp("sftp-3", port: 55030)
    .WithBindMount(".\\etc\\sftp\\users.conf", "/etc/sftp/users.conf", isReadOnly: true)
    .WithBindMount(".\\etc\\ssh\\ssh_host_ed25519_key", "/etc/ssh/ssh_host_ed25519_key")
    .WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key", "/etc/ssh/ssh_host_rsa_key")
    ;

// 4. log in with private/public key pair (no pwd in users string)
builder.AddSftp("sftp-4", port: 55040, args: "foo::::uploads")
    .WithBindMount(".\\etc\\ssh\\ssh_host_ed25519_key", "/etc/ssh/ssh_host_ed25519_key")
    .WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key", "/etc/ssh/ssh_host_rsa_key")
    .WithBindMount(".\\home\\foo\\.ssh\\keys\\id_rsa.pub", "/home/foo/.ssh/keys/id_rsa.pub", isReadOnly: true)
    ;

// 5. pass users as env var
builder.AddSftp("sftp-5", port: 55050)
    .WithEnvironment("SFTP_USERS", "foo:pass:::uploads")
    ;

// 6. use encrypted passwords (generated with: `wsl mkpasswd -m sha-512`)
builder.AddSftp("sftp-6", port: 55060)
    .WithEnvironment("SFTP_USERS", "foo:$5$t9qxNlrcFqVBNnad$U27ZrjbKNjv4JkRWvi6MjX4x6KXNQGr8NTIySOcDgi4:e:::uploads")
    ;

builder.Build().Run();
