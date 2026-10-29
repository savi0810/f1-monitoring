using F1.Collector.Consuming;
using F1.Collector.Extensions;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;
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
builder.Services.AddHostedService<ImportRequestedConsumer>();
builder.Services.AddHostedService<SessionCatalogPublisher>();

var app = builder.Build();

await app.Services.GetRequiredService<EventPublisher>().DeclareExchangeAsync();

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");

app.Run();
