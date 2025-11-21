namespace TagGame.Client.Ui.Views.Base;

public interface IPageWithModal
{
    Task OpenModalViewAsync(IModal modal);

    Task CloseModalViewAsync();
}
