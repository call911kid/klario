using Klario.Console.Components;
using Klario.PL.Configuration;
using Microsoft.Extensions.Options;

namespace Klario.PL.Extensions;

public static class ConsoleExtensions
{
    public static WebApplication MapKlarioConsole(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<ConsoleOptions>>().Value;
        if (!options.Enabled)
        {
            return app;
        }

        app.MapStaticAssets();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
