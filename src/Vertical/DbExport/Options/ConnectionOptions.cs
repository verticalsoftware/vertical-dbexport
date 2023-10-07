namespace Vertical.DbExport.Options;

public class ConnectionOptions
{
    public string ProviderId { get; set; } = default!;
    public string? Server { get; set; }
    public int? Port { get; set; }
    public string? Database { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public TimeSpan? ConnectTimeout { get; set; }
    public TimeSpan? CommandTimeout { get; set; }
    public int? ConnectRetries { get; set; }
    public int? CommandRetries { get; set; }
}