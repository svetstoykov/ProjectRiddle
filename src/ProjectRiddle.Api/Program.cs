using System.Globalization;
using ProjectRiddle.Api.Extensions;
using ProjectRiddle.Infrastructure.Composition;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                formatProvider: CultureInfo.InvariantCulture,
                theme: AnsiConsoleTheme.Code);
    });

builder.Services.AddProjectRiddleApi();
builder.Services.AddProjectRiddleInfrastructure(builder.Configuration);

var app = builder.Build();

await app.ApplyProjectRiddleMigrationsAsync();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapProjectRiddleSpaFallback(app.Environment);

app.Run();

/// <summary>
/// Exposes the generated host entry point to the integration-test host factory.
/// </summary>
public partial class Program;
