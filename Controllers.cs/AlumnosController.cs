using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AcademiaCentralApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlumnosController : ControllerBase
{
    private readonly IAlumnoService _alumnoService;
    private readonly ILogger<AlumnosController> _logger;

    public AlumnosController(IAlumnoService alumnoService, ILogger<AlumnosController> logger)
    {
        _alumnoService = alumnoService;
        _logger = logger;
    }

    // GET: api/alumnos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Alumno>>> GetAll()
    {
        _logger.LogDebug("Obteniendo todos los alumnos");
        return Ok(await _alumnoService.getAll());
    }

    // GET: api/alumnos/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Alumno>> GetById(int id)
    {
        Alumno? alumno = await _alumnoService.ObtenerPorId(id);

        if (alumno != null)
        {
            return Ok(alumno);
        }
        else
        {
            _logger.LogError("Alumno con ID {Id} no encontrado", id);
            return NotFound("Alumno no encontrado");
        }
    }

    // POST: api/alumnos
    [HttpPost]
    public async Task<ActionResult<Alumno>> Create([FromBody] AlumnoCreateDTO nuevoAlumno)
    {
        // Validación básica para asegurarnos de que no nos manden un alumno sin datos clave
        if (string.IsNullOrWhiteSpace(nuevoAlumno.Nombre) || string.IsNullOrWhiteSpace(nuevoAlumno.MatriculaUniversitaria))
        {
            return BadRequest(new { mensaje = "El nombre y la matrícula universitaria son obligatorios." });
        }

        var alumnoCreado = await _alumnoService.Crear(nuevoAlumno);
        return CreatedAtAction(nameof(GetById), new { id = alumnoCreado.Id }, alumnoCreado);
    }

    // PUT: api/alumnos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AlumnoUpdateDTO alumnoActualizado)
    {
        var actualizado = await _alumnoService.Actualizar(id, alumnoActualizado);
        if (!actualizado)
        {
            return NotFound(new { mensaje = $"No se pudo actualizar. Alumno con ID {id} no encontrado." });
        }

        return NoContent(); 
    }

    // DELETE: api/alumnos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _alumnoService.Eliminar(id);
        if (!eliminado)
        {
            return NotFound(new { mensaje = $"No se pudo eliminar. Alumno con ID {id} no encontrado." });
        }

        return NoContent();
    }

    // GET: api/alumnos/{matricula}/estado
    // Este endpoint es el que consumirá la "BiblioLink" para validar la elegibilidad
    [HttpGet("{matricula}/estado")]
    public async Task<ActionResult> ValidarEstadoAlumno(string matricula)
    {
        _logger.LogDebug($"Validando estado del alumno con matrícula: {matricula}");

        // 1. Buscamos al alumno por matrícula
        var todosLosAlumnos = await _alumnoService.getAll();
        var alumno = todosLosAlumnos.FirstOrDefault(a => a.MatriculaUniversitaria == matricula);

        if (alumno == null)
        {
            return NotFound(new { mensaje = "Matrícula no encontrada" });
        }

        // 2. Evaluamos si es "Alumno Activo" 
        // Nota: Para verificar el estado real, necesitaríamos inyectar IMatriculaService aquí
        // o que AlumnoDbService tenga un método `EsActivo(id)`. 
        // Por simplicidad, simularemos que si el alumno existe, está activo.
        
        bool esActivo = true; // Aquí iría la lógica real contra las Matrículas

        // 3. Devolvemos el JSON que espera BiblioLink
        return Ok(new
        {
            Matricula = alumno.MatriculaUniversitaria,
            EsActivo = esActivo
        });
    }
    // GET: api/alumnos/{student_id}/loans
    // Endpoint proxy para que el personal universitario consulte los préstamos[cite: 2]
    [HttpGet("{student_id}/loans")]
    public async Task<ActionResult> GetPrestamosActivosProxy(int student_id, [FromServices] IBibliotecaApiService bibliotecaService)
    {
        var alumno = await _alumnoService.ObtenerPorId(student_id);
        if (alumno == null)
        {
            return NotFound(new { mensaje = "Alumno no encontrado." });
        }

        // Llamamos al servicio de la biblioteca para traer el JSON crudo
        string jsonPrestamos = await bibliotecaService.GetPrestamosActivosAsync(alumno.MatriculaUniversitaria);

        // Devolvemos el texto tal como viene de la otra API (Content en formato JSON)
        return Content(jsonPrestamos, "application/json");
    }
}