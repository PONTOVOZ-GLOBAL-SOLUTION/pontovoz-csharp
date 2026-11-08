using PontoVoz.Api.Application.Avisos;
using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Application.Mapeamentos;

/// <summary>Conversão das entidades e dos resultados dos repositórios para os DTOs do contrato.</summary>
public static class MapeamentoDtos
{
    public static PaginaDto<TDto> ParaDto<TEntidade, TDto>(this Pagina<TEntidade> pagina, Func<TEntidade, TDto> converter)
    {
        ArgumentNullException.ThrowIfNull(pagina);
        return new PaginaDto<TDto>([.. pagina.Itens.Select(converter)], pagina.NumeroPagina, pagina.TamanhoPagina, pagina.TotalItens, pagina.TotalPaginas);
    }

    public static ParadaDto ParaDto(this Parada parada) =>
        new(parada.Id, parada.CodigoExterno, parada.Nome, parada.Endereco, parada.Coordenada?.ParaDto(), parada.Origem, parada.DadosSimulados);

    public static ParadaDetalheDto ParaDetalheDto(this Parada parada, IEnumerable<Linha> linhasAtivas) =>
        new(parada.Id, parada.CodigoExterno, parada.Nome, parada.Endereco, parada.Coordenada?.ParaDto(), parada.Origem, parada.DadosSimulados,
            [.. linhasAtivas.Select(ParaDto)]);

    public static LinhaDto ParaDto(this Linha linha) =>
        new(linha.Id, linha.Codigo, linha.Letreiro, linha.TerminalPrincipal, linha.TerminalSecundario, linha.Sentido, linha.Origem, linha.DadosSimulados);

    public static ReferenciaLinhaDto ParaReferenciaDto(this Linha linha) => new(linha.Id, linha.Codigo, linha.Letreiro);

    public static DestinoDto ParaDto(this Destino destino) =>
        new(destino.Id, destino.Nome, destino.Tipo, destino.Origem, destino.DadosSimulados);

    /// <summary>
    /// Passagem com minutos e desatualização calculados na <paramref name="referencia"/> pelas regras
    /// do domínio (seção 4.6.3). Nível, verificação e aviso vêm do tipo concreto da passagem.
    /// </summary>
    public static PassagemDto ParaDto(this RegistroPassagem passagem, DateTimeOffset referencia) =>
        new(
            passagem.Id,
            passagem.LinhaId,
            passagem.Linha.Codigo,
            passagem.Linha.Letreiro,
            HorarioSaoPaulo.Converter(passagem.ObservadaEmUtc),
            HorarioSaoPaulo.Converter(passagem.RegistradaEmUtc),
            passagem.MinutosDesdeObservacao(referencia),
            passagem.EstaDesatualizada(referencia),
            passagem.PrefixoVeiculo,
            passagem.NivelConfirmacao,
            passagem.Verificada,
            passagem.OficialSptrans,
            passagem.OrigemRegistro,
            passagem.DadosSimulados,
            CatalogoAvisos.DaPassagem(passagem.CodigoAviso));

    /// <summary><c>ParadaAdministrativa</c> (seção 5.2): a parada, ativa ou não, com o instante de criação.</summary>
    public static ParadaAdministrativaDto ParaAdministrativaDto(this Parada parada) =>
        new(parada.Id, parada.CodigoExterno, parada.Nome, parada.Endereco, parada.Coordenada?.ParaDto(), parada.Origem, parada.DadosSimulados,
            parada.Ativa, HorarioSaoPaulo.Converter(parada.CriadoEmUtc));

    /// <summary>Linha mais <c>ativa</c> e <c>criadoEm</c> (seção 5.4).</summary>
    public static LinhaAdministrativaDto ParaAdministrativaDto(this Linha linha) =>
        new(linha.Id, linha.Codigo, linha.Letreiro, linha.TerminalPrincipal, linha.TerminalSecundario, linha.Sentido, linha.Origem, linha.DadosSimulados,
            linha.Ativa, HorarioSaoPaulo.Converter(linha.CriadoEmUtc));

    /// <summary>Recibo do registro manual (seção 5.6): a mesma <c>Passagem</c> das consultas, mais <c>paradaId</c>.</summary>
    public static PassagemRegistradaDto ParaRegistradaDto(this RegistroPassagem passagem, DateTimeOffset referencia)
    {
        ArgumentNullException.ThrowIfNull(passagem);
        PassagemDto dto = passagem.ParaDto(referencia);
        return new PassagemRegistradaDto(
            passagem.ParadaId, dto.Id, dto.LinhaId, dto.CodigoLinha, dto.Letreiro, dto.ObservadaEm, dto.RegistradaEm,
            dto.MinutosDesdeObservacao, dto.Desatualizada, dto.PrefixoVeiculo, dto.NivelConfirmacao, dto.Verificada,
            dto.OficialSptrans, dto.OrigemRegistro, dto.DadosSimulados, dto.Aviso);
    }

    /// <summary><c>Previsao</c> (seção 4.5): horário em São Paulo e espera calculada na <paramref name="referencia"/>.</summary>
    public static PrevisaoDto ParaDto(this PrevisaoChegada previsao, DateTimeOffset referencia)
    {
        ArgumentNullException.ThrowIfNull(previsao);
        return new PrevisaoDto(
            previsao.Linha.LinhaId,
            previsao.Linha.CodigoLinha,
            previsao.Linha.Letreiro,
            HorarioSaoPaulo.Converter(previsao.PrevistaParaUtc),
            previsao.EsperaMinutos(referencia),
            previsao.PrefixoVeiculo,
            previsao.NivelConfirmacao);
    }

    private static CoordenadaDto ParaDto(this Coordenada coordenada) => new(coordenada.Latitude, coordenada.Longitude);
}
