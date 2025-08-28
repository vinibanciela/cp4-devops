// File: Program.cs

using KAOW.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using KAOW.Services;
using KAOW.DTOs;
using System.Reflection; // Necessário para usar Assembly.GetExecutingAssembly()

var builder = WebApplication.CreateBuilder(args);

// Configuração da conexão com o banco SQL Server Azure
builder.Services.AddDbContext<CrisisDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona suporte a controllers e API REST
builder.Services.AddControllers();

// Injeção de dependência para os serviços (camada de lógica de negócio)
builder.Services.AddScoped<InstituicaoService>();
builder.Services.AddScoped<EventoExtremoService>();
builder.Services.AddScoped<BaseEmergenciaService>();
builder.Services.AddScoped<EventoInstituicaoService>();

// Adiciona serviços para documentação da API (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KAOW API",
        Version = "v1",
        Description = "API para gerenciamento de Instituições, Eventos Extremos e Bases de Emergência"
    });
});

var app = builder.Build();

// ----- ORDEM DOS MIDDLEWARES -----
// A ordem é crucial. Use Swagger primeiro.

// Middleware do Swagger para gerar e servir a documentação
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // A URL do arquivo JSON do Swagger. Essa URL é fixa e confiável.
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KAOW API v1");

    // Serve a interface do Swagger na raiz do app: http://localhost:{porta}/
    // Isso é opcional, mas você já tinha no seu código.
    c.RoutePrefix = string.Empty;
});

// Mapeia os controllers e endpoints da API
app.MapControllers();

// ----- NOVO: Adiciona um endpoint de health check dedicado -----
// Este é um método mais robusto e confiável para verificar a saúde da aplicação.
app.MapGet("/health", () => Results.Ok("API is healthy."));

// Aplicar migrations automaticamente ao iniciar o app
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CrisisDbContext>();
    // Aplica todas as migrations pendentes
    context.Database.Migrate();
}

// Inicia a aplicação
app.Run();
