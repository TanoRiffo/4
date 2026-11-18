using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Models;

namespace AcademiaCentralApi.Interfaces;

public interface IAuthService
{
    Task<LoginRespuestaDTO?> Login(LoginDTO login);
    Task<Usuario?> CrearUsuario(UsuarioCreateDTO nuevoUsuario);
}
