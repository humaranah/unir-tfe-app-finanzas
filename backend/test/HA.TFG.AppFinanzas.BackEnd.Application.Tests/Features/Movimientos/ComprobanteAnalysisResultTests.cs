using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class ComprobanteAnalysisResultTests
{
    [Fact]
    public void Concepto_WithSingleItemAndDescription_ReturnsItemDescription()
    {
        // Arrange
        var result = new ComprobanteAnalysisResult
        {
            Items = [new ReceiptItemResult { Description = "Coca-Cola 500ml", TotalPrice = 2.50m }]
        };

        // Act & Assert
        Assert.Equal("Coca-Cola 500ml", result.Concepto);
    }

    [Fact]
    public void Concepto_WithMultipleItems_ReturnsNullForLlmInference()
    {
        // Arrange
        var result = new ComprobanteAnalysisResult
        {
            Items =
            [
                new ReceiptItemResult { Description = "Pan", TotalPrice = 1.20m },
                new ReceiptItemResult { Description = "Leche", TotalPrice = 0.90m },
            ]
        };

        // Act & Assert
        Assert.Null(result.Concepto);
    }

    [Fact]
    public void Concepto_WithoutItemsOrSingleItemWithoutDescription_ReturnsNull()
    {
        // Arrange & Act
        var noItems = new ComprobanteAnalysisResult();
        var singleItemNoDesc = new ComprobanteAnalysisResult
        {
            Items = [new ReceiptItemResult { Description = null, TotalPrice = 5.00m }]
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Null(noItems.Concepto);
            Assert.Null(singleItemNoDesc.Concepto);
        });
    }

    [Fact]
    public void FechaMovimiento_WithDateAndTime_CombinesCorrectly()
    {
        // Arrange
        var result = new ComprobanteAnalysisResult
        {
            TransactionDate = new DateOnly(2026, 5, 11),
            TransactionTime = new TimeOnly(14, 30, 0),
        };
        var expected = new DateTimeOffset(2026, 5, 11, 14, 30, 0, TimeSpan.Zero);

        // Act & Assert
        Assert.Equal(expected, result.FechaMovimiento);
    }

    [Fact]
    public void FechaMovimiento_WithDateButNoTime_UsesMidnightUtc()
    {
        // Arrange
        var result = new ComprobanteAnalysisResult
        {
            TransactionDate = new DateOnly(2026, 5, 11),
        };
        var expected = new DateTimeOffset(2026, 5, 11, 0, 0, 0, TimeSpan.Zero);

        // Act & Assert
        Assert.Equal(expected, result.FechaMovimiento);
    }

    [Fact]
    public void FechaMovimiento_WithoutDateOrTimeOnly_ReturnsNull()
    {
        // Arrange & Act
        var noDate = new ComprobanteAnalysisResult();
        var timeOnly = new ComprobanteAnalysisResult { TransactionTime = new TimeOnly(10, 0, 0) };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Null(noDate.FechaMovimiento);
            Assert.Null(timeOnly.FechaMovimiento);
        });
    }
}
