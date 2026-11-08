using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Repositories;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>
/// Gravação administrativa no SQLite real, com duas requisições simuladas por dois
/// <see cref="PontoVozDbContext"/> sobre o mesmo banco. A "corrida" é provocada de forma
/// determinística: a segunda requisição lê o banco antes de a primeira gravar. Assim se confere
/// que o índice único e a chave composta seguram a duplicata, que só a violação de unicidade vira
/// <see cref="ViolacaoUnicidadeException"/> (e depois 409 ou sucesso idempotente) e que outras
/// falhas, como a de chave estrangeira, continuam sendo erro inesperado.
/// </summary>
public sealed class UnidadeTrabalhoEfTests
{
    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private static readonly CadastroParadaDto ParadaTeste = new("TESTE-P800", "Parada da Corrida", null, null, true);

    private static AdministracaoParadasService Service(PontoVozDbContext contexto, ICadastroParadaRepository? paradas = null) =>
        new(paradas ?? new ParadaRepository(contexto), new LinhaRepository(contexto), new UnidadeTrabalhoEf(contexto), TimeProvider.System);

    [Fact]
    public async Task DoisCadastrosComOMesmoCodigo_OIndiceUnicoRecusaOSegundo()
    {
        await using BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext primeira = banco.NovoContexto();
        await using PontoVozDbContext segunda = banco.NovoContexto();

        // As duas requisições conferem o código antes de qualquer gravação: nenhuma o encontra.
        Assert.False(await new ParadaRepository(primeira).CodigoExternoExisteAsync("TESTE-P800", Ct));
        Assert.False(await new ParadaRepository(segunda).CodigoExternoExisteAsync("TESTE-P800", Ct));

        new ParadaRepository(primeira).Adicionar(Parada.CadastrarAdministrativa("TESTE-P800", "Primeira", null, null, true, Cenario.Agora));
        await new UnidadeTrabalhoEf(primeira).SalvarAsync(Ct);
        new ParadaRepository(segunda).Adicionar(Parada.CadastrarAdministrativa("TESTE-P800", "Segunda", null, null, true, Cenario.Agora));

        var erro = await Assert.ThrowsAsync<ViolacaoUnicidadeException>(() => new UnidadeTrabalhoEf(segunda).SalvarAsync(Ct));

        var original = Assert.IsType<DbUpdateException>(erro.InnerException);
        Assert.Equal(2067, Assert.IsType<SqliteException>(original.InnerException).SqliteExtendedErrorCode);
        Assert.DoesNotContain("TESTE-P800", erro.Message, StringComparison.Ordinal);
        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Equal("Primeira", (await leitura.Paradas.SingleAsync(p => p.CodigoExterno == "TESTE-P800", Ct)).Nome);
    }

    [Fact]
    public async Task CadastroDeParada_QuandoOutraRequisicaoGravaNoMeioDoCaminho_Responde409()
    {
        await using BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext outraRequisicao = banco.NovoContexto();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        // A verificação prévia desta requisição não acha o código; logo depois, a outra grava o mesmo código.
        var paradas = new ParadaRepositoryComCorrida(new ParadaRepository(contexto), async () =>
            await Service(outraRequisicao).CadastrarAsync(ParadaTeste with { Nome = "Gravada pela outra requisição" }, Ct));

        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service(contexto, paradas).CadastrarAsync(ParadaTeste, Ct));

        Assert.Equal((TipoErroAplicacao.Conflito, "CodigoJaCadastrado"), (erro.Tipo, erro.Codigo));
        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Equal(1, await leitura.Paradas.CountAsync(p => p.CodigoExterno == "TESTE-P800", Ct));
    }

    [Fact]
    public async Task AssociacaoGravadaPorOutraRequisicaoNoMeioDoCaminho_ContinuaIdempotenteSemDuplicar()
    {
        await using BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync();
        int paradaId, linhaId;
        await using (PontoVozDbContext preparo = banco.NovoContexto())
        {
            Parada parada = Parada.CadastrarAdministrativa("TESTE-P801", "Parada da Associação", null, null, true, Cenario.Agora);
            Linha linha = Linha.CadastrarAdministrativa("TESTE-L801", "TESTE-801", "Terminal A", "Terminal B", PontoVoz.Api.Domain.Enums.SentidoLinha.PrincipalParaSecundario, true, Cenario.Agora);
            preparo.AddRange(parada, linha);
            await preparo.SaveChangesAsync(Ct);
            (paradaId, linhaId) = (parada.Id, linha.Id);
        }

        await using PontoVozDbContext outraRequisicao = banco.NovoContexto();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        // Esta requisição carrega a parada ainda sem a associação; antes de gravar, a outra associa e grava.
        var paradas = new ParadaRepositoryComCorrida(new ParadaRepository(contexto), () =>
            Service(outraRequisicao).AssociarLinhaAsync(paradaId, linhaId, Ct));

        await Service(contexto, paradas).AssociarLinhaAsync(paradaId, linhaId, Ct);

        Assert.True(paradas.CorridaExecutada);
        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Equal(1, await leitura.ParadasLinhas.CountAsync(a => a.ParadaId == paradaId && a.LinhaId == linhaId, Ct));
    }

    [Fact]
    public async Task AssociacaoDuplicada_ViolaAChaveCompostaEViraViolacaoDeUnicidade()
    {
        await using BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        await using PontoVozDbContext primeira = banco.NovoContexto();
        await using PontoVozDbContext segunda = banco.NovoContexto();
        int paradaId = await primeira.Paradas.Where(p => p.CodigoExterno == "SIM-P002").Select(p => p.Id).SingleAsync(Ct);
        int linhaId = await primeira.Linhas.Where(l => l.Codigo == "SIM-L001").Select(l => l.Id).SingleAsync(Ct);

        // As duas carregam a parada sem a associação.
        Parada paradaA = (await new ParadaRepository(primeira).ObterParaAlteracaoAsync(paradaId, Ct))!;
        Parada paradaB = (await new ParadaRepository(segunda).ObterParaAlteracaoAsync(paradaId, Ct))!;
        paradaA.AssociarLinha((await new LinhaRepository(primeira).ObterParaAlteracaoAsync(linhaId, Ct))!, Cenario.Agora);
        paradaB.AssociarLinha((await new LinhaRepository(segunda).ObterParaAlteracaoAsync(linhaId, Ct))!, Cenario.Agora);
        await new UnidadeTrabalhoEf(primeira).SalvarAsync(Ct);

        var erro = await Assert.ThrowsAsync<ViolacaoUnicidadeException>(() => new UnidadeTrabalhoEf(segunda).SalvarAsync(Ct));

        var original = Assert.IsType<DbUpdateException>(erro.InnerException);
        Assert.Equal(1555, Assert.IsType<SqliteException>(original.InnerException).SqliteExtendedErrorCode);
    }

    [Fact]
    public async Task FalhaDeChaveEstrangeira_NaoViraViolacaoDeUnicidade()
    {
        await using BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada parada = Parada.CadastrarAdministrativa("TESTE-P802", "Parada da Chave Estrangeira", null, null, true, Cenario.Agora);
        Linha linha = Linha.CadastrarAdministrativa("TESTE-L802", "TESTE-802", "Terminal A", "Terminal B", PontoVoz.Api.Domain.Enums.SentidoLinha.PrincipalParaSecundario, true, Cenario.Agora);
        contexto.AddRange(parada, linha);
        await contexto.SaveChangesAsync(Ct);

        // Outra requisição apaga a linha direto no banco; esta ainda a tem em memória e tenta associá-la.
        await using (PontoVozDbContext outra = banco.NovoContexto())
        {
            await outra.Database.ExecuteSqlAsync($"DELETE FROM Linhas WHERE Id = {linha.Id}", Ct);
        }

        parada.AssociarLinha(linha, Cenario.Agora);

        var erro = await Assert.ThrowsAsync<DbUpdateException>(() => new UnidadeTrabalhoEf(contexto).SalvarAsync(Ct));

        Assert.Equal(787, Assert.IsType<SqliteException>(erro.InnerException).SqliteExtendedErrorCode);
        Assert.False(UnidadeTrabalhoEf.ViolouUnicidade(erro));
    }

    [Theory]
    [InlineData(19, 2067, true)]
    [InlineData(19, 1555, true)]
    [InlineData(19, 787, false)]
    [InlineData(19, 1299, false)]
    [InlineData(19, 19, false)]
    [InlineData(5, 5, false)]
    public void ClassificacaoDosErrosDoSqlite_SoUnicaEPrimariaContamComoUnicidade(int codigo, int estendido, bool esperado)
    {
        var erro = new DbUpdateException("Falha ao gravar.", new SqliteException("erro do SQLite", codigo, estendido));

        Assert.Equal(esperado, UnidadeTrabalhoEf.ViolouUnicidade(erro));
    }

    [Fact]
    public void ErroDeGravacaoSemSqliteException_NaoContaComoUnicidade() =>
        Assert.False(UnidadeTrabalhoEf.ViolouUnicidade(new DbUpdateException("Falha ao gravar.", new InvalidOperationException())));

    /// <summary>
    /// Repositório real com um gancho: depois da primeira leitura feita pelo service (verificação
    /// do código ou carga da parada), executa a "outra requisição" uma única vez.
    /// </summary>
    private sealed class ParadaRepositoryComCorrida(ICadastroParadaRepository real, Func<Task> outraRequisicao) : ICadastroParadaRepository
    {
        public bool CorridaExecutada { get; private set; }

        public async Task<bool> CodigoExternoExisteAsync(string codigoExterno, CancellationToken cancellationToken)
        {
            bool existe = await real.CodigoExternoExisteAsync(codigoExterno, cancellationToken);
            await CorrerAsync();
            return existe;
        }

        public async Task<Parada?> ObterParaAlteracaoAsync(int id, CancellationToken cancellationToken)
        {
            Parada? parada = await real.ObterParaAlteracaoAsync(id, cancellationToken);
            await CorrerAsync();
            return parada;
        }

        public void Adicionar(Parada parada) => real.Adicionar(parada);

        private async Task CorrerAsync()
        {
            if (!CorridaExecutada)
            {
                CorridaExecutada = true;
                await outraRequisicao();
            }
        }
    }
}
