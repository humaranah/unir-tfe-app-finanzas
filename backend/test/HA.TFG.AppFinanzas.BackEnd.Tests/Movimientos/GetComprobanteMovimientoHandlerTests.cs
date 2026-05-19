using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetComprobanteMovimientoQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Movimientos;

public class GetComprobanteMovimientoHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepo = Substitute.For<ICuentaRepository>();
    private readonly IMovimientoRepository _movimientoRepo = Substitute.For<IMovimientoRepository>();
    private readonly IComprobanteStorageService _storage = Substitute.For<IComprobanteStorageService>();
    private readonly GetComprobanteMovimientoQueryHandler _sut;

    private const string Email = "test@test.com";
    private static readonly Guid IdCuenta = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid IdMovimiento = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static readonly Usuario UsuarioValido = new() { IdUsuario = Guid.NewGuid(), Email = Email };
    private static readonly Cuenta CuentaValida = new() { IdCuenta = IdCuenta, Moneda = "EUR", Descripcion = "Cuenta test" };

    public GetComprobanteMovimientoHandlerTests()
    {
        _sut = new GetComprobanteMovimientoQueryHandler(_usuarioRepo, _cuentaRepo, _movimientoRepo, _storage);
    }

    [Fact]
    public async Task Handle_DevuelveComprobanteFile_CuandoExisteComprobante()
    {
        // Arrange
        var idComprobante = "abc123.pdf";
        var movimiento = new Movimiento { IdMovimiento = IdMovimiento, IdCuenta = IdCuenta, IdComprobante = idComprobante };
        var comprobanteEsperado = new ComprobanteFile(new MemoryStream(), "application/pdf", idComprobante);

        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(UsuarioValido);
        _cuentaRepo.GetCuentaByIdAsync(UsuarioValido.IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns(CuentaValida);
        _movimientoRepo.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);
        _storage.GetComprobanteAsync(IdCuenta, idComprobante, Arg.Any<CancellationToken>()).Returns(comprobanteEsperado);

        // Act
        var result = await _sut.Handle(new GetComprobanteMovimientoQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None);

        // Assert
        Assert.Equal(comprobanteEsperado, result);
        await _storage.Received(1).GetComprobanteAsync(IdCuenta, idComprobante, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LanzaNotFoundException_CuandoMovimientoNoTieneComprobante()
    {
        // Arrange
        var movimiento = new Movimiento { IdMovimiento = IdMovimiento, IdCuenta = IdCuenta, IdComprobante = null };

        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(UsuarioValido);
        _cuentaRepo.GetCuentaByIdAsync(UsuarioValido.IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns(CuentaValida);
        _movimientoRepo.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(new GetComprobanteMovimientoQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None).AsTask());

        await _storage.DidNotReceive().GetComprobanteAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LanzaNotFoundException_CuandoUsuarioNoExiste()
    {
        // Arrange
        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(new GetComprobanteMovimientoQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_LanzaNotFoundException_CuandoCuentaNoExiste()
    {
        // Arrange
        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(UsuarioValido);
        _cuentaRepo.GetCuentaByIdAsync(UsuarioValido.IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns((Cuenta?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(new GetComprobanteMovimientoQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None).AsTask());
    }
}
