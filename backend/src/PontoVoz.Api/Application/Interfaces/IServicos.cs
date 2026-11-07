using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;

namespace PontoVoz.Api.Application.Interfaces;

// Services de consulta usados pelos controllers públicos do B6. Todos validam os parâmetros,
// lançam ErroAplicacaoException com o código do contrato e devolvem DTOs prontos para serializar.

/// <summary><c>GET /api/v1/paradas</c> e <c>GET /api/v1/paradas/{id}</c> (contrato, seções 4.1 e 4.2).</summary>
public interface IParadaService
{
    Task<PaginaDto<ParadaDto>> BuscarAsync(ConsultaParadas consulta, CancellationToken cancellationToken);

    /// <summary>Detalhe da parada ativa com as linhas ativas que a atendem; 404 se inexistente ou inativa.</summary>
    Task<ParadaDetalheDto> ObterDetalheAsync(int id, CancellationToken cancellationToken);
}

/// <summary><c>GET /api/v1/linhas</c> (seção 4.3).</summary>
public interface ILinhaService
{
    Task<PaginaDto<LinhaDto>> BuscarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken);
}

/// <summary><c>GET /api/v1/destinos</c> (seção 4.4).</summary>
public interface IDestinoService
{
    Task<PaginaDto<DestinoDto>> BuscarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken);
}

/// <summary><c>GET /api/v1/paradas/{id}/ultimas-passagens</c> (seção 4.6).</summary>
public interface IUltimasPassagensService
{
    Task<UltimasPassagensParadaDto> ObterAsync(int paradaId, int? linhaId, CancellationToken cancellationToken);
}
