namespace AcademiaCentralApi.Models;

public class ConfiguracionApiBiblioteca
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UrlBase { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    protected ConfiguracionApiBiblioteca() { }

    public ConfiguracionApiBiblioteca(string nombre, string urlBase, string apiKey)
    {
        Nombre = nombre;
        UrlBase = urlBase;
        ApiKey = apiKey;
    }
}
