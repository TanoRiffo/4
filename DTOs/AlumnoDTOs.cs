namespace AcademiaCentralApi.DTOs;

public class AlumnoCreateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string MatriculaUniversitaria { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
}

// Lógica de negocio: No permitimos cambiar la Matrícula ni el DNI una vez creado el alumno.
public class AlumnoUpdateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}