namespace HA.TFG.AppFinanzas.BackEnd.Domain.Common;

public interface ISoftDeleteable
{
    DateTime? FechaEliminacion { get; }
}
