using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AcademiaCentralApi.Security;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        IConfiguration configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        string? configuredApiKey = configuration["BiblioLink:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            throw new InvalidOperationException("Falta configurar BiblioLink:ApiKey.");
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var suppliedApiKeys) ||
            suppliedApiKeys.Count != 1 ||
            string.IsNullOrEmpty(suppliedApiKeys[0]) ||
            !KeysMatch(suppliedApiKeys[0]!, configuredApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new { mensaje = "API key inválida o ausente." });
        }

        return Task.CompletedTask;
    }

    private static bool KeysMatch(string suppliedApiKey, string configuredApiKey)
    {
        byte[] suppliedBytes = Encoding.UTF8.GetBytes(suppliedApiKey);
        byte[] configuredBytes = Encoding.UTF8.GetBytes(configuredApiKey);

        return suppliedBytes.Length == configuredBytes.Length &&
            CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes);
    }
}
