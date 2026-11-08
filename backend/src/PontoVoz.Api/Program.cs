using PontoVoz.Api.Application;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Middleware;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Serviços (injeção de dependência nativa do ASP.NET Core).
builder.Services.AddWebApiPontoVoz(builder.Configuration);
builder.Services.AddOpenApi(opcoes => opcoes.AddDocumentTransformer((documento, _, _) =>
{
    documento.Info.Title = "PontoVoz API";
    documento.Info.Version = "v1";
    documento.Info.Description = "Consultas públicas do totem PontoVoz (CitizenTech FIAP). Lista só os endpoints já implementados; "
        + "previsões, alternativas, resumo e rotas administrativas estão planejados em contracts/api-v1.md.";
    return Task.CompletedTask;
}));
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistenciaPontoVoz(builder.Configuration, builder.Environment);
builder.Services.AddAplicacaoPontoVoz();

var app = builder.Build();

// Erros: exceções viram ProblemDetails (TratadorExcecoes) e status sem corpo, como o 404 de
// uma rota inexistente, também recebem um ProblemDetails.
app.UseExceptionHandler();
app.UseStatusCodePages();

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

app.UseCors(WebApiExtensions.PoliticaCors);
app.MapControllers();

app.Run();
