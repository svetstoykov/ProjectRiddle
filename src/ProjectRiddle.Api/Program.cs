using ProjectRiddle.Api.Composition;
using ProjectRiddle.Infrastructure.Composition;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProjectRiddleApi();
builder.Services.AddProjectRiddleInfrastructure(builder.Configuration);

var app = builder.Build();

await app.ApplyProjectRiddleMigrationsAsync();

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
