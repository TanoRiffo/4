using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Models;

namespace AcademiaCentralApi.Interfaces;

public interface IMatriculaService
{
    public Task<List<Matricula>> getAll();
    public Task<Matricula?> ObtenerPorId(int id);
    public Task<Matricula> Crear(MatriculaCreateDTO nuevaMatricula);
    public Task<bool> Actualizar(int id, MatriculaUpdateDTO matriculaActualizada);
    public Task<bool> Eliminar(int id);
}