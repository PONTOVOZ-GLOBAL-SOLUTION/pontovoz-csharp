# PontoVoz — Backend C# (ASP.NET Core / .NET 10)

Backend da Global Solution FIAP 2026.2 (CitizenTech) para a disciplina
**C# Software Development**. Visão geral do projeto: [../README.md](../README.md).

## Estágio atual

**Bloco B9 — alternativas de deslocamento e resumo do totem.** Hoje o backend:

- inicia localmente e responde em `GET /health`, que verifica o processo e o banco SQLite
  (200 quando os dois estão saudáveis, 503 quando o banco está indisponível);
- publica o documento OpenAPI e a interface Scalar em Development;
- tem o **modelo de domínio** em `src/PontoVoz.Api/Domain/` (veja [Domínio](#domínio-b2));
- **persiste** o domínio em SQLite com EF Core 10, com migrations e seed idempotente de dados
  fictícios (veja [Persistência](#persistência-b3));
- tem **repositórios com interfaces** e consultas LINQ assíncronas, executadas no SQLite, para os
  futuros endpoints de paradas, linhas, destinos, percursos e últimas passagens, incluindo a busca
  sem acentos (veja [Consultas](#consultas-linq-b4));
- tem **services de aplicação** que validam os parâmetros, aplicam as regras do contrato e montam
  as respostas (DTOs) de paradas, linhas, destinos e últimas passagens (veja
  [Services](#services-de-aplicação-b5));
- expõe por HTTP as **consultas públicas do B6**: busca de paradas (inclusive pelo
  `codigoExterno` do totem), detalhe da parada, linhas, destinos e últimas passagens, com erros
  em ProblemDetails, CORS configurável e documento OpenAPI exportado (veja [Web API](#web-api-b6));
- tem as **cinco rotas administrativas do B7**, usadas pelo operador para cadastrar paradas e
  linhas, associá-las e registrar observações manuais de passagem, protegidas pela chave
  `X-PontoVoz-Admin-Key` (veja [Rotas administrativas](#rotas-administrativas-b7));
- responde às **previsões de chegada do B8** com um provedor **simulado** (horários fictícios
  calculados no instante da consulta) e tem um adaptador para a API Olho Vivo da SPTrans,
  **desligado** e nunca testado contra a API real (veja [Previsões](#previsões-de-chegada-b8));
- compara as **alternativas de deslocamento** (linhas diretas até um destino, pelo menor tempo
  total) e monta o **resumo do totem** (parada, previsões e últimas passagens numa só resposta),
  completando as 13 rotas de negócio do contrato (veja [Alternativas e resumo](#alternativas-e-resumo-b9));
- tem **805 testes**: 278 de domínio, 144 da camada de aplicação, 172 de integração e de
  provedores e 211 pela API em execução (51 acrescentados no B9).

Todas as rotas do contrato estão implementadas. O estado de cada uma está em
[`../contracts/api-v1.md`](../contracts/api-v1.md), seção 9.

## Requisitos de ambiente

| Item | Versão |
|---|---|
| .NET SDK | 10.0.100 ou superior na linha 10.0 (testado com **10.0.403**) |
| Runtime ASP.NET Core | 10.0.x |
| Sistema | Testado no Windows 11 |
| Editor | Qualquer um (VS Code com C# Dev Kit, Visual Studio 2022+ ou Rider) |

O arquivo [global.json](global.json) exige SDK `10.0.100` com `rollForward: latestFeature`.
Qualquer SDK 10.0.x com feature band igual ou superior é aceito, sem exigir o mesmo patch de todos.

Não é necessário instalar Docker, banco de dados nem ferramentas globais.

## Tecnologias e pacotes

| Pacote | Versão | Uso |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.14 | Documento OpenAPI nativo |
| `Scalar.AspNetCore` | 2.18.7 | Interface de documentação em `/scalar` |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.14 | EF Core com SQLite: `PontoVozDbContext`, mapeamentos e seed |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.14 | Geração e aplicação das migrations pelo `dotnet ef` |
| `dotnet-ef` (ferramenta **local**) | 10.0.14 | Comandos de migrations, declarado em `.config/dotnet-tools.json` |
| `xunit.v3` | 4.0.2 | Testes automatizados (o projeto de testes é um executável, como exige o xUnit v3) |
| `xunit.runner.visualstudio` e `Microsoft.NET.Test.Sdk` | 4.0.1 e 17.14.1 | Compatibilidade com o Test Explorer das IDEs |
| `Microsoft.Extensions.TimeProvider.Testing` | 10.11.0 | `FakeTimeProvider`, relógio controlado nos testes das regras temporais (só no projeto de testes) |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.14 | `WebApplicationFactory`, para testar endpoints pela API real em memória (só no projeto de testes) |

## Executar a partir do GitHub

O repositório **não** contém o banco SQLite: ele é criado na primeira execução, a partir da
migration. Os pré-requisitos são o Git e o .NET SDK 10 (veja
[Requisitos de ambiente](#requisitos-de-ambiente)); Docker e banco de dados instalados não são
necessários. O passo a passo:

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

```bash
dotnet ef database update --project src/PontoVoz.Api
```

```bash
dotnet run --project src/PontoVoz.Api --launch-profile http
```

Com a API em execução, abra http://localhost:5080/scalar (documentação interativa) e
http://localhost:5080/health. Os testes rodam com `dotnet test`, sem precisar do banco local.
Sem o `dotnet ef database update`, a API inicia, mas o `/health` responde **503**, porque o banco
não existe.

Os dados são **fictícios**: paradas, linhas, destinos, percursos e passagens de demonstração com
o prefixo `SIM-`. Eles não representam a operação real da SPTrans (veja
[Dados de demonstração](#dados-de-demonstração)). No estágio atual, só o `/health` está
disponível como endpoint.

## Como executar

Todos os comandos devem ser executados **dentro da pasta `backend/`**.

```bash
dotnet tool restore
```

```bash
dotnet restore
```

```bash
dotnet build
```

```bash
dotnet test
```

Na primeira vez, crie o banco SQLite aplicando as migrations. O mesmo comando cria os dados de
demonstração (veja [Persistência](#persistência-b3)):

```bash
dotnet ef database update --project src/PontoVoz.Api
```

```bash
dotnet run --project src/PontoVoz.Api --launch-profile http
```

A API fica disponível em `http://localhost:5080` (somente localhost).

### Endereços em Development

| Recurso | Endereço |
|---|---|
| Saúde do processo e do banco | http://localhost:5080/health |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Interface Scalar | http://localhost:5080/scalar (redireciona para `/scalar/`) |
| Consultas públicas (B6) | http://localhost:5080/api/v1/... (veja [Web API](#web-api-b6)) |

Respostas reais de `GET /health`, registradas nas evidências do B3:

```json
{"status":"Healthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T21:47:05.9017706+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Healthy"}]}
```

Com o banco indisponível, a resposta é **503**:

```json
{"status":"Unhealthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T21:48:03.2666865+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Unhealthy"}]}
```

A verificação `banco-de-dados` (`BancoDadosHealthCheck`) confere se o SQLite está acessível e
se todas as migrations foram aplicadas. Ela não cria o banco, não aplica migrations e não expõe
caminhos nem a connection string.

### Testes

O projeto usa xUnit v3 sobre a Microsoft.Testing.Platform, declarada em
`global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`), como o
.NET 10 SDK exige para essa plataforma.

`dotnet test` executa os testes de domínio (B2), os de integração com SQLite (B3) e os do
`/health` pela API em execução, e termina com código de saída **0** quando todos passam. Os testes de integração usam um banco SQLite
**real** e isolado por teste, em memória ou em arquivo temporário, com o esquema criado pela
migration. Eles não usam o provedor InMemory e não tocam no `pontovoz.db`. Para ver cada teste:

```bash
dotnet test --output detailed
```

Para rodar só os casos J1 a J5 e T1 a T7 da regra temporal do contrato (seção 4.6.3):

```bash
dotnet test --filter-method "*CasosDoContrato"
```

Para rodar só os testes de integração:

```bash
dotnet test --filter-namespace "PontoVoz.Tests.Infrastructure"
```

Para rodar só os testes pela API em execução (HTTP):

```bash
dotnet test --filter-namespace "PontoVoz.Tests.Api"
```

As evidências estão em [Domínio](#evidências-dos-testes) e em
[`docs/evidencias/B3-persistencia.md`](docs/evidencias/B3-persistencia.md).

### HTTPS (opcional)

O perfil `https` usa `https://localhost:7080` e `http://localhost:5080`:

```bash
dotnet run --project src/PontoVoz.Api --launch-profile https
```

Para o navegador não mostrar aviso de segurança, o certificado de desenvolvimento
precisa ser marcado como confiável. Esse comando altera o repositório de certificados
do Windows, então execute-o só se quiser:

```bash
dotnet dev-certs https --trust
```

Em Development a API não redireciona HTTP para HTTPS, para que a equipe possa
usar a porta 5080 mesmo sem o certificado confiável. Fora de Development o
redirecionamento fica ativo.

### Ferramenta do EF Core

```bash
dotnet ef --version
```

Deve mostrar `10.0.14`. Comandos úteis:

```bash
dotnet ef migrations list --project src/PontoVoz.Api
```

```bash
dotnet ef migrations has-pending-model-changes --project src/PontoVoz.Api
```

## Estrutura

```
backend/
├─ PontoVoz.slnx                 solução (formato .slnx do .NET 10)
├─ global.json                   versão mínima do SDK e plataforma de testes
├─ .config/dotnet-tools.json     ferramenta local dotnet-ef
├─ .editorconfig                 padrão de estilo do código
├─ docs/evidencias/              evidências de funcionamento por bloco
├─ scripts/exportar-openapi.ps1  exporta o OpenAPI da API em execução (B6)
├─ src/PontoVoz.Api/
│  ├─ Program.cs                 composição da aplicação e injeção de dependência
│  ├─ Controllers/               controllers REST: Health, Paradas, Linhas e Destinos (B6)
│  │  └─ Administracao/          rotas /api/v1/admin (B7)
│  ├─ Documentacao/              documento OpenAPI: descrição, chave e campos obrigatórios (B7)
│  ├─ Domain/                    modelo de domínio (B2)
│  │  ├─ Entities/               entidades persistentes e associações
│  │  ├─ ValueObjects/           objetos de valor imutáveis
│  │  ├─ Enums/                  enums do contrato v1
│  │  ├─ Exceptions/             exceções de domínio com o código de erro do contrato
│  │  └─ Rules/                  regras compartilhadas (temporal e de cadastro)
│  ├─ Application/               camada de aplicação (B4 e B5)
│  │  ├─ Avisos/                 catálogo de avisos do contrato
│  │  ├─ Consultas/              Paginacao, Pagina<T> e parâmetros das consultas
│  │  ├─ Dtos/                   respostas no formato do contrato e regras de serialização JSON
│  │  ├─ Erros/                  erros de aplicação com os códigos do contrato
│  │  ├─ Interfaces/             interfaces dos repositórios e dos services
│  │  ├─ Mapeamentos/            entidades → DTOs e horário de São Paulo
│  │  ├─ Services/               services de consulta (B5) e administrativos (B7)
│  │  └─ Validacao/              validação dos parâmetros e dos corpos
│  ├─ Infrastructure/            EF Core, repositórios e provedores de previsão (B3, B4, B7, B8)
│  │  ├─ Persistence/            DbContext, mapeamentos, conversor UTC, busca, migrations e seed (B3, B4)
│  │  ├─ Repositories/           implementações das interfaces com EF Core e LINQ (B4 e B7)
│  │  └─ Providers/              provedor simulado, adaptador Olho Vivo e seletor (B8)
│  ├─ Security/                  chave das rotas administrativas (B7)
│  └─ Middleware/                ProblemDetails, tratamento global de erros, JSON e CORS (B6)
└─ tests/PontoVoz.Tests/         testes xUnit (referencia o projeto da API)
   ├─ Domain/                    testes de domínio (B2)
   ├─ Application/               testes dos services e da paginação (B4, B5 e B7)
   ├─ Infrastructure/            testes de integração com SQLite real (B3, B4 e B7)
   └─ Api/                       testes pela API em execução (B3.1, B5, B6 e B7)
```

## Domínio (B2)

O domínio fica em `src/PontoVoz.Api/Domain/` e não depende de ASP.NET Core nem de EF Core.
Ele segue o contrato [`../contracts/api-v1.md`](../contracts/api-v1.md) (versão B1.3).

### Entidades e relacionamentos

```mermaid
classDiagram
    class EntidadeBase {
        <<abstract>>
        +int Id
        +DateTimeOffset CriadoEmUtc
        +bool Persistida
        +MesmaEntidade(outra) bool
    }
    class Parada {
        +string CodigoExterno
        +string Nome
        +string? Endereco
        +Coordenada? Coordenada
        +bool Ativa
        +OrigemDado Origem
        +bool DadosSimulados
        +AtualizarDados()
        +Ativar()
        +Desativar()
        +AssociarLinha(linha, instante) ParadaLinha
        +ObterAssociacao(linha) ParadaLinha
    }
    class Linha {
        +string Codigo
        +string Letreiro
        +SentidoLinha Sentido
        +bool Ativa
        +OrigemDado Origem
        +bool DadosSimulados
    }
    class Destino {
        +string Nome
        +TipoDestino Tipo
        +bool Ativo
    }
    class ParadaLinha {
        +int ParadaId
        +int LinhaId
        +DateTimeOffset AssociadaEmUtc
    }
    class PercursoEstimado {
        +int ParadaId
        +int LinhaId
        +int DestinoId
        +int MinutosEstimados
    }
    class RegistroPassagem {
        <<abstract>>
        +DateTimeOffset ObservadaEmUtc
        +DateTimeOffset RegistradaEmUtc
        +NivelConfirmacao NivelConfirmacao
        +string CodigoAviso
        +bool DadosSimulados
        +bool Verificada
        +MinutosDesdeObservacao(ref) int
        +EstaDesatualizada(ref) bool
        +JaOcorreu(ref) bool
    }
    class PassagemSimulada {
        +string ChaveSeed
    }
    class PassagemRegistroManual {
        +string OrigemRegistro
    }
    class Coordenada {
        <<record>>
    }
    EntidadeBase <|-- Parada
    EntidadeBase <|-- Linha
    EntidadeBase <|-- Destino
    EntidadeBase <|-- RegistroPassagem
    RegistroPassagem <|-- PassagemSimulada
    RegistroPassagem <|-- PassagemRegistroManual
    Parada "1" *-- "0..*" ParadaLinha : linhas associadas
    ParadaLinha "0..*" --> "1" Linha
    PercursoEstimado --> ParadaLinha : criado a partir de
    PercursoEstimado --> Destino
    RegistroPassagem --> ParadaLinha : parada e linha
    Parada o-- Coordenada
```

| Tipo | Papel |
|---|---|
| `Parada` | Ponto de ônibus. Raiz das associações com linhas. |
| `Linha` | Linha em um sentido. Cada sentido é um cadastro próprio. |
| `ParadaLinha` | Associação N:N entre parada e linha, com o instante da associação |
| `Destino` | Destino consultável pelo totem. Só do seed no MVP. |
| `PercursoEstimado` | Minutos de uma linha, a partir de uma parada que ela atende, até um destino |
| `RegistroPassagem` (abstrata) | Passagem histórica de uma linha pela parada |
| `PassagemSimulada` | Observação de demonstração do provisionamento inicial, com `ChaveSeed` |
| `PassagemRegistroManual` | Observação informada por um operador, com `OrigemRegistro` |
| `Coordenada`, `ReferenciaLinha` | Objetos de valor imutáveis, comparados por valor |
| `PrevisaoChegada`, `ResultadoProvedor` | Previsão de chegada e resultado do provedor. Nunca são passagens. |
| `AlternativaDeslocamento` | Resultado calculado (espera + percurso). Não é persistido. |
| `RegraTemporalPassagem` | Regra da seção 4.6.3 do contrato, compartilhada por entidades e futuros services |

### Invariantes

- **Formatos:** as expressões regulares usam `\A` e `\z`, que exigem a entrada inteira. Com `^`
  e `$`, o .NET aceitaria um valor seguido de quebra de linha. Os formatos são
  `\A[A-Z0-9-]{3,30}\z` para códigos, `\A[A-Za-z0-9-]{1,20}\z` para prefixo de veículo,
  `\A[a-z0-9-]{3,40}\z` para `origemRegistro` e `\ASIM-[A-Z0-9-]{3,36}\z` para `ChaveSeed`.
- **Prefixo `SIM-` em códigos e letreiros:** depende da origem. É obrigatório na origem
  `Simulado` e proibido nos cadastros administrativos, inclusive nos fictícios, que usam outro
  prefixo, como `TESTE-`.
- **Prefixo `SIM-` em veículos:** depende da natureza do registro, não da origem. Um registro
  fictício (parada ou linha fictícia) identifica o veículo com `SIM-`. Um registro com dados
  reais não pode usar `SIM-`. O prefixo continua opcional.
- **Textos:** nome de 3 a 120 caracteres (sem os espaços das pontas), letreiro de 1 a 30,
  terminais de 3 a 120, endereço até 200 (vazio vira `null`).
- **Coordenada:** latitude de -90 a 90 e longitude de -180 a 180, sem `NaN`.
- **Natureza do dado:** a origem `Simulado` sempre tem `DadosSimulados = true`. Num cadastro
  administrativo, o operador declara a natureza, e o domínio não presume nenhum dos dois
  valores. Código, origem e `DadosSimulados` não mudam depois da criação. Uma parada fictícia
  não tem endereço nem coordenada.
- **Associações:** sem duplicidade, porque `AssociarLinha` é idempotente como o `PUT` do
  contrato. Parada inativa recusa com `ParadaInativa` e linha inativa com `LinhaInativa`.
- **Percurso:** de 1 a 240 minutos e criado a partir de uma `ParadaLinha`. Por isso não existe
  percurso de uma linha que não atende a parada.
- **Passagens:** horários definidos na criação, sem método que os altere. O registro manual
  aceita `observadaEm` de 24 horas antes a 2 minutos depois do instante do registro e exige
  `origemRegistro` no formato técnico, além de parada e linha ativas e associadas.
  Nenhum registro é verificado nem oficial.
- **Regra temporal:** minutos inteiros arredondados para baixo e nunca negativos. A passagem
  fica desatualizada quando o intervalo exato passa de 60 minutos. Um registro com horário
  futuro ainda não conta como passagem ocorrida (`JaOcorreu`).
- **Previsões vencidas:** `PrevisaoChegada.VencidaEm` indica um horário previsto anterior à
  referência. `ResultadoProvedor.Respondido` descarta essas previsões antes de calcular o status
  (só vencidas resulta em `SemPrevisoes`). `AlternativaDeslocamento.Calcular` trata uma previsão
  vencida como ausente (`SemPrevisao`). Uma previsão exatamente no instante da consulta é válida,
  com espera 0.
- **Erros:** `ValidacaoDominioException` corresponde a 400 `RequisicaoInvalida`, com o campo do
  contrato, e `RegraNegocioException` corresponde a 422, com o código do contrato.

### Conceitos de POO

| Conceito | Onde |
|---|---|
| Encapsulamento | Todas as propriedades das entidades têm `private set`. O estado muda só por métodos que preservam as invariantes (`Parada.AtualizarDados`, `AssociarLinha`, `Ativar`/`Desativar`). A coleção de linhas é exposta como somente leitura. |
| Abstração | `EntidadeBase` e `RegistroPassagem` são abstratas. `DominioException` abstrai as exceções que o B6 converte em ProblemDetails. |
| Herança | `Parada`, `Linha`, `Destino` e `RegistroPassagem` herdam de `EntidadeBase`. `PassagemSimulada` e `PassagemRegistroManual` herdam de `RegistroPassagem`. |
| Polimorfismo | `NivelConfirmacao`, `CodigoAviso`, `DadosSimulados` e `OrigemRegistro` são sobrescritos em cada tipo de passagem. `Verificada` e os cálculos temporais são herdados e funcionam igual para qualquer `RegistroPassagem`. |
| Composição | `Parada` compõe `ParadaLinha` e `Coordenada`. `PercursoEstimado` e `RegistroPassagem` são criados a partir de uma `ParadaLinha`. `PrevisaoChegada` e `AlternativaDeslocamento` compõem `ReferenciaLinha`. |
| Métodos de fábrica | `CriarSimulada`, `CadastrarAdministrativa`, `CriarNoProvisionamento` e `Registrar` deixam explícitas a origem e a natureza de cada dado. Não há construtor público. |
| Responsabilidade única | As regras temporais ficam em `RegraTemporalPassagem`, e as validações de cadastro em `RegrasCadastro`. As entidades as usam sem duplicá-las. |

Interfaces de repositório e de serviço entram nos blocos em que suas operações forem
implementadas (B4, B5 e B8).

### Evidências dos testes

Execução de `dotnet test` em 07/11/2026, depois de `dotnet restore` e `dotnet build`
(0 avisos, 0 erros):

```
Resumo da execução de teste: Aprovado!
  total: 278
  falhou: 0
  concluído com êxito: 278
  ignorados: 0
```

Código de saída: 0.

| Classe de teste | Testes | Cobre |
|---|---|---|
| `ParadaTests` | 44 | Código, nome, endereço, natureza do dado, atualização, ativação, associações |
| `LinhaTests` | 38 | Código, letreiro, prefixo `SIM-`, terminais, sentido, encapsulamento e construtores para o EF Core |
| `RegistroPassagemTests` | 40 | Polimorfismo, passagem simulada, registro manual, J1 a J5 na entidade, horário futuro, 422 |
| `RegraTemporalPassagemTests` | 32 | J1 a J5, T1 a T7 e valores intermediários e extremos |
| `PrevisaoEAlternativaTests` | 20 | Previsão, resultado do provedor e cálculo da alternativa |
| `DestinoEPercursoTests` | 14 | Destino e percurso de 1 a 240 minutos |
| `CoordenadaTests` | 12 | Limites e igualdade por valor |
| `FormatoEntradaTests` | 34 | Quebra de linha no início, no meio e no fim e caracteres fora do formato em códigos, veículo, `origemRegistro` e `ChaveSeed` |
| `PrefixoVeiculoTests` | 25 | Veículo `SIM-` em registros fictícios (do seed ou administrativos) e proibido em registros reais |
| `PrevisaoVencidaTests` | 19 | Previsões vencidas, no instante exato e futuras; resultados só vencidos e mistos; alternativa com previsão vencida |

Os testes também foram conferidos ao contrário, com mutações temporárias do código, depois
restaurado:

| Mutação | Testes que falham |
|---|---|
| `>` trocado por `>=` na desatualização e na tolerância de 2 minutos | 3 (T5 e J2) |
| `\A...\z` trocado por `^...$` | 5 (um por campo com quebra de linha final) |
| Sem o descarte de vencidas em `Respondido` | 3 |
| Sem a proteção de `AlternativaDeslocamento.Calcular` | 4 |
| Prefixo de veículo do registro manual só por formato | 11 |

## Persistência (B3)

A persistência fica em `src/PontoVoz.Api/Infrastructure/Persistence/`. O domínio não depende
dela.

| Arquivo | Papel |
|---|---|
| `PontoVozDbContext.cs` | `DbSet` de cada entidade e convenções comuns (instantes e enums) |
| `Configuracoes/` | Um mapeamento Fluent API por entidade (`IEntityTypeConfiguration`) |
| `Conversores/InstanteUtcParaTicksConverter.cs` | Conversão de `DateTimeOffset` para ticks UTC |
| `Seed/DadosDemonstracao.cs` | Monta os dados fictícios com as fábricas do domínio |
| `Seed/ProvisionamentoDemonstracao.cs` | Decide se o seed roda e o grava, nos fluxos síncrono e assíncrono |
| `PersistenciaExtensions.cs` | Registro do contexto na injeção de dependência e configuração do SQLite |
| `Migrations/` | Migration `InitialCreate` e o snapshot do modelo |

### Configuração do SQLite

A connection string fica em `appsettings.json` (`ConnectionStrings:PontoVoz`), sem credenciais:

```json
"ConnectionStrings": { "PontoVoz": "Data Source=pontovoz.db" }
```

Um caminho relativo é resolvido a partir da pasta do projeto, então a API e o `dotnet ef` usam
o mesmo arquivo, `src/PontoVoz.Api/pontovoz.db`. Esse arquivo é **local** e é ignorado pelo Git
(`*.db` no `.gitignore`). O `PontoVozDbContext` é registrado com
`builder.Services.AddPersistenciaPontoVoz(...)` no `Program.cs`, com tempo de vida *scoped*, o
padrão do `AddDbContext`.

### Criar o banco, aplicar a migration e o seed

```bash
dotnet ef database update --project src/PontoVoz.Api
```

O comando aplica as migrations pendentes e, em seguida, roda duas etapas pelo gancho
`UseSeeding` do EF Core: o preenchimento da coluna de busca dos registros antigos (B4) e o seed
de demonstração. O `Database.MigrateAsync` usa as mesmas etapas pelo gancho `UseAsyncSeeding`,
que os testes exercitam. A API **não** aplica migrations ao iniciar, e nenhuma etapa roda por
requisição HTTP. Repetir o comando não muda um banco já atualizado.

Num banco criado no B3, o mesmo comando aplica a migration `AddSearchColumns` do B4 sem apagar
nem alterar os dados existentes.

### Tabelas e relações

| Tabela | Chave | Relações e restrições |
|---|---|---|
| `Paradas` | `Id` | `CodigoExterno` único; `Latitude` e `Longitude` anuláveis (objeto de valor `Coordenada`) |
| `Linhas` | `Id` | `Codigo` único |
| `ParadasLinhas` | (`ParadaId`, `LinhaId`) | FK para `Paradas` e `Linhas` |
| `Destinos` | `Id` | `Nome` único (chave natural usada pelo seed; o contrato não tem código de destino) |
| `PercursosEstimados` | (`ParadaId`, `LinhaId`, `DestinoId`) | FK composta para `ParadasLinhas` (a linha precisa atender a parada) e FK para `Destinos`; `CHECK` de 1 a 240 minutos |
| `RegistrosPassagem` | `Id` | Hierarquia TPH com `TipoPassagem` (`Simulada` ou `RegistroManual`); FK composta para `ParadasLinhas`; `ChaveSeed` única quando não nula; índice (`ParadaId`, `LinhaId`, `ObservadaEmUtc`) para a consulta da última passagem |

- **Exclusões:** todas as chaves estrangeiras usam `RESTRICT`. Nenhuma exclusão apaga passagens
  históricas em cascata, porque os cadastros são desativados, não excluídos.
- **TPH:** a subclasse correta é reconstruída ao ler do banco. `NivelConfirmacao`,
  `CodigoAviso`, `Verificada` e `OficialSptrans` são calculados pelo tipo e não viram colunas.
- **Registro manual:** `OrigemRegistro` e `DadosSimulados` são gravados a partir dos campos
  privados da classe. Nas passagens simuladas essas colunas ficam nulas, e `DadosSimulados` é
  sempre `true` pelo próprio tipo.

### Instantes em UTC

Todo `DateTimeOffset` é gravado como **ticks UTC** (`INTEGER`), por
`InstanteUtcParaTicksConverter`. Motivos:

- o provedor SQLite do EF Core não traduz `ORDER BY` nem comparações sobre `DateTimeOffset`.
  Com inteiros, a ordenação e o filtro `ObservadaEmUtc <= @referencia` rodam no banco;
- a precisão de 100 ns é preservada, o que importa para a regra temporal do domínio, que
  compara o intervalo exato;
- a leitura devolve sempre deslocamento zero.

A API pública do domínio não mudou: as entidades continuam com `DateTimeOffset`.

### Dados de demonstração

No **primeiro provisionamento** de um banco vazio, o seed cria 21 registros coerentes com
`contracts/exemplos/`: 2 paradas, 4 linhas, 5 associações, 3 destinos, 5 percursos e 2 passagens
simuladas.

- **Identificação:** todos os registros têm origem `Simulado`, `DadosSimulados = true` e códigos
  com `SIM-`. As passagens simuladas têm `ChaveSeed` (`SIM-OBS-001` e `SIM-OBS-002`) e nunca são
  verificadas nem oficiais.
- **Horários:** são calculados uma única vez, 15 e 25 minutos antes do instante do
  provisionamento, lido do `TimeProvider`.
- **Banco já provisionado** (existe uma parada do seed): o seed não faz nada. Não duplica, não
  renova horários, não cria passagens e não toca nos registros manuais.
- **Passagem recente na demonstração:** vem do registro manual administrativo (B7), conforme a
  seção 4.6.2 do contrato.
- **Ids internos:** são gerados pelo banco e **não são estáveis** entre bancos diferentes, como
  diz a seção 1.2 do contrato. Os dados de demonstração são identificados pelos códigos (`SIM-P001`,
  `SIM-L004`...) e, nos destinos, pelo nome. Os ids dos exemplos JSON do contrato são ilustrativos.
  Nenhuma consulta, teste funcional ou regra depende de um id específico nem da ordem de inserção
  do seed. O teste `Provisionamento_ComIdsDiferentesDosExemplos_RelacionaTudoPeloCodigo` confirma
  que tudo continua correto quando os ids são outros.

### O que funciona e o que está planejado

| Funciona (B3) | Planejado |
|---|---|
| Esquema SQLite criado pela migration | Services e DTOs (B5) |
| Persistência e leitura de todas as entidades | Endpoints REST (B6 e B7) |
| Seed idempotente nos fluxos síncrono e assíncrono | |
| Ordenação e filtro por instante no SQL | |
| Verificação do banco no `/health` (200 ou 503) | |

Os repositórios e a busca sem acentos, previstos aqui para o B4, estão em
[Consultas](#consultas-linq-b4).

### Orientações registradas no B3

- **`DadosSimulados` das passagens:** a coluna `DadosSimulados` de `RegistrosPassagem` existe
  só para o registro manual e fica `NULL` nas passagens simuladas, que são fictícias pelo próprio
  tipo (`TipoPassagem = 'Simulada'`). Essa é uma decisão de armazenamento válida: o objeto lido
  do banco sempre informa `DadosSimulados` corretamente, e um filtro pela propriedade calculada
  (`Where(p => p.DadosSimulados)`) **não é traduzido**: o EF Core lança exceção, em vez de filtrar
  errado. Se o B4 precisar filtrar no SQL, deve usar a forma testada em
  `AuditoriaPersistenciaTests`:

  ```csharp
  passagens.Where(p => p is PassagemSimulada
      || (p is PassagemRegistroManual && EF.Property<bool>((PassagemRegistroManual)p, "_dadosSimulados")))
  ```

- **Busca sem acentos (R7):** o `LIKE` do SQLite ignora maiúsculas só em ASCII e não ignora
  acentos. O teste `BuscaComLikeDoSqlite_NaoIgnoraAcentos...` confirma: `%demonstracao%` não
  encontra "Demonstração", e `%DEMONSTRAÇÃO%` também não. O plano do B3 previa colunas por campo e
  indexadas. No B4, a implementação usa uma coluna por tabela e nenhum índice, pelos motivos
  descritos em [Busca sem acentos](#busca-sem-acentos-r7).
- **Unicidade de `Destinos.Nome`:** é coerente com o contrato. O destino não tem código próprio
  (seção 3.5), o MVP só tem destinos do seed e um nome repetido deixaria ambígua a escolha que a
  IA pede à pessoa (seção 4.4).

## Consultas LINQ (B4)

As consultas que os futuros services e endpoints vão usar ficam atrás de interfaces em
`Application/Interfaces/`, implementadas com EF Core e LINQ em `Infrastructure/Repositories/`.
Elas devolvem entidades do domínio ou `Pagina<T>`, nunca `IQueryable`, e são registradas na
injeção de dependência como *scoped*, junto com o `PontoVozDbContext`
(`AddPersistenciaPontoVoz`). Um service do B5 recebe, por exemplo, `IParadaRepository` pelo
construtor.

| Interface | Métodos | Uso no contrato |
|---|---|---|
| `IParadaRepository` | `BuscarAtivasAsync(termo, codigoExterno, paginacao)` · `ObterAtivaAsync(id)` | `GET /paradas`, `GET /paradas/{id}` e o cabeçalho das respostas da parada |
| `ILinhaRepository` | `BuscarAtivasAsync(termo, paginacao)` · `ObterAtivaAsync(id)` · `ListarAtivasDaParadaAsync(paradaId)` · `AtendeParadaAsync(paradaId, linhaId)` | `GET /linhas`, linhas da parada, 404 de linha e 422 `LinhaNaoAtendeParada` |
| `IDestinoRepository` | `BuscarAtivosAsync(termo, paginacao)` · `ObterAtivoAsync(id)` · `ListarPercursosDiretosAsync(paradaId, destinoId)` | `GET /destinos`, 404 de destino e os dados de entrada das alternativas (B9) |
| `IRegistroPassagemRepository` | `ListarUltimasPorLinhaAsync(paradaId, referencia, linhaId)` | `GET /paradas/{id}/ultimas-passagens` e o resumo |

As associações entre paradas e linhas ficam em `ILinhaRepository`, e os percursos, em
`IDestinoRepository`. Não há repositório genérico.

### Regras aplicadas nas consultas

- **Cadastros ativos:** todas as consultas públicas filtram `Ativa` ou `Ativo` (divergência D10
  do contrato). Linhas inativas saem da lista da parada, dos percursos e das últimas passagens,
  sem apagar o histórico.
- **Associação persistida:** "linha que atende a parada" é sempre uma linha em `ParadasLinhas`.
  As consultas partem dessa tabela, e não da existência separada da parada e da linha.
- **Ordenação fixa com desempate:** paradas e destinos por `nome` e `id`; linhas por `codigo` e
  `id`; percursos por minutos e código da linha; passagens do mais recente para o mais antigo e,
  depois, pelo código da linha.
- **Paginação no banco:** `COUNT(*)`, depois `ORDER BY ... LIMIT ... OFFSET`. `Paginacao` aceita
  página ≥ 1 e tamanho de 1 a 50 (padrão 20). `Pagina<T>` calcula `totalPaginas` como no contrato:
  0 quando não há itens, e uma página além do total devolve `itens` vazio.
- **Somente leitura:** todas as consultas usam `AsNoTracking`.
- **`CancellationToken`:** todos os métodos recebem e repassam o token ao EF Core. Um token
  cancelado interrompe a consulta, como mostra um dos testes.

Exemplo real, a busca de paradas (`ParadaRepository`):

```csharp
IQueryable<Parada> consulta = contexto.Paradas
    .AsNoTracking()
    .Where(parada => parada.Ativa)
    .ContendoTexto(termo);                       // LIKE na coluna normalizada

if (!string.IsNullOrWhiteSpace(codigoExterno))
{
    string codigo = codigoExterno.Trim().ToUpperInvariant();
    consulta = consulta.Where(parada => parada.CodigoExterno == codigo);
}

return await consulta
    .OrderBy(parada => parada.Nome)
    .ThenBy(parada => parada.Id)
    .PaginarAsync(paginacao, cancellationToken);  // CountAsync + Skip/Take + ToListAsync
```

O SQL que o EF Core gera para cada método está em
[`docs/evidencias/B4-consultas-linq.md`](docs/evidencias/B4-consultas-linq.md).

### Busca sem acentos (R7)

`termo` encontra "Demonstração" escrito como `demonstracao`, `DEMONSTRAÇÃO`, com espaços a mais
ou com acentos combinados (letra + sinal Unicode separado). Funcionamento:

1. **Normalização (`TextoBusca.Normalizar`):** decomposição Unicode (FormD), remoção dos sinais
   diacríticos, minúsculas e espaços reduzidos a um só. A mesma função é usada na gravação e no
   termo da consulta, então as duas comparam o mesmo formato.
2. **Coluna `TextoBusca`:** uma por tabela pesquisável. `Paradas` guarda nome e código, `Linhas`
   guarda código, letreiro e os dois terminais, e `Destinos` guarda o nome. Os campos são separados
   por quebra de linha. Como o termo normalizado nunca contém quebra de linha, uma busca não
   atravessa dois campos. É uma propriedade sombra do EF Core: o domínio não muda, e os campos
   exibidos (`Nome`, `Letreiro`...) continuam com o texto original.
3. **Atualização:** o `PontoVozDbContext` recalcula a coluna de toda parada, linha ou destino
   adicionado ou alterado, no `SaveChanges` e no `SaveChangesAsync`. Uma parada renomeada passa a
   ser encontrada pelo nome novo, e não pelo antigo.
4. **Consulta:** `LIKE @padrao ESCAPE '\'` sobre a coluna, no SQL. `%`, `_` e `\` do termo são
   escapados, e por isso `100%` procura o texto "100%" literalmente.
5. **Registros antigos:** a migration `AddSearchColumns` cria a coluna com valor vazio. Depois das
   migrations, a etapa `PreenchimentoTextoBusca` preenche as linhas vazias com a mesma
   normalização. A etapa é idempotente: num banco já preenchido, não grava nada. O SQL da
   migration não conseguiria reproduzir a normalização Unicode do .NET, por isso ela roda em C#.

**Sem índice para `TextoBusca`:** um `LIKE '%termo%'` começa com curinga e não usa índice.
`EXPLAIN QUERY PLAN` confirma `SCAN Paradas`. Com o volume do MVP, a varredura é adequada. A
busca exata por `codigoExterno` usa o índice único (`SEARCH ... USING INDEX
IX_Paradas_CodigoExterno`).

**Limitação:** quem aplicar as migrations por um script SQL gerado (`dotnet ef migrations
script`), fora do EF Core, precisa rodar a API ou o `dotnet ef database update` uma vez para que
a etapa de preenchimento aconteça.

### Última passagem por linha

`ListarUltimasPorLinhaAsync` escolhe, no SQL, uma passagem por linha ativa da parada:

```csharp
IQueryable<int> idsDasUltimas = candidatas          // parada, ObservadaEmUtc <= referencia, linha ativa
    .GroupBy(passagem => passagem.LinhaId)
    .Select(grupo => grupo
        .OrderByDescending(passagem => passagem.ObservadaEmUtc)
        .ThenByDescending(passagem => passagem.RegistradaEmUtc)
        .ThenByDescending(passagem => passagem.Id)
        .Select(passagem => passagem.Id)
        .First());

return await contexto.RegistrosPassagem
    .AsNoTracking()
    .Include(passagem => passagem.Linha)
    .Where(passagem => idsDasUltimas.Contains(passagem.Id))
    .OrderByDescending(passagem => passagem.ObservadaEmUtc)
    .ThenBy(passagem => passagem.Linha.Codigo)
    .ToListAsync(cancellationToken);
```

- **SQL:** o EF Core traduz para um único comando, com `GROUP BY "LinhaId"` e uma subconsulta
  `ORDER BY ObservadaEmUtc DESC, RegistradaEmUtc DESC, Id DESC LIMIT 1` por grupo. Nenhum registro
  é agrupado em memória.
- **Horário futuro:** um registro manual ainda no futuro (tolerância de 2 minutos) fica de fora
  até o horário chegar.
- **Desempate:** é o da seção 4.6.1 do contrato.
- **Tipo concreto:** cada item volta como `PassagemSimulada` ou `PassagemRegistroManual`, com
  `DadosSimulados`, nível de confirmação e aviso corretos. Nenhum é verificado.
- **O que fica com o B5:** os minutos desde a observação, a marcação de desatualizada, os avisos e
  a lista `linhasSemRegistro`, montada a partir de `ListarAtivasDaParadaAsync`.

### Executar os testes das consultas

```bash
dotnet test --filter-namespace "PontoVoz.Tests.Infrastructure"
```

| Classe | Testes | Cobre |
|---|---|---|
| `TextoBuscaTests` | 19 | Normalização, acentos combinados, espaços, curingas do `LIKE` |
| `RepositoriosConsultaTests` | 34 | Paradas, linhas, associações, destinos, percursos, inativos, paginação no SQL e cancelamento |
| `PaginacaoTests` (namespace `PontoVoz.Tests.Application`, fora do filtro acima) | 15 | Valores padrão (nunca tamanho 0), faixas aceitas e recusadas, `totalPaginas` |
| `UltimasPassagensTests` | 9 | Última por linha, desempates, horário futuro, filtro por linha, tipos e natureza, SQL em um comando |
| `MigracaoBuscaTests` | 4 | `AddSearchColumns` em banco com dados do B3, reaplicação, renomeação e curingas |

### Implementado e pendente

| Implementado (B4) | Pendente |
|---|---|
| 4 interfaces de repositório e suas implementações com EF Core e LINQ | Services, DTOs e validação de parâmetros com 400 (B5) |
| Busca sem acentos com coluna normalizada e migration `AddSearchColumns` | Endpoints REST (B6 e B7) |
| Paginação, ordenação e filtros executados no SQLite | Provedores de previsão (B8) |
| Última passagem por linha no SQL | Cálculo das alternativas e resumo (B9) |
| Registro na injeção de dependência | |

## Services de aplicação (B5)

Os services ficam em `Application/Services/` e são o que os controllers do B6 vão chamar. Cada
service recebe os repositórios do B4 **pelas interfaces**, por injeção de dependência no
construtor, e não conhece o `DbContext` nem o SQLite. O fluxo de cada consulta é sempre o mesmo:

1. valida os parâmetros e, se algum estiver errado, lança `ErroAplicacaoException` com todos os
   erros juntos, **sem consultar o banco**;
2. busca os dados nos repositórios (`await`, com o `CancellationToken` da requisição);
3. aplica as regras que não são de consulta (minutos, desatualização, avisos);
4. devolve um DTO no formato do contrato.

| Interface (`Application/Interfaces`) | Service | Método | Endpoint futuro (B6) |
|---|---|---|---|
| `IParadaService` | `ParadaService` | `BuscarAsync(ConsultaParadas)` · `ObterDetalheAsync(id)` | `GET /paradas` · `GET /paradas/{id}` |
| `ILinhaService` | `LinhaService` | `BuscarAsync(ConsultaPaginada)` | `GET /linhas` |
| `IDestinoService` | `DestinoService` | `BuscarAsync(ConsultaPaginada)` | `GET /destinos` |
| `IUltimasPassagensService` | `UltimasPassagensService` | `ObterAsync(paradaId, linhaId)` | `GET /paradas/{id}/ultimas-passagens` |

Os quatro são registrados como *scoped* por `AddAplicacaoPontoVoz()`, chamado no `Program.cs`. O
`TimeProvider` já registrado é injetado no service de últimas passagens.

### Validações e erros

| Regra (contrato, seções 1.5, 1.6 e 2) | Campo | Erro |
|---|---|---|
| `termo` de 2 a 60 caracteres, sem os espaços das pontas; vazio conta como ausente | `termo` | 400 `RequisicaoInvalida` |
| `codigoExterno` com 3 a 30 letras, dígitos ou hífen | `codigoExterno` | 400 |
| `pagina` ≥ 1 e `tamanhoPagina` de 1 a 50 (padrões 1 e 20) | `pagina`, `tamanhoPagina` | 400 |
| `linhaId`, quando informado, ≥ 1 | `linhaId` | 400 |
| Parada inexistente ou inativa (inclusive `id` < 1, que não corresponde à rota) | — | 404 `ParadaNaoEncontrada` |
| Linha inexistente ou inativa | — | 404 `LinhaNaoEncontrada` |
| Linha existe, mas não atende a parada | — | 422 `LinhaNaoAtendeParada` |

`ErroAplicacaoException` informa o `Tipo` (validação, não encontrado ou regra de negócio), o
`Codigo`, o `Titulo`, a mensagem e, na validação, o dicionário `Erros` (campo → mensagens). O B6
vai convertê-lo em ProblemDetails. Erros inesperados não usam essa classe. A ordem das
verificações é a do contrato: parâmetros (400), parada (404), linha (404) e associação (422).

### DTOs

Os DTOs (`Application/Dtos/Dtos.cs`) são *records* com os campos do contrato: `ParadaDto`,
`ParadaDetalheDto`, `LinhaDto`, `DestinoDto`, `PaginaDto<T>`, `ReferenciaLinhaDto`,
`PassagemDto`, `UltimasPassagensParadaDto`, `AvisoDto` e `CoordenadaDto`.

- **Serialização:** `JsonContrato` define as regras de serialização: nomes em camelCase, enums como
  texto e campos nulos sempre presentes.
- **Datas:** saem no horário de São Paulo (`-03:00`), pela conversão de `HorarioSaoPaulo`.
- **Avisos:** os textos estão em `CatalogoAvisos`.
- **Compatibilidade:** os testes de integração montam as respostas a partir do banco real com o
  seed e as comparam, campo a campo, com `paradas-listagem.json`, `paradas-busca-codigo.json`,
  `parada-detalhe.json`, `destinos-busca.json`, `ultimas-passagens.json` e o exemplo de
  `GET /linhas` do `api-v1.md`. Só os ids internos ficam de fora da comparação.

### Últimas passagens

`UltimasPassagensService.ObterAsync`:

1. valida `linhaId` (400), confere a parada ativa (404) e, com `linhaId`, a linha ativa (404) e a
   associação (422);
2. lê o instante atual **uma vez** do `TimeProvider`. O mesmo instante vale para o filtro de
   registros futuros, os minutos, a desatualização e o `consultadoEm`;
3. busca as linhas ativas da parada e a última passagem de cada uma (o repositório já exclui os
   registros com horário futuro);
4. monta as passagens:
   - os minutos são arredondados para baixo;
   - `desatualizada` só acima de 60 minutos exatos;
   - o nível de confirmação, a origem do registro e o aviso vêm do tipo concreto;
   - `verificada` e `oficialSptrans` saem sempre `false`;
5. lista em `linhasSemRegistro` as linhas sem passagem;
6. monta os avisos na ordem dos exemplos: `DadosSimulados`, `CadastroNaoVerificado`,
   `ParadaSemLinhas`, `PassagemDesatualizada` e `PassagemNaoRegistrada`, cada um só quando se
   aplica.

### Operações assíncronas

As operações com I/O real são as consultas ao SQLite feitas pelos repositórios (`CountAsync`,
`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`). Os services as aguardam em sequência e repassam
o `CancellationToken`, e um token cancelado interrompe a consulta, como mostram os testes. A
validação e a montagem dos DTOs são síncronas, porque não fazem I/O. Nenhuma operação usa
`Task.Run`.

### Executar os testes dos services

```bash
dotnet test --filter-namespace "PontoVoz.Tests.Application"
```

| Classe | Testes | Cobre |
|---|---|---|
| `ServicosConsultaTests` | 29 | Validação de termo, código, paginação e ids; erros do contrato; mapeamento; listas vazias |
| `UltimasPassagensServiceTests` | 17 | 400/404/422 e sua ordem; várias linhas; linha sem passagem; simulada e manual; 60 min e 60 min 1 s; uma leitura do relógio; avisos |
| `ServicosIntegracaoTests` | 9 | Respostas do banco real iguais aos exemplos do contrato; busca sem acentos; registro futuro excluído; cancelamento |
| `InjecaoDependenciaTests` (em `PontoVoz.Tests.Api`) | 6 | Services e repositórios resolvidos pelo contêiner da API |

Os testes unitários usam dublês de repositório em memória (`DublesRepositorio`), que guardam as
chamadas recebidas para confirmar que uma requisição inválida não consulta o banco. As consultas
reais continuam testadas contra o SQLite.

### Usados pela Web API (B6)

Os controllers do B6 chamam estes services sem repetir validação nem regra. A conversão dos
erros em ProblemDetails, o `JsonContrato` nas respostas, a restrição `int:min(1)` nas rotas e a
exportação do OpenAPI estão em [Web API](#web-api-b6).

## Web API (B6)

Os controllers expõem os services do B5 por HTTP. Eles só recebem os parâmetros, repassam o
`CancellationToken` da requisição e devolvem o DTO: não têm LINQ, não usam o `DbContext` e não
repetem validações. Evidências reais em
[`docs/evidencias/B6-web-api.md`](docs/evidencias/B6-web-api.md).

### Endpoints

| Método | Rota | Parâmetros | Respostas |
|---|---|---|---|
| GET | `/health` | — | 200, 503 |
| GET | `/api/v1/paradas` | `termo`, `codigoExterno`, `pagina`, `tamanhoPagina` | 200, 400 |
| GET | `/api/v1/paradas/{id}` | `id` na rota | 200, 404 |
| GET | `/api/v1/linhas` | `termo`, `pagina`, `tamanhoPagina` | 200, 400 |
| GET | `/api/v1/destinos` | `termo`, `pagina`, `tamanhoPagina` | 200, 400 |
| GET | `/api/v1/paradas/{id}/ultimas-passagens` | `id` na rota, `linhaId` opcional | 200, 400, 404, 422 |
| GET | `/api/v1/paradas/{id}/previsoes` (B8) | `id` na rota, `linhaId` opcional | 200, 400, 404, 422, 503 |
| GET | `/api/v1/paradas/{id}/alternativas` (B9) | `id` na rota, `destinoId` **obrigatório** | 200, 400, 404 |
| GET | `/api/v1/paradas/{id}/resumo` (B9) | `id` na rota | 200, 404 |

Todas podem devolver 500 `ErroInterno`. Paginação: `pagina` ≥ 1 (padrão 1) e `tamanhoPagina` de
1 a 50 (padrão 20). `termo` tem de 2 a 60 caracteres e não diferencia maiúsculas nem acentos.

As previsões estão em [Previsões de chegada](#previsões-de-chegada-b8), e as alternativas e o
resumo, em [Alternativas e resumo](#alternativas-e-resumo-b9).
As rotas administrativas estão em [Rotas administrativas](#rotas-administrativas-b7). O resumo estava previsto em parte para o B6, mas não foi antecipado:
a rota só será criada com todas as partes.

### Executar e consultar

Com o banco criado ([Como executar](#como-executar)):

```bash
dotnet run --project src/PontoVoz.Api --launch-profile http
```

URL base: `http://localhost:5080` (só localhost). Interface Scalar, para ver e testar as rotas:
`http://localhost:5080/scalar`.

O totem identifica a sua parada pelo **código**, porque o `id` interno pode mudar quando o banco
é recriado (contrato, seção 1.2):

```bash
curl "http://localhost:5080/api/v1/paradas?codigoExterno=SIM-P001"
```

O `id` está em `itens[0].id`. Com ele, consulte o detalhe e as últimas passagens (troque `1` pelo
`id` recebido):

```bash
curl "http://localhost:5080/api/v1/paradas/1"
```

```bash
curl "http://localhost:5080/api/v1/paradas/1/ultimas-passagens"
```

Outras consultas:

```bash
curl "http://localhost:5080/api/v1/paradas?termo=demonstracao"
```

```bash
curl "http://localhost:5080/api/v1/linhas?termo=104"
```

```bash
curl "http://localhost:5080/api/v1/destinos?termo=norte"
```

### Erros (ProblemDetails)

Todo erro sai em `application/problem+json`, com `type`, `title`, `status`, `instance`,
`traceId` e `codigo` (contrato, seção 7). O tratamento fica em um só lugar
([`Middleware/`](src/PontoVoz.Api/Middleware/README.md)):

| Situação | Status | `codigo` |
|---|---|---|
| Parâmetro fora do formato ou da faixa, inclusive valor que não é número (`pagina=abc`) | 400 | `RequisicaoInvalida`, com as mensagens em `errors` |
| Parada inexistente ou inativa | 404 | `ParadaNaoEncontrada` |
| `linhaId` inexistente ou inativo | 404 | `LinhaNaoEncontrada` |
| Caminho sem rota correspondente (`/api/v1/paradas/abc`, `/api/v1/paradas/0`, `/api/v1/rota-inexistente`) | 404 | `RotaNaoEncontrada` |
| Método não aceito pela rota (`POST /api/v1/paradas`) | 405 | `MetodoNaoPermitido`, com o cabeçalho `Allow` |
| Linha existente que não atende a parada | 422 | `LinhaNaoAtendeParada` |
| Erro inesperado | 500 | `ErroInterno`, com mensagem genérica; o erro real fica só no log |

Uma requisição cancelada pelo cliente não vira 500: o `CancellationToken` chega ao service (e
dele às consultas do EF Core), e o ASP.NET Core encerra a requisição sem resposta.

Exemplo real (`GET /api/v1/paradas?termo=a&tamanhoPagina=100`):

```json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"A requisição contém parâmetros inválidos.","status":400,"instance":"/api/v1/paradas","errors":{"termo":["O termo deve ter entre 2 e 60 caracteres."],"tamanhoPagina":["O tamanho da página deve estar entre 1 e 50."]},"codigo":"RequisicaoInvalida","traceId":"00-fdd03c62da328d1469a92ff7f54f2130-903ab235e3e7bc53-00"}
```

### JSON

As respostas usam o mesmo `JsonContrato` dos testes do B5: camelCase, enums como texto, campos
sem valor como `null` (nunca omitidos), listas vazias como `[]` e horários de negócio em
`-03:00`. O mesmo padrão vale para os controllers e para o ProblemDetails.

### CORS

O frontend do totem só consegue chamar a API pelo navegador se a sua origem estiver na lista
`Cors:OrigensPermitidas`. A lista vem **vazia**, e então nenhuma origem externa é liberada. A
URL definitiva do frontend ainda não foi definida pela equipe. Quando for, informe-a em
`appsettings.Development.json` ou por variável de ambiente, sem barra no final:

```json
"Cors": { "OrigensPermitidas": [ "http://localhost:5173" ] }
```

```bash
Cors__OrigensPermitidas__0=http://localhost:5173
```

(`http://localhost:5173` é só um exemplo.) As origens listadas podem fazer apenas `GET`, sem
credenciais. As rotas públicas continuam sem autenticação.

### Exportar o OpenAPI

O arquivo [`../contracts/openapi/pontovoz-v1.json`](../contracts/openapi/pontovoz-v1.json) é o
documento gerado pela API, só com os endpoints implementados. Para atualizá-lo depois de mudar
um endpoint (com a porta 5080 livre), a partir da pasta `backend/`:

```bash
pwsh ./scripts/exportar-openapi.ps1
```

O script compila a API, sobe uma instância local em Development, baixa `/openapi/v1.json`,
confere se é JSON válido, grava o arquivo e encerra a instância. Sem o PowerShell 7, use
`powershell -ExecutionPolicy Bypass -File .\scripts\exportar-openapi.ps1`. O teste
`OpenApiExportado_EstaIgualAoDocumentoGeradoPelaApi` falha se o arquivo ficar desatualizado.

### Testes HTTP

Os 58 testes do B6 (`tests/PontoVoz.Tests/Api/`) chamam a API real em memória
(`WebApplicationFactory`) sobre arquivos SQLite temporários e isolados, com migrations, seed e o
registro manual dos exemplos do contrato. O relógio da API fica parado no instante dos
exemplos (10/11/2026, 14h32), e as respostas são comparadas com os JSON de `contracts/` sem os
ids internos. Os ids usados nas requisições vêm das respostas da própria API. O banco de
desenvolvimento não é usado.

```bash
dotnet test --filter-namespace "PontoVoz.Tests.Api"
```

### O que funciona e o que está pendente

| Funciona (B6) | Pendente |
|---|---|
| Cinco consultas públicas e `/health` | Rotas administrativas com chave (feitas no B7) |
| ProblemDetails 400, 404, 422 e 500 | Previsões e o 503 `ProvedorPrevisoesIndisponivel` (feitos no B8) |
| CORS por configuração | Alternativas e resumo (B9) |
| OpenAPI, Scalar e exportação do documento | URL do frontend em `Cors:OrigensPermitidas` (equipe) |

## Rotas administrativas (B7)

Usadas pelo **operador** da demonstração, não pelo totem nem pela IA, para cadastrar paradas e
linhas, ligar uma linha a uma parada e registrar que um ônibus foi visto. Tudo o que elas gravam
vai para o mesmo SQLite das consultas públicas e aparece nelas logo em seguida. Evidências reais em
[`docs/evidencias/B7-admin.md`](docs/evidencias/B7-admin.md).

### Rotas

| Rota | Sucesso | Erros |
|---|---|---|
| `POST /api/v1/admin/paradas` | **201** `ParadaAdministrativa`, com `Location: /api/v1/paradas/{id}` | 400, 401, 403, 409 `CodigoJaCadastrado` |
| `PUT /api/v1/admin/paradas/{id}` | **200** `ParadaAdministrativa` | 400, 401, 403, 404 `ParadaNaoEncontrada` |
| `POST /api/v1/admin/linhas` | **201** linha com `ativa` e `criadoEm`, sem `Location` | 400, 401, 403, 409 `CodigoJaCadastrado` |
| `PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}` | **204**, idempotente | 401, 403, 404 `ParadaNaoEncontrada`/`LinhaNaoEncontrada`, 422 `ParadaInativa`/`LinhaInativa` |
| `POST /api/v1/admin/observacoes-passagem` | **201** `Passagem` mais `paradaId`, sem `Location` | 400, 401, 403, 404, 422 `LinhaNaoAtendeParada`/`ParadaInativa`/`LinhaInativa` |

Não há `DELETE`, consulta administrativa nem atualização de linha (respondem 405 `MetodoNaoPermitido`).
Desativar uma parada (`"ativa": false`) substitui a exclusão: ela some das consultas públicas, e
as associações e passagens continuam gravadas.

### Chave administrativa

Cabeçalho `X-PontoVoz-Admin-Key`, comparado em tempo constante com `Admin:Chave`:

| Ambiente | `Admin:Chave` configurada? | Resultado |
|---|---|---|
| Development | não | Permitido, com aviso no log a cada inicialização |
| Development | sim | Exige o cabeçalho; sem ele ou errado: **401** `ChaveAdministrativaInvalida` |
| Outros | não | **403** `OperacoesAdministrativasDesabilitadas` |
| Outros | sim | Exige o cabeçalho; sem ele ou errado: **401** `ChaveAdministrativaInvalida` |

- A verificação acontece **antes** da leitura do corpo: sem autorização, a resposta é 401 ou 403
  mesmo com um JSON inválido.
- O 401 traz `WWW-Authenticate: ApiKey header="X-PontoVoz-Admin-Key"`. Os erros saem em
  ProblemDetails com `codigo` e `traceId`, sem o valor da chave; o log também não o registra.
- **Development sem chave** existe só para a demonstração local, como prevê o contrato. Não é
  segurança adequada para uma API exposta.
- Limitações do MVP: chave única e compartilhada, sem usuários, auditoria nem limite de requisições.

Para configurar a chave em Development, use o user-secrets (fica fora do repositório, no perfil do
usuário). A partir da pasta `backend/`, troque o marcador por um valor seu:

```bash
dotnet user-secrets set "Admin:Chave" "<CHAVE_ADMINISTRATIVA_LOCAL>" --project src/PontoVoz.Api
```

Para voltar ao modo sem chave da demonstração local:

```bash
dotnet user-secrets remove "Admin:Chave" --project src/PontoVoz.Api
```

Em outro ambiente, use a variável de ambiente `Admin__Chave` (no PowerShell,
`$env:Admin__Chave = "<CHAVE_ADMINISTRATIVA_LOCAL>"`). Nunca coloque a chave em
`appsettings*.json`, no código, nos testes ou nas evidências. O `UserSecretsId` do `.csproj` só
identifica o armazenamento e não é segredo.

### Testar no Scalar

1. Inicie a API (`dotnet run --project src/PontoVoz.Api --launch-profile http`) e abra
   `http://localhost:5080/scalar`.
2. As rotas estão no grupo **Administração**. Se houver chave configurada, informe-a no campo de
   autenticação `ChaveAdministrativa` (ou no cabeçalho `X-PontoVoz-Admin-Key`) do cliente do Scalar.
3. Em **Test Request**, envie por exemplo o corpo de cadastro de parada:

   ```json
   {"codigoExterno":"TESTE-P003","nome":"Parada Cadastro de Teste","endereco":null,"coordenada":null,"dadosSimulados":true}
   ```

As rotas administrativas **gravam** no banco configurado. Para testes manuais, prefira um banco
de teste (por exemplo, `$env:ConnectionStrings__PontoVoz = "Data Source=C:\temp\pontovoz-teste.db"`
seguido de `dotnet ef database update --project src/PontoVoz.Api` com a mesma variável), para não
misturar cadastros de teste com o `pontovoz.db` de desenvolvimento.

Com curl, a observação manual de exemplo (troque `observadaEm` por um horário das últimas 24
horas e o marcador pela sua chave, ou omita o cabeçalho em Development sem chave):

```bash
curl -X POST http://localhost:5080/api/v1/admin/observacoes-passagem -H "Content-Type: application/json" -H "X-PontoVoz-Admin-Key: <CHAVE_ADMINISTRATIVA_LOCAL>" -d @../contracts/exemplos/admin-observacao-passagem-requisicao.json
```

### Validações

- Campos obrigatórios ausentes, inclusive os booleanos `dadosSimulados` e `ativa` e o enum
  `sentido`, devolvem **400** "O campo X é obrigatório.". A API nunca presume `false` nem o
  primeiro valor de um enum. Todos os erros do corpo vêm juntos em `errors`.
- Corpo ausente, JSON malformado ou `Content-Type` que não é JSON: 400 em `errors.corpo`.
  Número enviado como texto (`"5"`), enum desconhecido e data sem fuso também são 400.
- Formatos e faixas vêm das regras do domínio (`RegrasCadastro`, `Coordenada`), chamadas pelo
  service antes de qualquer acesso ao banco: código `^[A-Z0-9-]{3,30}$` sem o prefixo reservado
  `SIM-`, nome de 3 a 120 caracteres, letreiro de 1 a 30, terminais de 3 a 120, coordenada dentro
  das faixas. Um cadastro fictício (`dadosSimulados: true`) não pode ter endereço nem coordenada.
- A ordem é a do contrato: corpo (400), recursos (404), estado (422) e só então a gravação.
- **Código repetido:** o service consulta antes de gravar (409 claro), e o índice único do SQLite
  cobre duas gravações ao mesmo tempo: a violação de unicidade (códigos 2067 e 1555 do SQLite)
  vira 409, enquanto outras falhas do banco, como chave estrangeira, continuam 500.
- **Associação:** idempotente. A chave composta `(ParadaId, LinhaId)` impede duplicatas mesmo com
  duas requisições simultâneas; a repetição devolve 204.

### Registro manual de passagem

- `observadaEm` precisa ter fuso (`Z` ou `±hh:mm`) e estar entre **24 horas antes** e **2 minutos
  depois** do instante do servidor (`TimeProvider`), inclusive. O instante é guardado em UTC e
  devolvido no horário de São Paulo.
- O registro sai sempre como `RegistroManual`, com `verificada: false` e `oficialSptrans: false`.
  `dadosSimulados` é `true` se a parada ou a linha forem fictícias.
- `prefixoVeiculo` segue a natureza do registro: com `SIM-` em registro fictício e sem ele em
  registro não fictício.
- Um horário ainda no futuro (dentro da tolerância de 2 minutos) é aceito, mas só aparece em
  `ultimas-passagens` quando chegar. As passagens simuladas do seed nunca são renovadas.
- `origemRegistro` é um **rótulo técnico impessoal**, como `operador-local`. O padrão
  `^[a-z0-9-]{3,40}$` recusa nomes com espaço ou maiúscula, e-mails e telefones formatados, mas
  **não** impede toda informação pessoal: uma sequência só de dígitos passa. Nunca use nome,
  telefone ou e-mail de uma pessoa nesse campo.

### Testes do B7

| Classe | O que cobre |
|---|---|
| `AdministracaoAutorizacaoTests` | Matriz de ambiente e chave, ordem antes do corpo, 401 com `WWW-Authenticate`, 403, chave fora de respostas e logs, aviso de inicialização, rotas públicas sem chave |
| `AdministracaoCadastrosTests` | Paradas (201, `Location`, 400, 409, atualização, desativação, busca sem acentos após renomear, histórico preservado), linhas e associação (204 repetido sem duplicar, 404, 422) |
| `ObservacoesPassagemHttpTests` | Exemplo do contrato, janela J1 a J5, casos T5 e T6, tolerância futura, prefixo por natureza, `origemRegistro`, fuso obrigatório, 404, 422, gravação em UTC |
| `AdministracaoFluxoCompletoTests` | Fluxo do operador até a consulta pública e a desativação, com a chave exigida |
| `AdministracaoServicesTests` | Services sem banco: validação antes de ler ou gravar, erros esperados, `CancellationToken`, leitura única do `TimeProvider` |
| `UnidadeTrabalhoEfTests` | SQLite real: dois cadastros com o mesmo código, associação simultânea, falha de chave estrangeira que não vira 409 |

```bash
dotnet test --filter-class "*Administracao*"
```

```bash
dotnet test --filter-class "*UnidadeTrabalhoEfTests"
```

Os testes HTTP usam arquivos SQLite temporários e nunca o `pontovoz.db`. A chave dos testes é um
valor fictício identificado como tal, e a configuração `Admin:Chave` de cada teste é definida pelo
próprio teste, para que um user-secret da máquina não mude o resultado.

## Previsões de chegada (B8)

`GET /api/v1/paradas/{id}/previsoes` devolve as próximas chegadas previstas nas linhas da parada
(contrato, seção 4.5). Uma previsão é uma **estimativa**, com `nivelConfirmacao: "Previsao"`: ela
não é uma passagem observada e nunca aparece em `ultimas-passagens`. Evidências reais em
[`docs/evidencias/B8-previsoes.md`](docs/evidencias/B8-previsoes.md).

```bash
curl "http://localhost:5080/api/v1/paradas/1/previsoes"
```

```bash
curl "http://localhost:5080/api/v1/paradas/1/previsoes?linhaId=2"
```

(Troque `1` e `2` pelos ids recebidos de `/api/v1/paradas?codigoExterno=SIM-P001` e de
`/api/v1/linhas`.)

### Respostas

| Situação | Status | Corpo |
|---|---|---|
| Previsão para todas as linhas | 200 | `status: "Disponivel"`, `linhasSemPrevisao: []` |
| Previsão para parte das linhas | 200 | `status: "Disponivel"`, `linhasSemPrevisao` preenchida, aviso `PrevisaoParcial` |
| Provedor sem previsões | 200 | `status: "SemPrevisoes"`, `previsoes: []`, aviso `SemPrevisoes` |
| Parada sem linhas | 200 | listas vazias e aviso `ParadaSemLinhas`, **sem consultar o provedor** |
| `linhaId` inválido | 400 | `RequisicaoInvalida` |
| Parada ou linha inexistente ou inativa | 404 | `ParadaNaoEncontrada` ou `LinhaNaoEncontrada` |
| Linha que não atende a parada | 422 | `LinhaNaoAtendeParada` |
| Provedor indisponível | **503** | `ProvedorPrevisoesIndisponivel`, com o cabeçalho `Retry-After: 30` |

As validações (400, 404 e 422) vêm antes da consulta ao provedor. `previsoes` sai ordenada por
`previstaPara` e `codigoLinha`; `linhasSemPrevisao`, por `codigoLinha`. Horários no fuso de São
Paulo; `esperaMinutos` é arredondado para baixo e nunca é negativo.

### Provedores

| Componente | Papel |
|---|---|
| `IProvedorPrevisaoChegada` (Application) | Interface do provedor. Uma falha volta como `ResultadoProvedor.Indisponivel`, não como exceção, para o B9 reutilizar o provedor no resumo e nas alternativas |
| `ProvedorPrevisaoSimulado` | **Padrão.** Calcula as chegadas das linhas `SIM-` no instante da consulta, sem gravar nada (regra em [`../docs/dados.md`](../docs/dados.md)) |
| `ProvedorOlhoVivo` | Adaptador HTTP da API Olho Vivo v2.1. **Desligado**, testado só com respostas simuladas |
| `SeletorProvedorPrevisao` | Decide por configuração quem responde. Dados `SIM-` ficam sempre no simulado |
| `PrevisoesService` | Valida, consulta o provedor uma vez com o instante do `TimeProvider` e monta a resposta |

O provedor simulado reproduz o exemplo do contrato com o relógio em 10/11/2026 às 14h32
(`SIM-L002` às 14h33 e 14h51, `SIM-L004` às 14h40, `SIM-L001` sem previsão), mas não guarda essa
data: a grade de cada linha é contada em minutos desde uma âncora fixa e vale em qualquer dia.

### Configuração (`Previsoes`)

| Chave | Padrão | Para que serve |
|---|---|---|
| `Previsoes:SimularIndisponibilidade` | `false` | R6: faz as previsões responderem 503, para testar e demonstrar o erro. Não há rota para ligar. |
| `Previsoes:OlhoVivo:Habilitado` | `false` | Liga o adaptador Olho Vivo |
| `Previsoes:OlhoVivo:Token` | — | Token da SPTrans. **Só** por user-secrets ou variável de ambiente |
| `Previsoes:OlhoVivo:UrlBase` | `https://api.olhovivo.sptrans.com.br/v2.1/` | Endereço oficial (só HTTPS) |
| `Previsoes:OlhoVivo:TempoLimiteSegundos` | `5` | Prazo de cada chamada à SPTrans |
| `Previsoes:OlhoVivo:Paradas` e `:Linhas` | vazios | Correspondência declarada entre os códigos do PontoVoz e os da SPTrans |

Para demonstrar o 503 (PowerShell, a partir de `backend/`):

```powershell
$env:Previsoes__SimularIndisponibilidade = "true"; dotnet run --project src/PontoVoz.Api --launch-profile http
```

Com isso, `GET /previsoes` de uma parada com linhas responde 503; uma parada sem linhas continua
200, e as outras consultas continuam funcionando. Feche a API (ou remova a variável) para voltar
ao normal.

O adaptador Olho Vivo só faz chamadas com `Habilitado`, um token e códigos oficiais declarados,
e nunca para dados `SIM-`. Uma falha dele (autenticação, prazo, conexão, HTTP ou JSON inesperado)
vira 503, sem trocar por previsões simuladas. O que foi e o que **não** foi verificado está em
[`../docs/dados.md`](../docs/dados.md#dados-públicos-api-olho-vivo-da-sptrans).

### Testes do B8

| Classe | Testes | O que cobre |
|---|---|---|
| `ProvedorPrevisaoSimuladoTests` | 22 | Exemplo do contrato, grade em datas variadas, chegada no instante exato, previsão vencida, espera, meia-noite e virada de mês e ano, repetição, linhas sem configuração, indisponibilidade, cancelamento, nada gravado |
| `ProvedorOlhoVivoTests` | 32 | Adaptador com HTTP falso: resposta válida, sem previsões, autenticação recusada, erro HTTP, falha de conexão, prazo esgotado, cancelamento, JSON inválido, dados `SIM-` e códigos sem correspondência nunca enviados, horário `HH:mm` perto da meia-noite, token fora dos logs, seleção do provedor |
| `PrevisoesServiceTests` | 19 | Ordem 400 → 404 → 422 → provedor, estados, avisos, 503, parada sem linhas sem consultar o provedor, instante único, `CancellationToken` |
| `PrevisoesHttpTests` | 21 | Igualdade com `previsoes.json` e com o 503 do contrato, `Retry-After`, tipos do JSON, erros, acesso público, nada gravado, Olho Vivo com HTTP falso pela API real, cancelamento |

```bash
dotnet test --filter-class "*Previs*"
```

```bash
dotnet test --filter-class "*ProvedorOlhoVivoTests"
```

Nenhum teste acessa a SPTrans, e nenhum depende do relógio real: o instante é sempre controlado
(`FakeTimeProvider`) ou informado pelo teste.

## Alternativas e resumo (B9)

Evidências reais em [`docs/evidencias/B9-alternativas-resumo.md`](docs/evidencias/B9-alternativas-resumo.md).

### Alternativas de deslocamento

`GET /api/v1/paradas/{id}/alternativas?destinoId={destinoId}` compara as **linhas diretas** da
parada até um destino (contrato, seção 4.7). Uma linha entra quando está ativa, atende a parada e
tem um **percurso estimado cadastrado** até o destino (seed). Nada é calculado por distância nem
inferido pelo nome do terminal; não há baldeação (`baldeacoes: 0`).

```bash
curl "http://localhost:5080/api/v1/paradas/1/alternativas?destinoId=3"
```

(Troque `1` e `3` pelos ids recebidos de `/api/v1/paradas?codigoExterno=SIM-P001` e de
`/api/v1/destinos?termo=norte`.)

Para cada linha, com a próxima chegada válida do provedor:

```
tempoTotalEstimadoMinutos = esperaMinutos + percursoEstimadoMinutos
chegadaEstimadaAoDestino  = previstaPara + percursoEstimadoMinutos
```

- **Ranking:** primeiro as linhas com previsão (`Comparavel`), pelo menor tempo total; em empate,
  a menor espera e, depois, o código da linha. Recebem `posicao` 1, 2, 3...
- **Sem previsão:** vêm depois, por percurso e código, com `posicao`, `previstaPara`,
  `esperaMinutos`, `tempoTotalEstimadoMinutos` e `chegadaEstimadaAoDestino` nulos. Nunca são
  chamadas de mais rápidas.
- **`comparacaoCompleta`:** só é `true` quando todas as linhas foram comparadas. Se faltar alguma,
  a posição 1 é a mais rápida **entre as linhas com previsão** (aviso `ComparacaoIncompleta`).

| `situacao` | Quando | HTTP |
|---|---|---|
| `Comparadas` | Ao menos uma linha com previsão | 200 |
| `SemComparacao` | Linhas diretas sem nenhuma previsão, inclusive com o **provedor indisponível** (`statusPrevisao: "ProvedorIndisponivel"`) | 200 |
| `SemLinhaDireta` | Nenhum percurso cadastrado até o destino; o provedor **não** é consultado | 200 |

Erros: 400 `RequisicaoInvalida` (`destinoId` ausente, não numérico ou menor que 1), 404
`ParadaNaoEncontrada` e `DestinoNaoEncontrado`. Os percursos do MVP são **simulados**
(`origemPercurso: "Simulado"`, aviso `PercursoEstimado`), e o aviso `LimitacaoLinhasDiretas`
aparece sempre.

Exemplo do contrato (10/11/2026, 14h32): a SIM-L002 chega em 1 minuto, mas leva 40 até o
Terminal Norte (41 no total); a SIM-L004 chega em 8 e leva 20 (28 no total). A SIM-L004 fica em
1º lugar. A SIM-L001 não tem previsão e aparece por último, sem posição.

### Resumo do totem

`GET /api/v1/paradas/{id}/resumo` reúne a parada (mesmo formato de `GET /paradas/{id}`), as
previsões (mesmo formato de `GET /previsoes`) e as últimas passagens (mesmo formato de
`GET /ultimas-passagens`) numa só resposta (seção 4.8). As partes são montadas pelos mesmos
métodos das rotas individuais.

- **Instante único:** o relógio (`TimeProvider`) é lido uma vez por requisição; o mesmo instante
  vale para os três `consultadoEm`, as esperas, os minutos desde a observação e a desatualização.
- **Paralelismo seguro:** a parada e as linhas são lidas primeiro. Depois, o provedor de previsões
  e a consulta de últimas passagens rodam juntos com `Task.WhenAll`. A consulta de passagens é a
  única operação no `DbContext` nesse momento e o provedor não usa o banco, então nenhuma operação
  do EF Core roda em paralelo com outra. O provedor é consultado uma vez.
- **Completo ou parcial:** previsão parcial ou provedor sem previsões continuam `Completo`. Com o
  provedor **indisponível**, a resposta continua 200: `situacao: "Parcial"`,
  `partesIndisponiveis: ["previsoes"]`, aviso `ResumoParcial` e, dentro de `previsoes`,
  `status: "ProvedorIndisponivel"`. A parada e as últimas passagens continuam completas.
- Parada sem linhas: 200, sem consultar o provedor. Parada inexistente ou inativa: 404. Uma falha
  do banco continua sendo 500, nunca um resumo "parcial".

### A mesma indisponibilidade nas três rotas

Com `Previsoes:SimularIndisponibilidade = true` (seção [Previsões](#previsões-de-chegada-b8)):

| Rota | Resposta |
|---|---|
| `GET /previsoes` | **503** `ProvedorPrevisoesIndisponivel`, `Retry-After: 30` |
| `GET /alternativas` | **200**, `situacao: "SemComparacao"`, `statusPrevisao: "ProvedorIndisponivel"` |
| `GET /resumo` | **200**, `situacao: "Parcial"`, `partesIndisponiveis: ["previsoes"]` |

### Testes do B9

| Classe | Testes | O que cobre |
|---|---|---|
| `AlternativasServiceTests` | 20 | Validação, 404, exemplo do contrato, próxima previsão válida, comparação completa e incompleta, desempates por espera e código, linhas sem previsão, posições, `SemComparacao` (sem previsões e indisponível), `SemLinhaDireta` sem consultar o provedor, parada sem linhas, cadastro administrativo, instante e `CancellationToken` |
| `ResumoParadaServiceTests` | 12 | Instante único (uma leitura do relógio), partes iguais às rotas individuais, `Parcial`, previsão parcial ainda `Completo`, parada sem linhas, 404, cadastro administrativo, falha do banco não mascarada, paralelismo sem duas operações de banco ao mesmo tempo, cancelamento |
| `AlternativasResumoHttpTests` | 19 | Igualdade com os quatro exemplos do contrato, campos nulos, `SemLinhaDireta`, 400 e 404, a mesma indisponibilidade nas três rotas, parada sem linhas, registro futuro, nada gravado, acesso público, provedor assíncrono real sem concorrência no `DbContext`, cancelamento |

```bash
dotnet test --filter-class "*Alternativas*"
```

```bash
dotnet test --filter-class "*Resumo*"
```

## Próximas etapas

| Bloco | Conteúdo |
|---|---|
| B1 | Contrato v1 em `../contracts/` (concluído) |
| B2 | Entidades e regras de domínio, com testes (concluído) |
| B3 | `DbContext`, migration inicial e seed de dados simulados (concluído) |
| B4 | Repositórios com consultas LINQ, com testes (concluído) |
| B5 | Services, DTOs e injeção de dependência, com testes (concluído) |
| B6 | Endpoints públicos, tratamento de erros e exportação do contrato OpenAPI (concluído) |
| B7 | Endpoints administrativos com chave de acesso (concluído) |
| B8 | Provedores de previsão (simulado e adaptador HTTP Olho Vivo desligado) (concluído) |
| B9 | Alternativas de deslocamento e resumo da parada (concluído, aguardando auditoria) |
| B10 | Documentação final, evidências e entrega |
