using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.Sftp;

namespace Aspire.Hosting;

// TODO: users from config and ssh keys are just volumes...
// TODO: keep it simple and remove users class - can use hard-coded args for quick starts
// TODO: don't forget the missing xml comments!!!

/// <summary>
/// 
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
/// <param name="IsEncrypted"></param>
/// <param name="UserId"></param>
/// <param name="GroupId"></param>
/// <param name="Folders"></param>
public record SftpUser(string Username, string Password, bool IsEncrypted = false, string? UserId = null, string? GroupId = null, string[]? Folders = null)
{
    /// <summary>
    /// Returns a string that represents the current user credentials, including the username, password, user ID, group
    /// ID, and folders.
    /// </summary>
    /// <remarks>If the password is encrypted, an ":e" suffix is appended to the password segment in the
    /// returned string. This format can be used for serialization or logging purposes where a compact representation of
    /// the credentials is required.</remarks>
    /// <returns>A colon-delimited string containing the username, password (with an ":e" suffix if encrypted), user ID, group
    /// ID, and folders.</returns>
    public override string ToString()
    {
        var password = IsEncrypted ? $"{Password}:e" : Password;

        var folders = Folders == null || Folders.Length == 0 ? "" : Folders.Aggregate((f1, f2) => $"{f1}, {f2}");

        return $"{Username}:{password}:{UserId}:{GroupId}:{folders}";
    }
}

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
    /// <param name="port">The SFTP portnumber for the atmoz SFTP container</param>
    /// <param name="args">Optional arguments for the atmoz SFTP container</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<SftpContainerResource> AddSftp(this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int? port = null,
        string args = "")
    {
        ArgumentNullException.ThrowIfNull("Service name must be specified.", nameof(name));
        SftpContainerResource resource = new(name);

        IResourceBuilder<SftpContainerResource> rb = builder.AddResource(resource)
            .WithImage(SftpContainerImageTags.Image)
            .WithImageTag(SftpContainerImageTags.Tag)
            .WithImageRegistry(SftpContainerImageTags.Registry)
            .WithArgs(args)
            .WithEndpoint(targetPort: SftpContainerResource.SftpEndpointPort,
                port: port,
                name: SftpContainerResource.SftpEndpointName,
                scheme: "sftp")
            ;

        return rb;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="users"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IResourceBuilder<SftpContainerResource> WithUsers(this IResourceBuilder<SftpContainerResource> builder, SftpUser[] users)
    {
        throw new NotImplementedException();
    }
}
