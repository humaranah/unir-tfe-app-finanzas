using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Cuentas;

public class GetCuentasQueryHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly GetCuentasQueryHandler _sut;

    public GetCuentasQueryHandlerTests()
    {
        _sut = new GetCuentasQueryHandler(_usuarioRepository, _cuentaRepository,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<GetCuentasQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_UsuarioExistente_DevuelveSusCuentas()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Nombre = "Test" };
        var cuentas = new List<Cuenta>
        {
            new() { Id = 10, Nombre = "Cuenta A", Descripcion = "Desc A" },
            new() { Id = 11, Nombre = "Cuenta B", Descripcion = "Desc B" }
        };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.GetCuentasByUsuarioIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(cuentas);

        // Act
        var result = await _sut.Handle(new GetCuentasQuery(usuario.Email), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == 10 && r.Nombre == "Cuenta A" && r.Descripcion == "Desc A");
        Assert.Contains(result, r => r.Id == 11 && r.Nombre == "Cuenta B" && r.Descripcion == "Desc B");
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_DevuelveLista_Vacia()
    {
        // Arrange
        _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        // Act
        var result = await _sut.Handle(new GetCuentasQuery("noexiste@test.com"), CancellationToken.None);

        // Assert
        Assert.Empty(result);
        await _cuentaRepository.DidNotReceive().GetCuentasByUsuarioIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioSinCuentas_DevuelveLista_Vacia()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Nombre = "Test" };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.GetCuentasByUsuarioIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns([]);

        // Act
        var result = await _sut.Handle(new GetCuentasQuery(usuario.Email), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
