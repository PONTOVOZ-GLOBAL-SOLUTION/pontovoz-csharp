using PontoVoz.Api.Application.Avisos;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Mapeamentos;
using PontoVoz.Api.Application.Validacao;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Application.Services;

/// <summary>
/// Monta a resposta de <c>GET /api/v1/paradas/{id}/ultimas-passagens</c> (contrato, seção 4.6).
/// O instante de referência é lido uma única vez do <see cref="TimeProvider"/> e vale para o
/// filtro de registros futuros, os minutos, a desatualização e o <c>consultadoEm</c>.
/// Nenhuma passagem é apresentada como verificada: nível, aviso e verificação vêm do tipo concreto.
/// </summary>
public sealed class UltimasPassagensService(
    IParadaRepository paradas,
    ILinhaRepository linhas,
    IRegistroPassagemRepository passagens,
    TimeProvider relogio) : IUltimasPassagensService
{
    public async Task<UltimasPassagensParadaDto> ObterAsync(int paradaId, int? linhaId, CancellationToken cancellationToken)
    {
        // Ordem do contrato (seção 2): parâmetros (400), parada (404), linha (404), associação (422).
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
        IReadOnlyList<RegistroPassagem> ultimas = await passagens.ListarUltimasPorLinhaAsync(parada.Id, agora, linhaId, cancellationToken);
        return Montar(parada, linhasDaParada, linhaId, ultimas, agora);
    }

    /// <summary>
    /// Monta a resposta a partir dos dados já lidos e do instante <paramref name="agora"/>. Também é
    /// usado pelo resumo (B9), que lê o relógio uma única vez para todas as partes.
    /// </summary>
    internal static UltimasPassagensParadaDto Montar(Parada parada, IReadOnlyList<Linha> linhasDaParada, int? linhaId, IReadOnlyList<RegistroPassagem> ultimas, DateTimeOffset agora)
    {
        List<Linha> consultadas = [.. linhasDaParada.Where(linha => linhaId is null || linha.Id == linhaId)];
        List<PassagemDto> passagensDto = [.. ultimas.Select(passagem => passagem.ParaDto(agora))];
        var linhasComRegistro = ultimas.Select(passagem => passagem.LinhaId).ToHashSet();
        List<ReferenciaLinhaDto> semRegistro = [.. consultadas
            .Where(linha => !linhasComRegistro.Contains(linha.Id))
            .OrderBy(linha => linha.Codigo, StringComparer.Ordinal)
            .Select(MapeamentoDtos.ParaReferenciaDto)];

        bool dadosSimulados = parada.DadosSimulados
            || consultadas.Any(linha => linha.DadosSimulados)
            || ultimas.Any(passagem => passagem.DadosSimulados);

        return new UltimasPassagensParadaDto(
            parada.Id,
            parada.CodigoExterno,
            parada.Nome,
            dadosSimulados,
            HorarioSaoPaulo.Converter(agora),
            RegraTemporalPassagem.LimiteDesatualizacaoMinutos,
            passagensDto,
            semRegistro,
            MontarAvisos(parada, consultadas, passagensDto, semRegistro, dadosSimulados));
    }

    /// <summary>Avisos da resposta, na ordem dos exemplos do contrato.</summary>
    private static List<AvisoDto> MontarAvisos(Parada parada, List<Linha> consultadas, List<PassagemDto> passagens, List<ReferenciaLinhaDto> semRegistro, bool dadosSimulados)
    {
        var avisos = new List<AvisoDto>();
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

        if (passagens.Any(passagem => passagem.Desatualizada))
        {
            avisos.Add(CatalogoAvisos.PassagemDesatualizada);
        }

        if (semRegistro.Count > 0)
        {
            avisos.Add(CatalogoAvisos.PassagemNaoRegistrada);
        }

        return avisos;
    }
}
