using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HA.TFG.AppFinanzas.Core.Tests.ViewModels;

public class CategoriaFormViewModelTests
{
    private readonly ICuentasService _cuentasService = Substitute.For<ICuentasService>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();

    private static readonly Guid IdCuenta = Guid.NewGuid();
    private static readonly Guid IdCategoria = Guid.NewGuid();

    private CategoriaFormViewModel CreateSut() => new(_cuentasService, _navigationService);

    #region Estado inicial

    [Fact]
    public void Constructor_InitialState_MatchesExpectedSnapshot()
    {
        var sut = CreateSut();

        Assert.Multiple(
            () => Assert.Equal(string.Empty, sut.Nombre),
            () => Assert.Equal(TipoMovimiento.Gasto, sut.TipoSeleccionado),
            () => Assert.Equal(string.Empty, sut.Error),
            () => Assert.False(sut.HasError),
            () => Assert.False(sut.IsBusy),
            () => Assert.True(sut.IsNotBusy),
            () => Assert.False(sut.EsModoEdicion),
            () => Assert.Equal("Nueva categoría", sut.Titulo));
    }

    [Fact]
    public void Tipos_ContainsGastoAndIngreso()
    {
        var sut = CreateSut();

        Assert.Multiple(
            () => Assert.Equal(2, sut.Tipos.Count),
            () => Assert.Contains(TipoMovimiento.Gasto, sut.Tipos),
            () => Assert.Contains(TipoMovimiento.Ingreso, sut.Tipos));
    }

    [Fact]
    public void GuardarCommand_WhenNombreIsEmpty_CannotExecute()
    {
        var sut = CreateSut();
        sut.Nombre = string.Empty;

        Assert.False(sut.GuardarCommand.CanExecute(null));
    }

    [Fact]
    public void GuardarCommand_WhenNombreIsWhitespace_CannotExecute()
    {
        var sut = CreateSut();
        sut.Nombre = "   ";

        Assert.False(sut.GuardarCommand.CanExecute(null));
    }

    [Fact]
    public void GuardarCommand_WhenNombreHasContent_CanExecute()
    {
        var sut = CreateSut();
        sut.Nombre = "Alimentación";

        Assert.True(sut.GuardarCommand.CanExecute(null));
    }

    #endregion

    #region Initialize — modo creación

    [Fact]
    public void Initialize_WithoutIdCategoria_SetsModoCreacion()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta);

        Assert.Multiple(
            () => Assert.False(sut.EsModoEdicion),
            () => Assert.Equal("Nueva categoría", sut.Titulo));
    }

    [Fact]
    public void Initialize_WithNombreAndTipo_SetsProperties()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta, nombre: "Transporte", tipo: TipoMovimiento.Ingreso);

        Assert.Multiple(
            () => Assert.Equal("Transporte", sut.Nombre),
            () => Assert.Equal(TipoMovimiento.Ingreso, sut.TipoSeleccionado));
    }

    [Fact]
    public void Initialize_WithNullNombre_SetsNombreToEmpty()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta, nombre: null);

        Assert.Equal(string.Empty, sut.Nombre);
    }

    [Fact]
    public void Initialize_ClearsPreviousError()
    {
        var sut = CreateSut();
        sut.Error = "error previo";

        sut.Initialize(IdCuenta);

        Assert.Empty(sut.Error);
    }

    #endregion

    #region Initialize — modo edición

    [Fact]
    public void Initialize_WithIdCategoria_SetsModoEdicion()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta, IdCategoria, "Ocio", TipoMovimiento.Gasto);

        Assert.Multiple(
            () => Assert.True(sut.EsModoEdicion),
            () => Assert.Equal("Editar categoría", sut.Titulo));
    }

    [Fact]
    public void Initialize_WithIdCategoria_RestoresNombreYTipo()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta, IdCategoria, "Sueldo", TipoMovimiento.Ingreso);

        Assert.Multiple(
            () => Assert.Equal("Sueldo", sut.Nombre),
            () => Assert.Equal(TipoMovimiento.Ingreso, sut.TipoSeleccionado));
    }

    #endregion

    #region GuardarAsync — modo creación (caso feliz)

    [Fact]
    public async Task GuardarAsync_WhenModoCreacion_CallsCreateCategoria()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "Alimentación";
        sut.TipoSeleccionado = TipoMovimiento.Gasto;

        await sut.GuardarCommand.ExecuteAsync(null);

        await _cuentasService.Received(1).CreateCategoriaAsync(
            IdCuenta, "Alimentación", TipoMovimiento.Gasto, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GuardarAsync_WhenModoCreacion_DoesNotCallUpdateCategoria()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "Alimentación";

        await sut.GuardarCommand.ExecuteAsync(null);

        await _cuentasService.DidNotReceive().UpdateCategoriaAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(),
            Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GuardarAsync_WhenModoCreacion_TrimsNombreBeforeCallingService()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "  Supermercado  ";

        await sut.GuardarCommand.ExecuteAsync(null);

        await _cuentasService.Received(1).CreateCategoriaAsync(
            IdCuenta, "Supermercado", Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GuardarAsync_WhenModoCreacionSucceeds_NavigatesBack()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "Ocio";

        await sut.GuardarCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoBackAsync();
    }

    [Fact]
    public async Task GuardarAsync_WhenModoCreacionSucceeds_ClearsError()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "Ocio";

        await sut.GuardarCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.Empty(sut.Error),
            () => Assert.False(sut.HasError),
            () => Assert.False(sut.IsBusy));
    }

    #endregion

    #region GuardarAsync — modo edición (caso feliz)

    [Fact]
    public async Task GuardarAsync_WhenModoEdicion_CallsUpdateCategoria()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta, IdCategoria, "Ocio", TipoMovimiento.Gasto);
        sut.Nombre = "Ocio y entretenimiento";
        sut.TipoSeleccionado = TipoMovimiento.Gasto;

        await sut.GuardarCommand.ExecuteAsync(null);

        await _cuentasService.Received(1).UpdateCategoriaAsync(
            IdCuenta, IdCategoria, "Ocio y entretenimiento", TipoMovimiento.Gasto, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GuardarAsync_WhenModoEdicion_DoesNotCallCreateCategoria()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta, IdCategoria, "Ocio", TipoMovimiento.Gasto);

        await sut.GuardarCommand.ExecuteAsync(null);

        await _cuentasService.DidNotReceive().CreateCategoriaAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GuardarAsync_WhenModoEdicionSucceeds_NavigatesBack()
    {
        var sut = CreateSut();
        sut.Initialize(IdCuenta, IdCategoria, "Ocio", TipoMovimiento.Gasto);

        await sut.GuardarCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoBackAsync();
    }

    #endregion

    #region GuardarAsync — errores

    [Fact]
    public async Task GuardarAsync_WhenModoCreacionThrows_SetsError()
    {
        _cuentasService
            .CreateCategoriaAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("fallo"));

        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "Alimentación";

        await sut.GuardarCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => Assert.Equal("No se pudo crear la categoría. Inténtalo de nuevo.", sut.Error),
            () => Assert.True(sut.HasError),
            () => Assert.False(sut.IsBusy));
    }

    [Fact]
    public async Task GuardarAsync_WhenModoCreacionThrows_DoesNotNavigateBack()
    {
        _cuentasService
            .CreateCategoriaAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("fallo"));

        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "Alimentación";

        await sut.GuardarCommand.ExecuteAsync(null);

        await _navigationService.DidNotReceive().GoBackAsync();
    }

    [Fact]
    public async Task GuardarAsync_WhenModoEdicionThrows_SetsErrorDistintoAlDeCreacion()
    {
        _cuentasService
            .UpdateCategoriaAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("fallo"));

        var sut = CreateSut();
        sut.Initialize(IdCuenta, IdCategoria, "Ocio", TipoMovimiento.Gasto);

        await sut.GuardarCommand.ExecuteAsync(null);

        Assert.Equal("No se pudo actualizar la categoría. Inténtalo de nuevo.", sut.Error);
    }

    [Fact]
    public async Task GuardarAsync_WhenCalledAfterError_ClearsErrorBeforeAttempt()
    {
        _cuentasService
            .CreateCategoriaAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("fallo"));

        var sut = CreateSut();
        sut.Initialize(IdCuenta);
        sut.Nombre = "Alimentación";

        await sut.GuardarCommand.ExecuteAsync(null);
        Assert.NotEmpty(sut.Error);

        _cuentasService
            .CreateCategoriaAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<TipoMovimiento>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await sut.GuardarCommand.ExecuteAsync(null);

        Assert.Empty(sut.Error);
    }

    #endregion
}
