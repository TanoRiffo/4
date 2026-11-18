using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaCentralApi.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarUrlBaseBiblioteca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ConfiguracionesApiBiblioteca",
                keyColumn: "Id",
                keyValue: 9001,
                column: "UrlBase",
                value: "http://localhost:5245/api");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ConfiguracionesApiBiblioteca",
                keyColumn: "Id",
                keyValue: 9001,
                column: "UrlBase",
                value: "http://localhost:5000");
        }
    }
}
