using TagGame.Client.Ui.Components.QrCodes;

namespace TagGame.Client.Infrastructure.QrCodes;

public class QrCodeService : IQrCodeService
{
    public async Task<string?> ScanAsync(CancellationToken ct = default)
    {
        var canAccessCamera = await CanAccessCameraAsync();
        if (!canAccessCamera)
            return null;

        var shell = Shell.Current;
        if (shell.CurrentPage is not IPageWithModal page)
            return null;

        using var scanner = new QrCodeScanner();
        await page.OpenModalViewAsync(scanner);
        try
        {
            var code = await scanner.ScanAsync(ct);
            return code;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        finally
        {
            await page.CloseModalViewAsync();
        }
    }

    public Task<byte[]> GenerateAsync(string text, CancellationToken ct = default) => Task.FromResult(Array.Empty<byte>());

    private async Task<bool> CanAccessCameraAsync()
    {
        var result = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (result != PermissionStatus.Granted)
        {
            result = await Permissions.RequestAsync<Permissions.Camera>();
        }
        return result == PermissionStatus.Granted;
    }
}
