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
    }
}