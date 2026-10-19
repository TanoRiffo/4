using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Models;

namespace AcademiaCentralApi.Interfaces;

public interface ICarreraService
{
    public Task<List<Carrera>> getAll();
    public Task<Carrera?> ObtenerPorId(int id);
    public Task<Carrera> Crear(CarreraCreateDTO nuevaCarrera);
    public Task<bool> Actualizar(int id, CarreraUpdateDTO carreraActualizada);
    public Task<bool> Eliminar(int id);
}