using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");

app.Run();
