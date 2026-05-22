using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Remove_Redundant_Index_CuentaCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS [IX_cuenta_categorias_IdCuentaCategoria_IdCuenta] ON [cuenta_categorias];");
        }
    }
}
