using Riok.Mapperly.Abstractions;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;
using HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

namespace HA.TFG.AppFinanzas.BackEnd.Mappers;

[Mapper]
public static partial class UsuarioMapper
{
	[MapValue(nameof(EnsureUsuarioCommand.IdAuth0), "")]
	public static partial EnsureUsuarioCommand ToEnsureUsuarioCommand(this EnsureUsuarioRequest request);
}
