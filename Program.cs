
using Microsoft.EntityFrameworkCore;
using AcademiaCentralApi.Context; 
using AcademiaCentralApi.Interfaces;
using AcademiaCentralApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar soporte para Controladores
builder.Services.AddControllers();

// 2. Configurar la conexión a la base de datos SQLite
builder.Services.AddDbContext<AcademiaCentralContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configurar Swagger para tener una interfaz gráfica donde probar la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICarreraService, CarreraDbService>();
builder.Services.AddScoped<IAlumnoService, AlumnoDbService>();
builder.Services.AddScoped<IMatriculaService, MatriculaDbService>();


builder.Services.AddHttpClient<IBibliotecaApiService, BibliotecaApiService>();

var app = builder.Build();

// 4. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    // Habilitar la interfaz visual de Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5. Redirigir las rutas hacia nuestros Controladores (Carreras, Alumnos, Matriculas)
app.MapControllers();

app.Run();