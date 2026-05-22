using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class CreateMovimientoCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly IMovimientoRepository _movimientoRepository = Substitute.For<IMovimientoRepository>();
    private readonly IComprobanteStorageService _comprobanteStorage = Substitute.For<IComprobanteStorageService>();
    private readonly CreateMovimientoCommandHandler _sut;

    private static readonly Guid IdUsuario          = Guid.Parse("00000000-0000-7000-8000-000000000001");
    private static readonly Guid IdCuenta           = Guid.Parse("00000000-0000-7000-8000-000000000010");
    private static readonly Guid IdCuentaCategoria  = Guid.Parse("00000000-0000-7000-8000-000000000020");

    private readonly Usuario _usuario;
    private readonly Cuenta _cuenta;
    private readonly CuentaCategoria _cuentaCategoria;

    public CreateMovimientoCommandHandlerTests()
    {
        _sut = new CreateMovimientoCommandHandler(
            _usuarioRepository,
            _cuentaRepository,
            _movimientoRepository,
            _comprobanteStorage,
            NullLogger<CreateMovimientoCommandHandler>.Instance);

        _usuario         = new Usuario         { IdUsuario        = IdUsuario,        Email    = "test@test.com" };
        _cuenta          = new Cuenta          { IdCuenta         = IdCuenta,         Moneda   = "EUR", Descripcion = "Cuenta test" };
        _cuentaCategoria = new CuentaCategoria { IdCuentaCategoria = IdCuentaCategoria, IdCuenta = IdCuenta, Nombre = "Otros gastos" };

        _usuarioRepository.GetByEmailAsync(_usuario.Email, Arg.Any<CancellationToken>()).Returns(_usuario);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns(_cuenta);
        _cuentaRepository.GetCategoriaByIdAsync(IdCuenta, IdCuentaCategoria, Arg.Any<CancellationToken>()).Returns(_cuentaCategoria);
    }

    private CreateMovimientoCommand BuildCommand(Stream? comprobanteStream = null) => new()
    {
        Email             = _usuario.Email,
        IdCuenta          = IdCuenta,
        IdCuentaCategoria = IdCuentaCategoria,
        TipoMovimiento   = TipoMovimiento.Gasto,
        Concepto         = "Compra",
        Importe          = 50m,
        Moneda           = "EUR",
        FechaMovimiento  = DateTime.UtcNow,
        ComprobanteStream      = comprobanteStream,
        ComprobanteFileName    = comprobanteStream is not null ? "recibo.jpg" : null,
        ComprobanteContentType = comprobanteStream is not null ? "image/jpeg" : null
    };

    [Fact]
    public async Task Handle_SinComprobante_CreaMovimientoSinSubirArchivo()
    {
        // Arrange
        var command   = BuildCommand();
        var movimiento = new Movimiento { IdMovimiento = Guid.NewGuid(), Concepto = "Compra" };
        _movimientoRepository.AddMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>())
            .Returns(movimiento);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(movimiento.IdMovimiento, result.IdMovimiento);
        await _comprobanteStorage.DidNotReceive()
            .UploadComprobanteAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConComprobante_SubeArchivoYAsignaIdComprobante()
    {
        // Arrange
        await using var stream = new MemoryStream([0x01, 0x02]);
        var command   = BuildCommand(stream);
        var idComprobante = $"{Guid.NewGuid()}.jpg";
        var movimiento = new Movimiento { IdMovimiento = Guid.NewGuid(), IdComprobante = idComprobante };

        _comprobanteStorage
            .UploadComprobanteAsync(IdCuenta, Arg.Any<string>(), Arg.Any<string>(), stream, Arg.Any<CancellationToken>())
            .Returns(idComprobante);
        _movimientoRepository.AddMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>())
            .Returns(movimiento);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(idComprobante, result.IdComprobante);
        await _comprobanteStorage.Received(1)
            .UploadComprobanteAsync(IdCuenta, "recibo.jpg", "image/jpeg", stream, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FallaBaseDeDatos_EliminaComprobanteSubido()
    {
        // Arrange
        await using var stream = new MemoryStream([0x01, 0x02]);
        var command       = BuildCommand(stream);
        var idComprobante = $"{Guid.NewGuid()}.jpg";

        _comprobanteStorage
            .UploadComprobanteAsync(IdCuenta, Arg.Any<string>(), Arg.Any<string>(), stream, Arg.Any<CancellationToken>())
            .Returns(idComprobante);
        _movimientoRepository
            .AddMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Error BD"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None).AsTask());

        await _comprobanteStorage.Received(1)
            .DeleteComprobanteAsync(IdCuenta, idComprobante, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FallaBaseDeDatosSinComprobante_NoLlamaDeleteComprobante()
    {
        // Arrange
        var command = BuildCommand();
        _movimientoRepository
            .AddMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Error BD"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None).AsTask());

        await _comprobanteStorage.DidNotReceive()
            .DeleteComprobanteAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_CuentaNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns((Cuenta?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());
    }
}
