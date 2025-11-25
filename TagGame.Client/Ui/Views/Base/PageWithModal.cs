using TagGame.Client.Core.Ui.ViewModels;

namespace TagGame.Client.Ui.Views.Base;

public class PageWithModal : PageBase, IPageWithModal
{
    public PageWithModal() : base()
    {
    }

    public PageWithModal(ViewModelBase vm) : base(vm)
    { }

    public async Task OpenModalViewAsync(IModal modal)
    {
        root!.Children.Add(modal as ModalBase);
        await modal.OnShowAsync();
    }


    public async Task CloseModalViewAsync()
    {
        var modal = root!.Children.FirstOrDefault(x => x is IModal);
        if (modal is null)
            return;

        await (modal as IModal)!.OnHideAsync();
        root!.Children.Remove(modal);
    }
}
