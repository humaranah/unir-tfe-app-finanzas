using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Recomendaciones.ObtenerRecomendacionesQuery;

public class ObtenerRecomendacionesQueryHandler(
    IUsuarioRepository usuarioRepository,
    ICuentaRepository cuentaRepository,
    IMovimientoRepository movimientoRepository,
    ILlmService llmService,
    ILogger<ObtenerRecomendacionesQueryHandler> logger)
    : IRequestHandler<ObtenerRecomendacionesQuery, RecomendacionResult>
{
    // Instrucciones de sistema específicas para el rol de asesor financiero.
    private const string InstruccionesAsesor =
        """
        Eres un asesor financiero personal. Analizas datos de gastos reales del usuario
        y ofreces recomendaciones concretas, breves y accionables en español.
        Responde siempre en texto plano o Markdown ligero (sin bloques de código).
        Limita tus respuestas al ámbito de las finanzas personales del usuario.
        Si no hay suficientes datos para hacer recomendaciones, indícalo brevemente.
        """;

    public async ValueTask<RecomendacionResult> Handle(
        ObtenerRecomendacionesQuery request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Resolver usuario y verificar acceso a la cuenta.
        var usuario = await usuarioRepository.GetByEmailAsync(request.EmailUsuario, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.EmailUsuario);

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta);

        // Paso 2: Calcular rangos de fechas.
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioMesActual = new DateOnly(hoy.Year, hoy.Month, 1);
        var inicioPeriodoHistorico = inicioMesActual.AddMonths(-12);
        var finPeriodoHistorico = inicioMesActual.AddDays(-1);

        // Paso 3: Obtener resumen del mes actual y del histórico (hasta 12 meses anteriores).
        var resumenMesActual = await movimientoRepository.GetResumenGastosPorCategoriaAsync(
            cuenta.IdCuenta, inicioMesActual, hoy, cancellationToken);

        var resumenHistorico = await movimientoRepository.GetResumenGastosPorCategoriaAsync(
            cuenta.IdCuenta, inicioPeriodoHistorico, finPeriodoHistorico, cancellationToken);

        logger.LogInformation(
            "Generando recomendaciones para cuenta {IdCuenta}: {MesActual} categorías en el mes actual, {Historico} entradas históricas.",
            cuenta.IdCuenta, resumenMesActual.Count, resumenHistorico.Count);

        // Paso 4: Construir el prompt con los datos del usuario.
        var prompt = BuildPrompt(cuenta.Moneda, resumenMesActual, resumenHistorico, request.Consulta);

        // Paso 5: Llamar al LLM con el rol de asesor financiero.
        var respuesta = await llmService.EnviarPromptAsync(prompt, cancellationToken, InstruccionesAsesor);

        if (string.IsNullOrWhiteSpace(respuesta))
            throw new ExternalServiceException(
                "FoundryAI",
                "El modelo de IA no devolvió una respuesta válida. Inténtalo de nuevo más tarde.");

        return new RecomendacionResult
        {
            Contenido = respuesta.Trim(),
            GeneradoEn = DateTimeOffset.UtcNow
        };
    }

    private static string BuildPrompt(
        string monedaCuenta,
        IReadOnlyList<ResumenGastoCategoria> mesActual,
        IReadOnlyList<ResumenGastoCategoria> historico,
        string? consulta)
    {
        var sb = new StringBuilder();

        // Sección: mes actual
        sb.AppendLine("## GASTOS DEL MES ACTUAL (por categoría)");
        if (mesActual.Count == 0)
        {
            sb.AppendLine("Sin gastos registrados en el mes actual.");
        }
        else
        {
            foreach (var r in mesActual)
                sb.AppendLine($"- {r.Categoria}: {r.Total:F2} {r.Moneda}");
        }

        sb.AppendLine();

        // Sección: histórico agrupado por mes y categoría
        sb.AppendLine("## HISTÓRICO DE GASTOS (hasta 12 meses anteriores, por mes y categoría)");
        if (historico.Count == 0)
        {
            sb.AppendLine("Sin historial de gastos disponible.");
        }
        else
        {
            var porMes = historico
                .GroupBy(r => (r.Anio, r.Mes))
                .OrderByDescending(g => g.Key.Anio)
                .ThenByDescending(g => g.Key.Mes);

            foreach (var mes in porMes)
            {
                sb.AppendLine($"### {mes.Key.Anio}-{mes.Key.Mes:D2}");
                foreach (var r in mes.OrderByDescending(r => r.Total))
                    sb.AppendLine($"  - {r.Categoria}: {r.Total:F2} {r.Moneda}");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"La moneda principal de la cuenta es: {monedaCuenta}.");

        // Sección: consulta del usuario
        sb.AppendLine();
        sb.AppendLine("## CONSULTA DEL USUARIO");
        sb.AppendLine(string.IsNullOrWhiteSpace(consulta)
            ? "El usuario no ha indicado una consulta específica. Ofrece un análisis general con tendencias y 2-3 consejos accionables."
            : consulta.Trim());

        return sb.ToString();
    }
}
