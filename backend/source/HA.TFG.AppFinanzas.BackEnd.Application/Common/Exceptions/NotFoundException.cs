namespace HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;

public sealed class NotFoundException(string entityName, object key)
    : Exception($"No se encontró '{entityName}' con clave '{key}'.")
{
    public string EntityName { get; } = entityName;
    public object SafeKey { get; } = key;
}
