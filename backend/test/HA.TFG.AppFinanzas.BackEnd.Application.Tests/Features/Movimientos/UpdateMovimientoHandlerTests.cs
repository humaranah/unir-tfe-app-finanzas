using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class UpdateMovimientoCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly IMovimientoRepository _movimientoRepository = Substitute.For<IMovimientoRepository>();
    private readonly IComprobanteStorageService _comprobanteStorage = Substitute.For<IComprobanteStorageService>();
    private readonly UpdateMovimientoCommandHandler _sut;

    private static readonly Guid IdUsuario = Guid.Parse("00000000-0000-7000-8000-000000000001");
    private static readonly Guid IdCuenta = Guid.Parse("00000000-0000-7000-8000-000000000010");
    private static readonly Guid IdMovimiento = Guid.Parse("00000000-0000-7000-8000-000000000030");
    private static readonly Guid IdCuentaCategoriaNew = Guid.Parse("00000000-0000-7000-8000-000000000021");

    private readonly Usuario _usuario = new() { IdUsuario = IdUsuario, Email = "test@test.com" };
    private readonly Cuenta _cuenta = new() { IdCuenta = IdCuenta, Moneda = "EUR", Descripcion = "Cuenta test" };
    private readonly CuentaCategoria _cuentaCategoriaNew = new() { IdCuentaCategoria = IdCuentaCategoriaNew, IdCuenta = IdCuenta, Nombre = "Comida" };

    public UpdateMovimientoCommandHandlerTests()
    {
        _sut = new UpdateMovimientoCommandHandler(
            _usuarioRepository,
            _cuentaRepository,
            _movimientoRepository,
            _comprobanteStorage,
            NullLogger<UpdateMovimientoCommandHandler>.Instance);

        _usuarioRepository.GetByEmailAsync(_usuario.Email, Arg.Any<CancellationToken>()).Returns(_usuario);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns(_cuenta);
        _cuentaRepository.GetCategoriaByIdAsync(IdCuenta, IdCuentaCategoriaNew, Arg.Any<CancellationToken>()).Returns(_cuentaCategoriaNew);
    }

    private UpdateMovimientoCommand BuildCommand() => new()
    {
        Email = _usuario.Email,
        IdCuenta = IdCuenta,
        IdMovimiento = IdMovimiento,
        IdCuentaCategoria = IdCuentaCategoriaNew,
        TipoMovimiento = TipoMovimiento.Gasto,
        Concepto = "Compra nueva",
        Importe = 75m,
        Moneda = "EUR",
        FechaMovimiento = new DateTime(2024, 2, 1)
    };

    [Fact]
    public async Task Handle_ActualizaMovimientoExitosamente()
    {
        // Arrange
        var command = BuildCommand();
        var movimiento = new Movimiento { IdMovimiento = IdMovimiento, IdCuenta = IdCuenta, Concepto = "Compra antigua", Importe = 50m, FechaCreacion = DateTime.UtcNow };
        var movimientoActualizado = movimiento with { Concepto = command.Concepto, Importe = command.Importe, FechaModificacion = DateTime.UtcNow };

        _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);
        _movimientoRepository.UpdateMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>()).Returns(movimientoActualizado);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(IdMovimiento, result.IdMovimiento);
        Assert.Equal("Compra nueva", result.Concepto);
        Assert.Equal(75m, result.Importe);
    }

    [Theory]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, false)]
    public async Task Handle_RecursoNoEncontrado_LanzaNotFoundException(
        bool usuarioExiste, bool cuentaExiste, bool movimientoExiste, bool categoriaExiste)
    {
        var command = BuildCommand();
        var movimiento = new Movimiento
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            FechaCreacion = DateTime.UtcNow
        };

        if (!usuarioExiste)
            _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        if (!cuentaExiste)
            _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns((Cuenta?)null);

        if (movimientoExiste)
        {
            _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);
            _movimientoRepository.UpdateMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>()).Returns(movimiento);
        }
        else
        {
            _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns((Movimiento?)null);
        }

        if (!categoriaExiste)
            _cuentaRepository.GetCategoriaByIdAsync(IdCuenta, IdCuentaCategoriaNew, Arg.Any<CancellationToken>()).Returns((CuentaCategoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None).AsTask());
    }
}
