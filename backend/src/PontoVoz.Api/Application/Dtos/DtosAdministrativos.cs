using System.Text.Json.Serialization;

using PontoVoz.Api.Domain.Enums;

namespace PontoVoz.Api.Application.Dtos;

// Corpos e respostas das rotas administrativas (contracts/api-v1.md, seção 5).
// Os campos de entrada são anuláveis de propósito: assim um campo omitido chega como null e o
// service devolve 400, em vez de aceitar em silêncio o valor padrão do C# (false, 0 ou o
// primeiro valor de um enum). [CampoObrigatorio] marca no documento OpenAPI o que o service exige.

/// <summary>Marca um campo de entrada que o service exige (contrato, seção 5). Usado pelo documento OpenAPI.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class CampoObrigatorioAttribute : Attribute;

/// <summary><c>Coordenada</c> no corpo de uma requisição (seção 3.3).</summary>
public sealed record CoordenadaEntradaDto(
    [property: CampoObrigatorio] double? Latitude,
    [property: CampoObrigatorio] double? Longitude);

/// <summary>Corpo de <c>POST /api/v1/admin/paradas</c> (seção 5.2).</summary>
public sealed record CadastroParadaDto(
    [property: CampoObrigatorio] string? CodigoExterno,
    [property: CampoObrigatorio] string? Nome,
    string? Endereco,
    CoordenadaEntradaDto? Coordenada,
    [property: CampoObrigatorio] bool? DadosSimulados);

/// <summary>Corpo de <c>PUT /api/v1/admin/paradas/{id}</c> (seção 5.3): substituição completa dos dados editáveis.</summary>
public sealed record AtualizacaoParadaDto(
    [property: CampoObrigatorio] string? Nome,
    string? Endereco,
    CoordenadaEntradaDto? Coordenada,
    [property: CampoObrigatorio] bool? Ativa);

/// <summary>Corpo de <c>POST /api/v1/admin/linhas</c> (seção 5.4).</summary>
public sealed record CadastroLinhaDto(
    [property: CampoObrigatorio] string? Codigo,
    [property: CampoObrigatorio] string? Letreiro,
    [property: CampoObrigatorio] string? TerminalPrincipal,
    [property: CampoObrigatorio] string? TerminalSecundario,
    [property: CampoObrigatorio] SentidoLinha? Sentido,
    [property: CampoObrigatorio] bool? DadosSimulados);

/// <summary>Corpo de <c>POST /api/v1/admin/observacoes-passagem</c> (seção 5.6).</summary>
public sealed record ObservacaoPassagemDto(
    [property: CampoObrigatorio] int? ParadaId,
    [property: CampoObrigatorio] int? LinhaId,
    [property: CampoObrigatorio, JsonConverter(typeof(DataHoraComFusoConverter))] DateTimeOffset? ObservadaEm,
    string? PrefixoVeiculo,
    [property: CampoObrigatorio] string? OrigemRegistro);

/// <summary><c>ParadaAdministrativa</c> = <c>Parada</c> mais <c>ativa</c> e <c>criadoEm</c> (seção 5.2).</summary>
public sealed record ParadaAdministrativaDto(
    int Id,
    string CodigoExterno,
    string Nome,
    string? Endereco,
    CoordenadaDto? Coordenada,
    OrigemDado Origem,
    bool DadosSimulados,
    bool Ativa,
    DateTimeOffset CriadoEm);

/// <summary><c>Linha</c> mais <c>ativa</c> e <c>criadoEm</c> (seção 5.4).</summary>
public sealed record LinhaAdministrativaDto(
    int Id,
    string Codigo,
    string Letreiro,
    string TerminalPrincipal,
    string TerminalSecundario,
    SentidoLinha Sentido,
    OrigemDado Origem,
    bool DadosSimulados,
    bool Ativa,
    DateTimeOffset CriadoEm);

/// <summary>
/// <c>Passagem</c> mais <c>paradaId</c> (seção 5.6): o recibo do registro manual. Minutos e
/// desatualização são calculados no instante do registro (seção 4.6.3).
/// </summary>
public sealed record PassagemRegistradaDto(
    int ParadaId,
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
