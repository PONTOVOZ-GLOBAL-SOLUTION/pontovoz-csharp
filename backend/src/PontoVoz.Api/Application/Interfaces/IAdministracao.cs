using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Application.Interfaces;

// Operações administrativas (contracts/api-v1.md, seção 5). As rotas públicas continuam
// usando só as interfaces de consulta; estas são as de escrita, usadas apenas pelos services
// administrativos. Ao contrário das consultas públicas, aqui paradas e linhas inativas também
// são encontradas (contrato, D10).

/// <summary>Cadastro, atualização e associação de linhas a paradas (seções 5.2, 5.3 e 5.5).</summary>
public interface IAdministracaoParadasService
{
    Task<ParadaAdministrativaDto> CadastrarAsync(CadastroParadaDto requisicao, CancellationToken cancellationToken);

    Task<ParadaAdministrativaDto> AtualizarAsync(int id, AtualizacaoParadaDto requisicao, CancellationToken cancellationToken);

    /// <summary>Idempotente: repetir a associação não cria outra.</summary>
    Task AssociarLinhaAsync(int paradaId, int linhaId, CancellationToken cancellationToken);
}

/// <summary>Cadastro de linhas (seção 5.4).</summary>
public interface IAdministracaoLinhasService
{
    Task<LinhaAdministrativaDto> CadastrarAsync(CadastroLinhaDto requisicao, CancellationToken cancellationToken);
}

/// <summary>Registro manual de passagens observadas (seção 5.6).</summary>
public interface IObservacoesPassagemService
{
    Task<PassagemRegistradaDto> RegistrarAsync(ObservacaoPassagemDto requisicao, CancellationToken cancellationToken);
}

/// <summary>Escrita de paradas. Encontra paradas ativas e inativas.</summary>
public interface ICadastroParadaRepository
{
    Task<bool> CodigoExternoExisteAsync(string codigoExterno, CancellationToken cancellationToken);

    /// <summary>A parada rastreada para alteração, com as associações, ou <c>null</c> se não existe.</summary>
    Task<Parada?> ObterParaAlteracaoAsync(int id, CancellationToken cancellationToken);

    void Adicionar(Parada parada);
}

/// <summary>Escrita de linhas. Encontra linhas ativas e inativas.</summary>
public interface ICadastroLinhaRepository
{
    Task<bool> CodigoExisteAsync(string codigo, CancellationToken cancellationToken);

    /// <summary>A linha rastreada para alteração, ou <c>null</c> se não existe.</summary>
    Task<Linha?> ObterParaAlteracaoAsync(int id, CancellationToken cancellationToken);

    void Adicionar(Linha linha);
}

/// <summary>Escrita de passagens: só o acréscimo. Uma passagem gravada não é alterada nem apagada (seção 4.6.1).</summary>
public interface ICadastroPassagemRepository
{
    void Adicionar(RegistroPassagem passagem);
}

/// <summary>
/// Grava de uma vez o que os repositórios de escrita acumularam. Uma violação de unicidade do
/// banco vira <see cref="Erros.ViolacaoUnicidadeException"/>; qualquer outra falha continua como
/// erro inesperado.
/// </summary>
public interface IUnidadeTrabalho
{
    Task SalvarAsync(CancellationToken cancellationToken);
}
