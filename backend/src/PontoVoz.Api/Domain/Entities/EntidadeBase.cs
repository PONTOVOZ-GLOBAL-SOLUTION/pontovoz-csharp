using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Base das entidades persistentes com identificador próprio.
/// O <see cref="Id"/> é gerado pelo banco: enquanto vale 0, a entidade ainda não foi
/// persistida e só é igual a ela mesma.
/// </summary>
public abstract class EntidadeBase
{
    /// <summary>Usado pelo EF Core ao materializar a entidade.</summary>
    protected EntidadeBase()
    {
    }

    protected EntidadeBase(DateTimeOffset criadoEm)
    {
        CriadoEmUtc = RegrasCadastro.Utc(criadoEm, "criadoEm");
    }

    public int Id { get; private set; }

    /// <summary>Instante de criação, sempre com deslocamento zero (UTC).</summary>
    public DateTimeOffset CriadoEmUtc { get; private set; }

    public bool Persistida => Id > 0;

    /// <summary>
    /// Compara identidades: duas entidades persistidas são a mesma quando têm o mesmo tipo e o
    /// mesmo <see cref="Id"/>; uma entidade ainda não persistida só é igual à própria instância.
    /// </summary>
    public bool MesmaEntidade(EntidadeBase? outra)
    {
        if (outra is null)
        {
            return false;
        }

        if (ReferenceEquals(this, outra))
        {
            return true;
        }

        return Persistida && outra.Persistida && GetType() == outra.GetType() && Id == outra.Id;
    }
}
