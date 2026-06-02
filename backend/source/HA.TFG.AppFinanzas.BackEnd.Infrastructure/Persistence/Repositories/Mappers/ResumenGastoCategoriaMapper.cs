using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories.Dtos;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class ResumenGastoCategoriaMapper
{
    public static partial ResumenGastoCategoria ToContract(this ResumenGastoCategoriaRowDto dto);
}
