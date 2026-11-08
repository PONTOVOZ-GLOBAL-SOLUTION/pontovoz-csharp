using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Security;

namespace PontoVoz.Api.Controllers.Administracao;

/// <summary>
/// Base das rotas administrativas (contracts/api-v1.md, seção 5): prefixo <c>/api/v1/admin</c>,
/// filtro de chave em todas as ações e os erros comuns 401 e 403. Usadas pelo operador da
/// demonstração, não pelo totem nem pela IA.
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[ServiceFilter(typeof(ChaveAdministrativaFilter))]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
public abstract class AdministracaoControllerBase : ControllerBase
{
    public const string TagAdministracao = "Administração";
}
