using Live.Api.Consuming;
using Live.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddLiveServices();
builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddHostedService<LiveEventConsumer>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Live.Api v1"));

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
