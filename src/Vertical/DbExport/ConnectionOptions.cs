namespace Vertical.DbExport;

public class ConnectionOptions
{
    public string? Server { get; set; }
    public string? Database { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public TimeSpan? ConnectTimeout { get; set; }
    public TimeSpan? CommandTimeout { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}