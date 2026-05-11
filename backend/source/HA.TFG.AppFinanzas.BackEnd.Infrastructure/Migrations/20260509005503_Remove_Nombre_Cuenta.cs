using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Nombre_Cuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "cuentas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "cuentas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
