# Domain

Entidades, objetos de valor, enums e exceções de domínio do PontoVoz.

- Não depende de ASP.NET Core, EF Core nem de outras pastas do projeto.
- As regras de negócio (invariantes) ficam nas próprias entidades e em `Rules/`.
- Nenhuma classe lê o relógio: os instantes chegam como parâmetro, vindos do `TimeProvider` da
  camada de aplicação.

| Pasta | Conteúdo |
|---|---|
| `Entities/` | `EntidadeBase`, `Parada`, `Linha`, `ParadaLinha`, `Destino`, `PercursoEstimado` e a hierarquia `RegistroPassagem` → `PassagemSimulada` e `PassagemRegistroManual` |
| `ValueObjects/` | `Coordenada`, `ReferenciaLinha`, `PrevisaoChegada`, `ResultadoProvedor` e `AlternativaDeslocamento` |
| `Enums/` | Enums do contrato v1, com os mesmos nomes de valor |
| `Exceptions/` | `DominioException`, `ValidacaoDominioException` (400), `RegraNegocioException` (422) e `ProvedorPrevisoesIndisponivelException` (503) |
| `Rules/` | `RegraTemporalPassagem` (seção 4.6.3 do contrato) e `RegrasCadastro` (formatos ancorados com `\A` e `\z`, prefixo `SIM-` por origem em códigos e por natureza em veículos) |

Diagrama de classes, invariantes e conceitos de POO:
[`../../../README.md`](../../../README.md#domínio-e-poo).
