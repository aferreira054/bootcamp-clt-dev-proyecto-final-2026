using CleanArchitecture.Full.Api.ErrorHandling;
using CleanArchitecture.Full.Application;
using CleanArchitecture.Full.Infrastructure;
using CleanArchitecture.Full.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 0. Serilog: lee "Serilog" de configuración y agrega el sink de Seq solo si Seq:ServerUrl está seteado
// (así el proyecto corre sin problema con `dotnet run` local, sin depender de que Seq esté levantado).
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "CleanArchitecture.Full.Api");

    var seqServerUrl = context.Configuration["Seq:ServerUrl"];
    if (!string.IsNullOrWhiteSpace(seqServerUrl))
        loggerConfiguration.WriteTo.Seq(seqServerUrl);
});

// 1. Configurar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Cuentas - Clean Architecture CQRS",
        Version = "v1",
        Description = "CRUD de cuentas bancarias con patrón CQRS"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// 2.1 Manejo global de errores (ProblemDetails / RFC 7807)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 3. Application + Infrastructure (MediatR, FluentValidation, AutoMapper, DbContext)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 4. CORS (para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// 5. Configurar pipeline
// RequestId (TraceIdentifier de ASP.NET Core) empujado al LogContext de Serilog: todo lo que se
// loguee durante este request -incluyendo los behaviors de MediatR- queda correlacionado por esta
// propiedad, aun cuando el request viaja entre distintas réplicas del pod.
app.Use(async (context, next) =>
{
    using (Serilog.Context.LogContext.PushProperty("RequestId", context.TraceIdentifier))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
    };
});

app.UseExceptionHandler();

// Swagger queda disponible en todos los entornos: no hay autenticación ni datos sensibles
// en este proyecto, y facilita que se pueda inspeccionar el contrato de la API en el contenedor.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Cuentas v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 6. Migraciones + seed de datos de ejemplo
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedDataAsync(context);
}

app.Run();