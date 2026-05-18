using System;
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
                columns: new[] { "IdRol", "Descripcion", "FechaCreacion", "FechaEliminacion", "Nombre" },
                values: new object[] { new Guid("22e25092-c7c4-0c1f-58d4-c54b50a880a3"), "Usuario estándar de la aplicación", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "usuario" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "IdRol",
                keyValue: new Guid("22e25092-c7c4-0c1f-58d4-c54b50a880a3"));
        }
    }
}
