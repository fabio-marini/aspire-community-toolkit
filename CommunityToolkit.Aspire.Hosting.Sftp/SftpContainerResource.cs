namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Resource for the atmoz SFTP server.
/// </summary>
/// <param name="name"></param>
public class SftpContainerResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const int HttpEndpointPort = 8080;
    internal const int SftpEndpointPort = 2222;
    internal const string HttpEndpointName = "http";
    internal const string SftpEndpointName = "sftp";
    private EndpointReference? _sftpEndpoint;
    private EndpointReference SftpEndpoint => _sftpEndpoint ??= new EndpointReference(this, SftpEndpointName);

    /// <summary>
    /// Gets the host endpoint reference for the SFTP endpoint.
    /// </summary>
    public EndpointReferenceExpression Host => SftpEndpoint.Property(EndpointProperty.Host);

    /// <summary>
    /// Gets the port endpoint reference for the SFTP endpoint.
    /// </summary>
    public EndpointReferenceExpression Port => SftpEndpoint.Property(EndpointProperty.Port);

    /// <summary>
    /// ConnectionString for the atmoz SFTP server in the form of sftp://host:port.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create(
        $"Endpoint={SftpEndpoint.Scheme}://{SftpEndpoint.Property(EndpointProperty.Host)}:{SftpEndpoint.Property(EndpointProperty.Port)}");

    /// <summary>
    /// Gets the connection URI expression for the atmoz SFTP endpoint.
    /// </summary>
    /// <remarks>
    /// Format: <c>sftp://{host}:{port}</c>.
    /// </remarks>
    public ReferenceExpression UriExpression => ReferenceExpression.Create($"{SftpEndpoint.Scheme}://{Host}:{Port}");

    IEnumerable<KeyValuePair<string, ReferenceExpression>> IResourceWithConnectionString.GetConnectionProperties()
    {
        yield return new("Host", ReferenceExpression.Create($"{Host}"));
        yield return new("Port", ReferenceExpression.Create($"{Port}"));
        yield return new("Uri", UriExpression);
    }
}
