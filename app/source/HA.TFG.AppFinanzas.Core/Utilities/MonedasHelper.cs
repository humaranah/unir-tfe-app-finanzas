using System.Globalization;

namespace HA.TFG.AppFinanzas.Core.Utilities;

public static class MonedasHelper
{
    private static readonly Lazy<IReadOnlyList<KeyValuePair<string, string>>> _monedas =
        new(BuildMonedasCore, LazyThreadSafetyMode.ExecutionAndPublication);

    public static IReadOnlyList<KeyValuePair<string, string>> Monedas => _monedas.Value;

    public static KeyValuePair<string, string> DefaultMoneda => GetDefaultMoneda(Monedas);

    private static List<KeyValuePair<string, string>> BuildMonedasCore()
    {
        var isoLocal = GetIsoLocal();
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        var nombresLocalizados = BuildNombresLocalizados();

        var monedas = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(c =>
            {
                try { return new RegionInfo(c.Name); }
                catch { return null; }
            })
            .Where(r => r is not null)
            .DistinctBy(r => r!.ISOCurrencySymbol)
            .Where(r => !string.IsNullOrWhiteSpace(r!.ISOCurrencySymbol)
                     && !string.IsNullOrWhiteSpace(r!.CurrencyEnglishName))
            .OrderBy(r => r!.ISOCurrencySymbol switch
            {
                var iso when iso == isoLocal => 0,
                "USD" => 1,
                "EUR" => 2,
                _ => 3
            })
            .ThenBy(r => nombresLocalizados.GetValueOrDefault(r!.ISOCurrencySymbol, r!.CurrencyEnglishName))
            .Select(r =>
            {
                var nombre = nombresLocalizados.GetValueOrDefault(r!.ISOCurrencySymbol, r!.CurrencyEnglishName);
                return new KeyValuePair<string, string>(
                    r!.ISOCurrencySymbol,
                    $"{r.ISOCurrencySymbol} – {textInfo.ToTitleCase(nombre.ToLower())}");
            })
            .ToList();

        if (monedas.Count > 0)
            return monedas;

        try
        {
            var region = new RegionInfo(CultureInfo.CurrentCulture.Name);
            return
            [
                new KeyValuePair<string, string>(
                    region.ISOCurrencySymbol,
                    $"{region.ISOCurrencySymbol} – {textInfo.ToTitleCase(region.CurrencyEnglishName.ToLower())}")
            ];
        }
        catch
        {
            return [];
        }
    }

    private static KeyValuePair<string, string> GetDefaultMoneda(IReadOnlyList<KeyValuePair<string, string>> monedas)
    {
        try
        {
            var iso = new RegionInfo(CultureInfo.CurrentCulture.Name).ISOCurrencySymbol;
            var encontrada = monedas.FirstOrDefault(m => m.Key == iso);
            return encontrada.Key is not null ? encontrada : monedas[0];
        }
        catch
        {
            return monedas[0];
        }
    }

    // Diccionario ISO → nombre en el idioma actual del dispositivo.
    // Solo cubre monedas que son la moneda local de alguna región con ese idioma.
    // Las demás usan CurrencyEnglishName como fallback en BuildMonedas.
    private static Dictionary<string, string> BuildNombresLocalizados()
    {
        var idiomaActual = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        return CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Where(c => c.TwoLetterISOLanguageName == idiomaActual)
            .Select(c =>
            {
                try { return new RegionInfo(c.Name); }
                catch { return null; }
            })
            .Where(r => r is not null && !string.IsNullOrWhiteSpace(r!.CurrencyNativeName))
            .GroupBy(r => r!.ISOCurrencySymbol)
            .ToDictionary(g => g.Key, g => g.First()!.CurrencyNativeName);
    }

    private static string GetIsoLocal()
    {
        try { return new RegionInfo(CultureInfo.CurrentCulture.Name).ISOCurrencySymbol; }
        catch { return string.Empty; }
    }
}
