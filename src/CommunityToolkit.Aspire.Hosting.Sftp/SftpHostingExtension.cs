using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.Sftp;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding an SFTP resource to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class SftpHostingExtension
{
    /// <summary>
    /// Adds atmoz SFTP to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to add the resource to.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="port">The SFTP port number for the atmoz SFTP container</param>
    /// <param name="args">Optional arguments for the atmoz SFTP container</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<SftpContainerResource> AddSftp(this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int? port = null,
        string? args = null)
    {
        ArgumentNullException.ThrowIfNull("Service name must be specified.", nameof(name));
        SftpContainerResource resource = new(name);

        var resourceBuilder = builder.AddResource(resource)
            .WithImage(SftpContainerImageTags.Image)
            .WithImageTag(SftpContainerImageTags.Tag)
            .WithImageRegistry(SftpContainerImageTags.Registry)
            .WithEndpoint("sftp", ep =>
            {
                ep.Port = port;
                ep.TargetPort = SftpContainerResource.SftpEndpointPort;
                ep.UriScheme = "sftp";
                //ep.IsProxied = false;
            })
            ;

        if (!String.IsNullOrEmpty(args))
        {
            resourceBuilder.WithArgs(args);
        }

        return resourceBuilder;
    }
}
