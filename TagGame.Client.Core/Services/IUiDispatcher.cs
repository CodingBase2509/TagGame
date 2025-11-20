namespace TagGame.Client.Core.Services;

public interface IUiDispatcher
{
    Task OnMainThreadAsync(Func<Task> action);

    Task OnMainThreadAsync(Action action);
}
