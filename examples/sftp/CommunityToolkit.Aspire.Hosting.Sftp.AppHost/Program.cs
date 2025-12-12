using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args); ;

var users = "foo:pass:::upload";

builder.AddSftp("sftp", port: 55010, users);

builder.Build().Run();
