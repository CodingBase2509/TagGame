using System.Drawing;
using TagGame.Shared.Domain.Common;

namespace TagGame.Shared.Domain.Players;

public class User : IIdentifiable
{
    public Guid Id { get; set; }

    public string DefaultName { get; set; } = string.Empty;

    public Color DefaultAvatarColor { get; set; }
}
