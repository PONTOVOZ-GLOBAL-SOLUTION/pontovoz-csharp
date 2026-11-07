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

    private static CoordenadaDto ParaDto(this Coordenada coordenada) => new(coordenada.Latitude, coordenada.Longitude);
}
