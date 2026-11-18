using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcademiaCentralApi.Context;
using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AcademiaCentralApi.Services;

public class AuthService : IAuthService
{
    private readonly AcademiaCentralContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        AcademiaCentralContext context,
        IPasswordHasher<Usuario> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginRespuestaDTO?> Login(LoginDTO login)
    {
        Usuario? usuario = await _context.Usuarios
            .SingleOrDefaultAsync(u => u.NombreUsuario == login.NombreUsuario.Trim());

        if (usuario == null)
        {
            return null;
        }

        PasswordVerificationResult resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            login.Contrasena);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, login.Contrasena);
            await _context.SaveChangesAsync();
        }

        string issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Issuer.");
        string key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Key en user-secrets.");
        int expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        List<Claim> claims =
        [
            new("id", usuario.Id.ToString()),
            new("rol", usuario.Rol.ToString())
        ];

        if (usuario.AlumnoId.HasValue)
        {
            claims.Add(new Claim("alumnoId", usuario.AlumnoId.Value.ToString()));
        }

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: issuer,
            audience: null,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginRespuestaDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    public async Task<Usuario?> CrearUsuario(UsuarioCreateDTO nuevoUsuario)
    {
        if (string.IsNullOrWhiteSpace(nuevoUsuario.NombreUsuario))
        {
            throw new ArgumentException("El nombre de usuario es obligatorio.");
        }

        string nombreUsuario = nuevoUsuario.NombreUsuario.Trim();
        if (nombreUsuario.Length > 100)
        {
            throw new ArgumentException("El nombre de usuario no puede superar 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(nuevoUsuario.Contrasena) || nuevoUsuario.Contrasena.Length < 8)
        {
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
        }

        if (!Enum.IsDefined(nuevoUsuario.Rol))
        {
            throw new ArgumentException("El rol indicado no es válido.");
        }

        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario))
        {
            return null;
        }

        if (nuevoUsuario.AlumnoId.HasValue &&
            !await _context.Alumnos.AnyAsync(a => a.Id == nuevoUsuario.AlumnoId.Value))
        {
            throw new ArgumentException("El alumno indicado no existe.");
        }

        Usuario usuario = new(
            nombreUsuario,
            string.Empty,
            nuevoUsuario.Rol,
            nuevoUsuario.AlumnoId);

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, nuevoUsuario.Contrasena);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }
}
