namespace AcademiaCentralApi.Interfaces;

public interface IBibliotecaApiService
{
   
    // Devuelve true si el usuario tiene préstamos pendientes sin devolver o en mora.
    Task<bool> TienePrestamosPendientesAsync(string matriculaUniversitaria);

    // Método proxy para facilitar la lista de préstamos activos al personal universitario
    Task<string> GetPrestamosActivosAsync(string matriculaUniversitaria);
}