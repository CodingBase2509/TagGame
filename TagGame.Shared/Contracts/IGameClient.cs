namespace TagGame.Shared.Contracts;

public interface IGameClient
{
    Task GameState();

    Task LocationUpdate();

    Task PlayerTagged();

    Task RoundEnded();

    Task ChatMessagePosted();
}
