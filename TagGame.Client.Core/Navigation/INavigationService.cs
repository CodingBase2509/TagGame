namespace TagGame.Client.Core.Navigation;

public interface INavigationService
{
    Task GoToAsync(
        string route,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken ct = default);

    Task GoBackAsync(CancellationToken ct = default);


    Task OpenModalAsync(string route, IReadOnlyDictionary<string, object?>? parameters = null, CancellationToken ct = default);


    Task CloseModalAsync(CancellationToken ct = default);
}
