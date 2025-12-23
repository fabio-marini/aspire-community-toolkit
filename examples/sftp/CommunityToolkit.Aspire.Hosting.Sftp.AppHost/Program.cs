using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// TODO: all app host tests are using the aspire integration test class fixture with the example project
//       + all resource creation tests are using one TestDistributedApplicationBuilder per test and verifying the app model...
//       + would really like to verify the client connectivity instead (same as KurrentDB)!
// TODO: + remove all sftp resources from the example and refactor app host tests accordingly
//       + refactor func tests to use new client and implement all the different scenarios there (not app host)
// TODO: test all other extensions too, i.e. health check and disable tracing + client tests

#region [All scenarios]

// 1a. pass users as args
//builder.AddSftp("sftp-1a", port: 55011).WithArgs($"foo:pass:::uploads");

// 1b. pass users as environment variable
//builder.AddSftp("sftp-1b", port: 55012).WithEnvironment("SFTP_USERS", "foo:pass:::uploads");

// 1c. use encrypted passwords (generated with: `wsl mkpasswd -m sha-512`)
//builder.AddSftp("sftp-1c", port: 55013).WithEnvironment("SFTP_USERS", "foo:$5$t9qxNlrcFqVBNnad$U27ZrjbKNjv4JkRWvi6MjX4x6KXNQGr8NTIySOcDgi4:e:::uploads");

// 2. store users in config
//builder.AddSftp("sftp-2", port: 55020).WithUsersFile(".\\etc\\sftp\\users.conf");

// 3. use own host key
//builder.AddSftp("sftp-3", port: 55030).WithUsersFile(".\\etc\\sftp\\users.conf").WithHostKeyFile(".\\etc\\ssh\\ssh_host_ed25519_key", KeyType.Ed25519);

// 4a. log in with RSA private/public key pair (no pwd in users string)
//builder.AddSftp("sftp-4a", port: 55041).WithArgs("foo::::uploads").WithUserKeyFile("foo", ".\\home\\foo\\.ssh\\keys\\id_rsa.pub", KeyType.Rsa);

// 4b. log in with Ed25519 private/public key pair (no pwd in users string)
//builder.AddSftp("sftp-4b", port: 55042).WithArgs("foo::::uploads").WithUserKeyFile("foo", ".\\home\\foo\\.ssh\\keys\\id_ed25519.pub", KeyType.Ed25519);

#endregion

var sftp = builder.AddSftp("sftp").WithEnvironment("SFTP_USERS", "foo:$5$t9qxNlrcFqVBNnad$U27ZrjbKNjv4JkRWvi6MjX4x6KXNQGr8NTIySOcDgi4:e:::uploads");

builder.AddProject<Projects.CommunityToolkit_Aspire_Hosting_Sftp_ApiService>("api")
    .WithReference(sftp)
    .WaitForStart(sftp);

builder.Build().Run();
