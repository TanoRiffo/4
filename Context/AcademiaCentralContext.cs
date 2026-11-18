using Microsoft.EntityFrameworkCore;
using AcademiaCentralApi.Models;

namespace AcademiaCentralApi.Context;

public class AcademiaCentralContext : DbContext
{
    public AcademiaCentralContext(DbContextOptions<AcademiaCentralContext> options) : base(options)
    {
    }

    public DbSet<Carrera> Carreras { get; set; }
    public DbSet<Alumno> Alumnos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }
    public DbSet<ConfiguracionApiBiblioteca> ConfiguracionesApiBiblioteca { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aquí estaba el error: ahora usa MatriculaUniversitaria correctamente
        modelBuilder.Entity<Alumno>()
            .HasIndex(a => a.MatriculaUniversitaria)
            .IsUnique();

        modelBuilder.Entity<Carrera>()
            .HasIndex(c => c.CodigoUnico)
            .IsUnique();

        modelBuilder.Entity<ConfiguracionApiBiblioteca>().HasData(
            new
            {
                Id = 9001,
                Nombre = "BiblioLink",
                UrlBase = "http://localhost:5000",
                ApiKey = ""
            });

        modelBuilder.Entity<Carrera>().HasData(
            new
            {
                Id = 9001,
                Nombre = "Carrera de prueba",
                Duracion = 4,
                CodigoUnico = "TEST-9001"
            });

        modelBuilder.Entity<Alumno>().HasData(
            new
            {
                Id = 9001,
                Nombre = "Alumno",
                Apellido = "Activo",
                MatriculaUniversitaria = "TEST-ACTIVO-9001",
                Email = "alumno.activo@example.invalid",
                DNI = "TEST-9001"
            },
            new
            {
                Id = 9002,
                Nombre = "Alumno",
                Apellido = "Inactivo",
                MatriculaUniversitaria = "TEST-INACTIVO-9002",
                Email = "alumno.inactivo@example.invalid",
                DNI = "TEST-9002"
            });

        modelBuilder.Entity<Matricula>().HasData(
            new
            {
                Id = 9001,
                AlumnoId = 9001,
                CarreraId = 9001,
                PeriodoAcademico = "2026-2",
                FechaInicioVigencia = new DateTime(2026, 1, 1),
                FechaFinVigencia = new DateTime(2099, 12, 31),
                Estado = EstadoMatricula.Vigente
            },
            new
            {
                Id = 9002,
                AlumnoId = 9002,
                CarreraId = 9001,
                PeriodoAcademico = "2024-1",
                FechaInicioVigencia = new DateTime(2024, 1, 1),
                FechaFinVigencia = new DateTime(2024, 12, 31),
                Estado = EstadoMatricula.Finalizada
            });
    }
}