
using Microsoft.EntityFrameworkCore;
using AcademiaCentralApi.Context; 
using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Models;
using AcademiaCentralApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar soporte para Controladores
builder.Services.AddControllers();

string jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Falta configurar Jwt:Issuer.");
string jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
string biblioLinkApiKey = builder.Configuration["BiblioLink:ApiKey"] ?? string.Empty;
string biblioLinkOutboundApiKey = builder.Configuration["BiblioLink:OutboundApiKey"] ?? string.Empty;
int jwtExpirationMinutes = builder.Configuration.GetValue<int>("Jwt:ExpirationMinutes");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key debe configurarse en user-secrets con al menos 32 bytes.");
}

if (jwtExpirationMinutes <= 0)
{
    throw new InvalidOperationException("Jwt:ExpirationMinutes debe ser mayor que cero.");
}

if (string.IsNullOrWhiteSpace(biblioLinkApiKey))
{
    throw new InvalidOperationException("BiblioLink:ApiKey debe configurarse en user-secrets.");
}

if (string.IsNullOrWhiteSpace(biblioLinkOutboundApiKey))
{
    throw new InvalidOperationException("BiblioLink:OutboundApiKey debe configurarse en user-secrets.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            RoleClaimType = "rol",
            NameClaimType = "id"
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// 2. Configurar la conexión a la base de datos SQLite
builder.Services.AddDbContext<AcademiaCentralContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configurar Swagger para tener una interfaz gráfica donde probar la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.OperationFilter<AuthorizeOperationFilter>();

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });
});


builder.Services.AddScoped<ICarreraService, CarreraDbService>();
builder.Services.AddScoped<IAlumnoService, AlumnoDbService>();
builder.Services.AddScoped<IMatriculaService, MatriculaDbService>();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddHttpClient<IBibliotecaApiService, BibliotecaApiService>();

var app = builder.Build();

await BootstrapAdminAsync(app);

// 4. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    // Habilitar la interfaz visual de Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 5. Redirigir las rutas hacia nuestros Controladores (Carreras, Alumnos, Matriculas)
app.MapControllers();

app.Run();

static async Task BootstrapAdminAsync(WebApplication app)
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    AcademiaCentralContext context = scope.ServiceProvider.GetRequiredService<AcademiaCentralContext>();
    if (await context.Usuarios.AnyAsync())
    {
        return;
    }

    string adminUsername = app.Configuration["BootstrapAdmin:NombreUsuario"] ?? string.Empty;
    string adminPassword = app.Configuration["BootstrapAdmin:Contrasena"] ?? string.Empty;

    if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "La base no tiene usuarios. Configure BootstrapAdmin:NombreUsuario y BootstrapAdmin:Contrasena en user-secrets.");
    }

    IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.CrearUsuario(new UsuarioCreateDTO
    {
        NombreUsuario = adminUsername,
        Contrasena = adminPassword,
        Rol = RolUsuario.Admin
    });
}