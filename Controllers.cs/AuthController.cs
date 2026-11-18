using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademiaCentralApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginRespuestaDTO>> Login([FromBody] LoginDTO login)
    {
        LoginRespuestaDTO? respuesta = await _authService.Login(login);
        if (respuesta == null)
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña inválidos." });
        }

        return Ok(respuesta);
    }

    [Authorize(Roles = nameof(RolUsuario.Admin))]
    [HttpPost("usuarios")]
    public async Task<ActionResult> CrearUsuario([FromBody] UsuarioCreateDTO nuevoUsuario)
    {
        try
        {
            Usuario? usuario = await _authService.CrearUsuario(nuevoUsuario);
            if (usuario == null)
            {
                return Conflict(new { mensaje = "El nombre de usuario ya está registrado." });
            }

            return StatusCode(StatusCodes.Status201Created, new
            {
                usuario.Id,
                usuario.NombreUsuario,
                Rol = usuario.Rol.ToString(),
                usuario.AlumnoId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
