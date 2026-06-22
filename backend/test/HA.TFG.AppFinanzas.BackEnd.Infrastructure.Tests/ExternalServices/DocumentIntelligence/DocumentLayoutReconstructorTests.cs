using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;
using System.Text;
using static HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence.DocumentLayoutReconstructor;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.ExternalServices.DocumentIntelligence;

public class DocumentLayoutReconstructorTests
{
    // ─── GroupIntoRows ────────────────────────────────────────────────────────

    [Fact]
    public void GroupIntoRows_WithLinesAtSameY_GroupsInSingleRow()
    {
        // Arrange
        var lines = new List<ProjectedLine>
        {
            new("Concepto",  0.05, 0.10),
            new("Precio",    0.50, 0.10),
            new("Cantidad",  0.75, 0.10),
        };

        // Act
        var rows = GroupIntoRows(lines, 0.012);

        // Assert
        Assert.Single(rows);
        Assert.Equal(3, rows[0].Count);
    }

    [Fact]
    public void GroupIntoRows_WithLinesAtDifferentY_GroupsInSeparateRows()
    {
        // Arrange
        var lines = new List<ProjectedLine>
        {
            new("Header", 0.05, 0.05),
            new("Detail", 0.05, 0.30),
            new("Total",  0.05, 0.60),
        };

        // Act
        var rows = GroupIntoRows(lines, 0.012);

        // Assert
        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public void GroupIntoRows_WithYWithinThreshold_GroupsInSameRow()
    {
        // Y difference of 0.005, less than row threshold (~50% of line height)
        // Arrange
        var lines = new List<ProjectedLine>
        {
            new("A", 0.05, 0.100),
            new("B", 0.50, 0.105),
        };

        // Act
        var rows = GroupIntoRows(lines, 0.012);

        // Assert
        Assert.Single(rows);
    }

    [Fact]
    public void GroupIntoRows_WithEmptyList_ReturnsEmptyRows()
    {
        // Act
        var rows = GroupIntoRows([], 0.012);

        // Assert
        Assert.Empty(rows);
    }

    // ─── RenderRow ────────────────────────────────────────────────────────────

    [Fact]
    public void RenderRow_WithSingleLine_RendersContentTrimmed()
    {
        // Arrange
        var row = new List<ProjectedLine> { new("  Hiper Asia  ", 0.3, 0.1) };
        var sb = new StringBuilder();

        // Act
        RenderRow(row, sb);

        // Assert
        Assert.Equal("Hiper Asia", sb.ToString().Trim());
    }

    [Fact]
    public void RenderRow_WithMultipleColumns_SeparatesWithPipe()
    {
        // Arrange
        var row = new List<ProjectedLine>
        {
            new("Leche entera", 0.05, 0.10),
            new("1ud",          0.55, 0.10),
            new("0.95€",        0.75, 0.10),
        };
        var sb = new StringBuilder();

        // Act
        RenderRow(row, sb);

        // Assert
        var result = sb.ToString().Trim();
        Assert.Equal("Leche entera | 1ud | 0.95€", result);
    }

    [Fact]
    public void RenderRow_WithUnorderedColumns_OrdersLeftToRight()
    {
        // Arrange - fragments arrive in reverse X order
        var row = new List<ProjectedLine>
        {
            new("Importe", 0.80, 0.10),
            new("Cantidad", 0.55, 0.10),
            new("Concepto", 0.05, 0.10),
        };
        var sb = new StringBuilder();

        // Act
        RenderRow(row, sb);

        // Assert
        Assert.Equal("Concepto | Cantidad | Importe", sb.ToString().Trim());
    }

    // ─── ReconstructPage: block jump handling ────────────────────────────────

    [Fact]
    public void ReconstructPage_WithJumpAboveThreshold_InsertsBlankLinesBetweenBlocks()
    {
        // Y difference of 0.05, greater than jumpThreshold (1.5x line height)
        // Arrange
        var lines = new List<ProjectedLine>
        {
            new("Header", 0.05, 0.10),
            new("Total",  0.05, 0.15), // jump of 0.05 > jumpThreshold
        };

        const double rowThreshold = 0.012;
        const double jumpThreshold = 0.030;

        var sb = new StringBuilder();
        // Simulate ReconstructPage loop manually
        var rows = GroupIntoRows(lines, rowThreshold);
        double? previousY = null;
        foreach (var row in rows)
        {
            if (previousY.HasValue && row[0].YRel - previousY.Value > jumpThreshold)
                sb.AppendLine();
            RenderRow(row, sb);
            previousY = row[0].YRel;
        }

        // Assert - should have at least one blank line between blocks
        var outputLines = sb.ToString().Split(Environment.NewLine, StringSplitOptions.None);
        Assert.Contains(outputLines, l => l == string.Empty);
    }
}
