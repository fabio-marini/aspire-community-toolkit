using CommunityToolkit.Aspire.Testing;
using Polly;
using Projects;
using Renci.SshNet;
using Xunit.Abstractions;

namespace CommunityToolkit.Aspire.Hosting.Sftp.Tests;

public class AppHostTests(ITestOutputHelper log, AspireIntegrationTestFixture<CommunityToolkit_Aspire_Hosting_Sftp_AppHost> fix)
    : IClassFixture<AspireIntegrationTestFixture<CommunityToolkit_Aspire_Hosting_Sftp_AppHost>>
{
    [Fact]
    public async Task ResourcesStartAndClientConnects()
    {
        await fix.ResourceNotificationService.WaitForResourceAsync("sftp", "Running");

        var connectionString = await fix.GetConnectionString("sftp");

        var uri = new Uri(connectionString!);

        var client = new SftpClient(uri.Host, uri.Port, "foo", "pass");

        try
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500));

            await retryPolicy.ExecuteAsync(async () =>
            {
                log.WriteLine($"Connecting to resource 'sftp' using connection string: {connectionString}");

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
