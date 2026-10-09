using Hangfire;
using Hangfire.SqlServer;
using Klario.BLL.Interfaces;
using Klario.BLL.Services;
using Klario.DAL.Context;
using Klario.PL.Services;
using Klario.Providers.DI;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Register DbContext
builder.Services.AddDbContext<KlarioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Klario.BLL.IAssemblyMarker).Assembly));

// Register AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(Klario.BLL.IAssemblyMarker).Assembly, typeof(Program).Assembly);

// Register Telegram Service
builder.Services.AddHttpClient<ITelegramService, TelegramService>();

// Register Alert Formatter
builder.Services.AddScoped<IPostingAlertFormatter, TelegramPostingAlertFormatter>();

// Register Posting Providers
builder.Services.AddLinkedInProvider();

// Register Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
});

builder.Services.AddScoped<HangfirePostingScheduleManager>();
builder.Services.AddScoped<IPostingScheduleManager>(sp => sp.GetRequiredService<HangfirePostingScheduleManager>());

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");
app.MapControllers();

app.Run();
