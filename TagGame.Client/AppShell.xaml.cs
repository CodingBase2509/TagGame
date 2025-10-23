using Microsoft.Maui.Controls;
using TagGame.Client.Ui.Components;
using TagGame.Client.Ui.ToastMessages;
using TagGame.Client.Ui.Services;
using TagGame.Client.Ui.Views;

namespace TagGame.Client;

public partial class AppShell : Shell
{
	private readonly ToastView _toastOverlay;
	
	public AppShell()
		: this(null)
	{ }

	public AppShell(IToastService toast)
	{
		InitializeComponent();
		
		_toastOverlay = new ToastView
		{
			IsVisible = false,
			InputTransparent = true
		};

		if (toast is ToastService ts)
			ts.Initialize(_toastOverlay);

		Navigated += OnShellNavigated;
	}

	private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e) =>
		AttachToastToPage(CurrentPage);

	private void AttachToastToPage(Page? page)
	{
		if (page is not PageBase pageBase)
			return;

		if (_toastOverlay.Parent == pageBase.BaseLayout)
			return;
		
		_toastOverlay.VerticalOptions = LayoutOptions.End;
		_toastOverlay.HorizontalOptions = LayoutOptions.Fill;
		pageBase.BaseLayout.Children.Add(_toastOverlay);
	}
}

