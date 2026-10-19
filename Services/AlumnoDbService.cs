using AcademiaCentralApi.Context;
using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademiaCentralApi.Services;

public class AlumnoDbService : IAlumnoService
{
    private readonly AcademiaCentralContext _context;

    public AlumnoDbService(AcademiaCentralContext context)
    {
        _context = context;
    }

    public async Task<List<Alumno>> getAll()
    {
        return await _context.Alumnos.ToListAsync();
    }

    public async Task<Alumno?> ObtenerPorId(int id)
    {
        return await _context.Alumnos.FindAsync(id);
    }

    public async Task<Alumno> Crear(AlumnoCreateDTO nuevoAlumno)
    {
        // Instanciamos el alumno usando el constructor con los datos requeridos[cite: 1]
        Alumno alumno = new Alumno(
            nuevoAlumno.MatriculaUniversitaria,
            nuevoAlumno.Nombre, 
            nuevoAlumno.Apellido, 
            nuevoAlumno.DNI,
            nuevoAlumno.Email
        );
        
        _context.Alumnos.Add(alumno);
        await _context.SaveChangesAsync();
        return alumno;
    }

    public async Task<bool> Actualizar(int id, AlumnoUpdateDTO alumnoActualizado)
    {
        Alumno? alumnoExistente = await ObtenerPorId(id);
        if (alumnoExistente == null) return false;

        alumnoExistente.Nombre = alumnoActualizado.Nombre;
        alumnoExistente.Apellido = alumnoActualizado.Apellido;
        alumnoExistente.Email = alumnoActualizado.Email;
        // Según nuestros DTOs, la Matrícula y el DNI no se actualizan por seguridad

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        Alumno? alumnoExistente = await ObtenerPorId(id);
        if (alumnoExistente == null) return false;

        _context.Alumnos.Remove(alumnoExistente);
        await _context.SaveChangesAsync();
        return true;
    }
}