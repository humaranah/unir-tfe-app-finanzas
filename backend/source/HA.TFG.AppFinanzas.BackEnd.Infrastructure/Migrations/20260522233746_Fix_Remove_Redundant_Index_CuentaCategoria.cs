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
            migrationBuilder.DropIndex(
                name: "IX_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "cuenta_categorias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "cuenta_categorias",
                columns: new[] { "IdCuentaCategoria", "IdCuenta" },
                unique: true);
        }
    }
}
