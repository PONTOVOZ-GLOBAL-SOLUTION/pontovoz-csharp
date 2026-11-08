using PontoVoz.Api.Application;
using PontoVoz.Api.Documentacao;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Providers;
using PontoVoz.Api.Middleware;
using PontoVoz.Api.Security;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Serviços (injeção de dependência nativa do ASP.NET Core).
builder.Services.AddWebApiPontoVoz(builder.Configuration);
builder.Services.AddDocumentacaoPontoVoz();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistenciaPontoVoz(builder.Configuration, builder.Environment);
builder.Services.AddProvedoresPrevisaoPontoVoz(builder.Configuration);
builder.Services.AddAplicacaoPontoVoz();
builder.Services.AddSegurancaAdministrativa();

var app = builder.Build();

// Em Development (Banco:AplicarMigracoesAoIniciar), cria ou atualiza o SQLite antes de atender.
await app.AplicarMigracoesEmDesenvolvimentoAsync();

// Erros: exceções viram ProblemDetails (TratadorExcecoes) e status sem corpo, como o 404 de
// uma rota inexistente, também recebem um ProblemDetails.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI em /openapi/v1.json e interface Scalar em /scalar.
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Atalhos para a documentação: a raiz e o endereço habitual do Swagger levam ao Scalar.
    // Não são operações da API e ficam fora do documento OpenAPI.
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
    app.MapGet("/swagger/{**resto}", () => Results.Redirect("/scalar")).ExcludeFromDescription();
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
