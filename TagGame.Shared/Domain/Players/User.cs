using System.Drawing;

namespace TagGame.Shared.Domain.Players;

public class User
{
    public Guid Id { get; set; }

    public string DefaultName { get; set; } = string.Empty;

    public Color DefaultAvatarColor { get; set; }
}
