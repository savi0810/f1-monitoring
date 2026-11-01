using Microsoft.EntityFrameworkCore;
using Serilog;
using Telemetry.Api.Consuming;
using Telemetry.Api.Data;
using Telemetry.Api.Extensions;
using Telemetry.Api.Publishing;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddTelemetryServices(builder.Configuration);
builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHostedService<LapEventConsumer>();
builder.Services.AddHostedService<SessionCatalogConsumer>();

var app = builder.Build();

await app.Services
    .GetRequiredService<ImportRequestPublisher>()
    .DeclareQueueAsync();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
    if (database.IsNpgsql())
        database.Migrate();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Telemetry.Api v1"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
