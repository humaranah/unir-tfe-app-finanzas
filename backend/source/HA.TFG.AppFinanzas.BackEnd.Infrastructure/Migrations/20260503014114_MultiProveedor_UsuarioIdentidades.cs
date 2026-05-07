using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiProveedor_UsuarioIdentidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_IdAuth0",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "IdAuth0",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Proveedor",
                table: "usuarios");

            migrationBuilder.CreateTable(
                name: "usuario_identidades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAuth0 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdUsuario = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_identidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuario_identidades_usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuario_identidades_IdAuth0",
                table: "usuario_identidades",
                column: "IdAuth0",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_identidades_IdUsuario",
                table: "usuario_identidades",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_identidades");

            migrationBuilder.AddColumn<string>(
                name: "IdAuth0",
                table: "usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Proveedor",
                table: "usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_IdAuth0",
                table: "usuarios",
                column: "IdAuth0",
                unique: true);
        }
    }
}
