using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Storage;

public class NullComprobanteStorageServiceTests
{
    private readonly NullComprobanteStorageService _sut = new();

    [Fact]
    public async Task UploadComprobanteAsync_DevuelveStringVacio()
    {
        // Arrange
        await using var stream = new MemoryStream([0x01]);

        // Act
        var result = await _sut.UploadComprobanteAsync(
            Guid.NewGuid(), "archivo.jpg", "image/jpeg", stream, CancellationToken.None);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task DeleteComprobanteAsync_CompletaSinExcepcion()
    {
        // Act & Assert (no debe lanzar)
        await _sut.DeleteComprobanteAsync(Guid.NewGuid(), "algun-id.jpg", CancellationToken.None);
    }
}
