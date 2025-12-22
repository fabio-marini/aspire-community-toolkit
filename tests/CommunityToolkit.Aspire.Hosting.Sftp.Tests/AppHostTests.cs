using CommunityToolkit.Aspire.Testing;
using Polly;
using Projects;
using Renci.SshNet;
using Xunit.Abstractions;

namespace CommunityToolkit.Aspire.Hosting.Sftp.Tests;

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
