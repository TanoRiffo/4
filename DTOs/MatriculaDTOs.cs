using System;
using AcademiaCentralApi.Models;

namespace AcademiaCentralApi.DTOs;

public class MatriculaCreateDTO
{
    public int AlumnoId { get; set; }
    public int CarreraId { get; set; }
    public string PeriodoAcademico { get; set; } = string.Empty;
}

// Lógica de negocio: No se puede cambiar el Alumno ni la Carrera de una matrícula ya generada.
// Solo se permite actualizar hasta cuándo es válida y su estado (ej. para inhabilitarla manualmente).
public class MatriculaUpdateDTO
{
    public DateTime FechaFinVigencia { get; set; }
    public EstadoMatricula Estado { get; set; }
}