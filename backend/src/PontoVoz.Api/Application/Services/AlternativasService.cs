using PontoVoz.Api.Application.Avisos;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Mapeamentos;
using PontoVoz.Api.Application.Validacao;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Application.Services;

/// <summary>
/// Monta a resposta de <c>GET /api/v1/paradas/{id}/alternativas?destinoId=</c> (contrato, seção 4.7).
/// <para>
/// Só considera linhas diretas com percurso cadastrado da parada até o destino (seed), sem
/// baldeação nem rota inventada. Para cada uma, o domínio (<see cref="AlternativaDeslocamento"/>)
/// combina o percurso com a próxima chegada válida do provedor: espera + percurso = tempo total.
/// O ranking é pelo menor tempo total, depois a menor espera e o código da linha; linhas sem
/// previsão ficam no fim, sem posição.
/// </para>
/// <para>
/// A ordem é 400 → 404 → percursos → provedor. Não há <c>Task.WhenAll</c> aqui: o provedor precisa
/// das linhas diretas, que vêm da consulta de percursos, e com <c>SemLinhaDireta</c> ele não deve ser
/// consultado. A indisponibilidade do provedor não é erro: a resposta é 200 com <c>SemComparacao</c>.
/// </para>
/// </summary>
public sealed class AlternativasService(
    IParadaRepository paradas,
    ILinhaRepository linhas,
    IDestinoRepository destinos,
    IProvedorPrevisaoChegada provedor,
    TimeProvider relogio) : IAlternativasService
{
    public async Task<AlternativasDeslocamentoDto> ObterAsync(int paradaId, int? destinoId, CancellationToken cancellationToken)
    {
        var validacao = new ValidacaoParametros();
        int? destinoValido = validacao.IdObrigatorio(destinoId, "destinoId");
        validacao.Confirmar();

        Parada parada = (paradaId >= 1 ? await paradas.ObterAtivaAsync(paradaId, cancellationToken) : null)
            ?? throw ErroAplicacaoException.ParadaNaoEncontrada(paradaId);
        Destino destino = await destinos.ObterAtivoAsync(destinoValido!.Value, cancellationToken)
            ?? throw ErroAplicacaoException.DestinoNaoEncontrado(destinoValido.Value);

        DateTimeOffset agora = relogio.GetUtcNow();
        IReadOnlyList<PercursoEstimado> percursos = await destinos.ListarPercursosDiretosAsync(parada.Id, destino.Id, cancellationToken);

        if (percursos.Count == 0)
        {
            // Sem linha direta: 200, lista vazia e o provedor não é consultado.
            bool paradaSemLinhas = (await linhas.ListarAtivasDaParadaAsync(parada.Id, cancellationToken)).Count == 0;
            return Montar(parada, destino, SituacaoAlternativas.SemLinhaDireta, resultado: null, agora, [], [], percursosSimulados: false, paradaSemLinhas);
        }

        List<Linha> linhasDiretas = [.. percursos.Select(percurso => percurso.Linha)];
        ResultadoProvedor resultado = await provedor.ConsultarAsync(parada, linhasDiretas, agora, cancellationToken);

        List<AlternativaDeslocamento> calculadas = [.. percursos.Select(percurso =>
            AlternativaDeslocamento.Calcular(percurso, resultado.ProximaChegada(percurso.LinhaId), agora))];
        SituacaoAlternativas situacao = calculadas.Exists(a => a.Situacao == SituacaoAlternativa.Comparavel)
            ? SituacaoAlternativas.Comparadas
            : SituacaoAlternativas.SemComparacao;

        return Montar(parada, destino, situacao, resultado, agora, calculadas, linhasDiretas, percursos.Any(percurso => percurso.DadosSimulados), paradaSemLinhas: false);
    }

    /// <summary>
    /// Ranking (seção 4.7): comparáveis por tempo total, espera e código, com posição 1, 2, 3...;
    /// depois as sem previsão, por percurso e código, sem posição.
    /// </summary>
    public static List<AlternativaDto> Ordenar(IEnumerable<AlternativaDeslocamento> alternativas)
    {
        List<AlternativaDeslocamento> lista = [.. alternativas];
        IEnumerable<AlternativaDeslocamento> comparaveis = lista
            .Where(a => a.Situacao == SituacaoAlternativa.Comparavel)
            .OrderBy(a => a.TempoTotalEstimadoMinutos)
            .ThenBy(a => a.EsperaMinutos)
            .ThenBy(a => a.Linha.CodigoLinha, StringComparer.Ordinal);
        IEnumerable<AlternativaDeslocamento> semPrevisao = lista
            .Where(a => a.Situacao == SituacaoAlternativa.SemPrevisao)
            .OrderBy(a => a.PercursoEstimadoMinutos)
            .ThenBy(a => a.Linha.CodigoLinha, StringComparer.Ordinal);

        return [
            .. comparaveis.Select((a, indice) => ParaDto(a, indice + 1)),
            .. semPrevisao.Select(a => ParaDto(a, posicao: null)),
        ];
    }

    private static AlternativaDto ParaDto(AlternativaDeslocamento a, int? posicao) =>
        new(
            posicao,
            a.Linha.LinhaId,
            a.Linha.CodigoLinha,
            a.Linha.Letreiro,
            a.Situacao,
            a.PrevistaParaUtc is DateTimeOffset previsto ? HorarioSaoPaulo.Converter(previsto) : null,
            a.EsperaMinutos,
            a.PercursoEstimadoMinutos,
            a.TempoTotalEstimadoMinutos,
            a.ChegadaEstimadaAoDestinoUtc is DateTimeOffset chegada ? HorarioSaoPaulo.Converter(chegada) : null,
            a.Baldeacoes,
            a.OrigemPercurso);

    private static AlternativasDeslocamentoDto Montar(
        Parada parada,
        Destino destino,
        SituacaoAlternativas situacao,
        ResultadoProvedor? resultado,
        DateTimeOffset agora,
        List<AlternativaDeslocamento> calculadas,
        List<Linha> linhasDiretas,
        bool percursosSimulados,
        bool paradaSemLinhas)
    {
        List<AlternativaDto> alternativas = Ordenar(calculadas);
        bool comparacaoCompleta = situacao == SituacaoAlternativas.Comparadas
            && alternativas.TrueForAll(a => a.Situacao == SituacaoAlternativa.Comparavel);

        // No MVP os percursos são sempre simulados; também contam a parada, o destino, as linhas e previsões simuladas.
        bool dadosSimulados = parada.DadosSimulados
            || destino.DadosSimulados
            || percursosSimulados
            || linhasDiretas.Exists(linha => linha.DadosSimulados)
            || (resultado is { Origem: OrigemDado.Simulado } && resultado.Previsoes.Count > 0);

        bool cadastroNaoVerificado = parada.Origem == OrigemDado.CadastroAdministrativo
            || linhasDiretas.Exists(linha => linha.Origem == OrigemDado.CadastroAdministrativo);

        return new AlternativasDeslocamentoDto(
            parada.Id,
            parada.CodigoExterno,
            parada.Nome,
            new DestinoConsultadoDto(destino.Id, destino.Nome, destino.Tipo),
            CriterioAlternativas.MenorTempoTotalEstimado,
            situacao,
            comparacaoCompleta,
            resultado?.Status,
            resultado?.Origem,
            dadosSimulados,
            HorarioSaoPaulo.Converter(agora),
            alternativas,
            MontarAvisos(situacao, resultado, comparacaoCompleta, calculadas.Count > 0, dadosSimulados, cadastroNaoVerificado, paradaSemLinhas));
    }

    /// <summary>
    /// Ordem dos exemplos do contrato: primeiro a causa de uma situação sem comparação ou sem linha
    /// direta; depois a natureza do dado, o cadastro, as limitações do cálculo e, por último, a
    /// comparação incompleta.
    /// </summary>
    private static List<AvisoDto> MontarAvisos(
        SituacaoAlternativas situacao,
        ResultadoProvedor? resultado,
        bool comparacaoCompleta,
        bool temPercursos,
        bool dadosSimulados,
        bool cadastroNaoVerificado,
        bool paradaSemLinhas)
    {
        var avisos = new List<AvisoDto>();
        if (situacao == SituacaoAlternativas.SemLinhaDireta)
        {
            if (paradaSemLinhas)
            {
                avisos.Add(CatalogoAvisos.ParadaSemLinhas);
            }

            avisos.Add(CatalogoAvisos.SemLinhaDireta);
        }
        else if (situacao == SituacaoAlternativas.SemComparacao)
        {
            avisos.Add(resultado is { ProvedorDisponivel: false } ? CatalogoAvisos.ProvedorIndisponivel : CatalogoAvisos.SemPrevisoes);
            avisos.Add(CatalogoAvisos.SemComparacao);
        }

        if (dadosSimulados)
        {
            avisos.Add(CatalogoAvisos.DadosSimulados);
        }

        if (cadastroNaoVerificado)
        {
            avisos.Add(CatalogoAvisos.CadastroNaoVerificado);
        }

        avisos.Add(CatalogoAvisos.LimitacaoLinhasDiretas);
        if (temPercursos)
        {
            avisos.Add(CatalogoAvisos.PercursoEstimado);
        }

        if (situacao == SituacaoAlternativas.Comparadas && !comparacaoCompleta)
        {
            avisos.Add(CatalogoAvisos.ComparacaoIncompleta);
        }

        return avisos;
    }
}
