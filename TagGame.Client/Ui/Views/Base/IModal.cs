namespace TagGame.Client.Ui.Views.Base;

public interface IModal
{
    Task OnShowAsync();

    Task OnHideAsync();
}
