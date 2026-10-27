using F1.Collector.Extensions;
using F1.Collector.OpenF1;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddHealthChecks();
builder.Services.AddSingleton<OpenF1RateLimiter>();
builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddHttpClient<OpenF1Client>(client =>
{
    client.BaseAddress = new Uri("https://api.openf1.org/v1/");
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");

app.Run();
