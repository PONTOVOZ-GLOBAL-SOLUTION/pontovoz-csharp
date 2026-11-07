using System.Text.RegularExpressions;

using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Api.Domain.Rules;

/// <summary>
/// Validações compartilhadas pelos cadastros (paradas, linhas, destinos e passagens),
/// com os formatos da seção 5 do contrato.
/// </summary>
internal static partial class RegrasCadastro
{
    public const string PrefixoReservadoSimulado = "SIM-";

    // \A e \z exigem que a entrada inteira corresponda ao formato. Com ^ e $, o .NET aceitaria
    // um código seguido de uma quebra de linha final.
    [GeneratedRegex(@"\A[A-Z0-9-]{3,30}\z")]
    private static partial Regex FormatoCodigo();

    [GeneratedRegex(@"\A[A-Za-z0-9-]{1,20}\z")]
    private static partial Regex FormatoPrefixoVeiculo();

    [GeneratedRegex(@"\A[a-z0-9-]{3,40}\z")]
    private static partial Regex FormatoOrigemRegistro();

    /// <summary>
    /// Código estável de parada ou linha. Dados de origem <see cref="OrigemDado.Simulado"/> usam
    /// obrigatoriamente o prefixo <c>SIM-</c>; os demais não podem usá-lo.
    /// </summary>
    public static string Codigo(string? valor, string campo, OrigemDado origem)
    {
        if (valor is null || !FormatoCodigo().IsMatch(valor))
        {
            throw new ValidacaoDominioException(campo, $"O campo {campo} deve ter de 3 a 30 caracteres entre letras maiúsculas, dígitos e hífen.");
        }

        PrefixoReservado(valor, campo, origem);
        return valor;
    }

    /// <summary>Códigos e letreiros: o prefixo depende da origem do cadastro.</summary>
    public static void PrefixoReservado(string valor, string campo, OrigemDado origem) =>
        ExigirOuProibirPrefixo(valor, campo, exigir: origem == OrigemDado.Simulado);

    private static void ExigirOuProibirPrefixo(string valor, string campo, bool exigir)
    {
        if (exigir)
        {
            if (!valor.StartsWith(PrefixoReservadoSimulado, StringComparison.Ordinal) || valor.Length == PrefixoReservadoSimulado.Length)
            {
                throw new ValidacaoDominioException(campo, $"Dados simulados usam o prefixo {PrefixoReservadoSimulado} no campo {campo}.");
            }

            return;
        }

        if (valor.StartsWith(PrefixoReservadoSimulado, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidacaoDominioException(campo, $"O prefixo {PrefixoReservadoSimulado} é reservado aos dados de demonstração.");
        }
    }

    /// <summary>Texto obrigatório, comparado sem os espaços das pontas.</summary>
    public static string Texto(string? valor, string campo, int minimo, int maximo)
    {
        string texto = valor?.Trim() ?? string.Empty;
        if (texto.Length < minimo || texto.Length > maximo)
        {
            throw new ValidacaoDominioException(campo, $"O campo {campo} deve ter entre {minimo} e {maximo} caracteres.");
        }

        return texto;
    }

    /// <summary>Texto opcional: vazio ou só com espaços vira <c>null</c>.</summary>
    public static string? TextoOpcional(string? valor, string campo, int maximo)
    {
        string? texto = string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        if (texto is not null && texto.Length > maximo)
        {
            throw new ValidacaoDominioException(campo, $"O campo {campo} deve ter no máximo {maximo} caracteres.");
        }

        return texto;
    }

    /// <summary>Formato do prefixo de veículo, que é opcional.</summary>
    public static string? PrefixoVeiculo(string? valor)
    {
        if (valor is not null && !FormatoPrefixoVeiculo().IsMatch(valor))
        {
            throw new ValidacaoDominioException("prefixoVeiculo", "O prefixo do veículo deve ter de 1 a 20 caracteres entre letras, dígitos e hífen.");
        }

        return valor;
    }

    /// <summary>
    /// Prefixo de veículo de uma passagem. Além do formato, segue a <b>natureza</b> do registro,
    /// não a origem: um registro fictício identifica o veículo com <c>SIM-</c>, e um registro com
    /// dados reais não pode usar esse prefixo. Assim, um cadastro administrativo fictício
    /// (como <c>TESTE-P003</c>) usa veículos <c>SIM-</c>, e um cadastro administrativo real não.
    /// </summary>
    public static string? PrefixoVeiculo(string? valor, bool dadosSimulados)
    {
        string? prefixo = PrefixoVeiculo(valor);
        if (prefixo is not null)
        {
            ExigirOuProibirPrefixo(prefixo, "prefixoVeiculo", exigir: dadosSimulados);
        }

        return prefixo;
    }

    /// <summary>Rótulo técnico de quem registrou. O formato impede nome, e-mail ou telefone de pessoas.</summary>
    public static string OrigemRegistro(string? valor)
    {
        if (valor is null || !FormatoOrigemRegistro().IsMatch(valor))
        {
            throw new ValidacaoDominioException("origemRegistro", "A origem do registro deve ter de 3 a 40 caracteres entre letras minúsculas, dígitos e hífen.");
        }

        return valor;
    }

    public static TEnum Definido<TEnum>(TEnum valor, string campo)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(valor))
        {
            throw new ValidacaoDominioException(campo, $"Valor inválido para {campo}.");
        }

        return valor;
    }

    /// <summary>Normaliza um instante para UTC, rejeitando o valor padrão não inicializado.</summary>
    public static DateTimeOffset Utc(DateTimeOffset instante, string campo)
    {
        if (instante == default)
        {
            throw new ValidacaoDominioException(campo, $"O campo {campo} é obrigatório.");
        }

        return instante.ToUniversalTime();
    }
}
