using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.DeleteMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class DeleteMovimientoCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly IMovimientoRepository _movimientoRepository = Substitute.For<IMovimientoRepository>();
    private readonly IComprobanteStorageService _comprobanteStorage = Substitute.For<IComprobanteStorageService>();
    private readonly DeleteMovimientoCommandHandler _sut;

    private static readonly Guid IdUsuario = Guid.Parse("00000000-0000-7000-8000-000000000001");
    private static readonly Guid IdCuenta = Guid.Parse("00000000-0000-7000-8000-000000000010");
    private static readonly Guid IdMovimiento = Guid.Parse("00000000-0000-7000-8000-000000000030");

    private readonly Usuario _usuario = new() { IdUsuario = IdUsuario, Email = "test@test.com" };
    private readonly Cuenta _cuenta = new() { IdCuenta = IdCuenta, Moneda = "EUR", Descripcion = "Cuenta test" };

    public DeleteMovimientoCommandHandlerTests()
    {
        _sut = new DeleteMovimientoCommandHandler(
            _usuarioRepository,
            _cuentaRepository,
            _movimientoRepository,
            _comprobanteStorage,
            NullLogger<DeleteMovimientoCommandHandler>.Instance);

        _usuarioRepository.GetByEmailAsync(_usuario.Email, Arg.Any<CancellationToken>()).Returns(_usuario);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns(_cuenta);
    }

    private DeleteMovimientoCommand BuildCommand() => new()
    {
        Email = _usuario.Email,
        IdCuenta = IdCuenta,
        IdMovimiento = IdMovimiento
    };

    [Fact]
    public async Task Handle_EliminaMovimientoExitosamente()
    {
        // Arrange
        var command = BuildCommand();
        var movimiento = new Movimiento
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            Concepto = "Compra",
            Importe = 50m,
            Moneda = "EUR",
            FechaCreacion = DateTime.UtcNow,
            FechaMovimiento = DateTime.UtcNow,
            IdComprobante = null
        };
        var movimientoEliminado = movimiento with { FechaEliminacion = DateTime.UtcNow };

        _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);
        _movimientoRepository.DeleteMovimientoAsync(movimiento, Arg.Any<CancellationToken>()).Returns(movimientoEliminado);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(
            () => Assert.Equal(Mediator.Unit.Value, result),
            () => _movimientoRepository.Received(1).DeleteMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>())
        );
    }

    [Fact]
    public async Task Handle_EliminaMovimientoConComprobante()
    {
        // Arrange
        var command = BuildCommand();
        const string idComprobante = "comprobante-123";
        var movimiento = new Movimiento
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            Concepto = "Compra con recibo",
            Importe = 100m,
            Moneda = "EUR",
            FechaCreacion = DateTime.UtcNow,
            FechaMovimiento = DateTime.UtcNow,
            IdComprobante = idComprobante
        };
        var movimientoEliminado = movimiento with { FechaEliminacion = DateTime.UtcNow };

        _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);
        _movimientoRepository.DeleteMovimientoAsync(movimiento, Arg.Any<CancellationToken>()).Returns(movimientoEliminado);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(
            () => _movimientoRepository.Received(1).DeleteMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>()),
            () => _comprobanteStorage.Received(1).DeleteComprobanteAsync(IdCuenta, idComprobante, Arg.Any<CancellationToken>())
        );
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public async Task Handle_RecursoNoEncontrado_LanzaNotFoundException(
        bool usuarioExiste, bool cuentaExiste, bool movimientoExiste)
    {
        var command = BuildCommand();
        var movimiento = new Movimiento
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            FechaCreacion = DateTime.UtcNow,
            FechaMovimiento = DateTime.UtcNow
        };

        if (!usuarioExiste)
            _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        if (!cuentaExiste)
            _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>()).Returns((Cuenta?)null);

        if (movimientoExiste)
        {
            _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);
            _movimientoRepository.DeleteMovimientoAsync(Arg.Any<Movimiento>(), Arg.Any<CancellationToken>()).Returns(movimiento);
        }
        else
        {
            _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns((Movimiento?)null);
        }

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ErrorEnEliminacionDeComprobante_NoLanzaExcepción()
    {
        // Arrange
        var command = BuildCommand();
        const string idComprobante = "comprobante-123";
        var movimiento = new Movimiento
        {
            IdMovimiento = IdMovimiento,
            IdCuenta = IdCuenta,
            Concepto = "Compra",
            Importe = 50m,
            Moneda = "EUR",
            FechaCreacion = DateTime.UtcNow,
            FechaMovimiento = DateTime.UtcNow,
            IdComprobante = idComprobante
        };
        var movimientoEliminado = movimiento with { FechaEliminacion = DateTime.UtcNow };

        _movimientoRepository.GetMovimientoByIdAsync(IdCuenta, IdMovimiento, Arg.Any<CancellationToken>()).Returns(movimiento);
        _movimientoRepository.DeleteMovimientoAsync(movimiento, Arg.Any<CancellationToken>()).Returns(movimientoEliminado);
        _comprobanteStorage.DeleteComprobanteAsync(IdCuenta, idComprobante, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Error al eliminar comprobante")));

        // Act & Assert: No debe lanzar excepción
        var result = await _sut.Handle(command, CancellationToken.None);
        Assert.Equal(Mediator.Unit.Value, result);
    }
}
