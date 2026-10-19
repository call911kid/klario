using Klario.Console.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Klario.Console.Extensions;

public static class ConsoleServiceExtensions
{
    public static IServiceCollection AddKlarioConsole(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddHttpClient();
        services.AddScoped<ProtectedLocalStorage>();
        services.AddScoped<KlarioApiClient>();
        services.AddScoped<ConsoleState>();
        return services;
    }
}
