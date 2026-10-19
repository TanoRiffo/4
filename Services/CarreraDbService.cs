using AcademiaCentralApi.Context;
using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademiaCentralApi.Services;

public class CarreraDbService : ICarreraService
{
    private readonly AcademiaCentralContext _context;

    public CarreraDbService(AcademiaCentralContext context)
    {
        _context = context;
    }

    public async Task<List<Carrera>> getAll()
    {
        return await _context.Carreras.ToListAsync();
    }

    public async Task<Carrera?> ObtenerPorId(int id)
    {
        return await _context.Carreras.FindAsync(id);
    }

    public async Task<Carrera> Crear(CarreraCreateDTO nuevaCarrera)
    {
        Carrera carrera = new Carrera(nuevaCarrera.Nombre, nuevaCarrera.Duracion, nuevaCarrera.CodigoUnico);
        
        _context.Carreras.Add(carrera);
        await _context.SaveChangesAsync();
        return carrera;
    }

    public async Task<bool> Actualizar(int id, CarreraUpdateDTO carreraActualizada)
    {
        Carrera? carreraExistente = await ObtenerPorId(id);
        if (carreraExistente == null) return false;

        carreraExistente.Nombre = carreraActualizada.Nombre;
        carreraExistente.Duracion = carreraActualizada.Duracion;
        // Según nuestra lógica DTO, no se actualiza el CodigoUnico

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        Carrera? carreraExistente = await ObtenerPorId(id);
        if (carreraExistente == null) return false;

        _context.Carreras.Remove(carreraExistente);
        await _context.SaveChangesAsync();
        return true;
    }
}