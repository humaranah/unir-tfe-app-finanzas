using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Constraint_Movimiento_CuentaCategoria_MismaCuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_movimientos_cuenta_categorias_IdCuentaCategoria",
                table: "movimientos");

            migrationBuilder.DropIndex(
                name: "IX_movimientos_IdCuentaCategoria",
                table: "movimientos");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "cuenta_categorias",
                columns: new[] { "IdCuentaCategoria", "IdCuenta" });

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_IdCuentaCategoria_IdCuenta",
                table: "movimientos",
                columns: new[] { "IdCuentaCategoria", "IdCuenta" });

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "cuenta_categorias",
                columns: new[] { "IdCuentaCategoria", "IdCuenta" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_movimientos_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "movimientos",
                columns: new[] { "IdCuentaCategoria", "IdCuenta" },
                principalTable: "cuenta_categorias",
                principalColumns: new[] { "IdCuentaCategoria", "IdCuenta" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_movimientos_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "movimientos");

            migrationBuilder.DropIndex(
                name: "IX_movimientos_IdCuentaCategoria_IdCuenta",
                table: "movimientos");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "cuenta_categorias");

            migrationBuilder.DropIndex(
                name: "IX_cuenta_categorias_IdCuentaCategoria_IdCuenta",
                table: "cuenta_categorias");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_IdCuentaCategoria",
                table: "movimientos",
                column: "IdCuentaCategoria");

            migrationBuilder.AddForeignKey(
                name: "FK_movimientos_cuenta_categorias_IdCuentaCategoria",
                table: "movimientos",
                column: "IdCuentaCategoria",
                principalTable: "cuenta_categorias",
                principalColumn: "IdCuentaCategoria",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
