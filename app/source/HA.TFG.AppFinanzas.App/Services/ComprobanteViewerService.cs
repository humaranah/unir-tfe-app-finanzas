using HA.TFG.AppFinanzas.Core.Movimientos;

namespace HA.TFG.AppFinanzas.App.Services;

internal sealed class ComprobanteViewerService : IComprobanteViewerService
{
    public async Task AbrirComprobanteAsync(ComprobanteResult comprobante, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(comprobante.NombreArchivo);
        if (string.IsNullOrEmpty(extension))
            extension = comprobante.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "application/pdf" => ".pdf",
                _ => ".bin"
            };

        var tempPath = Path.Combine(FileSystem.CacheDirectory, $"comprobante_{Guid.NewGuid()}{extension}");
        await File.WriteAllBytesAsync(tempPath, comprobante.Bytes, cancellationToken);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                Title = "Ver comprobante",
                File = new ReadOnlyFile(tempPath)
            });
        });
    }
}
