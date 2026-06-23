using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Infrastructure.Clients;
using HA.TFG.AppFinanzas.Infrastructure.Mappers;

namespace HA.TFG.AppFinanzas.Infrastructure.Tests.Mappers;

public class MovimientoMapperTests
{
    #region ToMovimientoItem

    [Fact]
    public void ToMovimientoItem_WithValidResponse_MapsCorrectly()
    {
        // Arrange
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: Guid.NewGuid(),
            NombreCategoria: "Comida",
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: "Almuerzo",
            Importe: 25.50m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Multiple(
            () => Assert.Equal(response.IdMovimiento, result.IdMovimiento),
            () => Assert.Equal(response.IdCuenta, result.IdCuenta),
            () => Assert.Equal(response.IdCategoria, result.IdCategoria),
            () => Assert.Equal(response.NombreCategoria, result.NombreCategoria),
            () => Assert.Equal(response.TipoMovimiento, result.TipoMovimiento),
            () => Assert.Equal(response.Concepto, result.Concepto),
            () => Assert.Equal(response.Importe, result.Importe),
            () => Assert.Equal(response.Moneda, result.Moneda),
            () => Assert.Equal(response.FechaMovimiento, result.FechaMovimiento));
    }

    [Fact]
    public void ToMovimientoItem_WithNullCategory_MapsWithoutCategory()
    {
        // Arrange
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Ingreso,
            Concepto: "Salario",
            Importe: 1000m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Multiple(
            () => Assert.Null(result.IdCategoria),
            () => Assert.Null(result.NombreCategoria));
    }

    [Theory]
    [InlineData(TipoMovimiento.Ingreso)]
    [InlineData(TipoMovimiento.Gasto)]
    [InlineData(TipoMovimiento.Transferencia)]
    public void ToMovimientoItem_WithDifferentTypes_MapsCorrectly(TipoMovimiento tipo)
    {
        // Arrange
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: tipo,
            Concepto: "Test",
            Importe: 100m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Equal(tipo, result.TipoMovimiento);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ToMovimientoItem_WithZeroAmount_MapsCorrectly()
    {
        // Arrange
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: "Zero Amount",
            Importe: 0m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Equal(0m, result.Importe);
    }

    [Fact]
    public void ToMovimientoItem_WithLargeAmount_MapsCorrectly()
    {
        // Arrange
        var largeAmount = 999999999.99m;
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Ingreso,
            Concepto: "Large Amount",
            Importe: largeAmount,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Equal(largeAmount, result.Importe);
    }

    [Fact]
    public void ToMovimientoItem_WithNegativeAmount_MapsCorrectly()
    {
        // Arrange - Negative amounts might occur in some scenarios
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: "Negative Amount",
            Importe: -50m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Equal(-50m, result.Importe);
    }

    [Fact]
    public void ToMovimientoItem_WithDifferentCurrencies_MapsCorrectly()
    {
        // Arrange
        var currencies = new[] { "USD", "EUR", "ARS", "BRL", "MXN" };
        var movimientos = new List<(string Currency, decimal Importe)>();

        foreach (var currency in currencies)
        {
            var response = new MovimientoResponse(
                IdMovimiento: Guid.NewGuid(),
                IdCuenta: Guid.NewGuid(),
                IdCategoria: null,
                NombreCategoria: null,
                TipoMovimiento: TipoMovimiento.Gasto,
                Concepto: $"Test {currency}",
                Importe: 100m,
                Moneda: currency,
                FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

            var result = MovimientoMapper.ToMovimientoItem(response);
            movimientos.Add((result.Moneda, result.Importe));
        }

        // Assert
        Assert.All(movimientos, m => Assert.NotEmpty(m.Currency));
        Assert.Equal(5, movimientos.Count);
    }

    [Fact]
    public void ToMovimientoItem_WithEmptyConcepto_MapsCorrectly()
    {
        // Arrange
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: string.Empty,
            Importe: 100m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Empty(result.Concepto);
    }

    [Fact]
    public void ToMovimientoItem_WithVeryLongConcepto_MapsCorrectly()
    {
        // Arrange
        var longConcepto = new string('A', 1000);
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: longConcepto,
            Importe: 100m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Equal(longConcepto, result.Concepto);
    }

    [Fact]
    public void ToMovimientoItem_WithMinAndMaxDates_MapsCorrectly()
    {
        // Arrange
        var minDate = DateOnly.MinValue;
        var maxDate = DateOnly.MaxValue;

        var responseMin = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: "Min Date",
            Importe: 100m,
            Moneda: "USD",
            FechaMovimiento: minDate);

        var responseMax = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: null,
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: "Max Date",
            Importe: 100m,
            Moneda: "USD",
            FechaMovimiento: maxDate);

        // Act
        var resultMin = MovimientoMapper.ToMovimientoItem(responseMin);
        var resultMax = MovimientoMapper.ToMovimientoItem(responseMax);

        // Assert
        Assert.Equal(minDate, resultMin.FechaMovimiento);
        Assert.Equal(maxDate, resultMax.FechaMovimiento);
    }

    #endregion

    #region Category Mapping Scenarios

    [Fact]
    public void ToMovimientoItem_WithOnlyIdCategory_NombreIsNull()
    {
        // Arrange
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: Guid.NewGuid(),
            NombreCategoria: null,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: "Test",
            Importe: 100m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.NotNull(result.IdCategoria);
        Assert.Null(result.NombreCategoria);
    }

    [Fact]
    public void ToMovimientoItem_WithBothCategoryFields_BothMapped()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categoryName = "Alimentación";
        var response = new MovimientoResponse(
            IdMovimiento: Guid.NewGuid(),
            IdCuenta: Guid.NewGuid(),
            IdCategoria: categoryId,
            NombreCategoria: categoryName,
            TipoMovimiento: TipoMovimiento.Gasto,
            Concepto: "Test",
            Importe: 100m,
            Moneda: "USD",
            FechaMovimiento: DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = MovimientoMapper.ToMovimientoItem(response);

        // Assert
        Assert.Equal(categoryId, result.IdCategoria);
        Assert.Equal(categoryName, result.NombreCategoria);
    }

    #endregion
}
