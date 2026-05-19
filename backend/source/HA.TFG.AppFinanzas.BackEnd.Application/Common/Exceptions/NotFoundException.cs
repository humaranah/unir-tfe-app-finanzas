namespace HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;

public sealed class NotFoundException(string entityName, object? key)
    : Exception(key is not null
        ? $"No se encontró '{entityName}' con clave '{key}'."
        : $"No se encontró '{entityName}'.")
{
    public NotFoundException(string entityName) : this(entityName, null) { }

    public string EntityName { get; } = entityName;
    public object? SafeKey { get; } = key;
}
