using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Rol_Usuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "roles",
                columns: ["Id", "Nombre", "Descripcion", "FechaCreacion", "FechaEliminacion"],
                columnTypes: ["bigint", "nvarchar(100)", "nvarchar(500)", "datetime2", "datetime2"],
                values: new object[] { 1L, "usuario", "Rol base asignado a todos los usuarios registrados", DateTime.UtcNow, null! });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyColumnType: "bigint",
                keyValue: 1L);
        }
    }
}
