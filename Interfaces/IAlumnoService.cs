using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Models;

namespace AcademiaCentralApi.Interfaces;

public interface IAlumnoService
{
    public Task<List<Alumno>> getAll();
    public Task<Alumno?> ObtenerPorId(int id);
    public Task<Alumno> Crear(AlumnoCreateDTO nuevoAlumno);
    public Task<bool> Actualizar(int id, AlumnoUpdateDTO alumnoActualizado);
    public Task<bool> Eliminar(int id);
}