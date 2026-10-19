using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AcademiaCentralApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatriculasController : ControllerBase
{
    private readonly IMatriculaService _matriculaService;
    private readonly ILogger<MatriculasController> _logger;

    public MatriculasController(IMatriculaService matriculaService, ILogger<MatriculasController> logger)
    {
        _matriculaService = matriculaService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Matricula>>> GetAll()
    {
        return Ok(await _matriculaService.getAll());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Matricula>> GetById(int id)
    {
        var matricula = await _matriculaService.ObtenerPorId(id);
        if (matricula == null) return NotFound(new { mensaje = "Matrícula no encontrada" });
        return Ok(matricula);
    }

    [HttpPost]
    public async Task<ActionResult<Matricula>> Create([FromBody] MatriculaCreateDTO nuevaMatricula)
    {
        try
        {
            var matriculaCreada = await _matriculaService.Crear(nuevaMatricula);
            return CreatedAtAction(nameof(GetById), new { id = matriculaCreada.Id }, matriculaCreada);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Código 403 Forbidden si la API de la biblioteca reporta préstamos pendientes[cite: 2]
            _logger.LogWarning($"Matrícula denegada: {ex.Message}");
            return StatusCode(403, new { error = "Acceso Denegado", mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            // Código 400 BadRequest para otros errores (ej. Alumno no existe)
            _logger.LogError($"Error al crear matrícula: {ex.Message}");
            return BadRequest(new { error = "Petición Inválida", mensaje = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MatriculaUpdateDTO matriculaActualizada)
    {
        var actualizado = await _matriculaService.Actualizar(id, matriculaActualizada);
        if (!actualizado) return NotFound(new { mensaje = "Matrícula no encontrada." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _matriculaService.Eliminar(id);
        if (!eliminado) return NotFound(new { mensaje = "Matrícula no encontrada." });
        return NoContent();
    }
}