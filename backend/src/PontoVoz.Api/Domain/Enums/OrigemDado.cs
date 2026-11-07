namespace PontoVoz.Api.Domain.Enums;

/// <summary>Método pelo qual um dado entrou no sistema. Não indica se ele é fictício nem verificado.</summary>
public enum OrigemDado
{
    Simulado = 1,
    CadastroAdministrativo = 2,
    OlhoVivo = 3,
}
