using PontoVoz.Api.Domain.Enums;

namespace PontoVoz.Api.Application.Dtos;

// Respostas dos endpoints públicos, no formato de contracts/api-v1.md (seções 3 e 4).
// Os nomes viram camelCase e os enums viram texto pela configuração de JsonContrato.
// Todos os campos são sempre serializados, inclusive os nulos; listas vazias saem como [].

/// <summary><c>Aviso</c> (seção 3.2).</summary>
public sealed record AvisoDto(string Codigo, string Mensagem);

/// <summary>Envelope <c>Pagina&lt;T&gt;</c> (seção 3.1).</summary>
public sealed record PaginaDto<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int TotalItens, int TotalPaginas);

/// <summary><c>Coordenada</c> (seção 3.3).</summary>
public sealed record CoordenadaDto(double Latitude, double Longitude);

/// <summary><c>Parada</c> (seção 3.3).</summary>
public sealed record ParadaDto(int Id, string CodigoExterno, string Nome, string? Endereco, CoordenadaDto? Coordenada, OrigemDado Origem, bool DadosSimulados);

/// <summary><c>ParadaDetalhe</c>: <c>Parada</c> mais as linhas ativas que a atendem (seções 3.3 e 4.2).</summary>
public sealed record ParadaDetalheDto(int Id, string CodigoExterno, string Nome, string? Endereco, CoordenadaDto? Coordenada, OrigemDado Origem, bool DadosSimulados, IReadOnlyList<LinhaDto> Linhas);

/// <summary><c>Linha</c> (seção 3.4).</summary>
public sealed record LinhaDto(int Id, string Codigo, string Letreiro, string TerminalPrincipal, string TerminalSecundario, SentidoLinha Sentido, OrigemDado Origem, bool DadosSimulados);

/// <summary><c>ReferenciaLinha</c>, a forma curta da linha (seção 3.4).</summary>
public sealed record ReferenciaLinhaDto(int LinhaId, string CodigoLinha, string Letreiro);

/// <summary><c>Destino</c> (seção 3.5).</summary>
public sealed record DestinoDto(int Id, string Nome, TipoDestino Tipo, OrigemDado Origem, bool DadosSimulados);

/// <summary><c>Passagem</c> (seção 4.6). Instantes no horário de São Paulo.</summary>
public sealed record PassagemDto(
    int Id,
    int LinhaId,
    string CodigoLinha,
    string Letreiro,
    DateTimeOffset ObservadaEm,
    DateTimeOffset RegistradaEm,
    int MinutosDesdeObservacao,
    bool Desatualizada,
    string? PrefixoVeiculo,
    NivelConfirmacao NivelConfirmacao,
    bool Verificada,
    bool OficialSptrans,
    string? OrigemRegistro,
    bool DadosSimulados,
    AvisoDto Aviso);

/// <summary><c>UltimasPassagensParada</c> (seção 4.6).</summary>
public sealed record UltimasPassagensParadaDto(
    int ParadaId,
    string CodigoExternoParada,
    string NomeParada,
    bool DadosSimulados,
    DateTimeOffset ConsultadoEm,
    int LimiteDesatualizacaoMinutos,
    IReadOnlyList<PassagemDto> Passagens,
    IReadOnlyList<ReferenciaLinhaDto> LinhasSemRegistro,
    IReadOnlyList<AvisoDto> Avisos);

/// <summary>
/// <c>Previsao</c> (seção 4.5): uma chegada prevista, nunca uma passagem. <c>previstaPara</c> no
/// horário de São Paulo; <c>esperaMinutos</c> arredondado para baixo e nunca negativo.
/// </summary>
public sealed record PrevisaoDto(
    int LinhaId,
    string CodigoLinha,
    string Letreiro,
    DateTimeOffset PrevistaPara,
    int EsperaMinutos,
    string? PrefixoVeiculo,
    NivelConfirmacao NivelConfirmacao);

/// <summary><c>PrevisoesParada</c> (seção 4.5).</summary>
public sealed record PrevisoesParadaDto(
    int ParadaId,
    string CodigoExternoParada,
    string NomeParada,
    StatusPrevisao Status,
    OrigemDado Origem,
    bool DadosSimulados,
    DateTimeOffset ConsultadoEm,
    IReadOnlyList<PrevisaoDto> Previsoes,
    IReadOnlyList<ReferenciaLinhaDto> LinhasSemPrevisao,
    IReadOnlyList<AvisoDto> Avisos);
