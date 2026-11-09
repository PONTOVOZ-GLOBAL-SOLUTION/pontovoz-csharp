# Índice de evidências — C# Software Development

Este índice liga cada critério da disciplina (documento "GS CitizenTech - 3ES Fev - FIAP 2026",
página 30) às evidências registradas no repositório. Os caminhos são relativos a esta pasta
(`backend/docs/evidencias/`). Cada arquivo de evidência registra saídas reais de uma etapa de
execução e de validação, com a data em que foi produzido.

| Critério | Pontos |
|---|---:|
| Programação Orientada a Objetos | 3,0 |
| Web API | 2,5 |
| Entity Framework Core + LINQ | 1,5 |
| Async/Await | 1,0 |
| Qualidade do código | 2,0 |

## Evidências por tema

| Tema | Arquivo | O que mostra | Critério |
|---|---|---|---|
| **POO** | [`../../README.md`, "Domínio e POO"](../../README.md#domínio-e-poo) | Diagrama de classes, conceitos de POO com as classes reais e invariantes | POO |
| **POO** | Código: `src/PontoVoz.Api/Domain/` | `EntidadeBase` e `RegistroPassagem` abstratas; `PassagemSimulada` e `PassagemRegistroManual` com `override`; objetos de valor (`Coordenada`, `AlternativaDeslocamento`, `ResultadoProvedor`) | POO |
| **Web API** | [`B6-web-api.md`](B6-web-api.md) | Respostas reais das consultas públicas, ProblemDetails (400, 404, 405, 422, 500), CORS, OpenAPI e Scalar | Web API |
| **Web API** | [`B8-previsoes.md`](B8-previsoes.md) | Previsões 200, 400, 404, 422 e 503 com `Retry-After` | Web API |
| **Web API** | [`B9-alternativas-resumo.md`](B9-alternativas-resumo.md) | Alternativas (ranking, comparação incompleta, `SemLinhaDireta`) e resumo completo e parcial | Web API |
| **Web API** | [`B10-validacao-final.md`](B10-validacao-final.md#as-14-operações) | As 14 operações chamadas na API de um clone novo, com status e códigos de erro | Web API |
| **EF Core + LINQ** | [`B3-persistencia.md`](B3-persistencia.md) | Migration, esquema, seed idempotente e `/health` com verificação do banco | EF Core + LINQ |
| **EF Core + LINQ** | [`B4-consultas-linq.md`](B4-consultas-linq.md) | SQL gerado pelo EF Core para cada consulta LINQ, migration `AddSearchColumns`, busca sem acentos e plano de execução | EF Core + LINQ |
| **EF Core + LINQ** | [`B11-validacao.md`](B11-validacao.md#banco-criado-na-inicialização) | Banco criado pela API ao iniciar em Development, sem duplicar dados ao reiniciar | EF Core + LINQ |
| **Async/Await** | [`B5-servicos.md`](B5-servicos.md) | Services assíncronos com `CancellationToken` repassado ao EF Core | Async/Await |
| **Async/Await** | [`B9-alternativas-resumo.md`](B9-alternativas-resumo.md#instante-único-e-paralelismo) | `Task.WhenAll` no resumo sem duas operações simultâneas no mesmo `DbContext` | Async/Await |
| **Testes** | [`B11-validacao.md`](B11-validacao.md#testes) | 816 testes aprovados com binários recompilados, por grupo, com relatório TRX; registro de uma falha intermitente não identificada seguida de 28 execuções aprovadas | Qualidade |
| **Testes** | [`B3`](B3-persistencia.md#testes), [`B4`](B4-consultas-linq.md#testes), [`B5`](B5-servicos.md), [`B6`](B6-web-api.md#testes-automatizados), [`B7`](B7-admin.md), [`B8`](B8-previsoes.md), [`B9`](B9-alternativas-resumo.md#testes-automatizados) | Testes de cada etapa e o que cobrem | Qualidade |
| **Scalar/OpenAPI** | [`B0-scalar.jpg`](B0-scalar.jpg), [`B6-scalar-documentacao.jpg`](B6-scalar-documentacao.jpg), [`B6-scalar-requisicao.jpg`](B6-scalar-requisicao.jpg), [`B7-scalar-admin.jpg`](B7-scalar-admin.jpg), [`B8-scalar-previsoes.jpg`](B8-scalar-previsoes.jpg), [`B9-scalar-alternativas.jpg`](B9-scalar-alternativas.jpg) | Capturas reais da interface Scalar | Web API |
| **Scalar/OpenAPI** | [`../../../contracts/openapi/pontovoz-v1.json`](../../../contracts/openapi/pontovoz-v1.json) | Documento exportado, igual ao gerado pela API (teste `OpenApiExportado_EstaIgualAoDocumentoGeradoPelaApi`) | Web API |
| **Consulta do passageiro** | [`B10-validacao-final.md`](B10-validacao-final.md#jornada-do-cidadão) | Parada → linhas → previsões → últimas passagens → destino → alternativas → resumo | Web API |
| **Fluxo administrativo** | [`B7-admin.md`](B7-admin.md) | Matriz de chave, 401, 403, 409 e cadastro até a consulta pública | Web API / Qualidade |
| **Fluxo administrativo** | [`B10-validacao-final.md`](B10-validacao-final.md#fluxo-administrativo) | Fluxo completo num clone novo, com a chave exigida | Web API |
| **Tratamento de falhas** | [`B8-previsoes.md`](B8-previsoes.md), [`B9-alternativas-resumo.md`](B9-alternativas-resumo.md#provedor-indisponível-nas-três-rotas), [`B10-validacao-final.md`](B10-validacao-final.md#falha-controlada) | 503 nas previsões, 200 `SemComparacao` nas alternativas e 200 `Parcial` no resumo | Web API / Qualidade |
| **Segurança** | [`B7-admin.md`](B7-admin.md), [`B10-validacao-final.md`](B10-validacao-final.md#segurança-e-higiene-do-repositório) | Chave comparada em tempo constante, 401/403, chave fora de logs e respostas, ausência de segredos no Git | Qualidade |
| **Execução** | [`B0-inicializacao.md`](B0-inicializacao.md), [`B11-validacao.md`](B11-validacao.md) | SDK, ferramenta local do EF Core, build e testes na raiz e em `backend/`, API sem comando extra | Qualidade |

## Documentos de apoio

| Documento | Conteúdo |
|---|---|
| [`../../README.md`](../../README.md) | Documentação técnica: execução, arquitetura, domínio, Web API, persistência, async, segurança e testes |
| [`../../../contracts/api-v1.md`](../../../contracts/api-v1.md) | Contrato REST, erros, avisos e exemplos |
| [`../../../docs/dados.md`](../../../docs/dados.md) | Origem dos dados e situação da integração Olho Vivo |
| [`../../../prompts.md`](../../../prompts.md) | Registro do uso de IA, com os prompts originais |
