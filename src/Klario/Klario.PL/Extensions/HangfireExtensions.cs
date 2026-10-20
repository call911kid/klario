using Hangfire;
using Hangfire.SqlServer;
using Klario.BLL.Interfaces;
using Klario.PL.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Klario.PL.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
        {
            Attempts = 3,
            OnAttemptsExceeded = AttemptsExceededAction.Delete
        });

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            })
            .WithJobExpirationTimeout(TimeSpan.FromHours(72)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
        });

        services.AddScoped<HangfirePostingScheduleManager>();
        services.AddScoped<IPostingScheduleManager>(sp => sp.GetRequiredService<HangfirePostingScheduleManager>());

        return services;
    }

    public static async Task SyncHangfireSchedulesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var scheduleManager = scope.ServiceProvider.GetRequiredService<IPostingScheduleManager>();
        await scheduleManager.SyncSchedulesAsync();
    }
}

