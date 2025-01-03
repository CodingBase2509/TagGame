namespace TagGame.Client.Services;

public abstract class ConfigBase
{ }
    
internal class ServerConfig : ConfigBase
{
    public string Host { get; set; }
    public int? Port { get; set; }

    public static ServerConfig Default = new()
    {
        Host = "127.0.0.1",
        Port = 8080,
    };
}

internal class UserConfig : ConfigBase
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public Color AvatarColor { get; set; }

    public static UserConfig Default = new()
    {
        UserId = Guid.Empty,
        Username = string.Empty,
        AvatarColor = Colors.Transparent,
    };
}