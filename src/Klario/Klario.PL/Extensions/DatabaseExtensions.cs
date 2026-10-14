using Klario.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace Klario.PL.Extensions;

public static class DatabaseExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var dbContext = services.GetRequiredService<KlarioDbContext>();
            logger.LogInformation("Applying pending database migrations...");

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to apply database migrations on application startup.");
            throw;
        }
    }
}
