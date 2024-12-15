using TagGame.Shared.Domain.Common;
using TagGame.Shared.DTOs.Common;

namespace TagGame.Shared.Domain.Players;

public class User : IIdentifiable
{
    public Guid Id { get; set; }

    public string DefaultName { get; set; } = string.Empty;

    public ColorDTO DefaultAvatarColor { get; set; }
}
