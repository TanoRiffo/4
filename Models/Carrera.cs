namespace AcademiaCentralApi.Models;

public class Carrera
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Duracion { get; set; } 
    public string CodigoUnico { get; set; } = string.Empty;
        
    protected Carrera() { }

    public Carrera(string nombre, int duracion, string codigoUnico)
    {
        Nombre = nombre;
        Duracion = duracion;
        CodigoUnico = codigoUnico;
    }
}
