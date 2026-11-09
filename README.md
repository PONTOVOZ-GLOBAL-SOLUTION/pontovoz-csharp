# PontoVoz — Totem Inteligente de Mobilidade Urbana

**Global Solution FIAP 2026.2** · Engenharia de Software · Tema **CitizenTech** · Disciplina:
**C# Software Development**

Este repositório contém o backend em C#/.NET 10 do PontoVoz: uma API REST para consulta de paradas,
linhas, previsões de chegada simuladas, últimas passagens, destinos e alternativas de deslocamento,
com persistência em SQLite pelo Entity Framework Core, documentação OpenAPI/Scalar e 816 testes
automatizados.

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome | PontoVoz — Totem Inteligente de Mobilidade Urbana |
| Cidade e bairro | São Paulo, SP · bairro de referência: Vila Inglesa |
| Cidadão beneficiado | Passageiros de transporte público, incluindo idosos, pessoas com deficiência visual, usuários sem smartphone e pessoas que não conhecem bem a região |
| Problema, em uma frase | É difícil obter informações claras sobre paradas, linhas, previsão de chegada e alternativas de ônibus, especialmente para quem encontra barreiras no uso de aplicativos móveis. |
| Como funciona | A solução proposta é um totem de consulta no ponto de ônibus, com interface acessível, que poderá utilizar os serviços do backend C# para mostrar as próximas chegadas, a última passagem registrada de cada linha e as linhas diretas até um destino, ordenadas pelo menor tempo total (espera + percurso). |
| O que cada parte faz | **Backend C# (este repositório):** API REST que organiza paradas, linhas, destinos, percursos, previsões e passagens, aplica as regras de negócio e informa em cada resposta a origem dos dados e os avisos sobre dados simulados. O totem e sua interface são a proposta de uso da API e não fazem parte deste repositório. |
| Link do repositório | https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp |
| Indicador de impacto | Indicadores propostos para uma implantação piloto, em [Indicadores](#indicadores) |
| ODS conectados | ODS 10 (meta 10.2) e ODS 11 (meta 11.2) |

## Problema e jornada de referência

Muitas pessoas que usam ônibus não sabem quanto tempo falta para a linha chegar nem se existe uma
opção melhor para o seu destino. Os aplicativos de mobilidade resolvem parte disso, mas exigem
smartphone, internet móvel e familiaridade com a tecnologia, e nem sempre são acessíveis a pessoas
com deficiência visual.

**Jornada de referência** (cenário de uso que orienta o projeto, não uma pesquisa de campo): uma
pessoa que usa transporte público na região da Vila Inglesa precisa chegar a um destino. Hoje, ela
consulta um aplicativo de mobilidade, como o Moovit, para identificar a parada e o ônibus mais
convenientes, acompanha a previsão de chegada e vai até o ponto para aguardar. O PontoVoz propõe
oferecer informações semelhantes no próprio ponto de ônibus, de forma acessível, reduzindo a
dependência do celular.

## Proposta da solução

Um totem de consulta instalado no ponto de ônibus, com interface acessível, que poderá utilizar os
serviços deste backend para informar:

1. **Previsão de chegada:** quantos minutos faltam para cada linha.
2. **Última passagem registrada:** quando cada linha foi vista pela última vez, com o nível de
   confirmação, sem apresentar registros não verificados como oficiais.
3. **Alternativas de deslocamento:** as linhas diretas até um destino, comparadas pelo tempo total
   estimado (espera + percurso), e não apenas por quem chega primeiro.

O totem físico, a interface visual e o reconhecimento de voz **não** estão implementados neste
repositório; o componente entregue é o backend C#.

## ODS e contribuição

| ODS | Meta | Contribuição |
|---|---|---|
| **ODS 10 — Redução das desigualdades** | 10.2: até 2030, empoderar e promover a inclusão social, econômica e política de todos, independentemente da idade, gênero, deficiência, raça, etnia, origem, religião, condição econômica ou outra | A API entrega a informação de transporte pronta para ser exibida em tela ou lida em voz alta no ponto, sem exigir smartphone nem plano de dados, o que pode ampliar o acesso de idosos, pessoas com deficiência visual e quem não tem celular. Cada resposta indica se o dado é simulado ou não verificado. |
| **ODS 11 — Cidades e comunidades sustentáveis** | 11.2: até 2030, proporcionar o acesso a sistemas de transporte seguros, acessíveis, sustentáveis e a preço acessível para todos, melhorando a segurança rodoviária por meio da expansão dos transportes públicos, com especial atenção para as necessidades das pessoas em situação de vulnerabilidade, mulheres, crianças, pessoas com deficiência e idosos | Previsões, últimas passagens e alternativas pelo menor tempo total tornam o ônibus mais previsível no próprio ponto e ajudam a planejar a viagem; quando falta informação, a resposta avisa em vez de esconder. |

## Funcionalidades implementadas

| Rota | O que faz |
|---|---|
| `GET /api/v1/paradas` | Busca paradas por texto (sem diferenciar acentos) ou pelo código da parada |
| `GET /api/v1/paradas/{id}` | Detalhe da parada, com as linhas que a atendem |
| `GET /api/v1/linhas` · `GET /api/v1/destinos` | Busca de linhas e de destinos |
| `GET /api/v1/paradas/{id}/previsoes` | Próximas chegadas, calculadas por um provedor **simulado** no instante da consulta; 503 quando o provedor está indisponível |
| `GET /api/v1/paradas/{id}/ultimas-passagens` | Última passagem registrada de cada linha, com o nível de confirmação |
| `GET /api/v1/paradas/{id}/alternativas?destinoId=` | Linhas diretas até o destino, ordenadas pelo menor tempo total; linhas sem previsão ficam sem posição |
| `GET /api/v1/paradas/{id}/resumo` | Parada, previsões e últimas passagens numa resposta; com o provedor indisponível, responde como resumo parcial |
| `POST`/`PUT /api/v1/admin/...` (5 rotas) | Cadastro e atualização de paradas, cadastro de linhas, associação e registro manual de passagens, protegidos pela chave `X-PontoVoz-Admin-Key` |
| `GET /health` | Verifica o processo e o banco SQLite |

Não há rota `DELETE`: uma parada é retirada das consultas pelo `PUT /api/v1/admin/paradas/{id}`
com `"ativa": false` (exclusão lógica), e o histórico de passagens é preservado.

## Tecnologias e arquitetura

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

As camadas são ligadas por injeção de dependência no
[`Program.cs`](backend/src/PontoVoz.Api/Program.cs). Arquitetura, pastas e detalhes de cada camada:
[backend/README.md](backend/README.md#arquitetura).

## Como executar

Pré-requisitos: Git e o **.NET SDK 10** (10.0.100 ou superior na linha 10.0; confira com
`dotnet --list-sdks`). Opcionalmente, Visual Studio 2026 (18.0 ou superior), VS Code com C# Dev
Kit ou JetBrains Rider; versões anteriores do Visual Studio não têm suporte ao .NET 10. Não é
preciso instalar banco de dados nem Docker.

```bash
git clone https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp.git
```

```bash
cd pontovoz-csharp
```

```bash
dotnet build
```

```bash
dotnet run --project backend/src/PontoVoz.Api --launch-profile http
```

Em Development, a API cria o banco SQLite na primeira execução, aplicando as migrations e os dados
de demonstração. No Visual Studio 2026, abra `PontoVoz.sln`, escolha `PontoVoz.Api` como projeto de
inicialização e execute com o perfil `http`.

| Recurso | Endereço |
|---|---|
| Interface Scalar | http://localhost:5080/scalar (também por `http://localhost:5080/` e `/swagger`) |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Saúde do processo e do banco | http://localhost:5080/health |
| Exemplo de consulta | http://localhost:5080/api/v1/paradas?codigoExterno=SIM-P001 |

Configuração, criação do banco com `dotnet ef database update` e roteiro de demonstração:
[backend/README.md](backend/README.md#execução-e-configuração).

## Como testar

A partir da raiz do repositório:

```bash
dotnet test
```

Os 816 testes cobrem o domínio, os services, a persistência com SQLite real e isolado por teste e a
API em memória, com as respostas comparadas aos exemplos do contrato. Não usam o banco de
desenvolvimento nem serviços externos. Grupos e filtros: [backend/README.md](backend/README.md#testes).

## Endpoints e documentação

- **Scalar:** `http://localhost:5080/scalar`, com a API em execução em Development.
- **Contrato REST:** [contracts/api-v1.md](contracts/api-v1.md), com regras, erros e avisos;
  exemplos JSON em [contracts/exemplos/](contracts/exemplos/); guia de consumo em
  [contracts/README.md](contracts/README.md).
- **OpenAPI exportado da API:** [contracts/openapi/pontovoz-v1.json](contracts/openapi/pontovoz-v1.json),
  com 14 operações (8 públicas de negócio, 5 administrativas e o `/health`).
- **Exemplos prontos:** [`backend/src/PontoVoz.Api/PontoVoz.Api.http`](backend/src/PontoVoz.Api/PontoVoz.Api.http),
  com as 14 operações.

## Critérios de C# Software Development

Os caminhos de código partem de `backend/src/PontoVoz.Api/`, e os de teste, de
`backend/tests/PontoVoz.Tests/`.

| Critério | Onde está no código | Testes | Explicação e evidência |
|---|---|---|---|
| **POO (3,0)** | `Domain/Entities/`: `EntidadeBase` e `RegistroPassagem` (abstratas), `PassagemSimulada` e `PassagemRegistroManual` (`override`), `Parada` (encapsulamento e fábricas como `CadastrarAdministrativa`); `Domain/ValueObjects/` (`Coordenada`, `AlternativaDeslocamento`); interfaces em `Application/Interfaces/` | `Domain/` (278 testes), por exemplo `RegistroPassagemTests` | [Domínio e POO](backend/README.md#domínio-e-poo) |
| **Web API (2,5)** | `Controllers/` (7 controllers, 14 operações), `Middleware/TratadorExcecoes.cs` (ProblemDetails), `Security/ChaveAdministrativaFilter.cs`, `Documentacao/DocumentacaoOpenApi.cs` | `Api/` (222 testes), por exemplo `ConsultasPublicasHttpTests` e `AdministracaoAutorizacaoTests` | [Web API](backend/README.md#web-api) · [B6](backend/docs/evidencias/B6-web-api.md) · [B10](backend/docs/evidencias/B10-validacao-final.md#as-14-operações) |
| **EF Core + LINQ (1,5)** | `Infrastructure/Persistence/` (`PontoVozDbContext`, configurações, migrations, seed); `Infrastructure/Repositories/` (por exemplo `RegistroPassagemRepository.ListarUltimasPorLinhaAsync`, com `GroupBy` traduzido para SQL) | `Infrastructure/` (172 testes), `Api/MigracaoAutomaticaTests` | [Persistência e LINQ](backend/README.md#persistência-e-linq) · [B3](backend/docs/evidencias/B3-persistencia.md) · [B4](backend/docs/evidencias/B4-consultas-linq.md) |
| **Async/Await (1,0)** | Repositórios (`ToListAsync`, `CountAsync`), `Infrastructure/Repositories/UnidadeTrabalhoEf.cs` (`SaveChangesAsync`), `Application/Services/ResumoParadaService.cs` (`Task.WhenAll`), `CancellationToken` do controller ao EF Core | `Application/ResumoParadaServiceTests`, testes de cancelamento em `Api/` | [Async/Await](backend/README.md#asyncawait) · [B9](backend/docs/evidencias/B9-alternativas-resumo.md#instante-único-e-paralelismo) |
| **Qualidade do código (2,0)** | Camadas separadas, injeção de dependência em `Program.cs`, tratamento central de erros, validação antes do banco, segredos fora do repositório | 816 testes; verificador do contrato (`contracts/verificador/`) | [Segurança](backend/README.md#segurança) · [Testes](backend/README.md#testes) · [B11](backend/docs/evidencias/B11-validacao.md) |

## Evidências

- **Índice por critério:** [backend/docs/evidencias/INDICE-ENTREGA.md](backend/docs/evidencias/INDICE-ENTREGA.md).
- **Validação final:** execução num clone novo, as 14 operações, a consulta do passageiro, o fluxo
  administrativo e a indisponibilidade do provedor em
  [B10-validacao-final.md](backend/docs/evidencias/B10-validacao-final.md); build e testes na raiz e
  em `backend/`, criação do banco na inicialização e o resultado completo dos testes em
  [B11-validacao.md](backend/docs/evidencias/B11-validacao.md).
- **Capturas do Scalar e evidências por etapa:** [backend/docs/evidencias/](backend/docs/evidencias/).

### Indicadores

| Indicador | Tipo | Situação |
|---|---|---|
| Testes automatizados aprovados | Técnico, medido | 816 de 816 na validação final, com relatório TRX ([B11-validacao.md](backend/docs/evidencias/B11-validacao.md)) |
| Tempo de resposta das rotas públicas | Técnico, medido numa máquina de desenvolvimento | Média de 5 a 7 ms por rota em 50 requisições locais ([B11-validacao.md](backend/docs/evidencias/B11-validacao.md#tempo-de-resposta-local)); não representa a experiência no ponto |
| Consultas feitas por pessoas sem smartphone | Impacto, proposto para piloto | Não medido |
| Tempo de espera percebido no ponto | Impacto, proposto para piloto | Não medido |
| Avaliação de acessibilidade por pessoas com deficiência visual | Impacto, proposto para piloto | Não medido |

## Dados e limitações

- **Dados de demonstração:** o seed (paradas, linhas, destinos, percursos e passagens) e as
  previsões de chegada são fictícios e não representam paradas ou linhas reais. Os registros do
  seed usam o prefixo `SIM-` e saem com `dadosSimulados: true`, `origem: "Simulado"` e o aviso
  `DadosSimulados`.
- **Cadastros administrativos:** o operador declara em `dadosSimulados` se o cadastro é fictício;
  esses cadastros não são verificados (aviso `CadastroNaoVerificado`).
- **SPTrans:** não há dados oficiais de chegada integrados. O adaptador da API Olho Vivo existe no
  código, mas está desligado por padrão e nunca foi validado com credenciais oficiais.
- **Dados pessoais:** a API não pede dados de passageiros, e a demonstração não deve receber dados
  pessoais reais. O campo `origemRegistro` do registro manual recusa nomes, e-mails e telefones
  formatados, mas não garante, sozinho, a exclusão de toda informação pessoal; o operador deve usar
  apenas rótulos técnicos, como `operador-local`.
- **Segurança administrativa:** chave única e compartilhada, sem usuários nem auditoria. Em
  Development sem chave configurada, as rotas administrativas ficam liberadas para a demonstração
  local.
- **Ambiente:** testado no Windows 11 com o .NET SDK 10.0.403. Uma execução da suíte, na validação
  final, teve uma falha intermitente não identificada, seguida de 28 execuções completas aprovadas
  ([B11-validacao.md](backend/docs/evidencias/B11-validacao.md#falha-intermitente-observada)).

Origem de cada dado: [docs/dados.md](docs/dados.md).

## Integrantes

| Nome | RM |
|---|---|
| Albert Katri | RM556544 |
| Bruno Biletsky | RM554739 |
| Paulo Akira | RM556840 |

## Uso de inteligência artificial

O uso de ferramentas de IA no desenvolvimento está registrado em [prompts.md](prompts.md), com o
texto integral de cada prompt.
