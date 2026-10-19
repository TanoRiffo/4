using AcademiaCentralApi.Context;
using AcademiaCentralApi.DTOs;
using AcademiaCentralApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AcademiaCentralApi.Services;

public class BibliotecaApiService : IBibliotecaApiService
{
    private readonly HttpClient _httpClient;
    private readonly AcademiaCentralContext _context;
    private readonly ILogger<BibliotecaApiService> _logger;

    // Inyectamos HttpClient (que registraremos en Program.cs) y el Contexto para sacar la URL configurada
    public BibliotecaApiService(HttpClient httpClient, AcademiaCentralContext context, ILogger<BibliotecaApiService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> TienePrestamosPendientesAsync(string matriculaUniversitaria)
    {
        try
        {
            // 1. Obtener la configuración de la BD
            var config = await _context.ConfiguracionesApiBiblioteca.FirstOrDefaultAsync();
            if (config == null || string.IsNullOrEmpty(config.UrlBase))
            {
                _logger.LogError("No hay configuración de la API de la Biblioteca en la base de datos.");
                throw new Exception("Error interno de configuración.");
            }

            // 2. Armar la URL y hacer la petición GET
            string url = $"{config.UrlBase}/prestamos/activos/{matriculaUniversitaria}";
            
            // Simulación de envío de API Key en los headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                // Si da error de comunicación, por regla de negocio NO creamos la matrícula
                _logger.LogWarning($"La API de Biblioteca respondió con estado: {response.StatusCode}");
                return true; // Retornamos true (como si tuviera préstamos) para bloquear la matrícula por precaución
            }

            // 3. Leer y deserializar la respuesta
            string jsonResponse = await response.Content.ReadAsStringAsync();
            
            // Configuramos JsonSerializer para que no importe si las mayúsculas/minúsculas no coinciden
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var prestamos = JsonSerializer.Deserialize<List<PrestamoDTO>>(jsonResponse, options);

            // 4. Lógica de negocio: Si la lista tiene elementos, el alumno TIENE préstamos pendientes.
            return prestamos != null && prestamos.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al comunicarse con la biblioteca: {ex.Message}");
            return true; // Ante cualquier falla grave (timeout), bloqueamos asumiendo que debe algo.
        }
    }

    public async Task<string> GetPrestamosActivosAsync(string matriculaUniversitaria)
    {
        // Esta es la función Proxy para mostrar la info en crudo.
        // Hacemos casi lo mismo, pero devolvemos el JSON de respuesta.
        var config = await _context.ConfiguracionesApiBiblioteca.FirstOrDefaultAsync();
        if (config == null) return "[]";

        string url = $"{config.UrlBase}/prestamos/activos/{matriculaUniversitaria}";
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        
        return "[]"; // Si falla, devolvemos una lista vacía de préstamos.
    }
}