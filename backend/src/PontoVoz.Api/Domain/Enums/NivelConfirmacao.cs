namespace PontoVoz.Api.Domain.Enums;

/// <summary>Quanto uma informação de chegada ou de passagem foi confirmada.</summary>
public enum NivelConfirmacao
{
    Previsao = 1,
    ObservacaoSimulada = 2,
    RegistroManual = 3,

    /// <summary>Reservado. Nenhum fluxo do MVP cria este nível.</summary>
    VerificadaFonteConfiavel = 4,
}
