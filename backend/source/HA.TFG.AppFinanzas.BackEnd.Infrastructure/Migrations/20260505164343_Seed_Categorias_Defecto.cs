using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Categorias_Defecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "categorias",
                columns: new[] { "Id", "Descripcion", "FechaCreacion", "Nombre", "Slug" },
                values: new object[,]
                {
                    { 1L, "Cualquier tipo de ingreso", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ingresos", "ingresos" },
                    { 2L, "Alquiler, hipoteca y gastos del hogar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vivienda", "gastos-vivienda" },
                    { 3L, "Compras en supermercado y alimentación en casa", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Supermercado", "gastos-supermercado" },
                    { 4L, "Restaurantes, bares y comida para llevar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Restaurantes", "gastos-restaurantes" },
                    { 5L, "Combustible, transporte público y vehículo", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transporte", "gastos-transporte" },
                    { 6L, "Médico, farmacia y seguros de salud", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salud", "gastos-salud" },
                    { 7L, "Cursos, libros y formación", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Educación", "gastos-educacion" },
                    { 8L, "Cine, viajes, hobbies y deportes", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ocio y entretenimiento", "gastos-ocio" },
                    { 9L, "Prendas de vestir y complementos", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ropa y calzado", "gastos-ropa" },
                    { 10L, "Dispositivos, software y suscripciones digitales", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tecnología", "gastos-tecnologia" },
                    { 11L, "Electricidad, agua, gas e internet", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Servicios", "gastos-servicios" },
                    { 12L, "Gastos no clasificados", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Otros gastos", "gastos-otros" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: 12L);
        }
    }
}
