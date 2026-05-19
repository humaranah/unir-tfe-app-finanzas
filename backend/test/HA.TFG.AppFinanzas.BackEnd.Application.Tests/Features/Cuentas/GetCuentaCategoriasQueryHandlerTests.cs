using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentaCategoriasQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Cuentas;

public class GetCuentaCategoriasQueryHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICuentaRepository _cuentaRepository = Substitute.For<ICuentaRepository>();
    private readonly GetCuentaCategoriasQueryHandler _sut;

    private static readonly Guid IdUsuario = Guid.Parse("00000000-0000-7000-8000-000000000001");
    private static readonly Guid IdCuenta  = Guid.Parse("00000000-0000-7000-8000-000000000010");

    public GetCuentaCategoriasQueryHandlerTests()
    {
        _sut = new GetCuentaCategoriasQueryHandler(
            _usuarioRepository,
            _cuentaRepository);
    }

    [Fact]
    public async Task Handle_CuentaExistente_DevuelveCategorias()
    {
        // Arrange
        var usuario = new Usuario { IdUsuario = IdUsuario, Email = "test@test.com" };
        var categorias = new List<CuentaCategoria>
        {
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Nómina",    TipoMovimiento = TipoMovimiento.Ingreso },
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Supermercado", TipoMovimiento = TipoMovimiento.Gasto }
        };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(categorias);

        var query = new GetCuentaCategoriasQuery(usuario.Email, IdCuenta);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Nombre == "Nómina"        && r.TipoMovimiento == TipoMovimiento.Ingreso);
        Assert.Contains(result, r => r.Nombre == "Supermercado"  && r.TipoMovimiento == TipoMovimiento.Gasto);
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _usuarioRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Usuario?)null);

        var query = new GetCuentaCategoriasQuery("noexiste@test.com", IdCuenta);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Handle(query, CancellationToken.None).AsTask());

        await _cuentaRepository.DidNotReceive()
            .GetCategoriasByCuentaAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuentaSinCategorias_DevuelveListaVacia()
    {
        // Arrange
        var usuario = new Usuario { IdUsuario = IdUsuario, Email = "test@test.com" };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(new List<CuentaCategoria>());

        var query = new GetCuentaCategoriasQuery(usuario.Email, IdCuenta);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ConsultaRepositorioConIdUsuarioCorrecto()
    {
        // Arrange
        var usuario = new Usuario { IdUsuario = IdUsuario, Email = "test@test.com" };
        var categorias = new List<CuentaCategoria>
        {
            new() { IdCuentaCategoria = Guid.NewGuid(), Nombre = "Transporte", TipoMovimiento = TipoMovimiento.Gasto }
        };

        _usuarioRepository.GetByEmailAsync(usuario.Email, Arg.Any<CancellationToken>()).Returns(usuario);
        _cuentaRepository.GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>())
            .Returns(categorias);

        var query = new GetCuentaCategoriasQuery(usuario.Email, IdCuenta);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _cuentaRepository.Received(1)
            .GetCategoriasByCuentaAsync(IdUsuario, IdCuenta, Arg.Any<CancellationToken>());
    }
}
