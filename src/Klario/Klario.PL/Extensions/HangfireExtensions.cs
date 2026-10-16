using Klario.BLL.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Klario.PL.Extensions;

public static class HangfireExtensions
{
    public static async Task InitHangfireSchedulesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var scheduleManager = scope.ServiceProvider.GetRequiredService<IPostingScheduleManager>();
        await scheduleManager.RescheduleAllAsync();
    }
}
