using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Usuarios;

public class SyncUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly IRolRepository _rolRepository = Substitute.For<IRolRepository>();
    private readonly SyncUsuarioCommandHandler _sut;

    private static readonly Rol RolUsuario = new()
    {
        Id = 1,
        Nombre = Roles.Usuario,
        FechaCreacion = DateTime.UtcNow
    };

    public SyncUsuarioCommandHandlerTests()
    {
        _sut = new SyncUsuarioCommandHandler(_usuarioRepository, _rolRepository);
    }

    [Fact]
    public async Task Handle_UsuarioNuevo_CreaUsuarioConRolUsuario()
    {
        // Arrange
        var command = new SyncUsuarioCommand("auth0|123", "test@test.com", "Test User");

        _usuarioRepository.ObtenerPorIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _rolRepository.ObtenerPorNombreAsync(Roles.Usuario, Arg.Any<CancellationToken>()).Returns(RolUsuario);
        _usuarioRepository.CrearAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>()).Returns(call =>
            call.Arg<Usuario>() with { Id = 10 });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.EsNuevo);
        Assert.Equal(command.IdAuth0, result.IdAuth0);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.Nombre, result.Nombre);
        await _usuarioRepository.Received(1).CrearAsync(
            Arg.Is<Usuario>(u => u.Roles.Any(r => r.Nombre == Roles.Usuario)),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_UsuarioNuevo_LanzaExcepcionSiNoExisteRolUsuario()
    {
        // Arrange
        var command = new SyncUsuarioCommand("auth0|123", "test@test.com", "Test User");

        _usuarioRepository.ObtenerPorIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _rolRepository.ObtenerPorNombreAsync(Roles.Usuario, Arg.Any<CancellationToken>()).Returns((Rol?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_UsuarioExistenteSinCambios_NoActualiza()
    {
        // Arrange
        var command = new SyncUsuarioCommand("auth0|456", "existente@test.com", "Existente");
        var usuarioExistente = new Usuario
        {
            Id = 5,
            IdAuth0 = command.IdAuth0,
            Email = command.Email,
            Nombre = command.Nombre,
            FechaCreacion = DateTime.UtcNow
        };

        _usuarioRepository.ObtenerPorIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns(usuarioExistente);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.EsNuevo);
        Assert.Equal(usuarioExistente.Id, result.Id);
        await _usuarioRepository.DidNotReceive().ActualizarAsync(Arg.Any<Usuario>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_UsuarioExistenteConCambios_Actualiza()
    {
        // Arrange
        var command = new SyncUsuarioCommand("auth0|789", "nuevo@test.com", "Nuevo Nombre");
        var usuarioExistente = new Usuario
        {
            Id = 7,
            IdAuth0 = command.IdAuth0,
            Email = "viejo@test.com",
            Nombre = "Viejo Nombre",
            FechaCreacion = DateTime.UtcNow
        };
        var usuarioActualizado = usuarioExistente with
        {
            Email = command.Email,
            Nombre = command.Nombre
        };

        _usuarioRepository.ObtenerPorIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns(usuarioExistente);
        _usuarioRepository.ActualizarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>()).Returns(usuarioActualizado);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.EsNuevo);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.Nombre, result.Nombre);
        await _usuarioRepository.Received(1).ActualizarAsync(
            Arg.Is<Usuario>(u => u.Email == command.Email && u.Nombre == command.Nombre),
            CancellationToken.None);
    }
}
