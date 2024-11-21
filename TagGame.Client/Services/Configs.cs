namespace TagGame.Client.Services;

public abstract class ConfigBase
{ }
    
internal class ServerConfig : ConfigBase
{
    public string Host { get; set; } = "localhost";
    public int? Port { get; set; } = 5000;
}

internal class UserConfig : ConfigBase
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Username { get; set; } = string.Empty;
    public Color AvatarColor { get; set; } = Colors.Black;
}