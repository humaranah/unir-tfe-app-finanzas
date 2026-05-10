using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Cuentas;

public class CreateCuentaCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly CreateCuentaCommandHandler _sut;

    public CreateCuentaCommandHandlerTests()
    {
        _sut = new CreateCuentaCommandHandler(_usuarioRepository, _cuentaRepository);
    }

    [Fact]
    public async Task Handle_UsuarioExistente_CreaCuentaYDevuelveResult()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Nombre = "Test" };
        var cuentaCreada = new Cuenta { Id = 10, Moneda = "EUR", Descripcion = "Mi cuenta" };
        var command = new CreateCuentaCommand { Email = usuario.Email, Moneda = "EUR", Descripcion = "Mi cuenta" };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.CreateCuentaConCategoriasAsync(Arg.Any<Cuenta>(), Arg.Any<CancellationToken>()).Returns(cuentaCreada);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.IdCuenta);
        Assert.Equal("EUR", result.Moneda);
        Assert.Equal("Mi cuenta", result.Descripcion);
    }

    [Fact]
    public async Task Handle_UsuarioExistente_LlamaCuentaRepositoryConUsuarioAsociado()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Nombre = "Test" };
        var command = new CreateCuentaCommand { Email = usuario.Email, Moneda = "USD" };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.CreateCuentaConCategoriasAsync(Arg.Any<Cuenta>(), Arg.Any<CancellationToken>())
            .Returns(c => c.Arg<Cuenta>());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _cuentaRepository.Received(1).CreateCuentaConCategoriasAsync(
            Arg.Is<Cuenta>(c => c.Moneda == "USD" && c.Usuarios.Contains(usuario)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaInvalidOperationException()
    {
        // Arrange
        _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        var command = new CreateCuentaCommand { Email = "noexiste@test.com", Moneda = "EUR" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None).AsTask());

        await _cuentaRepository.DidNotReceive().CreateCuentaConCategoriasAsync(Arg.Any<Cuenta>(), Arg.Any<CancellationToken>());
    }
}
