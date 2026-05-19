using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.GetMovimientoDetalleQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Movimientos;

public class GetMovimientoDetalleHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepo = Substitute.For<ICuentaRepository>();
    private readonly IMovimientoRepository _movimientoRepo = Substitute.For<IMovimientoRepository>();
    private readonly GetMovimientoDetalleQueryHandler _sut;

    private const string Email = "test@test.com";
    private static readonly Guid IdCuenta = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid IdMovimiento = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static readonly Usuario UsuarioValido = new() { IdUsuario = Guid.NewGuid(), Email = Email };
    private static readonly Cuenta CuentaValida = new() { IdCuenta = IdCuenta, Moneda = "EUR", Descripcion = "Cuenta test" };

    public GetMovimientoDetalleHandlerTests()
    {
        _sut = new GetMovimientoDetalleQueryHandler(_usuarioRepo, _cuentaRepo, _movimientoRepo);
    }

    [Fact]
    public async Task Handle_DevuelveDetalle_CuandoMovimientoExiste()
    {
        // Arrange
        var movimiento = new Movimiento
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            IdCuentaCategoria = Guid.NewGuid(),
            TipoMovimiento = TipoMovimiento.Gasto,
            Concepto = "Supermercado",
            Importe = 45.50m,
            Moneda = "EUR",
            Nota = string.Empty,
            FechaMovimiento = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow,
            Categoria = new CuentaCategoria { Nombre = "Alimentación" }
        };

        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(UsuarioValido);
        _cuentaRepo.GetCuentaByIdAsync(UsuarioValido.IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns(CuentaValida);
        _movimientoRepo.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);

        // Act
        var result = await _sut.Handle(new GetMovimientoDetalleQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None);

        // Assert
        Assert.Equal(IdMovimiento, result.IdMovimiento);
        Assert.Equal("Supermercado", result.Concepto);
        Assert.Equal(45.50m, result.Importe);
        Assert.Equal("Alimentación", result.NombreCategoria);
        Assert.Equal(TipoMovimiento.Gasto, result.TipoMovimiento);
    }

    [Fact]
    public async Task Handle_LanzaNotFoundException_CuandoUsuarioNoExiste()
    {
        // Arrange
        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(new GetMovimientoDetalleQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_LanzaNotFoundException_CuandoCuentaNoExiste()
    {
        // Arrange
        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(UsuarioValido);
        _cuentaRepo.GetCuentaByIdAsync(UsuarioValido.IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns((Cuenta?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(new GetMovimientoDetalleQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_LanzaNotFoundException_CuandoMovimientoNoExiste()
    {
        // Arrange
        _usuarioRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(UsuarioValido);
        _cuentaRepo.GetCuentaByIdAsync(UsuarioValido.IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns(CuentaValida);
        _movimientoRepo.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns((Movimiento?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(new GetMovimientoDetalleQuery(Email, IdCuenta, IdMovimiento), CancellationToken.None).AsTask());
    }
}
