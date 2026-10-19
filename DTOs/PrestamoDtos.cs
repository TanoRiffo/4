namespace AcademiaCentralApi.DTOs;

public class PrestamoDTO
{
    public int PrestamoId { get; set; }
    public string LibroTitulo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // Ej: "En Mora", "Pendiente"
}