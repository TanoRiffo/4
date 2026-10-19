namespace AcademiaCentralApi.Models;

public class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string MatriculaUniversitaria { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;

    protected Alumno() { }

    // Constructor sin el 'id' (lo genera la BD) y con los parámetros en el orden correcto
    public Alumno(string matriculaUniversitaria, string nombre, string apellido, string dni, string email)
    {
        MatriculaUniversitaria = matriculaUniversitaria;
        Nombre = nombre;
        Apellido = apellido;
        DNI = dni;
        Email = email;
    }
}