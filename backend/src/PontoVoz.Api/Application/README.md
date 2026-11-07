# Application

Casos de uso da API: interfaces de services e repositórios, services com as regras
de aplicação, DTOs de entrada e saída e mapeamentos entre entidades e DTOs.

- Depende de `Domain`.
- Não conhece detalhes de EF Core nem de HTTP externo (usa apenas interfaces).

| Pasta | Conteúdo | Estado |
|---|---|---|
| `Consultas/` | `Paginacao`, `Pagina<T>`, `ConsultaParadas` e `ConsultaPaginada` | B4 e B5 |
| `Interfaces/` | Interfaces dos repositórios (B4) e dos services `IParadaService`, `ILinhaService`, `IDestinoService` e `IUltimasPassagensService` (B5) | B4 e B5 |
| `Services/` | Implementações dos services de consulta | B5 |
| `Dtos/` | DTOs do contrato e `JsonContrato` (regras de serialização) | B5 |
| `Validacao/` | `ValidacaoParametros` | B5 |
| `Erros/` | `ErroAplicacaoException` e `TipoErroAplicacao` | B5 |
| `Avisos/` | `CatalogoAvisos` | B5 |
| `Mapeamentos/` | `MapeamentoDtos` e `HorarioSaoPaulo` | B5 |

`AplicacaoExtensions.AddAplicacaoPontoVoz()` registra os services na injeção de dependência.
Detalhes em [`../../../README.md`](../../../README.md#services-de-aplicação-b5).
