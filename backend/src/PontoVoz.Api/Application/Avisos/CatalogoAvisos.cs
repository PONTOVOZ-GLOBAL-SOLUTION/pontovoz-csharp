using PontoVoz.Api.Application.Dtos;

namespace PontoVoz.Api.Application.Avisos;

/// <summary>
/// Avisos usados pelos services deste bloco (contracts/api-v1.md, seção 8). O código é estável;
/// a mensagem é a mesma dos exemplos do contrato.
/// </summary>
public static class CatalogoAvisos
{
    public static readonly AvisoDto DadosSimulados =
        new("DadosSimulados", "Dados simulados para demonstração. Não representam a operação real da SPTrans.");

    public static readonly AvisoDto CadastroNaoVerificado =
        new("CadastroNaoVerificado", "Cadastro feito por uma rota administrativa, sem conferência com fonte oficial.");

    public static readonly AvisoDto ParadaSemLinhas =
        new("ParadaSemLinhas", "Nenhuma linha está associada a esta parada.");

    public static readonly AvisoDto PassagemDesatualizada =
        new("PassagemDesatualizada", "Algumas passagens foram observadas há mais de 60 minutos e podem não refletir a situação atual.");

    public static readonly AvisoDto PassagemNaoRegistrada =
        new("PassagemNaoRegistrada", "Não há passagem registrada para algumas linhas desta parada.");

    public static readonly AvisoDto ObservacaoSimulada =
        new("ObservacaoSimulada", "Observação simulada criada com os dados de demonstração. Não é um registro oficial da SPTrans.");

    public static readonly AvisoDto RegistroManualNaoVerificado =
        new("RegistroManualNaoVerificado", "Registro manual sem verificação independente. Não é um dado oficial da SPTrans.");

    /// <summary>O aviso que explica o nível de confirmação de uma passagem, pelo código que a própria entidade informa.</summary>
    public static AvisoDto DaPassagem(string codigoAviso) => codigoAviso switch
    {
        "ObservacaoSimulada" => ObservacaoSimulada,
        "RegistroManualNaoVerificado" => RegistroManualNaoVerificado,
        _ => throw new InvalidOperationException($"Aviso de passagem sem mensagem no catálogo: {codigoAviso}."),
    };
}
