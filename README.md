# PontoVoz — Totem Inteligente de Mobilidade Urbana

**Global Solution FIAP 2026.2** · Engenharia de Software · Tema **CitizenTech** · Disciplina deste
repositório: **C# Software Development** (backend)

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | PontoVoz — Totem Inteligente de Mobilidade Urbana |
| Cidade e bairro | São Paulo, SP · bairro: **Pendente** — a equipe deve indicar o ponto de ônibus real escolhido |
| Cidadão beneficiado | Passageiros de ônibus, com atenção a idosos, pessoas com deficiência visual, pessoas sem smartphone ou sem internet móvel e quem não conhece as linhas da região |
| Problema, em uma frase | No ponto de ônibus, quem não tem smartphone ou tem deficiência visual não consegue saber quando a linha chega nem qual linha leva mais rápido ao seu destino. |
| Como funciona | A proposta é um totem no ponto, usado por toque ou voz, que consultaria esta API para informar as próximas chegadas, a última passagem registrada de cada linha e as linhas diretas até um destino, ordenadas pelo menor tempo total (espera + percurso). Neste repositório está o backend; o totem não faz parte dele. |
| O que cada parte faz | **Backend C# (este repositório):** disponibiliza uma API REST, com as regras, a persistência e os dados da demonstração, que pode ser utilizada por outras partes da solução. A integração efetiva com as entregas das outras disciplinas ainda **não foi confirmada**: os integrantes desenvolvem suas disciplinas separadamente, a partir da mesma ideia do PontoVoz. **Demais partes:** **Pendente** — a equipe deve descrever o que cada disciplina entrega e se ela usa esta API. |
| Link do repositório | https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp |
| Indicador de impacto | Ver [Indicadores](#indicadores). Nenhum indicador de impacto social foi medido; os indicadores técnicos medidos estão na mesma seção. |
| ODS conectados | ODS 10 (meta 10.2) e ODS 11 (meta 11.2) — ver [ODS](#objetivos-de-desenvolvimento-sustentável-ods) |

### Validação com o cidadão

**Pendente — preencher pela equipe:** data e local da conversa com uma pessoa que usa o ponto de
ônibus escolhido, o perfil dela (sem nome nem dados pessoais), o que ela faz hoje para saber quando
o ônibus chega e o que a atrapalha. Até esse registro, a descrição do problema é a hipótese de
trabalho da equipe, não um resultado de pesquisa.

## 1. Problema e proposta

Em São Paulo, quem espera o ônibus muitas vezes não sabe quanto tempo falta para a linha chegar
nem se existe uma opção melhor para o seu destino. Os aplicativos de mobilidade exigem smartphone,
internet móvel e familiaridade com tecnologia, e nem sempre são acessíveis a pessoas com
deficiência visual.

A proposta é um totem no ponto de ônibus que responde por voz ou por toque. Este repositório
contém o backend em C#/.NET 10: a API que o totem consulta. A interface e a voz não fazem parte
deste repositório.

### Objetivos de Desenvolvimento Sustentável (ODS)

| ODS | Meta | Como o backend contribui |
|---|---|---|
| **ODS 10 — Redução das desigualdades** | 10.2: até 2030, empoderar e promover a inclusão social, econômica e política de todos, independentemente de idade, gênero, deficiência, raça, etnia, origem, religião, condição econômica ou outra | A API entrega a informação de transporte pronta para ser exibida em tela grande ou lida em voz alta pelo totem, sem exigir smartphone, plano de dados nem leitura de tela pequena. Cada resposta diz se o dado é simulado ou não verificado, para que a informação falada ao passageiro não pareça oficial quando não é. |
| **ODS 11 — Cidades e comunidades sustentáveis** | 11.2: até 2030, proporcionar o acesso a sistemas de transporte seguros, acessíveis, sustentáveis e a preço acessível para todos, melhorando a segurança rodoviária por meio da expansão dos transportes públicos, com especial atenção para as necessidades das pessoas em situação de vulnerabilidade, mulheres, crianças, pessoas com deficiência e idosos | As previsões, as últimas passagens e as alternativas pelo menor tempo total tornam o ônibus mais previsível no próprio ponto, o que ajuda a planejar a viagem; quando falta informação, a resposta avisa em vez de esconder. |

> A redação das justificativas precisa da aprovação final da equipe.

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

**Exclusão lógica:** não há rota `DELETE`. Uma parada é retirada das consultas pelo
`PUT /api/v1/admin/paradas/{id}` com `"ativa": false`, e o histórico de passagens é preservado
(as chaves estrangeiras não permitem apagar em cascata).

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

As camadas são pastas de um mesmo projeto, ligadas por injeção de dependência no
[`Program.cs`](backend/src/PontoVoz.Api/Program.cs). Versões dos pacotes e detalhes de cada camada:
[backend/README.md](backend/README.md).

## 4. Como executar

### Pré-requisitos

| Item | Versão |
|---|---|
| .NET SDK | **10** (10.0.100 ou superior na linha 10.0). Confira com `dotnet --list-sdks`; download em https://dotnet.microsoft.com/download/dotnet/10.0 |
| IDE (opcional) | **Visual Studio 2026** (18.0 ou superior), VS Code com a extensão C# Dev Kit, ou JetBrains Rider. Versões anteriores do Visual Studio não têm suporte ao .NET 10. |
| Git | Qualquer versão recente |

Não é preciso instalar banco de dados, Docker nem ferramentas globais. Com um SDK anterior ao 10
(por exemplo, só o .NET 8), o build não funciona.

### Pela linha de comando

A partir da pasta do repositório clonado:

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

Em Development, a API cria o banco SQLite na primeira execução (aplica as migrations e o seed de
demonstração), sem nenhum comando extra. Nas execuções seguintes, o banco existente é mantido.

### Pelo Visual Studio 2026

Abra `PontoVoz.sln` (na raiz), escolha `PontoVoz.Api` como projeto de inicialização e o perfil
`http`, e execute com F5. O perfil está configurado para abrir o Scalar no navegador, e a API cria o
banco na primeira execução, como pela linha de comando. Esse fluxo não foi testado no Visual Studio
2026; a linha de comando foi testada (veja
[B11-validacao.md](backend/docs/evidencias/B11-validacao.md)).

### Endereços

| Recurso | Endereço |
|---|---|
| Interface Scalar (documentação) | http://localhost:5080/scalar — `http://localhost:5080/` e `/swagger` também levam a ela |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Saúde do processo e do banco | http://localhost:5080/health |
| Exemplo de consulta | http://localhost:5080/api/v1/paradas?codigoExterno=SIM-P001 |

O roteiro de demonstração (consultas do passageiro, operador e falha controlada) e o comando
`dotnet ef database update`, para criar o banco sem iniciar a API, estão em
[backend/README.md](backend/README.md#roteiro-de-demonstração). O arquivo
[`PontoVoz.Api.http`](backend/src/PontoVoz.Api/PontoVoz.Api.http) tem exemplos das 14 operações
para o Visual Studio, o VS Code ou o Rider.

## 5. Como testar

A partir da raiz do repositório (ou da pasta `backend/`):

```bash
dotnet test
```

Os 816 testes cobrem o domínio, os services, a persistência com SQLite real e isolado por teste e
a API em memória, com as respostas comparadas aos exemplos do contrato. Não usam o banco de
desenvolvimento nem acessam serviços externos. Filtros por grupo:
[backend/README.md](backend/README.md#testes).

## 6. Documentação dos endpoints

- **Scalar:** `http://localhost:5080/scalar`, com a API em execução em Development.
- **Contrato REST:** [contracts/api-v1.md](contracts/api-v1.md), com regras, erros, avisos e
  exemplos JSON em [contracts/exemplos/](contracts/exemplos/).
- **OpenAPI exportado da API:** [contracts/openapi/pontovoz-v1.json](contracts/openapi/pontovoz-v1.json)
  (14 operações: 8 públicas de negócio, 5 administrativas e o `/health`).

## 7. Evidências dos critérios da disciplina

| Critério | Código | Testes | Explicação e evidência |
|---|---|---|---|
| POO (3,0) | `Domain/Entities/` (`EntidadeBase`, `RegistroPassagem` e subclasses), `Domain/ValueObjects/` | `tests/PontoVoz.Tests/Domain/` | [Guia, itens 3 e 9](backend/docs/GUIA-APRESENTACAO-CSHARP.md#9-onde-existem-herança-e-polimorfismo) · [Domínio (B2)](backend/README.md#domínio-b2) |
| Web API (2,5) | `Controllers/`, `Middleware/`, `Security/` | `tests/PontoVoz.Tests/Api/` | [Guia, itens 6 e 18](backend/docs/GUIA-APRESENTACAO-CSHARP.md#6-o-papel-da-api) · [B6](backend/docs/evidencias/B6-web-api.md), [B10](backend/docs/evidencias/B10-validacao-final.md#as-14-operações) |
| EF Core + LINQ (1,5) | `Infrastructure/Persistence/`, `Infrastructure/Repositories/` | `tests/PontoVoz.Tests/Infrastructure/`, `MigracaoAutomaticaTests` | [Guia, itens 10 e 11](backend/docs/GUIA-APRESENTACAO-CSHARP.md#11-onde-o-linq-aparece) · [B3](backend/docs/evidencias/B3-persistencia.md), [B4](backend/docs/evidencias/B4-consultas-linq.md) |
| Async/Await (1,0) | Repositórios (`ToListAsync`), `Application/Services/ResumoParadaService.cs` (`Task.WhenAll`) | `ResumoParadaServiceTests`, testes de cancelamento | [Guia, itens 12 e 13](backend/docs/GUIA-APRESENTACAO-CSHARP.md#12-onde-o-asyncawait-aparece) · [B9](backend/docs/evidencias/B9-alternativas-resumo.md#instante-único-e-paralelismo) |
| Qualidade do código (2,0) | Camadas, `TratadorExcecoes`, `ChaveAdministrativaFilter` | 816 testes | [Guia, item 19](backend/docs/GUIA-APRESENTACAO-CSHARP.md#19-como-os-testes-comprovam-as-regras) · [B11](backend/docs/evidencias/B11-validacao.md) |

Os caminhos de código partem de `backend/src/PontoVoz.Api/`. Exemplos de POO no código:

```csharp
// Abstração e herança: Domain/Entities/RegistroPassagem.cs
public abstract class RegistroPassagem : EntidadeBase
{
    public abstract NivelConfirmacao NivelConfirmacao { get; }
    public abstract bool DadosSimulados { get; }
}

// Polimorfismo por sobrescrita: Domain/Entities/PassagemSimulada.cs
public override NivelConfirmacao NivelConfirmacao => NivelConfirmacao.ObservacaoSimulada;
public override bool DadosSimulados => true;

// Polimorfismo por interface: três implementações de IProvedorPrevisaoChegada
public sealed class ProvedorPrevisaoSimulado(IOptionsMonitor<OpcoesPrevisoes> opcoes) : IProvedorPrevisaoChegada
```

(trechos resumidos; os arquivos têm o código completo). O índice das evidências está em
[backend/docs/evidencias/INDICE-ENTREGA.md](backend/docs/evidencias/INDICE-ENTREGA.md), e o guia
para explicar o código, em [backend/docs/GUIA-APRESENTACAO-CSHARP.md](backend/docs/GUIA-APRESENTACAO-CSHARP.md).

## Indicadores

| Indicador | Tipo | Situação |
|---|---|---|
| Testes automatizados aprovados | Técnico, medido | 816 de 816 (0 falhas, 0 ignorados), em [B11-validacao.md](backend/docs/evidencias/B11-validacao.md) |
| Tempo de resposta das rotas públicas (local, banco SQLite de demonstração) | Técnico, medido | Ver [B11-validacao.md](backend/docs/evidencias/B11-validacao.md#tempo-de-resposta-local); mede o backend numa máquina de desenvolvimento, não a experiência no ponto |
| Consultas feitas por pessoas sem smartphone | Impacto, exige piloto | Meta para um piloto — sem dados coletados |
| Tempo de espera percebido no ponto | Impacto, exige pesquisa com usuários | Meta para um piloto — sem dados coletados |
| Avaliação de acessibilidade por pessoas com deficiência visual | Impacto, exige pesquisa com usuários | Meta para um piloto — sem dados coletados |

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
- **Banco automático:** a criação do banco na inicialização vale só em Development
  (`Banco:AplicarMigracoesAoIniciar`); nos outros ambientes, o banco precisa ser preparado com
  `dotnet ef database update`.
- **Escopo:** o backend não inclui a interface do totem, o reconhecimento de voz nem a IA
  conversacional.

Detalhes sobre a origem de cada dado: [docs/dados.md](docs/dados.md).

## 9. Integrantes

| Nome | RM | Componente |
|---|---|---|
| Albert Katri | RM556544 | Pendente |
| Bruno Biletsky | RM554739 | Pendente |
| Paulo Akira | RM556840 | C# Software Development (backend deste repositório). Também ficará responsável por Testing & Quality Assurance, entrega ainda **não iniciada**. |

> **Pendente antes do envio à FIAP:** confirmar o componente ou a disciplina de Albert Katri e de
> Bruno Biletsky.

## Uso de inteligência artificial

O uso de IA está registrado em [prompts.md](prompts.md), com o texto integral de cada prompt: o
ChatGPT apoiou o planejamento, a elaboração dos prompts e as revisões, e o Claude Code executou a
implementação, os testes e a documentação.

## Estrutura do repositório

```
pontovoz-csharp/
├─ README.md          ← este arquivo
├─ PontoVoz.sln       ← solução (Visual Studio 2026, dotnet build e dotnet test na raiz)
├─ global.json        ← versão do SDK e plataforma de testes
├─ .config/           ← ferramenta local dotnet-ef
├─ prompts.md         ← registro do uso de IA
├─ docs/dados.md      ← dados reais, públicos e simulados
├─ contracts/         ← contrato REST, exemplos JSON, OpenAPI exportado e verificador
└─ backend/           ← código .NET 10
   ├─ README.md       ← detalhes técnicos de cada parte
   ├─ PontoVoz.slnx   ← a mesma solução, no formato novo, para quem trabalha dentro de backend/
   ├─ docs/           ← guia de apresentação e evidências
   ├─ src/PontoVoz.Api/     ← API (Controllers, Application, Domain, Infrastructure, Security, Middleware)
   └─ tests/PontoVoz.Tests/ ← testes automatizados
```
