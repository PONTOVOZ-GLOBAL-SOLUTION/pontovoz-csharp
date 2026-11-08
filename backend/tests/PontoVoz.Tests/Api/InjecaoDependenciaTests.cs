using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Infrastructure.Repositories;

namespace PontoVoz.Tests.Api;

/// <summary>Confirma, pela API real, que os services e repositórios estão registrados no contêiner.</summary>
public sealed class InjecaoDependenciaTests : IDisposable
{
    private readonly string _arquivo = Path.Combine(Path.GetTempPath(), $"pontovoz-di-{Guid.NewGuid():N}.db");

    [Theory]
    [InlineData(typeof(IParadaService), typeof(ParadaService))]
    [InlineData(typeof(ILinhaService), typeof(LinhaService))]
    [InlineData(typeof(IDestinoService), typeof(DestinoService))]
    [InlineData(typeof(IUltimasPassagensService), typeof(UltimasPassagensService))]
    [InlineData(typeof(IParadaRepository), typeof(ParadaRepository))]
    [InlineData(typeof(IRegistroPassagemRepository), typeof(RegistroPassagemRepository))]
    public void Interfaces_SaoResolvidasPelaInjecaoDeDependencia(Type interfaceDoServico, Type implementacao)
    {
        using WebApplicationFactory<Program> api = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .UseSetting("ConnectionStrings:PontoVoz", $"Data Source={_arquivo}")
                .UseSetting("Banco:AplicarMigracoesAoIniciar", "false"));
        using IServiceScope escopo = api.Services.CreateScope();

        object servico = escopo.ServiceProvider.GetRequiredService(interfaceDoServico);

        Assert.IsType(implementacao, servico);
        Assert.False(File.Exists(_arquivo), "Resolver os services não pode criar o banco.");
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_arquivo);
    }
}
