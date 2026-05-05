namespace HA.TFG.AppFinanzas.BackEnd.Domain.Common;

public interface IAuditable
{
    DateTime FechaCreacion { get; }
    DateTime? FechaModificacion { get; }
}
