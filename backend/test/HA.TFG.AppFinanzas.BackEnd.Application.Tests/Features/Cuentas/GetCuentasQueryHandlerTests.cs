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
        var usuario = new Usuario { IdUsuario = Guid.Parse("00000000-0000-7000-8000-000000000001"), Email = "test@test.com", Nombre = "Test" };
        var idA = Guid.Parse("00000000-0000-7000-8000-000000000010");
        var idB = Guid.Parse("00000000-0000-7000-8000-000000000011");
        var cuentas = new List<Cuenta>
        {
            new() { IdCuenta = idA, Moneda = "EUR", Descripcion = "Desc A" },
            new() { IdCuenta = idB, Moneda = "EUR", Descripcion = "Desc B" }
        };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.GetCuentasByUsuarioIdAsync(usuario.IdUsuario, Arg.Any<CancellationToken>()).Returns(cuentas);

        // Act
        var result = await _sut.Handle(new GetCuentasQuery(usuario.Email), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == idA && r.Descripcion == "Desc A");
        Assert.Contains(result, r => r.Id == idB && r.Descripcion == "Desc B");
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
        await _cuentaRepository.DidNotReceive().GetCuentasByUsuarioIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioSinCuentas_DevuelveLista_Vacia()
    {
        // Arrange
        var usuario = new Usuario { IdUsuario = Guid.Parse("00000000-0000-7000-8000-000000000001"), Email = "test@test.com", Nombre = "Test" };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.GetCuentasByUsuarioIdAsync(usuario.IdUsuario, Arg.Any<CancellationToken>()).Returns([]);

        // Act
        var result = await _sut.Handle(new GetCuentasQuery(usuario.Email), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
