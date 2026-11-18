using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AcademiaCentralApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedDatosPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Alumnos",
                columns: new[] { "Id", "Apellido", "DNI", "Email", "MatriculaUniversitaria", "Nombre" },
                values: new object[,]
                {
                    { 9001, "Activo", "TEST-9001", "alumno.activo@example.invalid", "TEST-ACTIVO-9001", "Alumno" },
                    { 9002, "Inactivo", "TEST-9002", "alumno.inactivo@example.invalid", "TEST-INACTIVO-9002", "Alumno" }
                });

            migrationBuilder.InsertData(
                table: "Carreras",
                columns: new[] { "Id", "CodigoUnico", "Duracion", "Nombre" },
                values: new object[] { 9001, "TEST-9001", 4, "Carrera de prueba" });

            migrationBuilder.InsertData(
                table: "ConfiguracionesApiBiblioteca",
                columns: new[] { "Id", "ApiKey", "Nombre", "UrlBase" },
                values: new object[] { 9001, "", "BiblioLink", "http://localhost:5000" });

            migrationBuilder.InsertData(
                table: "Matriculas",
                columns: new[] { "Id", "AlumnoId", "CarreraId", "Estado", "FechaFinVigencia", "FechaInicioVigencia", "PeriodoAcademico" },
                values: new object[,]
                {
                    { 9001, 9001, 9001, 0, new DateTime(2099, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "2026-2" },
                    { 9002, 9002, 9001, 1, new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "2024-1" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfiguracionesApiBiblioteca",
                keyColumn: "Id",
                keyValue: 9001);

            migrationBuilder.DeleteData(
                table: "Matriculas",
                keyColumn: "Id",
                keyValue: 9001);

            migrationBuilder.DeleteData(
                table: "Matriculas",
                keyColumn: "Id",
                keyValue: 9002);

            migrationBuilder.DeleteData(
                table: "Alumnos",
                keyColumn: "Id",
                keyValue: 9001);

            migrationBuilder.DeleteData(
                table: "Alumnos",
                keyColumn: "Id",
                keyValue: 9002);

            migrationBuilder.DeleteData(
                table: "Carreras",
                keyColumn: "Id",
                keyValue: 9001);
        }
    }
}
