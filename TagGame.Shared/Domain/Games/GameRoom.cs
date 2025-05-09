﻿using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Players;

namespace TagGame.Shared.Domain.Games;

public class GameRoom : IIdentifiable
{
    public Guid Id { get; set; }

    public string AccessCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public GameState State { get; set; } = GameState.Lobby;

    public GameSettings Settings { get; set; } = new();

    public Guid OwnerUserId { get; set; }

    public List<Player> Players { get; set; } = [];
}
