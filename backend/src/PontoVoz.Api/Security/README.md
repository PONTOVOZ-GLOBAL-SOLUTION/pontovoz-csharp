# Security

Proteção das rotas administrativas (`/api/v1/admin/...`) pela chave do cabeçalho
`X-PontoVoz-Admin-Key` (contrato, seção 5.1). Implementado no B7.

| Arquivo | Papel |
|---|---|
| `ChaveAdministrativaFilter.cs` | Filtro de **autorização** do MVC (roda antes do model binding): aplica a matriz de ambiente e chave, compara em tempo constante (`CryptographicOperations.FixedTimeEquals` sobre SHA-256) e devolve 401 `ChaveAdministrativaInvalida` (com `WWW-Authenticate`) ou 403 `OperacoesAdministrativasDesabilitadas` em ProblemDetails. |
| `AvisoChaveAdministrativa.cs` | Aviso no log a cada inicialização quando não há chave: em Development, as rotas ficam abertas para a demonstração local; nos demais ambientes, ficam desabilitadas. |
| `SegurancaExtensions.cs` | Registra o filtro e o aviso na injeção de dependência. |

O filtro é aplicado pela base `Controllers/Administracao/AdministracaoControllerBase`, herdada
por todos os controllers administrativos.

A chave é lida de `Admin:Chave`: `dotnet user-secrets` em Development ou a variável de ambiente
`Admin__Chave`. Ela nunca vai para arquivos versionados, respostas ou logs. Como configurar:
[`../../../README.md`](../../../README.md#chave-administrativa).
