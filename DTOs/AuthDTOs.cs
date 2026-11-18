using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AcademiaCentralApi.Models;

namespace AcademiaCentralApi.DTOs;

public class LoginDTO
{
    [Required]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;
}

public class LoginRespuestaDTO
{
    public string Token { get; set; } = string.Empty;
}

public class UsuarioCreateDTO
{
    [Required]
    [StringLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Contrasena { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RolUsuario Rol { get; set; } = RolUsuario.Alumno;

    public int? AlumnoId { get; set; }
}
