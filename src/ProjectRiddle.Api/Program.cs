using System.Globalization;
using ProjectRiddle.Api.Extensions;
using ProjectRiddle.Infrastructure.Extensions;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ProjectRiddle")
            .WriteTo.Console(
                formatProvider: CultureInfo.InvariantCulture,
                theme: AnsiConsoleTheme.Code);

        var seqServerUrl = context.Configuration["Seq:ServerUrl"]?.Trim();
        if (!string.IsNullOrEmpty(seqServerUrl))
        {
            loggerConfiguration.WriteTo.Seq(
                seqServerUrl,
                formatProvider: CultureInfo.InvariantCulture);
        }
    });

builder.Services
    .AddProjectRiddleInfrastructure(builder.Configuration)
    .AddProjectRiddleMvc()
    .AddProjectRiddleIdentity()
    .AddProjectRiddleAuthorization()
    .AddProjectRiddleDataProtection(builder.Configuration, builder.Environment)
    .AddProjectRiddleApplicationServices();

var app = builder.Build();

await app.ApplyProjectRiddleMigrationsAsync();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapProjectRiddleSpaFallback(app.Environment);

app.Run();

namespace ProjectRiddle.Api
{
    /// <summary>
    /// Exposes the generated host entry point to the integration-test host factory.
    /// </summary>
    public partial class Program;
}
