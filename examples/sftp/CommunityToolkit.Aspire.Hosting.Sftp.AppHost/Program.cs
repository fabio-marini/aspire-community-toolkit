using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args); ;

// 1. pass users as args
builder.AddSftp("sftp-1", port: 55010, args: "foo:pass:::uploads");

// 2. store users in config
builder.AddSftp("sftp-2", port: 55020)
    .WithBindMount(".\\etc\\sftp\\users.conf", "/etc/sftp/users.conf", isReadOnly: true);

// 3. use own host key
builder.AddSftp("sftp-3", port: 55030)
    .WithBindMount(".\\etc\\sftp\\users.conf", "/etc/sftp/users.conf", isReadOnly: true)
    .WithBindMount(".\\etc\\ssh\\ssh_host_ed25519_key", "/etc/ssh/sftp/ssh_host_ed25519_key", isReadOnly: true)
    .WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key", "/etc/ssh/sftp/ssh_host_rsa_key", isReadOnly: true);

// 4. log in with private/public key pair (no pwd in users string)
builder.AddSftp("sftp-4", port: 55040, args: "foo::::uploads")
    //.WithBindMount(".\\etc\\ssh\\ssh_host_ed25519_key", "/etc/ssh/sftp/ssh_host_ed25519_key", isReadOnly: true)
    //.WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key", "/etc/ssh/sftp/ssh_host_rsa_key", isReadOnly: true)
    .WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key", "/home/foo/.ssh/keys/ssh_host_rsa_key", isReadOnly: true)
    .WithBindMount(".\\etc\\ssh\\ssh_host_rsa_key.pub", "/home/foo/.ssh/keys/ssh_host_rsa_key.pub", isReadOnly: true);

builder.Build().Run();
