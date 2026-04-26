using FluentValidation.Results;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;

public sealed class ValidationException(IEnumerable<ValidationFailure> failures) : Exception("Se produjeron errores de validación.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = failures
        .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
        .ToDictionary(g => g.Key, g => g.ToArray());
}
