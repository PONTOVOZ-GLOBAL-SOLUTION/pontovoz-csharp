using PontoVoz.Api.Application;
using PontoVoz.Api.Infrastructure.Persistence;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Serviços (injeção de dependência nativa do ASP.NET Core).
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistenciaPontoVoz(builder.Configuration, builder.Environment);
builder.Services.AddAplicacaoPontoVoz();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI em /openapi/v1.json e interface Scalar em /scalar.
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    // Em Development o acesso local por HTTP (porta 5080) fica liberado para a equipe,
    // já que o certificado de desenvolvimento pode não estar confiável em todas as máquinas.
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();
