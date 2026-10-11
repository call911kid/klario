using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Klario.PL.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        var configuredKey = _configuration["Authentication:ApiKey"];

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey) ||
            string.IsNullOrEmpty(configuredKey) ||
            !string.Equals(configuredKey, extractedApiKey))
        {
            throw new UnauthorizedAccessException("Invalid or missing API key.");
        }

        await _next(context);
    }
}
