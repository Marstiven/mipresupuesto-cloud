using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiPresupuesto_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Icono = table.Column<string>(type: "text", nullable: false),
                    ColorHex = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PagosRecurrentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Concepto = table.Column<string>(type: "text", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric", nullable: false),
                    DiaDelMes = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosRecurrentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestosMensuales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    IngresoEstimado = table.Column<decimal>(type: "numeric", nullable: false),
                    MetaAhorro = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestosMensuales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transacciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    EsGastoFijo = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Activa", "ColorHex", "Icono", "Nombre", "Tipo", "UsuarioId" },
                values: new object[,]
                {
                    { 1, true, "#3498DB", "🏠", "Vivienda y Servicios", "Gasto", null },
                    { 2, true, "#E67E22", "🛒", "Alimentación y Súper", "Gasto", null },
                    { 3, true, "#9B59B6", "🚌", "Transporte", "Gasto", null },
                    { 4, true, "#E74C3C", "🩺", "Salud y Medicinas", "Gasto", null },
                    { 5, true, "#C0392B", "💳", "Préstamos y Tarjetas", "Gasto", null },
                    { 6, true, "#1ABC9C", "📱", "Suscripciones", "Gasto", null },
                    { 7, true, "#F39C12", "🎬", "Ocio y Salidas", "Gasto", null },
                    { 8, true, "#D35400", "☕", "Café y Restaurantes", "Gasto", null },
                    { 9, true, "#7F8C8D", "⚠️", "Imprevistos", "Gasto", null },
                    { 10, true, "#2ECC71", "💼", "Salario / Sueldo", "Ingreso", null },
                    { 11, true, "#27AE60", "🎁", "Bono / Prima", "Ingreso", null },
                    { 12, true, "#16A085", "⚡", "Horas Extras / Extra", "Ingreso", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosMensuales_UsuarioId_Mes_Anio",
                table: "PresupuestosMensuales",
                columns: new[] { "UsuarioId", "Mes", "Anio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_UsuarioId_Fecha",
                table: "Transacciones",
                columns: new[] { "UsuarioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "PagosRecurrentes");

            migrationBuilder.DropTable(
                name: "PresupuestosMensuales");

            migrationBuilder.DropTable(
                name: "Transacciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
