using Microsoft.EntityFrameworkCore;
using StarnetCosmo.App.Data;
using StarnetCosmo.App.Endpoints;
using StarnetCosmo.Core.Data;
using StarnetCosmo.Core.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para escuchar en localhost:5100
builder.WebHost.UseUrls("http://localhost:5100");

// Base de datos SQLite local
var dbPath = Path.Combine(AppContext.BaseDirectory, "starnet_leads.db");
builder.Services.AddDbContext<StarnetDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Inyección de dependencias
builder.Services.AddScoped<ILeadService, LeadService>();

// Configurar serialización JSON para ignorar ciclos de referencia
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// CORS abierto para permitir llamadas desde scrapers locales, extensiones o n8n
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllLocal", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicialización de la base de datos SQLite y semillero
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StarnetDbContext>();
    await DbInitializer.InitializeAsync(db);
}

app.UseCors("AllowAllLocal");

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "STARNET COSMO API v1");
    c.RoutePrefix = "swagger";
});

// Archivos estáticos de la interfaz web/desktop
app.UseDefaultFiles();
app.UseStaticFiles();

// Mapeo de Endpoints
app.MapLeadEndpoints();

// Redirección raíz a la interfaz
app.MapFallbackToFile("index.html");

// Lanzador automático del navegador en modo App Desktop al iniciar
_ = Task.Run(async () =>
{
    await Task.Delay(1200); // Esperar a que Kestrel esté listo
    try
    {
        var url = "http://localhost:5100";
        // Intentar abrir como app de escritorio independiente usando Edge o Chrome si está disponible, o el navegador por defecto
        var psi = new ProcessStartInfo
        {
            FileName = "msedge.exe",
            Arguments = $"--app={url}",
            UseShellExecute = true
        };
        try
        {
            Process.Start(psi);
        }
        catch
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
    }
    catch
    {
        // Ignorar si no se puede abrir ventana automática
    }
});

Console.WriteLine("=================================================");
Console.WriteLine("  STARNET COSMO - Hub de Prospección & Ventas    ");
Console.WriteLine("  UI Desktop:  http://localhost:5100             ");
Console.WriteLine("  Swagger API: http://localhost:5100/swagger     ");
Console.WriteLine($"  Base de datos: {dbPath}");
Console.WriteLine("=================================================");

app.Run();
