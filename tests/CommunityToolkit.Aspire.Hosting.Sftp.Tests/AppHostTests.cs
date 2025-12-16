using Aspire.Components.Common.Tests;
using CommunityToolkit.Aspire.Testing;
using Renci.SshNet;

namespace CommunityToolkit.Aspire.Hosting.PapercutSmtp.Tests;

[RequiresDocker]
public class AppHostTests(AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_Sftp_AppHost> fixture) : IClassFixture<AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_Sftp_AppHost>>
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

    private async Task<Uri> GetEndpoint(string resourceName)
    {
        await fixture.ResourceNotificationService.WaitForResourceHealthyAsync(resourceName).WaitAsync(TimeSpan.FromMinutes(2));

        return fixture.GetEndpoint(resourceName, "sftp");
    }

    [Fact]
    public async Task Resource1StartsAndUserAuthenticates()
    {
        var endpoint = await GetEndpoint("sftp-1");

        var client = new SftpClient(endpoint.Host, endpoint.Port, "foo", "pass");

        await Run(client);
    }

    [Fact]
    public async Task Resource2StartsAndUserAuthenticates()
    {
        var endpoint = await GetEndpoint("sftp-2");

        var client = new SftpClient(endpoint.Host, endpoint.Port, "foo", "pass");

        await Run(client);
    }

    [Fact]
    public async Task Resource3StartsAndUserAuthenticates()
    {
        var endpoint = await GetEndpoint("sftp-3");

        var client = new SftpClient(endpoint.Host, endpoint.Port, "foo", "pass");

        client.HostKeyReceived += (obj, args) =>
        {
            Assert.Equal("zfOQDzgMTHSJruZIK37h8L8Gfy3XIJmCXYdqW0OXS7s", args.FingerPrintSHA256);
        };

        await Run(client);
    }

    [Fact]
    public async Task Resource4StartsAndUserAuthenticates()
    {
        var endpoint = await GetEndpoint("sftp-4");

        var privateKey = new PrivateKeyFile("id_rsa");

        var client = new SftpClient(endpoint.Host, endpoint.Port, "foo", privateKey);

        await Run(client);
    }
}