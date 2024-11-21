namespace TagGame.Shared.Constants;

public static class GameOptions
{
    public static TimeSpan DefaultHideTimeout => TimeSpan.FromMinutes(5);

    public static bool DefaultPingEnabled = false;
    
    public static TimeSpan DefaultPingInterval = TimeSpan.FromMinutes(-1);

    public static GameAreaType DefaultGameArea = GameAreaType.Circle;
}
