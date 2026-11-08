using PontoVoz.Api.Application.Avisos;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Mapeamentos;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Application.Services;

/// <summary>
/// Monta <c>GET /api/v1/paradas/{id}/resumo</c> (contrato, seção 4.8): parada, previsões e últimas
/// passagens numa só resposta, com o mesmo formato das rotas individuais.
/// <para>
/// <b>Instante único:</b> o <see cref="TimeProvider"/> é lido uma vez, e esse instante vale para o
/// <c>consultadoEm</c> do resumo e das partes, as esperas, os minutos desde a observação e a
/// desatualização. As partes são montadas pelos mesmos métodos dos services individuais
/// (<see cref="PrevisoesService.Montar"/> e <see cref="UltimasPassagensService.Montar"/>), sem copiar regras.
/// </para>
/// <para>
/// <b>Paralelismo seguro:</b> a parada e as linhas são lidas primeiro (404 e linhas a consultar).
/// Depois, a consulta ao provedor e a das últimas passagens rodam juntas com <c>Task.WhenAll</c>: são
/// independentes, e a das passagens é a única operação no <c>DbContext</c> nesse momento (o provedor não
/// usa o banco). Nenhuma operação do EF Core roda em paralelo com outra, e o provedor é consultado uma vez.
/// </para>
/// Com o provedor indisponível, o resumo continua 200 (<c>Parcial</c>, <c>partesIndisponiveis: ["previsoes"]</c>).
/// Uma falha do banco não é tratada: continua sendo erro interno (500).
/// </summary>
public sealed class ResumoParadaService(
    IParadaRepository paradas,
    ILinhaRepository linhas,
    IRegistroPassagemRepository passagens,
    IProvedorPrevisaoChegada provedor,
    TimeProvider relogio) : IResumoParadaService
{
    public const string PartePrevisoes = "previsoes";

    public async Task<ResumoParadaDto> ObterAsync(int paradaId, CancellationToken cancellationToken)
    {
        Parada parada = (paradaId >= 1 ? await paradas.ObterAtivaAsync(paradaId, cancellationToken) : null)
            ?? throw ErroAplicacaoException.ParadaNaoEncontrada(paradaId);

        DateTimeOffset agora = relogio.GetUtcNow();
        IReadOnlyList<Linha> linhasDaParada = await linhas.ListarAtivasDaParadaAsync(parada.Id, cancellationToken);

        // Parada sem linhas: o provedor não é consultado.
        Task<ResultadoProvedor?> consultaProvedor = linhasDaParada.Count == 0
            ? Task.FromResult<ResultadoProvedor?>(null)
            : ConsultarProvedorAsync(parada, linhasDaParada, agora, cancellationToken);
        Task<IReadOnlyList<RegistroPassagem>> consultaPassagens = passagens.ListarUltimasPorLinhaAsync(parada.Id, agora, linhaId: null, cancellationToken);
        await Task.WhenAll(consultaProvedor, consultaPassagens);

        ResultadoProvedor? resultado = await consultaProvedor;
        OrigemDado origemPrevisoes = resultado?.Origem ?? provedor.OrigemPara(parada);

        ParadaDetalheDto detalhe = parada.ParaDetalheDto(linhasDaParada);
        PrevisoesParadaDto previsoes = PrevisoesService.Montar(parada, linhasDaParada, resultado, origemPrevisoes, agora);
        UltimasPassagensParadaDto ultimas = UltimasPassagensService.Montar(parada, linhasDaParada, linhaId: null, await consultaPassagens, agora);

        bool parcial = resultado is { ProvedorDisponivel: false };
        return new ResumoParadaDto(
            HorarioSaoPaulo.Converter(agora),
            detalhe.DadosSimulados || previsoes.DadosSimulados || ultimas.DadosSimulados || detalhe.Linhas.Any(linha => linha.DadosSimulados),
            parcial ? SituacaoResumo.Parcial : SituacaoResumo.Completo,
            parcial ? [PartePrevisoes] : [],
            detalhe,
            previsoes,
            ultimas,
            parcial ? [CatalogoAvisos.ResumoParcial] : []);
    }

    private async Task<ResultadoProvedor?> ConsultarProvedorAsync(Parada parada, IReadOnlyList<Linha> linhasDaParada, DateTimeOffset agora, CancellationToken cancellationToken) =>
        await provedor.ConsultarAsync(parada, linhasDaParada, agora, cancellationToken);
}
