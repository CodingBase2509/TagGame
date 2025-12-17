namespace TagGame.Client.Core.Services;

public interface IQrCodeService
{
    Task<string?> ScanAsync(CancellationToken ct = default);

    Task<byte[]> GenerateAsync(string text, CancellationToken ct = default);
}
