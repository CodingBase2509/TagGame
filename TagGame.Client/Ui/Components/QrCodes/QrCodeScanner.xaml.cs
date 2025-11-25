using ZXing.Net.Maui;

namespace TagGame.Client.Ui.Components.QrCodes;

public sealed partial class QrCodeScanner : ModalBase, IModal, IDisposable
{
    private readonly TaskCompletionSource<string?> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenRegistration? _cancellationRegistration;

    public QrCodeScanner()
    {
        InitializeComponent();
        ReaderView.Options = new BarcodeReaderOptions
        {
            AutoRotate = false,
            Formats = BarcodeFormat.QrCode,
            CharacterSet = "UTF-8",
            Multiple = false
        };

        ReaderView.CameraLocation = CameraLocation.Rear;
        ReaderView.BarcodesDetected += OnQrCodeDetected;

        CloseButton.Command = new Command(CloseScanner);
    }

    public void Dispose()
    {
        ReaderView.BarcodesDetected -= OnQrCodeDetected;
        _cancellationRegistration?.Dispose();
        _completion.TrySetCanceled();
    }

    public Task<string?> ScanAsync(CancellationToken ct = default)
    {
        _cancellationRegistration = ct.Register(() => _completion.TrySetCanceled(ct));
        return _completion.Task;
    }

    private void OnQrCodeDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var code = e.Results.FirstOrDefault()?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return;

        ReaderView.IsDetecting = false;
        ReaderView.BarcodesDetected -= OnQrCodeDetected;
        _cancellationRegistration?.Dispose();

        _completion.TrySetResult(code);
    }

    private void CloseScanner() => _completion.TrySetCanceled();
}
