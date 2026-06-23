namespace HA.TFG.AppFinanzas.Core.Tests.Fixtures;

/// <summary>
/// Factory para crear datos de prueba de forma fluida usando builders
/// </summary>
public class TestDataBuilder
{
    public static UsuarioInfoBuilder Usuario => new();
    public static MovimientoItemBuilder Movimiento => new();
    public static CategoriaItemBuilder Categoria => new();
}
