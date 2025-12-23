using Aspire.Hosting;
using Aspire.Hosting.Utils;
using CommunityToolkit.Aspire.Testing;
using Microsoft.Extensions.Hosting;
using Polly;
using Projects;
using Renci.SshNet;
using Xunit.Abstractions;

namespace CommunityToolkit.Aspire.Hosting.Sftp.Tests;

public class SftpFunctionalTests : IDisposable
{
    private readonly IDistributedApplicationTestingBuilder builder;

    private IResourceBuilder<SftpContainerResource>? resourceBuilder;
    private DistributedApplication? distributedApplication;
    private IHost? host;

    public SftpFunctionalTests(ITestOutputHelper logger)
    {
        builder = TestDistributedApplicationBuilder.Create().WithTestAndResourceLogging(logger);
    }

    private async Task RunTestAsync(Action<HostApplicationBuilder> configure)
    {
        Assert.NotNull(resourceBuilder);

        distributedApplication = builder.Build();

        await distributedApplication.StartAsync();

        var rns = distributedApplication.Services.GetRequiredService<ResourceNotificationService>();

        try
        {
            await rns.WaitForResourceAsync(resourceBuilder.Resource.Name, "Running", new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token);
        }
        catch
        {
            ResourceEvent? resourceEvent = null;

            var res = rns.TryGetCurrentState(resourceBuilder.Resource.Name, out resourceEvent);

            throw;
        }

        var hostBuilder = Host.CreateApplicationBuilder();

        hostBuilder.Configuration[$"ConnectionStrings:{resourceBuilder.Resource.Name}"] = await resourceBuilder.Resource.ConnectionStringExpression.GetValueAsync(default);

        configure(hostBuilder);

        host = hostBuilder.Build();

        await host.StartAsync();

        var client = host.Services.GetRequiredService<SftpClient>();

        try
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500));

            var tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));

            await retryPolicy.ExecuteAsync(async () =>
            {
                await client.ConnectAsync(tokenSource.Token);
            });
        }
        catch
        {
            throw;
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public async Task VerifySftpResourceWithArgs()
    {
        resourceBuilder = builder
            .AddSftp("sftp")
            .WithArgs($"foo:pass:::uploads");

        await RunTestAsync(bld =>
        {
            bld.AddSftpClient(resourceBuilder.Resource.Name, cfg =>
            {
                cfg.Username = "foo";
                cfg.Password = "pass";
            });
        });
    }

    [Fact]
    public async Task VerifySftpResourceWithUsersEnvironmentVariable()
    {
        resourceBuilder = builder
            .AddSftp("sftp")
            .WithEnvironment("SFTP_USERS", "foo:pass:::uploads");

        await RunTestAsync(bld =>
        {
            bld.AddSftpClient(resourceBuilder.Resource.Name, cfg =>
            {
                cfg.Username = "foo";
                cfg.Password = "pass";
            });
        });
    }

    [Fact]
    public async Task VerifySftpResourceWithEncryptedPassword()
    {
        resourceBuilder = builder
            .AddSftp("sftp")
            .WithEnvironment("SFTP_USERS", "foo:$5$t9qxNlrcFqVBNnad$U27ZrjbKNjv4JkRWvi6MjX4x6KXNQGr8NTIySOcDgi4:e:::uploads");

        await RunTestAsync(bld =>
        {
            bld.AddSftpClient(resourceBuilder.Resource.Name, cfg =>
            {
                cfg.Username = "foo";
                cfg.Password = "pass";
            });
        });
    }

    [Fact]
    public async Task VerifySftpResourceWithUsersFile()
    {
        resourceBuilder = builder
            .AddSftp("sftp")
            .WithUsersFile("users.conf");

        await RunTestAsync(bld =>
        {
            bld.AddSftpClient(resourceBuilder.Resource.Name, cfg =>
            {
                cfg.Username = "foo";
                cfg.Password = "pass";
            });
        });
    }

    [Fact]
    public async Task VerifySftpResourceWithHostKey()
    {
        resourceBuilder = builder
            .AddSftp("sftp")
            .WithUsersFile("users.conf")
            .WithHostKeyFile("ssh_host_ed25519_key", KeyType.Ed25519);

        await RunTestAsync(bld =>
        {
            bld.AddSftpClient(resourceBuilder.Resource.Name, cfg =>
            {
                cfg.Username = "foo";
                cfg.Password = "pass";
            });
        });
    }

    [Fact]
    public async Task VerifySftpResourceWithRsaKeys()
    {
        resourceBuilder = builder
            .AddSftp("sftp")
            .WithArgs($"foo::::uploads")
            .WithUserKeyFile("foo", "id_rsa.pub", KeyType.Rsa);

        await RunTestAsync(bld =>
        {
            bld.AddSftpClient(resourceBuilder.Resource.Name, cfg =>
            {
                cfg.Username = "foo";
                cfg.PrivateKeyFile = "id_rsa";
            });
        });
    }

    [Fact]
    public async Task VerifySftpResourceWithEd25519Keys()
    {
        resourceBuilder = builder
            .AddSftp("sftp")
            .WithArgs($"foo::::uploads")
            .WithUserKeyFile("foo", "id_ed25519.pub", KeyType.Ed25519);

        await RunTestAsync(bld =>
        {
            bld.AddSftpClient(resourceBuilder.Resource.Name, cfg =>
            {
                cfg.Username = "foo";
                cfg.PrivateKeyFile = "id_ed25519";
            });
        });
    }

    public void Dispose()
    {
        builder.Dispose();

        if (distributedApplication is not null)
        {
            distributedApplication.Dispose();
        }

        if (host is not null)
        {
            host.Dispose();
        }
    }
}

public class AppHostTests(ITestOutputHelper log, AspireIntegrationTestFixture<CommunityToolkit_Aspire_Hosting_Sftp_AppHost> fix)
    : IClassFixture<AspireIntegrationTestFixture<CommunityToolkit_Aspire_Hosting_Sftp_AppHost>>
{
    [Theory]
    [InlineData("sftp-1a")]
    [InlineData("sftp-1b")]
    [InlineData("sftp-1c")]
    [InlineData("sftp-2")]
    [InlineData("sftp-3")]
    [InlineData("sftp-4a")]
    [InlineData("sftp-4b")]
    public async Task ResourcesStartAndClientConnects(string res)
    {
        SftpClient? client;

        await fix.ResourceNotificationService.WaitForResourceAsync(res, "Running");

        var connectionString = await fix.GetConnectionString(res);

        var uri = new Uri(connectionString!);

        switch (res)
        {
            case "sftp-1a":
            case "sftp-1b":
            case "sftp-1c":
            case "sftp-2":
                client = new SftpClient(uri.Host, uri.Port, "foo", "pass");
                break;

            case "sftp-3":
                client = new SftpClient(uri.Host, uri.Port, "foo", "pass");
                client.HostKeyReceived += (obj, args) =>
                {
                    // server will always use the Ed25519 key - if not provided, it will generate one
                    Assert.Equal("zfOQDzgMTHSJruZIK37h8L8Gfy3XIJmCXYdqW0OXS7s", args.FingerPrintSHA256);
                };
                break;

            case "sftp-4a":
                client = new SftpClient(uri.Host, uri.Port, "foo", new PrivateKeyFile("id_rsa"));
                break;

            case "sftp-4b":
                client = new SftpClient(uri.Host, uri.Port, "foo", new PrivateKeyFile("id_ed25519"));
                break;

            default:
                throw new NotSupportedException($"Resource {res}");
        }

        try
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500));

            await retryPolicy.ExecuteAsync(async () =>
            {
                log.WriteLine($"Connecting to resource '{res}' using connection string: {connectionString}");

                await client.ConnectAsync(CancellationToken.None);
            });
        }
        catch
        {
            throw;
        }
        finally
        {
            client.Disconnect();
        }
    }
}
