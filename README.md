# PontoVoz — Totem Inteligente de Mobilidade Urbana

**Global Solution FIAP 2026.2** · Engenharia de Software · Tema **CitizenTech** · Disciplina deste
repositório: **C# Software Development** (backend)

Backend em C#/.NET 10 de um totem de informação para pontos de ônibus de São Paulo. A API
implementa as 13 rotas de negócio do contrato e o `GET /health`, com documentação OpenAPI/Scalar e
805 testes automatizados. A demonstração usa dados simulados; não há dados oficiais da SPTrans
integrados. A interface do totem e o componente de voz e IA são de outros integrantes e não fazem
parte deste repositório.

## 1. Problema e proposta CitizenTech

Em São Paulo, quem espera o ônibus muitas vezes não sabe quanto tempo falta para a linha chegar
nem se existe uma opção melhor para o seu destino. Os aplicativos de mobilidade exigem smartphone,
internet móvel e familiaridade com tecnologia, e nem sempre são acessíveis a pessoas com
deficiência visual.

A proposta é um totem no ponto de ônibus, usado por voz ou por toque, que informa a previsão de
chegada das linhas, a última passagem registrada e as alternativas de deslocamento até um destino.
Este backend é a API que o totem e a IA de voz consultam.

| Campo | Conteúdo |
|---|---|
| Cidade | São Paulo, SP |
| Bairro / ponto de aplicação | **Pendente** — será definido quando a equipe escolher um local real |
| Cidadãos beneficiados | Passageiros de ônibus, com atenção a idosos, pessoas com deficiência visual, pessoas sem smartphone ou sem internet móvel e quem não conhece as linhas da região |
| ODS relacionados | ODS 10 (meta 10.2, inclusão independentemente de idade ou deficiência) e ODS 11 (meta 11.2, transporte acessível) — justificativa ainda em aprovação pela equipe |

> A equipe não realizou pesquisa de campo nem entrevistas. A descrição do problema é a hipótese de
> trabalho do projeto, não um resultado de pesquisa. Nenhum indicador de impacto foi medido.

## 2. Funcionalidades implementadas

| Rota | O que faz |
|---|---|
| `GET /api/v1/paradas` | Busca paradas por texto (sem diferenciar acentos) ou pelo código do totem |
| `GET /api/v1/paradas/{id}` | Detalhe da parada, com as linhas que a atendem |
| `GET /api/v1/linhas` · `GET /api/v1/destinos` | Busca de linhas e de destinos |
| `GET /api/v1/paradas/{id}/previsoes` | Próximas chegadas, calculadas por um provedor **simulado** no instante da consulta; 503 quando o provedor está indisponível |
| `GET /api/v1/paradas/{id}/ultimas-passagens` | Última passagem registrada de cada linha, com o nível de confirmação (nunca apresentada como oficial) |
| `GET /api/v1/paradas/{id}/alternativas?destinoId=` | Linhas diretas até o destino, ordenadas pelo menor tempo total (espera + percurso); linhas sem previsão ficam sem posição |
| `GET /api/v1/paradas/{id}/resumo` | Parada, previsões e últimas passagens numa resposta; com o provedor indisponível, responde como resumo parcial |
| `POST`/`PUT /api/v1/admin/...` (5 rotas) | Cadastro e atualização de paradas, cadastro de linhas, associação e registro manual de passagens, protegidos pela chave `X-PontoVoz-Admin-Key` |
| `GET /health` | Verifica o processo e o banco SQLite |

## 3. Tecnologias e arquitetura

| Tecnologia | Uso |
|---|---|
| C# e .NET 10 | Linguagem e plataforma |
| ASP.NET Core Web API (controllers) | Rotas REST, validação, ProblemDetails, filtro da chave administrativa |
| Entity Framework Core 10 + SQLite | Persistência, migrations, seed e consultas LINQ |
| Microsoft.AspNetCore.OpenApi + Scalar | Documento OpenAPI e interface interativa da API |
| xUnit v3, `WebApplicationFactory` e `FakeTimeProvider` | Testes de domínio, aplicação, integração com SQLite e HTTP |

```
Controllers (API REST)  →  Application (services, DTOs, validação)  →  Domain (entidades e regras)
                                     ↓ interfaces
                           Infrastructure (EF Core/SQLite, repositórios LINQ, provedores de previsão)
```

- **Domain:** entidades, objetos de valor e regras de negócio, sem dependência de ASP.NET Core ou
  EF Core.
- **Application:** services que validam, aplicam as regras e montam as respostas; interfaces dos
  repositórios e do provedor de previsões.
- **Infrastructure:** `PontoVozDbContext`, mapeamentos, migrations, seed, repositórios com LINQ e
  os provedores de previsão (simulado e adaptador Olho Vivo desligado).
- **API:** controllers, tratamento central de erros, segurança administrativa e documentação.

As camadas são ligadas por injeção de dependência no
[`Program.cs`](backend/src/PontoVoz.Api/Program.cs). Versões dos pacotes e detalhes de cada camada:
[backend/README.md](backend/README.md).

## 4. Como executar

Pré-requisitos: Git e o **.NET SDK 10** (10.0.100 ou superior na linha 10.0). Não é preciso
instalar banco de dados, Docker nem ferramentas globais.

```bash
git clone https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp.git
```

```bash
cd pontovoz-csharp/backend
```

```bash
dotnet tool restore
```

```bash
dotnet restore
```

```bash
dotnet build
```

Crie o banco SQLite; as migrations criam o esquema e o seed cria os dados de demonstração:

```bash
dotnet ef database update --project src/PontoVoz.Api
```

Inicie a API (só em `localhost`):

```bash
dotnet run --project src/PontoVoz.Api --launch-profile http
```

| Recurso | Endereço |
|---|---|
| Saúde do processo e do banco | http://localhost:5080/health |
| Interface Scalar | http://localhost:5080/scalar |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Exemplo de consulta | http://localhost:5080/api/v1/paradas?codigoExterno=SIM-P001 |

O roteiro de demonstração (consultas do passageiro, operador e falha controlada) está em
[backend/README.md](backend/README.md#roteiro-de-demonstração).

## 5. Como testar

A partir de `backend/`:

```bash
dotnet test
```

Os 805 testes cobrem o domínio, os services, a persistência com SQLite real e isolado por teste e
a API em memória, com as respostas comparadas aos exemplos do contrato. Não usam o banco de
desenvolvimento nem acessam serviços externos. Filtros por grupo:
[backend/README.md](backend/README.md#testes).

## 6. Documentação dos endpoints

- **Contrato REST:** [contracts/api-v1.md](contracts/api-v1.md), com regras, erros, avisos e
  exemplos JSON em [contracts/exemplos/](contracts/exemplos/).
- **OpenAPI exportado da API:** [contracts/openapi/pontovoz-v1.json](contracts/openapi/pontovoz-v1.json)
  (14 operações: 8 públicas de negócio, 5 administrativas e o `/health`).
- **Scalar:** `http://localhost:5080/scalar`, com a API em execução em Development.

## 7. Evidências dos critérios da disciplina

| Critério | Código | Testes | Explicação e evidência |
|---|---|---|---|
| POO (3,0) | `Domain/Entities/` (`EntidadeBase`, `RegistroPassagem` e subclasses), `Domain/ValueObjects/` | `tests/PontoVoz.Tests/Domain/` | [Guia, itens 3 e 9](backend/docs/GUIA-APRESENTACAO-CSHARP.md#9-onde-existem-herança-e-polimorfismo) · [Domínio (B2)](backend/README.md#domínio-b2) |
| Web API (2,5) | `Controllers/`, `Middleware/`, `Security/` | `tests/PontoVoz.Tests/Api/` | [Guia, itens 6 e 18](backend/docs/GUIA-APRESENTACAO-CSHARP.md#6-o-papel-da-api) · [B6](backend/docs/evidencias/B6-web-api.md), [B10](backend/docs/evidencias/B10-validacao-final.md#as-14-operações) |
| EF Core + LINQ (1,5) | `Infrastructure/Persistence/`, `Infrastructure/Repositories/` | `tests/PontoVoz.Tests/Infrastructure/` | [Guia, itens 10 e 11](backend/docs/GUIA-APRESENTACAO-CSHARP.md#11-onde-o-linq-aparece) · [B3](backend/docs/evidencias/B3-persistencia.md), [B4](backend/docs/evidencias/B4-consultas-linq.md) |
| Async/Await (1,0) | Repositórios (`ToListAsync`), `Application/Services/ResumoParadaService.cs` (`Task.WhenAll`) | `ResumoParadaServiceTests`, testes de cancelamento | [Guia, itens 12 e 13](backend/docs/GUIA-APRESENTACAO-CSHARP.md#12-onde-o-asyncawait-aparece) · [B9](backend/docs/evidencias/B9-alternativas-resumo.md#instante-único-e-paralelismo) |
| Qualidade do código (2,0) | Camadas, `TratadorExcecoes`, `ChaveAdministrativaFilter` | 805 testes | [Guia, item 19](backend/docs/GUIA-APRESENTACAO-CSHARP.md#19-como-os-testes-comprovam-as-regras) · [B10](backend/docs/evidencias/B10-validacao-final.md) |

Os caminhos de código partem de `backend/src/PontoVoz.Api/`. O índice completo das evidências está
em [backend/docs/evidencias/INDICE-ENTREGA.md](backend/docs/evidencias/INDICE-ENTREGA.md), e o guia
para explicar o código, em [backend/docs/GUIA-APRESENTACAO-CSHARP.md](backend/docs/GUIA-APRESENTACAO-CSHARP.md).

## 8. Dados e limitações

- **Dados de demonstração:** o seed (paradas, linhas, destinos, percursos e passagens) e as
  previsões de chegada são fictícios. Os registros do seed usam o prefixo `SIM-` e saem com
  `dadosSimulados: true`, `origem: "Simulado"` e o aviso `DadosSimulados`.
- **Cadastros administrativos:** o operador pode cadastrar paradas, linhas e passagens e declara em
  `dadosSimulados` se o cadastro é fictício. Esses cadastros não são verificados (aviso
  `CadastroNaoVerificado`).
- **SPTrans:** não há dados oficiais de chegada integrados ao ambiente demonstrado. O adaptador da
  API Olho Vivo existe no código, mas está desligado por padrão e nunca foi validado com
  credenciais oficiais.
- **Dados pessoais:** a API não pede dados de passageiros, e a demonstração não deve receber dados
  pessoais reais. O campo `origemRegistro` do registro manual recusa nomes com espaço ou
  maiúscula, e-mails e telefones formatados, mas essa validação não garante, sozinha, a exclusão de
  toda informação pessoal (uma sequência só de dígitos é aceita). Por isso, o operador deve usar
  apenas rótulos técnicos, como `operador-local`.
- **Segurança administrativa:** chave única e compartilhada, sem usuários, auditoria nem limite de
  requisições. Em Development sem chave configurada, as rotas administrativas ficam liberadas para a
  demonstração local.
- **Escopo:** o backend não inclui a interface do totem, o reconhecimento de voz nem a IA
  conversacional.

Detalhes sobre a origem de cada dado: [docs/dados.md](docs/dados.md).

## 9. Integrantes

| Nome | RM | Componente |
|---|---|---|
| Albert Katri | RM556544 | Pendente |
| Bruno Biletsky | RM554739 | Pendente |
| Paulo Akira | RM556840 | Pendente |

> **Pendente antes do envio à FIAP:** indicar o componente ou a disciplina de cada integrante.

## Uso de inteligência artificial

O uso de IA está registrado em [prompts.md](prompts.md), com o texto integral de cada prompt: o
ChatGPT apoiou o planejamento, a elaboração dos prompts e as revisões, e o Claude Code executou a
implementação, os testes e a documentação.

## Estrutura do repositório

```
pontovoz-csharp/
├─ README.md          ← este arquivo
├─ prompts.md         ← registro do uso de IA
├─ docs/dados.md      ← dados reais, públicos e simulados
├─ contracts/         ← contrato REST, exemplos JSON, OpenAPI exportado e verificador
└─ backend/           ← solução .NET 10
   ├─ README.md       ← detalhes técnicos de cada bloco
   ├─ docs/           ← guia de apresentação e evidências
   ├─ src/PontoVoz.Api/     ← API (Controllers, Application, Domain, Infrastructure, Security, Middleware)
   └─ tests/PontoVoz.Tests/ ← testes automatizados
```
