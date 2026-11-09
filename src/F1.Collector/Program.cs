using F1.Collector.Extensions;
using F1.Collector.Publishing;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddHealthChecks();
builder.Services.AddCollectorServices(builder.Configuration);
builder.Services.AddRabbitMq(builder.Configuration);

var app = builder.Build();

await app.Services.GetRequiredService<EventPublisher>().DeclareExchangeAsync();

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");

app.Run();
