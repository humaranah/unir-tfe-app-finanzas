using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCategoriaCommand;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Cuentas;

public class CreateCuentaCategoriaCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly ICuentaCategoriaRepository _categoriaRepository = Substitute.For<ICuentaCategoriaRepository>();
    private readonly CreateCuentaCategoriaCommandHandler _sut;

    private static readonly Guid IdUsuario = Guid.Parse("00000000-0000-7000-8000-000000000001");
    private static readonly Guid IdCuenta  = Guid.Parse("00000000-0000-7000-8000-000000000010");

    private static readonly Usuario UsuarioValido = new() { IdUsuario = IdUsuario, Email = "test@test.com" };
    private static readonly Cuenta CuentaValida   = new() { IdCuenta = IdCuenta, Descripcion = "Test", Moneda = "EUR" };

    public CreateCuentaCategoriaCommandHandlerTests()
    {
        _sut = new CreateCuentaCategoriaCommandHandler(
            _usuarioRepository, _cuentaRepository, _categoriaRepository);
    }

    private CreateCuentaCategoriaCommand BuildCommand(
        string nombre = "Alimentación",
        TipoMovimiento tipo = TipoMovimiento.Gasto) =>
        new()
        {
            Email = UsuarioValido.Email,
            IdCuenta = IdCuenta,
            Nombre = nombre,
            TipoMovimiento = tipo
        };

    private void ConfigurarHappyPath(string nombre = "Alimentación")
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaValida);
        _categoriaRepository.GetCategoriaByNombreAsync(IdCuenta, nombre, Arg.Any<CancellationToken>())
            .ReturnsNull();
        _categoriaRepository.CreateCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<CuentaCategoria>());
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DatosValidos_LlamaCreateCategoriaAsync()
    {
        ConfigurarHappyPath();

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        await _categoriaRepository.Received(1)
            .CreateCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DatosValidos_CreaCategoriaCon_IdCuenta_Nombre_Tipo_Correctos()
    {
        ConfigurarHappyPath("Transporte");
        var command = BuildCommand("Transporte", TipoMovimiento.Ingreso);

        await _sut.Handle(command, CancellationToken.None);

        await _categoriaRepository.Received(1).CreateCategoriaAsync(
            Arg.Is<CuentaCategoria>(c =>
                c.IdCuenta        == IdCuenta        &&
                c.Nombre          == "Transporte"    &&
                c.TipoMovimiento  == TipoMovimiento.Ingreso),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DatosValidos_FechaCreacionEsUtcNow()
    {
        ConfigurarHappyPath();
        var antes = DateTime.UtcNow;

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        var despues = DateTime.UtcNow;
        await _categoriaRepository.Received(1).CreateCategoriaAsync(
            Arg.Is<CuentaCategoria>(c =>
                c.FechaCreacion >= antes && c.FechaCreacion <= despues),
            Arg.Any<CancellationToken>());
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
    public async Task Handle_UsuarioNoExiste_NoConsultaCuentaNiRepositorioCategorias()
    {
        _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());

        await _cuentaRepository.DidNotReceive()
            .GetCuentaByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _categoriaRepository.DidNotReceive()
            .CreateCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
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
    public async Task Handle_CuentaNoExiste_NoLlamaCreateCategoriaAsync()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(BuildCommand(), CancellationToken.None).AsTask());

        await _categoriaRepository.DidNotReceive()
            .CreateCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
    }

    // ── Nombre duplicado ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NombreDuplicado_LanzaConflictException()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaValida);
        _categoriaRepository.GetCategoriaByNombreAsync(IdCuenta, "Alimentación", Arg.Any<CancellationToken>())
            .Returns(new CuentaCategoria { IdCuenta = IdCuenta, Nombre = "Alimentación" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.Handle(BuildCommand("Alimentación"), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NombreDuplicado_NoLlamaCreateCategoriaAsync()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaValida);
        _categoriaRepository.GetCategoriaByNombreAsync(IdCuenta, "Alimentación", Arg.Any<CancellationToken>())
            .Returns(new CuentaCategoria { IdCuenta = IdCuenta, Nombre = "Alimentación" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.Handle(BuildCommand("Alimentación"), CancellationToken.None).AsTask());

        await _categoriaRepository.DidNotReceive()
            .CreateCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BuscaDuplicadoConIdCuentaYNombreDelCommand()
    {
        _usuarioRepository.GetByEmailAsync(UsuarioValido.Email, Arg.Any<CancellationToken>())
            .Returns(UsuarioValido);
        _cuentaRepository.GetCuentaByIdAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(CuentaValida);
        _categoriaRepository.GetCategoriaByNombreAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();
        _categoriaRepository.CreateCategoriaAsync(Arg.Any<CuentaCategoria>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<CuentaCategoria>());

        await _sut.Handle(BuildCommand("Nómina"), CancellationToken.None);

        await _categoriaRepository.Received(1)
            .GetCategoriaByNombreAsync(IdCuenta, "Nómina", Arg.Any<CancellationToken>());
    }
}
