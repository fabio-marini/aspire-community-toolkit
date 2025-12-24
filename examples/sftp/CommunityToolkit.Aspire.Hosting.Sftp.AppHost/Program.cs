using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// FIXME: func test #4 should verify the host key!

// TODO: add app host tests to call api upload and download endpoints?
// TODO: test all other extensions too, i.e. health check and disable tracing + client tests

var sftp = builder.AddSftp("sftp").WithEnvironment("SFTP_USERS", "foo:$5$t9qxNlrcFqVBNnad$U27ZrjbKNjv4JkRWvi6MjX4x6KXNQGr8NTIySOcDgi4:e:::uploads");

builder.AddProject<Projects.CommunityToolkit_Aspire_Hosting_Sftp_ApiService>("api")
    .WithReference(sftp)
    .WaitForStart(sftp);

builder.Build().Run();
