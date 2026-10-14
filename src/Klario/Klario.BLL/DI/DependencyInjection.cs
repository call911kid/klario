using Klario.BLL.Interfaces;
using Klario.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Klario.BLL.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddHttpClient<ITelegramService, TelegramService>();
        services.AddScoped<IPostingAlertFormatter, TelegramPostingAlertFormatter>();
        services.AddScoped<IPostingMatcher, PostingMatcher>();

        return services;
    }
}
