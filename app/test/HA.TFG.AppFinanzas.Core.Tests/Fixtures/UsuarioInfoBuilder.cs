using HA.TFG.AppFinanzas.Core.Models;

namespace HA.TFG.AppFinanzas.Core.Tests.Fixtures;

public class UsuarioInfoBuilder
{
    private string _email = "test@example.com";
    private string _nombre = "Test User";
    private string? _fotoPerfil = null;
    private string? _proveedor = null;
    private bool _emailVerificado = false;
    private DateTimeOffset? _ultimaActualizacion = null;

    public UsuarioInfoBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UsuarioInfoBuilder WithName(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    public UsuarioInfoBuilder WithProvider(string proveedor)
    {
        _proveedor = proveedor;
        return this;
    }

    public UsuarioInfoBuilder WithEmailVerified(bool verified)
    {
        _emailVerificado = verified;
        return this;
    }

    public UsuarioInfo Build() => new UsuarioInfo(_email, _nombre, _fotoPerfil, _proveedor, _emailVerificado, _ultimaActualizacion);
}
