using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

public class PersistenciaCadastrosTests
{
    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private static async Task<(int ParadaId, int LinhaId)> SalvarParadaComLinhaAsync(BancoSqliteTeste banco)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        contexto.Paradas.Add(parada);
        await contexto.SaveChangesAsync(Ct);
        return (parada.Id, linha.Id);
    }

    [Fact]
    public async Task Parada_FicticiaSemEnderecoNemCoordenada_ERecuperadaComOsMesmosDados()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        int id;
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            Parada parada = Cenario.ParadaSimulada();
            contexto.Paradas.Add(parada);
            await contexto.SaveChangesAsync(Ct);
            id = parada.Id;
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        Parada salva = await leitura.Paradas.SingleAsync(parada => parada.Id == id, Ct);

        Assert.True(id > 0);
        Assert.Equal("SIM-P001", salva.CodigoExterno);
        Assert.Equal("Parada Demonstração Centro", salva.Nome);
        Assert.Null(salva.Endereco);
        Assert.Null(salva.Coordenada);
        Assert.True(salva.Ativa);
        Assert.Equal(OrigemDado.Simulado, salva.Origem);
        Assert.True(salva.DadosSimulados);
        Assert.Equal(Cenario.Agora, salva.CriadoEmUtc);
        Assert.Equal(TimeSpan.Zero, salva.CriadoEmUtc.Offset);
    }

    [Fact]
    public async Task Parada_AdministrativaReal_ComEnderecoECoordenada_ERecuperada()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        var coordenada = new Coordenada(-23.550520, -46.633308);
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            contexto.Paradas.Add(Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", "Avenida Exemplo, 100", coordenada, false, Cenario.Agora));
            await contexto.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        Parada salva = await leitura.Paradas.SingleAsync(Ct);

        Assert.Equal(coordenada, salva.Coordenada);
        Assert.Equal("Avenida Exemplo, 100", salva.Endereco);
        Assert.Equal(OrigemDado.CadastroAdministrativo, salva.Origem);
        Assert.False(salva.DadosSimulados);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CadastroAdministrativo_PreservaANaturezaDeclarada(bool dadosSimulados)
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            contexto.Paradas.Add(Cenario.ParadaAdministrativa(dadosSimulados));
            contexto.Linhas.Add(Cenario.LinhaAdministrativa(dadosSimulados));
            await contexto.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();

        Assert.Equal(dadosSimulados, (await leitura.Paradas.SingleAsync(Ct)).DadosSimulados);
        Assert.Equal(dadosSimulados, (await leitura.Linhas.SingleAsync(Ct)).DadosSimulados);
        Assert.Equal(OrigemDado.CadastroAdministrativo, (await leitura.Linhas.SingleAsync(Ct)).Origem);
    }

    [Fact]
    public async Task Linha_EnumsSaoGravadosComoTextoERecuperados()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            contexto.Linhas.Add(Linha.CriarSimulada("SIM-L004", "SIM-104", "Terminal Demonstração Centro", "Terminal Demonstração Norte", SentidoLinha.SecundarioParaPrincipal, Cenario.Agora));
            contexto.Destinos.Add(Destino.CriarSimulado("Estação Demonstração", TipoDestino.Estacao, Cenario.Agora));
            await contexto.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        Linha linha = await leitura.Linhas.SingleAsync(Ct);
        Destino destino = await leitura.Destinos.SingleAsync(Ct);
        List<string> gravados = await leitura.Database
            .SqlQuery<string>($"SELECT Sentido || '|' || Origem AS Value FROM Linhas UNION ALL SELECT Tipo || '|' || Origem FROM Destinos")
            .ToListAsync(Ct);

        Assert.Equal(SentidoLinha.SecundarioParaPrincipal, linha.Sentido);
        Assert.Equal("SIM-104", linha.Letreiro);
        Assert.Equal("Terminal Demonstração Norte", linha.TerminalSecundario);
        Assert.Equal(TipoDestino.Estacao, destino.Tipo);
        Assert.True(destino.Ativo);
        Assert.Equal(new[] { "SecundarioParaPrincipal|Simulado", "Estacao|Simulado" }, gravados);
    }

    [Fact]
    public async Task Associacao_ERecuperadaPelaNavegacaoDaParada()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        (int paradaId, int linhaId) = await SalvarParadaComLinhaAsync(banco);

        await using PontoVozDbContext leitura = banco.NovoContexto();
        Parada parada = await leitura.Paradas
            .Include(p => p.LinhasAssociadas)
            .ThenInclude(associacao => associacao.Linha)
            .SingleAsync(p => p.Id == paradaId, Ct);

        ParadaLinha associacao = Assert.Single(parada.LinhasAssociadas);
        Assert.Equal(linhaId, associacao.LinhaId);
        Assert.Equal("SIM-L004", associacao.Linha.Codigo);
        Assert.Equal(Cenario.Agora, associacao.AssociadaEmUtc);
        Assert.True(parada.AtendidaPor(associacao.Linha));
    }

    [Fact]
    public async Task Associacao_DuplicadaFeitaSemCarregarAsExistentes_ERejeitadaPeloBanco()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        (int paradaId, int linhaId) = await SalvarParadaComLinhaAsync(banco);

        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada parada = await contexto.Paradas.SingleAsync(p => p.Id == paradaId, Ct);
        Linha linha = await contexto.Linhas.SingleAsync(l => l.Id == linhaId, Ct);
        parada.AssociarLinha(linha, Cenario.Agora);

        var erro = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Ct));
        Assert.Contains("UNIQUE constraint failed: ParadasLinhas", erro.InnerException!.Message);
    }

    [Fact]
    public async Task Parada_ComCodigoDuplicado_ERejeitadaPeloBanco()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();
        contexto.Paradas.Add(Cenario.ParadaSimulada("SIM-P001", "Primeira Parada"));
        await contexto.SaveChangesAsync(Ct);

        contexto.Paradas.Add(Cenario.ParadaSimulada("SIM-P001", "Segunda Parada"));

        var erro = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Ct));
        Assert.Contains("UNIQUE constraint failed: Paradas.CodigoExterno", erro.InnerException!.Message);
    }

    [Fact]
    public async Task Linha_ComCodigoDuplicado_ERejeitadaPeloBanco()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();
        contexto.Linhas.Add(Cenario.LinhaSimulada("SIM-L001", "SIM-101"));
        await contexto.SaveChangesAsync(Ct);

        contexto.Linhas.Add(Cenario.LinhaSimulada("SIM-L001", "SIM-999"));

        var erro = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Ct));
        Assert.Contains("UNIQUE constraint failed: Linhas.Codigo", erro.InnerException!.Message);
    }

    [Fact]
    public async Task Percurso_ERecuperadoComAChaveCompostaEOsRelacionamentos()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            (Parada parada, _, ParadaLinha associacao) = Cenario.ParadaComLinha();
            contexto.Paradas.Add(parada);
            contexto.PercursosEstimados.Add(PercursoEstimado.CriarSimulado(associacao, Cenario.DestinoSimulado(), 20));
            await contexto.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        PercursoEstimado percurso = await leitura.PercursosEstimados
            .Include(p => p.Linha)
            .Include(p => p.Destino)
            .SingleAsync(Ct);

        Assert.Equal(20, percurso.MinutosEstimados);
        Assert.Equal("SIM-L004", percurso.Linha.Codigo);
        Assert.Equal("Terminal Demonstração Norte", percurso.Destino.Nome);
        Assert.Equal(OrigemDado.Simulado, percurso.Origem);
        Assert.True(percurso.DadosSimulados);
        Assert.True(percurso.ParadaId > 0 && percurso.LinhaId > 0 && percurso.DestinoId > 0);
    }

    [Fact]
    public async Task Percurso_ForaDaFaixa_ERejeitadoPelaRestricaoDoBanco()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        (int paradaId, int linhaId) = await SalvarParadaComLinhaAsync(banco);
        await using PontoVozDbContext contexto = banco.NovoContexto();
        contexto.Destinos.Add(Cenario.DestinoSimulado());
        await contexto.SaveChangesAsync(Ct);
        int destinoId = (await contexto.Destinos.SingleAsync(Ct)).Id;

        // O domínio já recusa 241 minutos; o SQL direto prova que o banco também recusa.
        var erro = await Assert.ThrowsAsync<SqliteException>(() => contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO PercursosEstimados (ParadaId, LinhaId, DestinoId, MinutosEstimados, Origem, DadosSimulados) VALUES ({paradaId}, {linhaId}, {destinoId}, {241}, 'Simulado', 1)", Ct));

        Assert.Contains("CHECK constraint failed", erro.Message);
    }

    [Fact]
    public async Task Passagem_DeLinhaQueNaoAtendeAParada_ERejeitadaPelaChaveEstrangeiraComposta()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        (int paradaId, _) = await SalvarParadaComLinhaAsync(banco);
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Linha outraLinha = Cenario.LinhaSimulada("SIM-L003", "SIM-203");
        contexto.Linhas.Add(outraLinha);
        await contexto.SaveChangesAsync(Ct);

        // O domínio impede essa passagem; o SQL direto mostra que o banco também a impede.
        var erro = await Assert.ThrowsAsync<SqliteException>(() => contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO RegistrosPassagem (ParadaId, LinhaId, ObservadaEmUtc, RegistradaEmUtc, TipoPassagem, OrigemRegistro, DadosSimulados, CriadoEmUtc) VALUES ({paradaId}, {outraLinha.Id}, 0, 0, 'RegistroManual', 'operador-local', 1, 0)", Ct));

        Assert.Contains("FOREIGN KEY constraint failed", erro.Message);
    }

    [Fact]
    public async Task Linha_ComHistorico_NaoPodeSerExcluida_EOHistoricoContinua()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
            contexto.Paradas.Add(parada);
            contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, "operador-local", Cenario.Agora));
            await contexto.SaveChangesAsync(Ct);
        }

        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            contexto.Linhas.Remove(await contexto.Linhas.SingleAsync(Ct));
            var erro = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Ct));
            Assert.Contains("FOREIGN KEY constraint failed", erro.InnerException!.Message);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Equal(1, await leitura.Linhas.CountAsync(Ct));
        Assert.Equal(1, await leitura.ParadasLinhas.CountAsync(Ct));
        Assert.Equal(1, await leitura.RegistrosPassagem.CountAsync(Ct));
    }
}
