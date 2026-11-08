using PontoVoz.Api.Application.Avisos;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Mapeamentos;
using PontoVoz.Api.Application.Validacao;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Application.Services;

/// <summary>
/// Monta a resposta de <c>GET /api/v1/paradas/{id}/previsoes</c> (contrato, seção 4.5). A ordem
/// é a da seção 2: parâmetros (400), parada (404), linha (404), associação (422) e só então o
/// provedor. O instante é lido uma única vez do <see cref="TimeProvider"/> e vale para o cálculo das
/// previsões, as esperas e o <c>consultadoEm</c>. As previsões não são gravadas e nunca entram em
/// ultimas-passagens.
/// </summary>
public sealed class PrevisoesService(
    IParadaRepository paradas,
    ILinhaRepository linhas,
    IProvedorPrevisaoChegada provedor,
    TimeProvider relogio) : IPrevisoesService
{
    public async Task<PrevisoesParadaDto> ObterAsync(int paradaId, int? linhaId, CancellationToken cancellationToken)
    {
        var validacao = new ValidacaoParametros();
        validacao.IdOpcional(linhaId, "linhaId");
        validacao.Confirmar();

        Parada parada = (paradaId >= 1 ? await paradas.ObterAtivaAsync(paradaId, cancellationToken) : null)
            ?? throw ErroAplicacaoException.ParadaNaoEncontrada(paradaId);

        if (linhaId is int id)
        {
            _ = await linhas.ObterAtivaAsync(id, cancellationToken) ?? throw ErroAplicacaoException.LinhaNaoEncontrada(id);
            if (!await linhas.AtendeParadaAsync(parada.Id, id, cancellationToken))
            {
                throw ErroAplicacaoException.LinhaNaoAtendeParada(parada.Id, id);
            }
        }

        DateTimeOffset agora = relogio.GetUtcNow();
        IReadOnlyList<Linha> linhasDaParada = await linhas.ListarAtivasDaParadaAsync(parada.Id, cancellationToken);
        List<Linha> consultadas = [.. linhasDaParada.Where(linha => linhaId is null || linha.Id == linhaId)];

        // Parada sem linhas: 200 com listas vazias, sem consultar o provedor (seção 4.5).
        if (consultadas.Count == 0)
        {
            return Montar(parada, consultadas, resultado: null, provedor.OrigemPara(parada), agora);
        }

        ResultadoProvedor resultado = await provedor.ConsultarAsync(parada, consultadas, agora, cancellationToken);

        // Neste endpoint a previsão é todo o conteúdo: indisponível vira 503 (seção 2, D2).
        if (!resultado.ProvedorDisponivel)
        {
            throw new ProvedorPrevisoesIndisponivelException();
        }

        return Montar(parada, consultadas, resultado, resultado.Origem, agora);
    }

    /// <summary>
    /// Monta <c>PrevisoesParada</c> a partir do resultado do provedor (ou <c>null</c> quando a parada
    /// não tem linhas e o provedor não foi consultado) e do instante <paramref name="agora"/>. Também é
    /// usado pelo resumo: lá um provedor indisponível aparece como
    /// <c>status: "ProvedorIndisponivel"</c>, com <c>previsoes: []</c> e todas as linhas em
    /// <c>linhasSemPrevisao</c>, em vez do 503 (seções 4.5 e 4.8).
    /// </summary>
    internal static PrevisoesParadaDto Montar(Parada parada, IReadOnlyList<Linha> linhasConsultadas, ResultadoProvedor? resultado, OrigemDado origem, DateTimeOffset agora)
    {
        List<Linha> consultadas = [.. linhasConsultadas];

        // Só as linhas consultadas; a ordem do contrato é previstaPara e codigoLinha, com o prefixo como desempate final.
        var idsConsultados = consultadas.Select(linha => linha.Id).ToHashSet();
        List<PrevisaoChegada> previsoes = [.. (resultado?.Previsoes ?? [])
            .Where(previsao => idsConsultados.Contains(previsao.Linha.LinhaId))
            .OrderBy(previsao => previsao.PrevistaParaUtc)
            .ThenBy(previsao => previsao.Linha.CodigoLinha, StringComparer.Ordinal)
            .ThenBy(previsao => previsao.PrefixoVeiculo, StringComparer.Ordinal)];

        StatusPrevisao status = resultado is { ProvedorDisponivel: false } ? StatusPrevisao.ProvedorIndisponivel
            : previsoes.Count > 0 ? StatusPrevisao.Disponivel
            : StatusPrevisao.SemPrevisoes;

        var comPrevisao = previsoes.Select(previsao => previsao.Linha.LinhaId).ToHashSet();
        List<ReferenciaLinhaDto> semPrevisao = [.. consultadas
            .Where(linha => !comPrevisao.Contains(linha.Id))
            .OrderBy(linha => linha.Codigo, StringComparer.Ordinal)
            .ThenBy(linha => linha.Id)
            .Select(MapeamentoDtos.ParaReferenciaDto)];

        bool dadosSimulados = parada.DadosSimulados
            || consultadas.Any(linha => linha.DadosSimulados)
            || (origem == OrigemDado.Simulado && previsoes.Count > 0);

        return new PrevisoesParadaDto(
            parada.Id,
            parada.CodigoExterno,
            parada.Nome,
            status,
            origem,
            dadosSimulados,
            HorarioSaoPaulo.Converter(agora),
            [.. previsoes.Select(previsao => previsao.ParaDto(agora))],
            semPrevisao,
            MontarAvisos(parada, consultadas, status, previsoes, semPrevisao, dadosSimulados));
    }

    /// <summary>
    /// Avisos na ordem dos exemplos do contrato: com o provedor indisponível, esse aviso vem primeiro
    /// (<c>resumo-provedor-indisponivel.json</c>); depois a natureza do dado, o cadastro e a situação da consulta.
    /// </summary>
    private static List<AvisoDto> MontarAvisos(Parada parada, List<Linha> consultadas, StatusPrevisao status, List<PrevisaoChegada> previsoes, List<ReferenciaLinhaDto> semPrevisao, bool dadosSimulados)
    {
        var avisos = new List<AvisoDto>();
        bool indisponivel = status == StatusPrevisao.ProvedorIndisponivel;
        if (indisponivel)
        {
            avisos.Add(CatalogoAvisos.ProvedorIndisponivel);
        }

        if (dadosSimulados)
        {
            avisos.Add(CatalogoAvisos.DadosSimulados);
        }

        if (parada.Origem == OrigemDado.CadastroAdministrativo || consultadas.Any(linha => linha.Origem == OrigemDado.CadastroAdministrativo))
        {
            avisos.Add(CatalogoAvisos.CadastroNaoVerificado);
        }

        if (consultadas.Count == 0)
        {
            avisos.Add(CatalogoAvisos.ParadaSemLinhas);
        }
        else if (indisponivel)
        {
            // A indisponibilidade já foi avisada; sem SemPrevisoes nem PrevisaoParcial.
        }
        else if (previsoes.Count == 0)
        {
            avisos.Add(CatalogoAvisos.SemPrevisoes);
        }
        else if (semPrevisao.Count > 0)
        {
            avisos.Add(CatalogoAvisos.PrevisaoParcial);
        }

        return avisos;
    }
}
