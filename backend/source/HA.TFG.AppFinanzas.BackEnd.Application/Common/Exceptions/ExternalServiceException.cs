namespace HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;

public sealed class ExternalServiceException(string serviceName, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public string ServiceName { get; } = serviceName;
}
