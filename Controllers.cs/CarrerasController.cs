using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AcademiaCentralApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarrerasController : ControllerBase
{
    private readonly ICarreraService _carreraService;
    private readonly ILogger<CarrerasController> _logger;

    public CarrerasController(ICarreraService carreraService, ILogger<CarrerasController> logger)
    {
        _carreraService = carreraService;
        _logger = logger;
    }

    // GET: api/carreras
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Carrera>>> GetAll()
    {
        _logger.LogDebug("Obteniendo todas las carreras");
        return Ok(await _carreraService.getAll());
    }

    // GET: api/carreras/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Carrera>> GetById(int id)
    {
        Carrera? carrera = await _carreraService.ObtenerPorId(id);

        if (carrera != null)
        {
            return Ok(carrera);
        }
        else
        {
            _logger.LogError("Carrera con ID {Id} no encontrada", id);
            return NotFound("Carrera no encontrada");
        }
    }

    // POST: api/carreras
    [HttpPost]
    public async Task<ActionResult<Carrera>> Create([FromBody] CarreraCreateDTO nuevaCarrera)
    {
        if (string.IsNullOrWhiteSpace(nuevaCarrera.Nombre) || string.IsNullOrWhiteSpace(nuevaCarrera.CodigoUnico))
        {
            return BadRequest(new { mensaje = "El nombre y el código único son obligatorios." });
        }

        var carreraCreada = await _carreraService.Crear(nuevaCarrera);
        return CreatedAtAction(nameof(GetById), new { id = carreraCreada.Id }, carreraCreada);
    }

    // PUT: api/carreras/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CarreraUpdateDTO carreraActualizada)
    {
        var actualizado = await _carreraService.Actualizar(id, carreraActualizada);
        if (!actualizado)
        {
            return NotFound(new { mensaje = $"No se pudo actualizar. Carrera con ID {id} no encontrada." });
        }

        return NoContent(); 
    }

    // DELETE: api/carreras/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _carreraService.Eliminar(id);
        if (!eliminado)
        {
            return NotFound(new { mensaje = $"No se pudo eliminar. Carrera con ID {id} no encontrada." });
        }

        return NoContent();
    }
}