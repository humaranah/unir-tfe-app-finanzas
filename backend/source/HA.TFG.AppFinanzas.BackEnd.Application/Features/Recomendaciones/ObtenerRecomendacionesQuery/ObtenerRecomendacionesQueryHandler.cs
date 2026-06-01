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
    private const string AdvisorInstructions =
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
        var usuario = await usuarioRepository.GetByEmailAsync(request.UserEmail, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.UserEmail);

        var cuenta = await cuentaRepository.GetCuentaByIdAsync(usuario.IdUsuario, request.IdCuenta, cancellationToken)
            ?? throw new NotFoundException(nameof(Cuenta), request.IdCuenta);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);
        var historicStart = currentMonthStart.AddMonths(-12);
        var historicEnd = currentMonthStart.AddDays(-1);

        var currentMonthSummary = await movimientoRepository.GetResumenGastosPorCategoriaAsync(
            cuenta.IdCuenta, currentMonthStart, today, cancellationToken);

        var historicSummary = await movimientoRepository.GetResumenGastosPorCategoriaAsync(
            cuenta.IdCuenta, historicStart, historicEnd, cancellationToken);

        logger.LogInformation(
            "Generando recomendaciones para cuenta {IdCuenta}: {CurrentMonth} categorías en el mes actual, {Historic} entradas históricas.",
            cuenta.IdCuenta, currentMonthSummary.Count, historicSummary.Count);

        var prompt = BuildPrompt(cuenta.Moneda, currentMonthSummary, historicSummary, request.Query);

        var response = await llmService.EnviarPromptAsync(prompt, cancellationToken, AdvisorInstructions);

        if (string.IsNullOrWhiteSpace(response))
            throw new ExternalServiceException(
                "FoundryAI",
                "El modelo de IA no devolvió una respuesta válida. Inténtalo de nuevo más tarde.");

        return new RecomendacionResult
        {
            Content = response.Trim(),
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static string BuildPrompt(
        string accountCurrency,
        IReadOnlyList<ResumenGastoCategoria> currentMonth,
        IReadOnlyList<ResumenGastoCategoria> historic,
        string? query)
    {
        var sb = new StringBuilder();

        sb.AppendLine("## GASTOS DEL MES ACTUAL (por categoría)");
        if (currentMonth.Count == 0)
        {
            sb.AppendLine("Sin gastos registrados en el mes actual.");
        }
        else
        {
            foreach (var r in currentMonth)
                sb.AppendLine($"- {r.NombreCategoria}: {r.Total:F2} {r.Moneda}");
        }

        sb.AppendLine();

        sb.AppendLine("## HISTÓRICO DE GASTOS (hasta 12 meses anteriores, por mes y categoría)");
        if (historic.Count == 0)
        {
            sb.AppendLine("Sin historial de gastos disponible.");
        }
        else
        {
            var byMonth = historic
                .GroupBy(r => (r.Año, r.Mes))
                .OrderByDescending(g => g.Key.Año)
                .ThenByDescending(g => g.Key.Mes);

            foreach (var month in byMonth)
            {
                sb.AppendLine($"### {month.Key.Año}-{month.Key.Mes:D2}");
                foreach (var r in month.OrderByDescending(r => r.Total))
                    sb.AppendLine($"  - {r.NombreCategoria}: {r.Total:F2} {r.Moneda}");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"La moneda principal de la cuenta es: {accountCurrency}.");

        sb.AppendLine();
        sb.AppendLine("## CONSULTA DEL USUARIO");
        sb.AppendLine(string.IsNullOrWhiteSpace(query)
            ? "El usuario no ha indicado una consulta específica. Ofrece un análisis general con tendencias y 2-3 consejos accionables."
            : query.Trim());

        return sb.ToString();
    }
}
