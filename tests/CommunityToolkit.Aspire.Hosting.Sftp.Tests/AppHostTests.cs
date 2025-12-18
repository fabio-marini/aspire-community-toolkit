using Projects;
using Renci.SshNet;
using Xunit.Abstractions;

namespace CommunityToolkit.Aspire.Hosting.Sftp.Tests;

public class AppHostTests2(ITestOutputHelper log)
{
    private async Task Run(SftpClient client)
    {
        try
        {
            await client.ConnectAsync(CancellationToken.None);
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
    public async Task Resource1StartsAndUserAuthenticates2()
    {
        var resourceNames = new string[] { "sftp-1", "sftp-2", "sftp-3", "sftp-4", "sftp-5", "sftp-6" };

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<CommunityToolkit_Aspire_Hosting_Sftp_AppHost>();

        using var app = builder.Build();

        await app.StartAsync();

        var rns = app.Services.GetRequiredService<ResourceNotificationService>();

        foreach (var res in resourceNames)
        {
            await rns.WaitForResourceAsync(res, "Running");
        }

        foreach (var res in resourceNames)
        {
            var sftp = builder.Resources.OfType<SftpContainerResource>().Single(x => x.Name == res);

            var connectionString = await sftp.ConnectionStringExpression.GetValueAsync(default);

            log.WriteLine($"Resource {res} using connection string: {connectionString}");

            var uri = new Uri(connectionString!);

            SftpClient client;

            switch (res)
            {
                case "sftp-1":
                case "sftp-2":
                case "sftp-5":
                case "sftp-6":
                    client = new SftpClient(uri.Host, uri.Port, "foo", "pass");
                    break;

                case "sftp-3":
                    client = new SftpClient(uri.Host, uri.Port, "foo", "pass");
                    client.HostKeyReceived += (obj, args) =>
                    {
                        Assert.Equal("zfOQDzgMTHSJruZIK37h8L8Gfy3XIJmCXYdqW0OXS7s", args.FingerPrintSHA256);
                    };
                    break;

                case "sftp-4":
                    client = new SftpClient(uri.Host, uri.Port, "foo", new PrivateKeyFile("id_rsa"));
                    break;

                default:
                    throw new NotSupportedException($"Resource {res}");
            }

            await Run(client);
        }

        await app.StopAsync();
    }
}
