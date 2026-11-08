# Application

Casos de uso da API: interfaces de services e repositórios, services com as regras
de aplicação, DTOs de entrada e saída e mapeamentos entre entidades e DTOs.

- Depende de `Domain`.
- Não conhece detalhes de EF Core nem de HTTP externo (usa apenas interfaces).

| Pasta | Conteúdo | Estado |
|---|---|---|
| `Consultas/` | `Paginacao`, `Pagina<T>`, `ConsultaParadas` e `ConsultaPaginada` | B4 e B5 |
| `Interfaces/` | Interfaces dos repositórios (B4), dos services de consulta (B5), em `IAdministracao.cs` os administrativos e `IUnidadeTrabalho` (B7), e em `IPrevisoes.cs` o `IProvedorPrevisaoChegada`, o `IPrevisoesService` (B8), o `IAlternativasService` e o `IResumoParadaService` (B9) | B4, B5, B7, B8 e B9 |
| `Services/` | Services de consulta (B5), administrativos (B7), `PrevisoesService` (B8), `AlternativasService` e `ResumoParadaService` (B9). O resumo reaproveita os métodos `Montar` de `PrevisoesService` e `UltimasPassagensService`, que recebem o instante já lido do relógio | B5, B7, B8 e B9 |
| `Dtos/` | DTOs do contrato, `JsonContrato`, corpos e respostas administrativos e `DataHoraComFusoConverter`, que exige o fuso em `observadaEm` | B5 e B7 |
| `Validacao/` | `ValidacaoParametros` (query) e `ValidacaoCorpo` (corpos administrativos), sobre a base `ColetorErros`; aplicam as regras do domínio e devolvem todos os erros juntos | B5 e B7 |
| `Erros/` | `ErroAplicacaoException` (inclusive 409 `CodigoJaCadastrado`), `TipoErroAplicacao` e `ViolacaoUnicidadeException` | B5 e B7 |
| `Avisos/` | `CatalogoAvisos` | B5 |
| `Mapeamentos/` | `MapeamentoDtos` e `HorarioSaoPaulo` | B5 |

`AplicacaoExtensions.AddAplicacaoPontoVoz()` registra os services na injeção de dependência.
Os services administrativos validam o corpo antes de qualquer acesso ao banco, chamam as regras do
domínio em vez de reescrevê-las e decidem o que uma violação de unicidade significa: 409 num
cadastro, sucesso numa associação repetida. Detalhes em
[`../../../README.md`](../../../README.md#rotas-administrativas-b7).
Detalhes em [`../../../README.md`](../../../README.md#services-de-aplicação-b5).
