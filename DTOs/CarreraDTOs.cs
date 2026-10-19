namespace AcademiaCentralApi.DTOs;

public class CarreraCreateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public int Duracion { get; set; }
    public string CodigoUnico { get; set; } = string.Empty;
}

// Lógica de negocio: El código único de la carrera no se puede cambiar.
public class CarreraUpdateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public int Duracion { get; set; }
}