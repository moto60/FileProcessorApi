using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace FileProcessorApi.Middleware;

public sealed class ApiKeyMiddleware
{
    public const string HeaderName = "X-API-Key";
    public const string ConfigurationKey = "FILE_PROCESSOR_API_KEY";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<ApiKeyRequiredAttribute>() is null)
        {
            await _next(context);
            return;
        }

        var configuredApiKey = _configuration[ConfigurationKey];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            _logger.LogError(
                "The required API key configuration value {ConfigurationKey} is missing.",
                ConfigurationKey);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues suppliedApiKeys)
            || suppliedApiKeys.Count != 1
            || string.IsNullOrEmpty(suppliedApiKeys[0])
            || !KeysMatch(configuredApiKey, suppliedApiKeys[0]!))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }

    private static bool KeysMatch(string expected, string supplied)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);

        return expectedBytes.Length == suppliedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }
}
