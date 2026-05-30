using HA.TFG.AppFinanzas.Core.Movimientos;

namespace HA.TFG.AppFinanzas.App.Services;

internal sealed class ComprobantePickerService : IComprobantePickerService
{
    private const long MaxBytes = 1 * 1024 * 1024; // 1 MB

    private static readonly PickOptions FilePickOptions = new()
    {
        PickerTitle = "Seleccionar comprobante",
        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, ["image/jpeg", "image/png", "application/pdf"] },
            { DevicePlatform.iOS, ["public.jpeg", "public.png", "com.adobe.pdf"] },
            { DevicePlatform.WinUI, [".jpg", ".jpeg", ".png", ".pdf"] },
            { DevicePlatform.MacCatalyst, ["public.jpeg", "public.png", "com.adobe.pdf"] },
        })
    };

    public async Task<ComprobanteResult?> SeleccionarArchivoAsync(CancellationToken cancellationToken = default)
    {
        var result = await FilePicker.Default.PickAsync(FilePickOptions);
        if (result is null)
            return null;

        return await LeerArchivoAsync(result.FullPath, result.FileName, result.ContentType, cancellationToken);
    }

    public async Task<ComprobanteResult?> TomarFotoAsync(CancellationToken cancellationToken = default)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            return null;

        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo is null)
            return null;

        return await LeerArchivoAsync(photo.FullPath, photo.FileName, photo.ContentType, cancellationToken);
    }

    private static async Task<ComprobanteResult?> LeerArchivoAsync(
        string fullPath, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        if (bytes.Length > MaxBytes)
            throw new InvalidOperationException("El archivo supera el tamaño máximo permitido de 1 MB.");

        return new ComprobanteResult(bytes, fileName, contentType);
    }
}
