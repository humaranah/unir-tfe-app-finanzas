using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Usuarios;

public class EnsureUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly IRolRepository _rolRepository = Substitute.For<IRolRepository>();
    private readonly EnsureUsuarioCommandHandler _sut;

    private static readonly Rol RolUsuario = new()
    {
        Id = 1,
        Nombre = Roles.Usuario,
        FechaCreacion = DateTime.UtcNow
    };

    public EnsureUsuarioCommandHandlerTests()
    {
        _sut = new EnsureUsuarioCommandHandler(_usuarioRepository, _rolRepository);
    }

    [Fact]
    public async Task Handle_UsuarioNuevo_CreaUsuarioConRolUsuario()
    {
        // Arrange
        var command = new EnsureUsuarioCommand("auth0|123", "test@test.com", "Test User", null, true, null);

        _usuarioRepository.GetByIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _usuarioRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _rolRepository.GetByNombreAsync(Roles.Usuario, Arg.Any<CancellationToken>()).Returns(RolUsuario);
        _usuarioRepository.CreateAsync(Arg.Any<Usuario>(), Arg.Any<UsuarioIdentidad>(), Arg.Any<CancellationToken>()).Returns(call =>
            call.Arg<Usuario>() with { Id = 10 });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.EsNuevo);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.Nombre, result.Nombre);
        await _usuarioRepository.Received(1).CreateAsync(
            Arg.Is<Usuario>(u => u.Roles.Any(r => r.Nombre == Roles.Usuario)),
            Arg.Is<UsuarioIdentidad>(i => i.IdAuth0 == command.IdAuth0 && i.Proveedor == "auth0"),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_UsuarioNuevo_LanzaExcepcionSiNoExisteRolUsuario()
    {
        // Arrange
        var command = new EnsureUsuarioCommand("auth0|123", "test@test.com", "Test User", null, true, null);

        _usuarioRepository.GetByIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _usuarioRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _rolRepository.GetByNombreAsync(Roles.Usuario, Arg.Any<CancellationToken>()).Returns((Rol?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_EmailExistenteNuevoProveedor_AnadeIdentidadSinCrearUsuario()
    {
        // Arrange
        var command = new EnsureUsuarioCommand("microsoft|456", "test@test.com", "Test User", null, true, null);
        var usuarioExistente = new Usuario
        {
            Id = 5,
            Email = command.Email,
            Nombre = command.Nombre,
            FechaCreacion = DateTime.UtcNow
        };

        _usuarioRepository.GetByIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        _usuarioRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(usuarioExistente);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.EsNuevo);
        Assert.Equal(usuarioExistente.Id, result.Id);
        await _usuarioRepository.Received(1).AddIdentidadAsync(
            usuarioExistente.Id,
            Arg.Is<UsuarioIdentidad>(i => i.IdAuth0 == command.IdAuth0 && i.Proveedor == "microsoft"),
            CancellationToken.None);
        await _usuarioRepository.DidNotReceive().CreateAsync(
            Arg.Any<Usuario>(), Arg.Any<UsuarioIdentidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioExistente_DevuelveUsuarioSinCrearNiActualizar()
    {
        // Arrange
        var command = new EnsureUsuarioCommand("auth0|456", "existente@test.com", "Existente", null, true, null);
        var usuarioExistente = new Usuario
        {
            Id = 5,
            Email = command.Email,
            Nombre = command.Nombre,
            FechaCreacion = DateTime.UtcNow
        };

        _usuarioRepository.GetByIdAuth0Async(command.IdAuth0, Arg.Any<CancellationToken>()).Returns(usuarioExistente);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.EsNuevo);
        Assert.Equal(usuarioExistente.Id, result.Id);
        await _usuarioRepository.DidNotReceive().UpdateAsync(Arg.Any<Usuario>(), CancellationToken.None);
    }

    }
