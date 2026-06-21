using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.DeleteCuentaCategoriaCommand;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Cuentas;

public class DeleteCuentaCategoriaCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly ICuentaCategoriaRepository _categoriaRepository = Substitute.For<ICuentaCategoriaRepository>();
    private readonly DeleteCuentaCategoriaCommandHandler _sut;

    private static readonly Guid IdUsuario   = Guid.Parse("00000000-0000-7000-8000-000000000001");
    private static readonly Guid IdCuenta    = Guid.Parse("00000000-0000-7000-8000-000000000010");
    private static readonly Guid IdCategoria = Guid.Parse("00000000-0000-7000-8000-000000000100");

    private static readonly Usuario UsuarioValido = new() { IdUsuario = IdUsuario, Email = "test@test.com" };
    private static readonly Cuenta  CuentaValida  = new() { IdCuenta = IdCuenta, Descripcion = "Test", Moneda = "EUR" };
    private static readonly CuentaCategoria CategoriaValida = new()
    {
        IdCuentaCategoria = IdCategoria,
        IdCuenta = IdCuenta,
        Nombre = "Alimentación",
        TipoMovimiento = TipoMovimiento.Gasto
    };

    public DeleteCuentaCategoriaCommandHandlerTests()
    {
        _sut = new DeleteCuentaCategoriaCommandHandler(
            _usuarioRepository, _cuentaRepository, _categoriaRepository);
    }

    private DeleteCuentaCategoriaCommand BuildCommand() =>
        new() { Email = UsuarioValido.Email, IdCuenta = IdCuenta, IdCuentaCategoria = IdCategoria };

    private void ConfigurarHappyPath()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaValida);
        _categoriaRepository.GetCategoriaByIdAsync(IdCuenta, IdCategoria, Arg.Any<CancellationToken>())
            .Returns(CategoriaValida);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CategoriaExistente_LlamaDeleteCategoriaAsync()
    {
        ConfigurarHappyPath();

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        await _categoriaRepository.Received(1)
            .DeleteCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CategoriaExistente_PassaLaCategoriaCorrecta()
    {
        ConfigurarHappyPath();

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        await _categoriaRepository.Received(1)
            .DeleteCategoriaAsync(
                Arg.Is<CuentaCategoria>(c => c.IdCuentaCategoria == IdCategoria),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CategoriaExistente_BuscaPorIdCuentaEIdCategoria()
    {
        ConfigurarHappyPath();

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        await _categoriaRepository.Received(1)
            .GetCategoriaByIdAsync(IdCuenta, IdCategoria, Arg.Any<CancellationToken>());
    }

    // ── Usuario no existe ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_NoConsultaCuentaNiLlamaDelete()
    {
        _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());

        await _cuentaRepository.DidNotReceive()
            .GetCuentaByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _categoriaRepository.DidNotReceive()
            .DeleteCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
    }

    // ── Cuenta no existe / no pertenece al usuario ────────────────────────────

    [Fact]
    public async Task Handle_CuentaNoExiste_LanzaNotFoundException()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_CuentaNoExiste_NoConsultaCategoriaYNoLlamaDelete()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());

        await _categoriaRepository.DidNotReceive()
            .GetCategoriaByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _categoriaRepository.DidNotReceive()
            .DeleteCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
    }

    // ── Categoría no existe ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CategoriaNoExiste_LanzaNotFoundException()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaValida);
        _categoriaRepository.GetCategoriaByIdAsync(IdCuenta, IdCategoria, Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_CategoriaNoExiste_NoLlamaDeleteCategoriaAsync()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaValida);
        _categoriaRepository.GetCategoriaByIdAsync(IdCuenta, IdCategoria, Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());

        await _categoriaRepository.DidNotReceive()
            .DeleteCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
    }
}
