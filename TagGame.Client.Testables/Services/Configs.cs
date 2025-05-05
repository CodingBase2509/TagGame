using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using TagGame.Shared.Domain.Games;

[assembly: InternalsVisibleTo("TagGame.Client.Tests")]
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
    public string Username { get; set; } = string.Empty;
    public Color AvatarColor { get; set; } = Colors.White;

    public static UserConfig Default = new()
    {
        UserId = Guid.Empty,
        Username = string.Empty,
        AvatarColor = Colors.Transparent,
    };
}

internal class RoomConfig : ConfigBase
{
    public Guid RoomId { get; set; }
    
    public string RoomName { get; set; } = string.Empty;
    
    public string AccessCode { get; set; } = string.Empty;
    
    public GameState State { get; set; }
}