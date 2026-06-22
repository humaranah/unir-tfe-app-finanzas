using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class ComprobantePromptBuilderTests
{
    private static readonly Guid IdCuenta = Guid.CreateVersion7();

    private static readonly IReadOnlyList<CuentaCategoria> DefaultCategories =
    [
        new() { IdCuentaCategoria = Guid.CreateVersion7(), IdCuenta = IdCuenta, TipoMovimiento = TipoMovimiento.Gasto,   Nombre = "Alimentación" },
        new() { IdCuentaCategoria = Guid.CreateVersion7(), IdCuenta = IdCuenta, TipoMovimiento = TipoMovimiento.Ingreso, Nombre = "Nómina" },
    ];

    [Fact]
    public void Build_WithExtractedFields_SchemaDynamicallyExcludesThemAndIncludesOnlyRequired()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult
        {
            Texto           = "t",
            MerchantName    = "Tienda",
            Total           = 10m,
            Currency        = "PEN",
            TransactionDate = new DateOnly(2026, 1, 1),
            Items           = [new ReceiptItemResult { Description = "Producto único", TotalPrice = 10m }]
        };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Multiple(() =>
        {
            // Fields extracted by DI must not appear
            Assert.DoesNotContain("\"establecimiento\"", prompt);
            Assert.DoesNotContain("\"concepto\"", prompt);
            Assert.DoesNotContain("\"importe\"", prompt);
            Assert.DoesNotContain("\"moneda\"", prompt);
            Assert.DoesNotContain("\"fechaMovimiento\"", prompt);

            // Fields that LLM must always complete remain
            Assert.Contains("\"tipoMovimiento\"", prompt);
            Assert.Contains("\"idCuentaCategoria\"", prompt);
            Assert.Contains("\"nota\"", prompt);
        });
    }

    [Fact]
    public void Build_WithMissingDiFields_SchemaIncludesAllOptionalFields()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult { Texto = "Texto del ticket" };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Contains("\"establecimiento\"", prompt);
            Assert.Contains("\"concepto\"", prompt);
            Assert.Contains("\"importe\"", prompt);
            Assert.Contains("\"moneda\"", prompt);
            Assert.Contains("\"fechaMovimiento\"", prompt);
        });
    }

    [Fact]
    public void Build_WithExtractedMerchantAndCurrency_ShowsOnlyExtractedFieldsInDiSection()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult { Texto = "t", MerchantName = "SUPERMERCADO XYZ", Currency = "PEN" };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Contains("SUPERMERCADO XYZ", prompt);
            Assert.Contains("PEN", prompt);
            Assert.DoesNotContain("ninguno extraído", prompt);
        });
    }

    [Fact]
    public void Build_WithNoExtractedFields_DiSectionShowsNoneExtracted()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult { Texto = "t" };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Contains("ninguno extraído", prompt);
    }

    [Fact]
    public void Build_WithSingleItemDescription_DiSectionShowsConceptoDirectly()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult
        {
            Texto = "t",
            Items = [new ReceiptItemResult { Description = "Zapatillas deportivas", TotalPrice = 89.99m }]
        };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Contains("Zapatillas deportivas", prompt);
    }

    [Fact]
    public void Build_WithDateAndTime_DiSectionShowsBothDateAndTime()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult
        {
            Texto           = "t",
            TransactionDate = new DateOnly(2026, 3, 15),
            TransactionTime = new TimeOnly(18, 45, 0),
        };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Contains("2026-03-15", prompt);
            Assert.Contains("18:45:00", prompt);
        });
    }

    [Fact]
    public void Build_WithSingleItemDescription_InstructionsOmitConceptoRule()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult
        {
            Texto = "t",
            Items = [new ReceiptItemResult { Description = "Artículo único", TotalPrice = 5m }]
        };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert - rule "- concepto:" should not appear in INSTRUCTIONS
        Assert.DoesNotContain("- concepto:", prompt);
    }

    [Fact]
    public void Build_WithMultipleItemsOrNoItems_ConceptoRuleHintsAccordingly()
    {
        // Arrange
        var multipleItems = new ComprobanteAnalysisResult
        {
            Texto = "t",
            Items =
            [
                new ReceiptItemResult { Description = "Item A", TotalPrice = 3m },
                new ReceiptItemResult { Description = "Item B", TotalPrice = 2m },
            ]
        };
        var noItems = new ComprobanteAnalysisResult { Texto = "t" };

        // Act
        var promptMultiple = ComprobantePromptBuilder.Build(multipleItems, DefaultCategories);
        var promptEmpty = ComprobantePromptBuilder.Build(noItems, DefaultCategories);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Contains("resumir los artículos DI", promptMultiple);
            Assert.Contains("extraer del texto del comprobante", promptEmpty);
        });
    }

    [Fact]
    public void Build_WithCountryRegionButNoCurrency_CurrencyRuleIncludesRegionHint()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult { Texto = "t", CountryRegion = "PE" };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Contains("del país PE", prompt);
    }

    [Fact]
    public void Build_Always_ContainsAllRequiredSectionsAndCategories()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult { Texto = "t" };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Contains("DATOS CONFIRMADOS", prompt);
            Assert.Contains("INSTRUCCIONES", prompt);
            Assert.Contains("CATEGORÍAS", prompt);
            Assert.Contains("Alimentación", prompt);
            Assert.Contains("Nómina", prompt);
        });
    }

    [Fact]
    public void Build_WithOcrText_TextSectionAppearsAtEndWithContent()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult { Texto = "LINEA UNICA DEL TICKET" };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Contains("TEXTO DEL COMPROBANTE", prompt);
            Assert.Contains("LINEA UNICA DEL TICKET", prompt);
        });
    }

    [Fact]
    public void Build_WithEmptyText_OmitsTextSection()
    {
        // Arrange
        var di = new ComprobanteAnalysisResult { Texto = "" };

        // Act
        var prompt = ComprobantePromptBuilder.Build(di, DefaultCategories);

        // Assert
        Assert.DoesNotContain("TEXTO DEL COMPROBANTE", prompt);
    }
}
