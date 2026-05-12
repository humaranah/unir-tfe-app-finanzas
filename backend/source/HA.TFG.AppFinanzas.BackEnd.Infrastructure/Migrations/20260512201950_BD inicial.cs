using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BDinicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    IdCategoria = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.IdCategoria);
                });

            migrationBuilder.CreateTable(
                name: "cuentas",
                columns: table => new
                {
                    IdCuenta = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas", x => x.IdCuenta);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    IdRol = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FotoPerfil = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    EmailVerificado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "cuenta_categorias",
                columns: table => new
                {
                    IdCuentaCategoria = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCuenta = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCategoria = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta_categorias", x => x.IdCuentaCategoria);
                    table.ForeignKey(
                        name: "FK_cuenta_categorias_categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cuenta_categorias_cuentas_IdCuenta",
                        column: x => x.IdCuenta,
                        principalTable: "cuentas",
                        principalColumn: "IdCuenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_cuentas",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCuenta = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_cuentas", x => new { x.IdUsuario, x.IdCuenta });
                    table.ForeignKey(
                        name: "FK_usuario_cuentas_cuentas_IdCuenta",
                        column: x => x.IdCuenta,
                        principalTable: "cuentas",
                        principalColumn: "IdCuenta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_cuentas_usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_identidades",
                columns: table => new
                {
                    IdIdentidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdAuth0 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_identidades", x => x.IdIdentidad);
                    table.ForeignKey(
                        name: "FK_usuario_identidades_usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdRol = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => new { x.IdUsuario, x.IdRol });
                    table.ForeignKey(
                        name: "FK_usuario_roles_roles_IdRol",
                        column: x => x.IdRol,
                        principalTable: "roles",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimientos",
                columns: table => new
                {
                    IdMovimiento = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCuenta = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCategoria = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    IdComprobante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nota = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos", x => x.IdMovimiento);
                    table.ForeignKey(
                        name: "FK_movimientos_cuenta_categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "cuenta_categorias",
                        principalColumn: "IdCuentaCategoria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimientos_cuentas_IdCuenta",
                        column: x => x.IdCuenta,
                        principalTable: "cuentas",
                        principalColumn: "IdCuenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categorias",
                columns: new[] { "IdCategoria", "Descripcion", "FechaCreacion", "Nombre", "TipoMovimiento" },
                values: new object[,]
                {
                    { new Guid("05d99d95-5bd2-d9b4-9bc5-282be6c98ce5"), "Restaurantes, bares y comida para llevar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Restaurantes", "Gasto" },
                    { new Guid("0ecb90bb-f405-92d9-9c49-08adb9e43ac3"), "Médico, farmacia y seguros de salud", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salud", "Gasto" },
                    { new Guid("104a0f1c-93f3-9160-0052-51d32f85ee7b"), "Cursos, libros y formación", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Educación", "Gasto" },
                    { new Guid("3ca32666-7234-8ffa-1743-87fe755d1b21"), "Compras en supermercado y alimentación en casa", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Supermercado", "Gasto" },
                    { new Guid("5c1b4a03-8a80-94ec-e3fa-ed108fba5af1"), "Alquiler, hipoteca y gastos del hogar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vivienda", "Gasto" },
                    { new Guid("87a094ed-c89e-8cb7-a230-24e661e6b81e"), "Cualquier tipo de ingreso", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ingresos", "Ingreso" },
                    { new Guid("9a6cf402-04f6-49d4-7cd0-bd0f540040c3"), "Cine, viajes, hobbies y deportes", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ocio y entretenimiento", "Gasto" },
                    { new Guid("a44f7452-de22-2136-9cda-9d446f5f86eb"), "Electricidad, agua, gas e internet", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Servicios", "Gasto" },
                    { new Guid("b40532f5-ec9b-8d31-2180-ccb81fe954cf"), "Prendas de vestir y complementos", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ropa y calzado", "Gasto" },
                    { new Guid("c73ebca7-2bf5-fd6e-e041-642b86a9aa02"), "Gastos no clasificados", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Otros gastos", "Gasto" },
                    { new Guid("e6a6ad16-fd61-835b-3c45-ac310298f556"), "Combustible, transporte público y vehículo", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transporte", "Gasto" },
                    { new Guid("ef82c217-d3f9-18c4-a956-b0a414ece3b2"), "Dispositivos, software y suscripciones digitales", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tecnología", "Gasto" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_Nombre",
                table: "categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_categorias_IdCategoria",
                table: "cuenta_categorias",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_categorias_IdCuenta_Nombre",
                table: "cuenta_categorias",
                columns: new[] { "IdCuenta", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_IdCategoria",
                table: "movimientos",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_IdCuenta",
                table: "movimientos",
                column: "IdCuenta");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_cuentas_IdCuenta",
                table: "usuario_cuentas",
                column: "IdCuenta");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_identidades_IdAuth0",
                table: "usuario_identidades",
                column: "IdAuth0",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_identidades_IdUsuario",
                table: "usuario_identidades",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_IdRol",
                table: "usuario_roles",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimientos");

            migrationBuilder.DropTable(
                name: "usuario_cuentas");

            migrationBuilder.DropTable(
                name: "usuario_identidades");

            migrationBuilder.DropTable(
                name: "usuario_roles");

            migrationBuilder.DropTable(
                name: "cuenta_categorias");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "cuentas");
        }
    }
}
