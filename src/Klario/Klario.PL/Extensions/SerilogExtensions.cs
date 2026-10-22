using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Klario.PL.Extensions;

public static class SerilogExtensions
{
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((servicesProvider, lc) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var columnOptions = new ColumnOptions();
            columnOptions.TimeStamp.ConvertToUtc = true;
            columnOptions.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn
                {
                    ColumnName = "MachineName",
                    DataType = SqlDbType.NVarChar,
                    DataLength = 128,
                    AllowNull = true
                }
            };

            lc.ReadFrom.Configuration(configuration)
              .ReadFrom.Services(servicesProvider)
              .MinimumLevel.Information()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
              .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
              .MinimumLevel.Override("System", LogEventLevel.Warning)
              .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
              .Enrich.FromLogContext()
              .Enrich.WithProperty("MachineName", Environment.MachineName)
              .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                lc.WriteTo.Logger(sub => sub
                    .Filter.ByIncludingOnly(evt =>
                        evt.Level >= LogEventLevel.Warning ||
                        (evt.Properties.TryGetValue("SourceContext", out var sc) && sc.ToString().Contains("Klario")))
                    .WriteTo.MSSqlServer(
                        connectionString: connectionString,
                        sinkOptions: new MSSqlServerSinkOptions
                        {
                            TableName = "Logs",
                            AutoCreateSqlTable = false
                        },
                        columnOptions: columnOptions));
            }
        });

        return services;
    }
}
