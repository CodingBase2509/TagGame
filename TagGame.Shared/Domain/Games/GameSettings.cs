using TagGame.Shared.Domain.Common;

namespace TagGame.Shared.Domain.Games;

public class GameSettings : IIdentifiable
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public GameArea Area { get; set; } = new();

    public List<Guid> SeekerIds { get; set; } = [];

    public TimeSpan HideTimeout { get; set; }
    
    public bool IsPingEnabled { get; set; }
    
    public TimeSpan? PingInterval { get; set; }
}
