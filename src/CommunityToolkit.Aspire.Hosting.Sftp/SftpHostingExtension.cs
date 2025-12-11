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
    /// <param name="httpPort">The HTTP portnumber for the web-console to the atmoz SFTP container.</param>
    /// <param name="sftpPort">The SFTP portnumber for the atmoz SFTP Container</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<SftpContainerResource> AddSftp(this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int? httpPort = null,
        int? sftpPort = null)
    {
        ArgumentNullException.ThrowIfNull("Service name must be specified.", nameof(name));
        SftpContainerResource resource = new(name);

        IResourceBuilder<SftpContainerResource> rb = builder.AddResource(resource)
            .WithImage(SftpContainerImageTags.Image)
            .WithImageTag(SftpContainerImageTags.Tag)
            .WithImageRegistry(SftpContainerImageTags.Registry)
            .WithEndpoint(targetPort: SftpContainerResource.SftpEndpointPort,
                port: sftpPort,
                name: SftpContainerResource.SftpEndpointName,
                scheme: "sftp")
            .WithHttpEndpoint(targetPort: SftpContainerResource.HttpEndpointPort,
                port: httpPort,
                name: SftpContainerResource.HttpEndpointName)
            .WithHttpHealthCheck("/health", endpointName: SftpContainerResource.HttpEndpointName);

        return rb;
    }
}
