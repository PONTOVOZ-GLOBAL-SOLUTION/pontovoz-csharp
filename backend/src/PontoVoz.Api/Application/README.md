# Application

Casos de uso da API: interfaces de services e repositórios, services com as regras de aplicação,
DTOs de entrada e saída e mapeamentos entre entidades e DTOs.

- Depende de `Domain`.
- Não conhece EF Core nem HTTP: usa apenas interfaces.

| Pasta | Conteúdo |
|---|---|
| `Consultas/` | `Paginacao`, `Pagina<T>`, `ConsultaParadas` e `ConsultaPaginada` |
| `Interfaces/` | Interfaces dos repositórios de consulta e de cadastro, de `IUnidadeTrabalho`, dos services e do provedor de previsões (`IProvedorPrevisaoChegada`, em `IPrevisoes.cs`) |
| `Services/` | Services de consulta, administrativos, `PrevisoesService`, `AlternativasService` e `ResumoParadaService`. O resumo reaproveita os métodos `Montar` de `PrevisoesService` e `UltimasPassagensService`, que recebem o instante já lido do relógio |
| `Dtos/` | DTOs do contrato, `JsonContrato`, corpos e respostas administrativos e `DataHoraComFusoConverter`, que exige o fuso em `observadaEm` |
| `Validacao/` | `ValidacaoParametros` (query) e `ValidacaoCorpo` (corpos administrativos), sobre a base `ColetorErros`; aplicam as regras do domínio e devolvem todos os erros juntos |
| `Erros/` | `ErroAplicacaoException` (inclusive 409 `CodigoJaCadastrado`), `TipoErroAplicacao` e `ViolacaoUnicidadeException` |
| `Avisos/` | `CatalogoAvisos` |
| `Mapeamentos/` | `MapeamentoDtos` e `HorarioSaoPaulo` |

`AplicacaoExtensions.AddAplicacaoPontoVoz()` registra os services na injeção de dependência. Os
services validam os parâmetros e os corpos antes de qualquer acesso ao banco, chamam as regras do
domínio em vez de reescrevê-las e decidem o que uma violação de unicidade significa: 409 num
cadastro, sucesso numa associação repetida. Detalhes em
[`../../../README.md`](../../../README.md#web-api).
