namespace AcademiaCentralApi.Models;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public int? AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    protected Usuario() { }

    public Usuario(string nombreUsuario, string passwordHash, RolUsuario rol, int? alumnoId = null)
    {
        NombreUsuario = nombreUsuario;
        PasswordHash = passwordHash;
        Rol = rol;
        AlumnoId = alumnoId;
    }
}
