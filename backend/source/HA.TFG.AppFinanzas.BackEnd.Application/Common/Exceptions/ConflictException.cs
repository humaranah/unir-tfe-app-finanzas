namespace HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;

public sealed class ConflictException(string entityName, object? key)
    : Exception(key is not null
        ? $"Ya existe '{entityName}' con valor '{key}'."
        : $"Ya existe '{entityName}'.")
{
    public string EntityName { get; } = entityName;
    public object? SafeKey { get; } = key;
}
