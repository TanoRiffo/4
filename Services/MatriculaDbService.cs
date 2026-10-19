using AcademiaCentralApi.Context;
using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademiaCentralApi.Services;

public class MatriculaDbService : IMatriculaService
{
    private readonly AcademiaCentralContext _context;
    private readonly IBibliotecaApiService _bibliotecaService;

    public MatriculaDbService(AcademiaCentralContext context, IBibliotecaApiService bibliotecaService)
    {
        _context = context;
        _bibliotecaService = bibliotecaService;
    }

    public async Task<List<Matricula>> getAll()
    {
        // Usamos Include para que nos traiga los datos del alumno y la carrera, no solo el ID
        return await _context.Matriculas
            .Include(m => m.Alumno)
            .Include(m => m.Carrera)
            .ToListAsync();
    }

    public async Task<Matricula?> ObtenerPorId(int id)
    {
        return await _context.Matriculas
            .Include(m => m.Alumno)
            .Include(m => m.Carrera)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Matricula> Crear(MatriculaCreateDTO nuevaMatricula)
    {
        // 1. Verificar que el alumno y la carrera existan
        var alumno = await _context.Alumnos.FindAsync(nuevaMatricula.AlumnoId);
        if (alumno == null) throw new Exception("El alumno especificado no existe.");

        var carrera = await _context.Carreras.FindAsync(nuevaMatricula.CarreraId);
        if (carrera == null) throw new Exception("La carrera especificada no existe.");

        // 2. Verificar que no exista una matrícula ya vigente para el mismo período[cite: 2]
        bool existeVigente = await _context.Matriculas.AnyAsync(m => 
            m.AlumnoId == nuevaMatricula.AlumnoId && 
            m.PeriodoAcademico == nuevaMatricula.PeriodoAcademico && 
            m.Estado == EstadoMatricula.Vigente);
        
        if (existeVigente) throw new Exception("El alumno ya tiene una matrícula vigente para este período.");

        // 3. Paso Crítico: Consumir API de la Biblioteca[cite: 2]
        bool debeLibros = await _bibliotecaService.TienePrestamosPendientesAsync(alumno.MatriculaUniversitaria);
        
        if (debeLibros) 
        {
            // Lanzamos UnauthorizedAccessException para que el Controller sepa que debe devolver un 403
            throw new UnauthorizedAccessException("Debe devolver los libros pendientes o en mora antes de matricularse.");
        }

        // 4. Regla de Acceso exitosa: Crear la matrícula en estado "Vigente"[cite: 2]
        Matricula matricula = new Matricula(
            nuevaMatricula.AlumnoId,
            nuevaMatricula.CarreraId,
            nuevaMatricula.PeriodoAcademico,
            DateTime.Now, // Inicio de vigencia
            DateTime.Now.AddMonths(6), // Fin de vigencia (ejemplo 1 semestre)
            EstadoMatricula.Vigente
        );

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();
        return matricula;
    }

    public async Task<bool> Actualizar(int id, MatriculaUpdateDTO matriculaActualizada)
    {
        var matriculaExistente = await ObtenerPorId(id);
        if (matriculaExistente == null) return false;

        matriculaExistente.FechaFinVigencia = matriculaActualizada.FechaFinVigencia;
        matriculaExistente.Estado = matriculaActualizada.Estado;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        var matriculaExistente = await ObtenerPorId(id);
        if (matriculaExistente == null) return false;

        _context.Matriculas.Remove(matriculaExistente);
        await _context.SaveChangesAsync();
        return true;
    }
}