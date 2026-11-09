# PontoVoz — Backend C# (ASP.NET Core / .NET 10)

Documentação técnica do backend da Global Solution FIAP 2026.2 (CitizenTech), disciplina
**C# Software Development**. A visão geral do projeto está no [README principal](../README.md); o
contrato REST, em [`../contracts/api-v1.md`](../contracts/api-v1.md).

| Assunto | Seção |
|---|---|
| Versões, SDK e IDE | [Requisitos](#requisitos) |
| Executar, banco, configuração e roteiro de demonstração | [Execução e configuração](#execução-e-configuração) |
| Camadas, pastas e injeção de dependência | [Arquitetura](#arquitetura) |
| Entidades, herança, polimorfismo e invariantes | [Domínio e POO](#domínio-e-poo) |
| Rotas, erros e regras de cada endpoint | [Web API](#web-api) |
| `DbContext`, migrations, seed e consultas LINQ | [Persistência e LINQ](#persistência-e-linq) |
| Operações assíncronas, cancelamento e paralelismo | [Async/Await](#asyncawait) |
| Chave administrativa, CORS e erros | [Segurança](#segurança) |
| Como rodar os testes e o que cobrem | [Testes](#testes) |
| Dados de demonstração e limites conhecidos | [Dados simulados e limitações](#dados-simulados-e-limitações) |

As evidências de execução, organizadas pelos critérios da disciplina, estão em
[`docs/evidencias/INDICE-ENTREGA.md`](docs/evidencias/INDICE-ENTREGA.md).

## Requisitos

| Item | Versão |
|---|---|
| .NET SDK | 10.0.100 ou superior na linha 10.0 (testado com **10.0.403**) |
| Runtime ASP.NET Core | 10.0.x |
| Sistema | Testado no Windows 11 |
| IDE (opcional) | **Visual Studio 2026** (18.0 ou superior), VS Code com a extensão C# Dev Kit, ou JetBrains Rider. Versões anteriores do Visual Studio não têm suporte ao .NET 10. |

O [`global.json`](../global.json), na raiz do repositório, exige SDK `10.0.100` com
`rollForward: latestFeature` e declara a Microsoft.Testing.Platform para os testes. Com um SDK
anterior ao 10 (por exemplo, só o .NET 8), o build não funciona. Não é necessário instalar Docker,
banco de dados nem ferramentas globais.

| Pacote | Versão | Uso |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.14 | Documento OpenAPI nativo |
| `Scalar.AspNetCore` | 2.18.7 | Interface de documentação em `/scalar` |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.14 | EF Core com SQLite |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.14 | Geração e aplicação das migrations pelo `dotnet ef` |
| `dotnet-ef` (ferramenta **local**) | 10.0.14 | Declarada em `.config/dotnet-tools.json`, na raiz do repositório |
| `xunit.v3` | 4.0.2 | Testes automatizados |
| `xunit.runner.visualstudio` e `Microsoft.NET.Test.Sdk` | 4.0.1 e 17.14.1 | Compatibilidade com o Test Explorer das IDEs |
| `Microsoft.Extensions.TimeProvider.Testing` | 10.11.0 | `FakeTimeProvider`, relógio controlado nos testes (só no projeto de testes) |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.14 | `WebApplicationFactory`, a API real em memória nos testes (só no projeto de testes) |

## Execução e configuração

### Executar

A partir da raiz do repositório clonado:

```bash
dotnet build
```

```bash
dotnet test
```

```bash
dotnet run --project backend/src/PontoVoz.Api --launch-profile http
```

No Visual Studio 2026, abra `PontoVoz.sln`, na raiz, escolha `PontoVoz.Api` como projeto de
inicialização e execute com o perfil `http`; o perfil abre o Scalar no navegador. Esse fluxo não
foi testado no Visual Studio 2026; a linha de comando foi.

Os mesmos comandos funcionam dentro de `backend/`, que tem a solução `PontoVoz.slnx`. O passo a
passo completo nessa pasta, com o banco criado antes de iniciar a API:

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

Os dois caminhos foram executados numa cópia limpa do repositório, sem banco nem binários
anteriores ([`docs/evidencias/B11-validacao.md`](docs/evidencias/B11-validacao.md)).

| Recurso | Endereço (somente `localhost`) |
|---|---|
| Interface Scalar | http://localhost:5080/scalar; em Development, `/` e `/swagger` também levam a ela |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Saúde do processo e do banco | http://localhost:5080/health |
| Rotas da API | http://localhost:5080/api/v1/... |

O perfil `https` usa `https://localhost:7080` e precisa do certificado de desenvolvimento
confiável (`dotnet dev-certs https --trust`, que altera o repositório de certificados do Windows).
Em Development a API não redireciona HTTP para HTTPS; nos outros ambientes, redireciona.

### Banco de dados

A connection string fica em `appsettings.json` (`ConnectionStrings:PontoVoz`), sem credenciais:
`Data Source=pontovoz.db`. Um caminho relativo é resolvido a partir da pasta do projeto, então a
API e o `dotnet ef` usam o mesmo arquivo, `src/PontoVoz.Api/pontovoz.db`, que é local e ignorado
pelo Git.

- **Em Development**, com `Banco:AplicarMigracoesAoIniciar` verdadeira (valor do
  `appsettings.Development.json`), a API aplica as migrations pendentes e o seed antes de atender a
  primeira requisição (`PersistenciaExtensions.AplicarMigracoesEmDesenvolvimentoAsync`, com
  `Database.MigrateAsync`, sem `EnsureCreated`). Num banco já atualizado e provisionado, nada é
  gravado, e reiniciar a API não duplica dados.
- **Nos outros ambientes**, a opção é ignorada mesmo se ligada (com aviso no log), e o banco é
  preparado com `dotnet ef database update --project src/PontoVoz.Api`.
- O `/health` responde **503** quando o banco não existe ou não tem todas as migrations; ele só lê
  e nunca cria o banco.

### Configuração

| Chave | Padrão | Para que serve |
|---|---|---|
| `ConnectionStrings:PontoVoz` | `Data Source=pontovoz.db` | Arquivo SQLite |
| `Banco:AplicarMigracoesAoIniciar` | `false` (`true` em Development) | Cria ou atualiza o banco ao iniciar; só vale em Development |
| `Admin:Chave` | — | Chave das rotas administrativas. **Só** por user-secrets ou variável de ambiente (veja [Segurança](#segurança)) |
| `Cors:OrigensPermitidas` | `[]` | Origens de frontend liberadas para `GET` |
| `Previsoes:SimularIndisponibilidade` | `false` | Faz o provedor de previsões responder como indisponível (requisito R6), para demonstrar o 503 e o resumo parcial |
| `Previsoes:OlhoVivo:Habilitado` | `false` | Liga o adaptador Olho Vivo |
| `Previsoes:OlhoVivo:Token` | — | Token da SPTrans. **Só** por user-secrets ou variável de ambiente |
| `Previsoes:OlhoVivo:UrlBase` | `https://api.olhovivo.sptrans.com.br/v2.1/` | Endereço oficial (só HTTPS) |
| `Previsoes:OlhoVivo:TempoLimiteSegundos` | `5` | Prazo de cada chamada à SPTrans |
| `Previsoes:OlhoVivo:Paradas` e `:Linhas` | vazios | Correspondência declarada entre os códigos do PontoVoz e os da SPTrans |

Uma chave com `:` vira variável de ambiente com `__`, por exemplo
`Previsoes__SimularIndisponibilidade=true`.

### Roteiro de demonstração

Com a API em execução e um banco novo, a parada `SIM-P001` tem o id 1 e o Terminal Norte tem o
id 3; os ids podem mudar quando o banco é recriado, então confira-os no primeiro e no quinto
comando. O arquivo [`src/PontoVoz.Api/PontoVoz.Api.http`](src/PontoVoz.Api/PontoVoz.Api.http) tem
as 14 operações para o Visual Studio, o VS Code (REST Client) ou o Rider.

**Consulta do passageiro** (rotas públicas, sem chave):

```bash
curl "http://localhost:5080/api/v1/paradas?codigoExterno=SIM-P001"
```

```bash
curl "http://localhost:5080/api/v1/paradas/1"
```

```bash
curl "http://localhost:5080/api/v1/paradas/1/previsoes"
```

```bash
curl "http://localhost:5080/api/v1/paradas/1/ultimas-passagens"
```

```bash
curl "http://localhost:5080/api/v1/destinos?termo=norte"
```

```bash
curl "http://localhost:5080/api/v1/paradas/1/alternativas?destinoId=3"
```

```bash
curl "http://localhost:5080/api/v1/paradas/1/resumo"
```

1. A parada é encontrada pelo código; o detalhe lista as linhas que a atendem.
2. As previsões mostram as próximas chegadas **simuladas** (`origem: "Simulado"`, aviso
   `DadosSimulados`).
3. As últimas passagens mostram quando cada linha foi vista pela última vez, com o nível de
   confirmação (nunca oficial).
4. Com o destino escolhido, as alternativas ordenam as linhas diretas pelo menor tempo total
   (espera + percurso). Uma linha sem previsão fica sem posição, com o aviso `ComparacaoIncompleta`.
5. O resumo reúne parada, previsões e últimas passagens num só instante.

**Operador:** as rotas administrativas gravam no banco configurado. Para testá-las, prefira um
banco separado e uma chave configurada (veja [Segurança](#segurança)); sem a chave, a resposta é
401, e com ela, 201.

**Falha controlada:** com `Previsoes__SimularIndisponibilidade=true`, `/previsoes` responde 503
com `Retry-After: 30`, `/alternativas` responde 200 `SemComparacao` e `/resumo` responde 200
`Parcial`; parada, linhas e últimas passagens continuam disponíveis. No PowerShell:

```powershell
$env:Previsoes__SimularIndisponibilidade = "true"; dotnet run --project src/PontoVoz.Api --launch-profile http
```

### Exportar o OpenAPI

[`../contracts/openapi/pontovoz-v1.json`](../contracts/openapi/pontovoz-v1.json) é o documento
gerado pela própria API. Para atualizá-lo depois de mudar um endpoint (com a porta 5080 livre), a
partir de `backend/`:

```bash
pwsh ./scripts/exportar-openapi.ps1
```

O script compila a API, sobe uma instância local em Development, baixa `/openapi/v1.json`, grava o
arquivo e encerra a instância. Sem o PowerShell 7, use
`powershell -ExecutionPolicy Bypass -File .\scripts\exportar-openapi.ps1`. O teste
`OpenApiExportado_EstaIgualAoDocumentoGeradoPelaApi` falha se o arquivo ficar desatualizado.

## Arquitetura

```
Controllers (API REST)  →  Application (services, DTOs, validação)  →  Domain (entidades e regras)
                                     ↓ interfaces
                           Infrastructure (EF Core/SQLite, repositórios LINQ, provedores de previsão)
```

As camadas são pastas de um mesmo projeto (`src/PontoVoz.Api`). O `Domain` não depende de ASP.NET
Core nem de EF Core; a `Application` usa os repositórios e o provedor de previsões só por
interfaces; os controllers não têm LINQ nem regra de negócio.

```
pontovoz-csharp/
├─ PontoVoz.sln                  solução (formato clássico), usada na raiz e no Visual Studio 2026
├─ global.json                   versão do SDK e plataforma de testes
├─ .config/dotnet-tools.json     ferramenta local dotnet-ef
└─ backend/
   ├─ PontoVoz.slnx              a mesma solução, no formato .slnx
   ├─ .editorconfig              padrão de estilo do código
   ├─ docs/evidencias/           evidências de execução e índice por critério
   ├─ scripts/exportar-openapi.ps1
   ├─ src/PontoVoz.Api/
   │  ├─ Program.cs              composição da aplicação e injeção de dependência
   │  ├─ Controllers/            Health, Paradas, Linhas, Destinos e Administracao/
   │  ├─ Documentacao/           ajustes do documento OpenAPI (descrição, chave, campos obrigatórios)
   │  ├─ Domain/                 entidades, objetos de valor, enums, exceções e regras
   │  ├─ Application/            services, interfaces, DTOs, validação, avisos e mapeamentos
   │  ├─ Infrastructure/         Persistence/ (EF Core), Repositories/ (LINQ) e Providers/ (previsões)
   │  ├─ Security/               chave das rotas administrativas
   │  └─ Middleware/             ProblemDetails, tratamento global de erros, JSON e CORS
   └─ tests/PontoVoz.Tests/      Domain/, Application/, Infrastructure/ e Api/
```

Cada camada tem um README próprio: [`Domain`](src/PontoVoz.Api/Domain/README.md),
[`Application`](src/PontoVoz.Api/Application/README.md),
[`Infrastructure`](src/PontoVoz.Api/Infrastructure/README.md),
[`Security`](src/PontoVoz.Api/Security/README.md) e
[`Middleware`](src/PontoVoz.Api/Middleware/README.md).

**Injeção de dependência:** o `Program.cs` registra tudo no contêiner do ASP.NET Core:
`AddWebApiPontoVoz`, `AddDocumentacaoPontoVoz`, `AddSingleton(TimeProvider.System)`,
`AddPersistenciaPontoVoz` (DbContext e repositórios, *scoped*), `AddProvedoresPrevisaoPontoVoz`,
`AddAplicacaoPontoVoz` (services, *scoped*) e `AddSegurancaAdministrativa`. As classes recebem as
dependências pelo construtor, por exemplo
`AlternativasService(IParadaRepository, ILinhaRepository, IDestinoRepository, IProvedorPrevisaoChegada, TimeProvider)`.
O teste `InjecaoDependenciaTests` confirma que o contêiner resolve os services e os repositórios.

## Domínio e POO

O domínio fica em `src/PontoVoz.Api/Domain/` e segue o contrato
[`../contracts/api-v1.md`](../contracts/api-v1.md).

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
| `Destino` | Destino consultável. Só do seed. |
| `PercursoEstimado` | Minutos de uma linha, a partir de uma parada que ela atende, até um destino |
| `RegistroPassagem` (abstrata) | Passagem histórica de uma linha pela parada |
| `PassagemSimulada` | Observação de demonstração do provisionamento inicial, com `ChaveSeed` |
| `PassagemRegistroManual` | Observação informada por um operador, com `OrigemRegistro` |
| `Coordenada`, `ReferenciaLinha` | Objetos de valor imutáveis, comparados por valor |
| `PrevisaoChegada`, `ResultadoProvedor` | Previsão de chegada e resultado do provedor. Nunca são passagens. |
| `AlternativaDeslocamento` | Resultado calculado (espera + percurso). Não é persistido. |
| `RegraTemporalPassagem` | Regra da seção 4.6.3 do contrato, compartilhada por entidades e services |

### Conceitos de POO

| Conceito | Onde |
|---|---|
| Encapsulamento | As propriedades das entidades têm `private set`. O estado muda só por métodos que preservam as invariantes (`Parada.AtualizarDados`, `AssociarLinha`, `Ativar`/`Desativar`). A coleção de linhas é exposta como somente leitura. |
| Abstração | `EntidadeBase` e `RegistroPassagem` são abstratas. `DominioException` abstrai as exceções que o tratamento global converte em ProblemDetails. |
| Herança | `Parada`, `Linha`, `Destino` e `RegistroPassagem` herdam de `EntidadeBase`. `PassagemSimulada` e `PassagemRegistroManual` herdam de `RegistroPassagem`. `AdministracaoControllerBase` é a base dos controllers administrativos. |
| Polimorfismo | `RegistroPassagem` declara `abstract NivelConfirmacao`, `CodigoAviso` e `DadosSimulados`, e cada subclasse faz `override`; os services tratam todas como `RegistroPassagem`. `IProvedorPrevisaoChegada` tem três implementações (`ProvedorPrevisaoSimulado`, `ProvedorOlhoVivo` e `SeletorProvedorPrevisao`). |
| Interfaces | Repositórios (`IParadaRepository`, `ILinhaRepository`, `IDestinoRepository`, `IRegistroPassagemRepository`, `ICadastro*Repository`), `IUnidadeTrabalho`, services e o provedor de previsões, em `Application/Interfaces/`. |
| Composição | `Parada` compõe `ParadaLinha` e `Coordenada`. `PercursoEstimado` e `RegistroPassagem` são criados a partir de uma `ParadaLinha`. `PrevisaoChegada` e `AlternativaDeslocamento` compõem `ReferenciaLinha`. |
| Métodos de fábrica | `CriarSimulada`, `CadastrarAdministrativa`, `CriarNoProvisionamento` e `Registrar` deixam explícitas a origem e a natureza de cada dado. Não há construtor público. |
| Responsabilidade única | As regras temporais ficam em `RegraTemporalPassagem`, e as validações de cadastro em `RegrasCadastro`. As entidades as usam sem duplicá-las. |

### Invariantes

- **Formatos:** as expressões regulares usam `\A` e `\z`, que exigem a entrada inteira (com `^` e
  `$`, o .NET aceitaria um valor seguido de quebra de linha): `\A[A-Z0-9-]{3,30}\z` para códigos,
  `\A[A-Za-z0-9-]{1,20}\z` para prefixo de veículo, `\A[a-z0-9-]{3,40}\z` para `origemRegistro` e
  `\ASIM-[A-Z0-9-]{3,36}\z` para `ChaveSeed`.
- **Prefixo `SIM-` em códigos e letreiros:** obrigatório na origem `Simulado` e proibido nos
  cadastros administrativos, inclusive nos fictícios, que usam outro prefixo, como `TESTE-`.
- **Prefixo `SIM-` em veículos:** depende da natureza do registro. Um registro fictício identifica
  o veículo com `SIM-`; um registro com dados reais não pode usá-lo. O prefixo é opcional.
- **Textos:** nome de 3 a 120 caracteres (sem os espaços das pontas), letreiro de 1 a 30,
  terminais de 3 a 120, endereço até 200 (vazio vira `null`).
- **Coordenada:** latitude de -90 a 90 e longitude de -180 a 180, sem `NaN`.
- **Natureza do dado:** a origem `Simulado` sempre tem `DadosSimulados = true`. Num cadastro
  administrativo, o operador declara a natureza, e o domínio não presume nenhum valor. Código,
  origem e `DadosSimulados` não mudam depois da criação. Uma parada fictícia não tem endereço nem
  coordenada.
- **Associações:** sem duplicidade, porque `AssociarLinha` é idempotente. Parada inativa recusa
  com `ParadaInativa` e linha inativa com `LinhaInativa`.
- **Percurso:** de 1 a 240 minutos e criado a partir de uma `ParadaLinha`; não existe percurso de
  uma linha que não atende a parada.
- **Passagens:** horários definidos na criação, sem método que os altere. O registro manual aceita
  `observadaEm` de 24 horas antes a 2 minutos depois do instante do registro e exige
  `origemRegistro` no formato técnico, além de parada e linha ativas e associadas. Nenhum registro
  é verificado nem oficial.
- **Regra temporal:** minutos inteiros arredondados para baixo e nunca negativos. A passagem fica
  desatualizada quando o intervalo exato passa de 60 minutos. Um registro com horário futuro ainda
  não conta como passagem ocorrida (`JaOcorreu`).
- **Previsões vencidas:** `ResultadoProvedor.Respondido` descarta previsões anteriores à referência
  antes de calcular o status, e `AlternativaDeslocamento.Calcular` trata uma previsão vencida como
  ausente. Uma previsão exatamente no instante da consulta é válida, com espera 0.
- **Erros:** `ValidacaoDominioException` corresponde a 400 `RequisicaoInvalida`, com o campo do
  contrato, e `RegraNegocioException` corresponde a 422, com o código do contrato.

## Web API

Os controllers recebem os parâmetros, repassam o `CancellationToken` da requisição e devolvem o
DTO; validação, regras e erros ficam nos services e no tratamento global.

| Método | Rota | Parâmetros | Respostas |
|---|---|---|---|
| GET | `/health` | — | 200, 503 |
| GET | `/api/v1/paradas` | `termo`, `codigoExterno`, `pagina`, `tamanhoPagina` | 200, 400 |
| GET | `/api/v1/paradas/{id}` | `id` na rota | 200, 404 |
| GET | `/api/v1/linhas` | `termo`, `pagina`, `tamanhoPagina` | 200, 400 |
| GET | `/api/v1/destinos` | `termo`, `pagina`, `tamanhoPagina` | 200, 400 |
| GET | `/api/v1/paradas/{id}/previsoes` | `id` na rota, `linhaId` opcional | 200, 400, 404, 422, 503 |
| GET | `/api/v1/paradas/{id}/ultimas-passagens` | `id` na rota, `linhaId` opcional | 200, 400, 404, 422 |
| GET | `/api/v1/paradas/{id}/alternativas` | `id` na rota, `destinoId` **obrigatório** | 200, 400, 404 |
| GET | `/api/v1/paradas/{id}/resumo` | `id` na rota | 200, 404 |
| POST | `/api/v1/admin/paradas` | corpo JSON | **201** com `Location`, 400, 401, 403, 409 `CodigoJaCadastrado` |
| PUT | `/api/v1/admin/paradas/{id}` | corpo JSON | 200, 400, 401, 403, 404 |
| POST | `/api/v1/admin/linhas` | corpo JSON | **201**, 400, 401, 403, 409 |
| PUT | `/api/v1/admin/paradas/{id}/linhas/{linhaId}` | — | **204** (idempotente), 401, 403, 404, 422 |
| POST | `/api/v1/admin/observacoes-passagem` | corpo JSON | **201**, 400, 401, 403, 404, 422 |

Todas podem devolver 500 `ErroInterno`. Paginação: `pagina` ≥ 1 (padrão 1) e `tamanhoPagina` de 1
a 50 (padrão 20). `termo` tem de 2 a 60 caracteres e não diferencia maiúsculas nem acentos. Não há
`DELETE`: a parada é desativada pelo `PUT` com `"ativa": false` (exclusão lógica), some das
consultas públicas e mantém o histórico de passagens. As rotas e os campos completos estão no
contrato; os DTOs são *records* em `Application/Dtos/`.

### JSON e erros

As respostas usam o `JsonContrato`: camelCase, enums como texto, campos sem valor como `null`
(nunca omitidos), listas vazias como `[]` e horários de negócio em `-03:00`. Todo erro sai em
`application/problem+json`, com `type`, `title`, `status`, `instance`, `traceId` e `codigo`
(contrato, seção 7):

| Situação | Status | `codigo` |
|---|---|---|
| Parâmetro ou corpo fora do formato ou da faixa, inclusive valor que não é número (`pagina=abc`) e JSON inválido | 400 | `RequisicaoInvalida`, com as mensagens em `errors` |
| Chave administrativa ausente ou errada | 401 | `ChaveAdministrativaInvalida` |
| Rotas administrativas sem chave configurada fora de Development | 403 | `OperacoesAdministrativasDesabilitadas` |
| Parada, linha ou destino inexistente ou inativo | 404 | `ParadaNaoEncontrada`, `LinhaNaoEncontrada`, `DestinoNaoEncontrado` |
| Caminho sem rota correspondente (`/api/v1/paradas/abc`, `/api/v1/paradas/0`) | 404 | `RotaNaoEncontrada` |
| Método não aceito pela rota | 405 | `MetodoNaoPermitido`, com o cabeçalho `Allow` |
| Código já cadastrado | 409 | `CodigoJaCadastrado` |
| Linha que não atende a parada, parada ou linha inativa | 422 | `LinhaNaoAtendeParada`, `ParadaInativa`, `LinhaInativa` |
| Provedor de previsões indisponível (só em `/previsoes`) | 503 | `ProvedorPrevisoesIndisponivel`, com `Retry-After: 30` |
| Erro inesperado | 500 | `ErroInterno`, com mensagem genérica; o erro real fica só no log |

A ordem das verificações é a do contrato: parâmetros (400), recursos (404), estado (422) e só então
a consulta ao provedor ou a gravação.

### Últimas passagens

`UltimasPassagensService` lê o instante do `TimeProvider` uma vez; o mesmo instante vale para o
filtro de registros futuros, os minutos, a desatualização e o `consultadoEm`. Os minutos são
arredondados para baixo, `desatualizada` só acima de 60 minutos exatos, e o nível de confirmação, a
origem do registro e o aviso vêm do tipo concreto da passagem; `verificada` e `oficialSptrans` saem
sempre `false`. Linhas sem passagem vão para `linhasSemRegistro`.

### Previsões de chegada

Uma previsão é uma **estimativa** (`nivelConfirmacao: "Previsao"`): não é uma passagem observada e
nunca aparece em `ultimas-passagens`.

| Situação | Status | Corpo |
|---|---|---|
| Previsão para todas as linhas | 200 | `status: "Disponivel"`, `linhasSemPrevisao: []` |
| Previsão para parte das linhas | 200 | `status: "Disponivel"`, `linhasSemPrevisao` preenchida, aviso `PrevisaoParcial` |
| Provedor sem previsões | 200 | `status: "SemPrevisoes"`, `previsoes: []`, aviso `SemPrevisoes` |
| Parada sem linhas | 200 | listas vazias e aviso `ParadaSemLinhas`, sem consultar o provedor |
| Provedor indisponível | **503** | `ProvedorPrevisoesIndisponivel`, com `Retry-After: 30` |

`previsoes` sai ordenada por `previstaPara` e `codigoLinha`; `esperaMinutos` é arredondado para
baixo e nunca é negativo.

| Componente | Papel |
|---|---|
| `IProvedorPrevisaoChegada` (Application) | Interface do provedor. Uma falha volta como `ResultadoProvedor.Indisponivel`, não como exceção, para que o resumo e as alternativas decidam o que fazer |
| `ProvedorPrevisaoSimulado` | **Padrão.** Calcula as chegadas das linhas `SIM-` no instante da consulta, sem gravar nada (regra em [`../docs/dados.md`](../docs/dados.md)) |
| `ProvedorOlhoVivo` | Adaptador HTTP da API Olho Vivo v2.1. **Desligado** e testado só com respostas simuladas |
| `SeletorProvedorPrevisao` | Decide por configuração quem responde. Dados `SIM-` ficam sempre no simulado |
| `PrevisoesService` | Valida, consulta o provedor uma vez com o instante do `TimeProvider` e monta a resposta |

### Alternativas de deslocamento

`GET /api/v1/paradas/{id}/alternativas?destinoId=` compara as **linhas diretas** da parada até um
destino. Uma linha entra quando está ativa, atende a parada e tem um **percurso estimado
cadastrado** até o destino; nada é calculado por distância, e não há baldeação. Para cada linha,
com a próxima chegada válida do provedor (`AlternativaDeslocamento.Calcular`):

```
tempoTotalEstimadoMinutos = esperaMinutos + percursoEstimadoMinutos
chegadaEstimadaAoDestino  = previstaPara + percursoEstimadoMinutos
```

- **Ranking** (`AlternativasService.Ordenar`): primeiro as linhas com previsão, pelo menor tempo
  total; em empate, a menor espera e, depois, o código da linha. Recebem `posicao` 1, 2, 3...
- **Sem previsão:** vêm depois, por percurso e código, com `posicao`, `previstaPara`,
  `esperaMinutos`, `tempoTotalEstimadoMinutos` e `chegadaEstimadaAoDestino` nulos.
- **`comparacaoCompleta`:** só é `true` quando todas as linhas foram comparadas; se faltar alguma,
  a posição 1 é a mais rápida **entre as linhas com previsão** (aviso `ComparacaoIncompleta`).

| `situacao` | Quando | HTTP |
|---|---|---|
| `Comparadas` | Ao menos uma linha com previsão | 200 |
| `SemComparacao` | Nenhuma linha com previsão, inclusive com o provedor indisponível (`statusPrevisao: "ProvedorIndisponivel"`) | 200 |
| `SemLinhaDireta` | Nenhum percurso cadastrado até o destino; o provedor não é consultado | 200 |

No exemplo do contrato (10/11/2026, 14h32), a SIM-L002 chega em 1 minuto, mas leva 40 até o
Terminal Norte (41 no total); a SIM-L004 chega em 8 e leva 20 (28 no total) e fica em 1º lugar. A
SIM-L001 não tem previsão e aparece por último, sem posição.

### Resumo da parada

`GET /api/v1/paradas/{id}/resumo` reúne a parada, as previsões e as últimas passagens, cada parte
no mesmo formato da rota individual e montada pelos mesmos métodos (`PrevisoesService.Montar` e
`UltimasPassagensService.Montar`). Com o provedor **indisponível**, a resposta continua 200:
`situacao: "Parcial"`, `partesIndisponiveis: ["previsoes"]`, aviso `ResumoParcial` e, dentro de
`previsoes`, `status: "ProvedorIndisponivel"`. Previsão parcial ou provedor sem previsões continuam
`Completo`; uma falha do banco continua sendo 500.

### Rotas administrativas

Usadas pelo operador para cadastrar paradas e linhas, associá-las e registrar uma passagem
observada. O que gravam aparece em seguida nas consultas públicas.

- Campos obrigatórios ausentes, inclusive os booleanos `dadosSimulados` e `ativa` e o enum
  `sentido`, devolvem 400 "O campo X é obrigatório."; a API não presume `false` nem o primeiro valor
  de um enum. Todos os erros do corpo vêm juntos em `errors`.
- Os formatos vêm das regras do domínio (`RegrasCadastro`, `Coordenada`), chamadas pelo service
  antes de qualquer acesso ao banco. Um cadastro fictício (`dadosSimulados: true`) não pode ter
  endereço nem coordenada.
- **Código repetido:** o service consulta antes de gravar, e o índice único do SQLite cobre duas
  gravações simultâneas; a violação de unicidade vira 409, e outras falhas do banco continuam 500.
- **Associação:** idempotente; a chave composta `(ParadaId, LinhaId)` impede duplicatas mesmo com
  duas requisições simultâneas.
- **Registro manual:** `observadaEm` precisa ter fuso e estar entre 24 horas antes e 2 minutos
  depois do instante do servidor. O registro sai como `RegistroManual`, com `verificada: false` e
  `oficialSptrans: false`; um horário ainda no futuro só aparece em `ultimas-passagens` quando
  chegar.

## Persistência e LINQ

A persistência fica em `src/PontoVoz.Api/Infrastructure/Persistence/`.

| Arquivo | Papel |
|---|---|
| `PontoVozDbContext.cs` | `DbSet` de cada entidade, convenções comuns e o recálculo da coluna de busca no `SaveChanges` |
| `Configuracoes/` | Um mapeamento Fluent API por entidade (`IEntityTypeConfiguration`) |
| `Conversores/InstanteUtcParaTicksConverter.cs` | Conversão de `DateTimeOffset` para ticks UTC |
| `Seed/DadosDemonstracao.cs` e `Seed/ProvisionamentoDemonstracao.cs` | Dados fictícios montados com as fábricas do domínio e gravados uma vez |
| `Busca/` | Normalização do texto e preenchimento da coluna `TextoBusca` |
| `PersistenciaExtensions.cs` | Registro do contexto e dos repositórios, configuração do SQLite e migrations em Development |
| `Migrations/` | `InitialCreate` e `AddSearchColumns`, com o snapshot do modelo |

| Tabela | Chave | Relações e restrições |
|---|---|---|
| `Paradas` | `Id` | `CodigoExterno` único; `Latitude` e `Longitude` anuláveis (objeto de valor `Coordenada`) |
| `Linhas` | `Id` | `Codigo` único |
| `ParadasLinhas` | (`ParadaId`, `LinhaId`) | FK para `Paradas` e `Linhas` |
| `Destinos` | `Id` | `Nome` único |
| `PercursosEstimados` | (`ParadaId`, `LinhaId`, `DestinoId`) | FK composta para `ParadasLinhas` (a linha precisa atender a parada) e FK para `Destinos`; `CHECK` de 1 a 240 minutos |
| `RegistrosPassagem` | `Id` | Hierarquia TPH com `TipoPassagem` (`Simulada` ou `RegistroManual`); FK composta para `ParadasLinhas`; `ChaveSeed` única quando não nula; índice (`ParadaId`, `LinhaId`, `ObservadaEmUtc`) |

- **Exclusões:** todas as chaves estrangeiras usam `RESTRICT`; os cadastros são desativados, não
  excluídos, e nenhuma passagem é apagada em cascata.
- **TPH:** a subclasse correta é reconstruída ao ler do banco; `NivelConfirmacao`, `CodigoAviso`,
  `Verificada` e `OficialSptrans` são calculados pelo tipo e não viram colunas.
- **Instantes em UTC:** todo `DateTimeOffset` é gravado como ticks UTC (`INTEGER`), porque o
  provedor SQLite do EF Core não traduz `ORDER BY` nem comparações sobre `DateTimeOffset`; com
  inteiros, ordenação e filtro rodam no banco, com precisão de 100 ns.
- **Seed:** no primeiro provisionamento de um banco vazio, cria 21 registros coerentes com
  `contracts/exemplos/` (2 paradas, 4 linhas, 5 associações, 3 destinos, 5 percursos e 2 passagens
  simuladas, 15 e 25 minutos antes do instante do provisionamento). Num banco já provisionado, não
  faz nada. Os ids são gerados pelo banco e não são estáveis entre bancos; os dados são
  identificados pelos códigos (`SIM-P001`, `SIM-L004`...).

### Consultas LINQ

As consultas ficam atrás de interfaces em `Application/Interfaces/`, implementadas com EF Core e
LINQ em `Infrastructure/Repositories/`. Elas devolvem entidades do domínio ou `Pagina<T>`, nunca
`IQueryable`.

| Interface | Métodos | Uso |
|---|---|---|
| `IParadaRepository` | `BuscarAtivasAsync(termo, codigoExterno, paginacao)` · `ObterAtivaAsync(id)` | `GET /paradas`, `GET /paradas/{id}` e o cabeçalho das respostas da parada |
| `ILinhaRepository` | `BuscarAtivasAsync(termo, paginacao)` · `ObterAtivaAsync(id)` · `ListarAtivasDaParadaAsync(paradaId)` · `AtendeParadaAsync(paradaId, linhaId)` | `GET /linhas`, linhas da parada, 404 e 422 |
| `IDestinoRepository` | `BuscarAtivosAsync(termo, paginacao)` · `ObterAtivoAsync(id)` · `ListarPercursosDiretosAsync(paradaId, destinoId)` | `GET /destinos` e as alternativas |
| `IRegistroPassagemRepository` | `ListarUltimasPorLinhaAsync(paradaId, referencia, linhaId)` | Últimas passagens e resumo |

- **Cadastros ativos:** as consultas públicas filtram `Ativa`/`Ativo` (contrato, D10).
- **Ordenação fixa com desempate:** paradas e destinos por nome e id; linhas por código e id;
  percursos por minutos e código da linha; passagens da mais recente para a mais antiga.
- **Paginação no banco:** `COUNT(*)` e depois `ORDER BY ... LIMIT ... OFFSET` (`ConsultaExtensions.PaginarAsync`).
- **Somente leitura:** as consultas usam `AsNoTracking`.

Busca de paradas (`ParadaRepository`):

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

Última passagem por linha (`RegistroPassagemRepository.ListarUltimasPorLinhaAsync`), traduzida
para um único comando SQL com `GROUP BY "LinhaId"` e uma subconsulta por grupo:

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

**Busca sem acentos (requisito R7):** o `LIKE` do SQLite não ignora acentos. `TextoBusca.Normalizar`
decompõe o texto (FormD), retira os sinais diacríticos, passa para minúsculas e reduz os espaços; o
mesmo formato é gravado na coluna `TextoBusca` de cada tabela pesquisável (propriedade sombra do EF
Core, recalculada no `SaveChanges`) e usado no termo. A consulta usa `LIKE @padrao ESCAPE '\'`, com
`%`, `_` e `\` escapados. Um `LIKE '%termo%'` não usa índice (`EXPLAIN QUERY PLAN` mostra
`SCAN Paradas`), o que é adequado ao volume da demonstração; a busca por `codigoExterno` usa o
índice único. O SQL gerado para cada consulta está em
[`docs/evidencias/B4-consultas-linq.md`](docs/evidencias/B4-consultas-linq.md).

## Async/Await

- **I/O assíncrono:** as consultas e gravações usam `ToListAsync`, `FirstOrDefaultAsync`,
  `CountAsync`, `AnyAsync` e `SaveChangesAsync` (`UnidadeTrabalhoEf`); os services aguardam cada
  operação e o adaptador Olho Vivo usa `HttpClient` de forma assíncrona. Nenhuma operação usa
  `Task.Run`, `.Result` ou `.Wait()`.
- **`CancellationToken`:** o token da requisição vai do controller ao service, ao repositório e ao
  EF Core. Uma requisição cancelada pelo cliente é interrompida e não vira 500 (testes de
  cancelamento em `InfraestruturaHttpTests`, `PrevisoesHttpTests` e `AlternativasResumoHttpTests`).
- **`TimeProvider`:** os services leem o relógio uma vez por requisição (`relogio.GetUtcNow()`), e o
  mesmo instante vale para toda a resposta. Nos testes, `FakeTimeProvider` fixa o relógio.
- **`Task.WhenAll` no resumo:** `ResumoParadaService.ObterAsync` lê a parada e as linhas em
  sequência e depois executa juntos o provedor de previsões e a consulta de últimas passagens. A
  consulta de passagens é a única operação no `DbContext` nesse momento e o provedor não usa o
  banco, então nenhuma operação do EF Core roda em paralelo com outra (o `DbContext` não aceita
  duas). O teste `ProvedorEPassagensEmParalelo_SemDuasOperacoesDeBancoAoMesmoTempo` confirma no
  máximo uma operação de banco por vez.
- **Sem `Task.WhenAll` nas alternativas:** o provedor depende das linhas que vêm da consulta de
  percursos e não é consultado quando não há linha direta.

## Segurança

**Chave administrativa:** cabeçalho `X-PontoVoz-Admin-Key`, comparado em tempo constante com
`Admin:Chave` (`ChaveAdministrativaFilter`, aplicado em `AdministracaoControllerBase`).

| Ambiente | `Admin:Chave` configurada? | Resultado |
|---|---|---|
| Development | não | Permitido, com aviso no log a cada inicialização |
| Development | sim | Exige o cabeçalho; sem ele ou errado: **401** `ChaveAdministrativaInvalida` |
| Outros | não | **403** `OperacoesAdministrativasDesabilitadas` |
| Outros | sim | Exige o cabeçalho; sem ele ou errado: **401** `ChaveAdministrativaInvalida` |

- A verificação acontece antes da leitura do corpo: sem autorização, a resposta é 401 ou 403
  mesmo com um JSON inválido. O 401 traz `WWW-Authenticate: ApiKey header="X-PontoVoz-Admin-Key"`.
- O valor da chave nunca aparece em respostas nem em logs.
- Para configurar a chave em Development, use o user-secrets (fora do repositório), a partir de
  `backend/`, trocando o marcador por um valor seu:

  ```bash
  dotnet user-secrets set "Admin:Chave" "<CHAVE_ADMINISTRATIVA_LOCAL>" --project src/PontoVoz.Api
  ```

  Em outro ambiente, use a variável de ambiente `Admin__Chave`. Nunca coloque a chave em
  `appsettings*.json`, no código, nos testes ou nas evidências.
- No Scalar, as rotas estão no grupo **Administração**, e a chave vai no campo de autenticação
  `ChaveAdministrativa`. Para testes manuais, use um banco separado, por exemplo
  `$env:ConnectionStrings__PontoVoz = "Data Source=C:\temp\pontovoz-teste.db"`, para não misturar
  cadastros de teste com o `pontovoz.db`.

**Outras proteções:**

- **CORS:** a lista `Cors:OrigensPermitidas` vem vazia, e então nenhuma origem externa é liberada;
  as origens listadas podem fazer apenas `GET`, sem credenciais.
- **Erros 500:** mensagem genérica, sem detalhes internos; o erro real fica só no log.
- **Entrada e SQL:** parâmetros e corpos são validados antes do banco, e as consultas usam LINQ
  parametrizado pelo EF Core, sem SQL bruto.
- **Rede:** a API escuta só em `localhost`.
- **Segredos:** nenhuma chave, token ou senha nos arquivos versionados; o token da Olho Vivo não
  aparece em logs, porque o registro automático de requisições do `HttpClient` foi desligado para
  esse cliente.

## Testes

A partir da raiz do repositório ou de `backend/`:

```bash
dotnet test
```

São **816 testes** em xUnit v3, sobre a Microsoft.Testing.Platform: 278 de domínio, 144 da camada
de aplicação, 172 de integração e de provedores e 222 pela API em execução. Os testes de integração
usam SQLite real e isolado por teste (em memória ou em arquivo temporário), com o esquema criado
pelas migrations; os testes HTTP usam a API real em memória (`WebApplicationFactory`), com o relógio
parado no instante dos exemplos do contrato (10/11/2026, 14h32) e as respostas comparadas campo a
campo com os JSON de `contracts/`, sem os ids internos. Nenhum teste usa o `pontovoz.db`, acessa a
SPTrans ou depende do relógio real para as regras de negócio.

| Grupo | Classe | Testes | O que cobre |
|---|---|---|---|
| Domínio | `ParadaTests` | 44 | Código, nome, endereço, natureza do dado, atualização, ativação, associações |
| | `LinhaTests` | 38 | Código, letreiro, prefixo `SIM-`, terminais, sentido, encapsulamento |
| | `RegistroPassagemTests` | 40 | Polimorfismo, passagem simulada, registro manual, horário futuro, 422 |
| | `RegraTemporalPassagemTests` | 32 | Casos J1 a J5 e T1 a T7 do contrato e valores extremos |
| | `FormatoEntradaTests` | 34 | Quebras de linha e caracteres fora do formato |
| | `PrefixoVeiculoTests` | 25 | Prefixo `SIM-` de veículo por natureza do registro |
| | `PrevisaoEAlternativaTests` · `PrevisaoVencidaTests` | 20 · 19 | Previsão, resultado do provedor, cálculo da alternativa, previsões vencidas |
| | `DestinoEPercursoTests` · `CoordenadaTests` | 14 · 12 | Destino, percurso de 1 a 240 minutos, coordenada |
| Aplicação | `ServicosConsultaTests` · `ServicosIntegracaoTests` · `PaginacaoTests` | 29 · 9 · 15 | Validação, erros, mapeamento, respostas do banco real iguais aos exemplos, paginação |
| | `UltimasPassagensServiceTests` · `PrevisoesServiceTests` | 17 · 19 | Ordem 400 → 404 → 422, estados, avisos, 503, instante único |
| | `AlternativasServiceTests` · `ResumoParadaServiceTests` | 20 · 12 | Ranking, desempates, linhas sem previsão, `SemLinhaDireta`, resumo parcial, paralelismo sem duas operações de banco |
| | `AdministracaoServicesTests` | 23 | Validação antes de ler ou gravar, erros esperados, `CancellationToken` |
| Infraestrutura | `RepositoriosConsultaTests` · `UltimasPassagensTests` · `TextoBuscaTests` | 34 · 9 · 19 | Consultas LINQ no SQLite, paginação no SQL, busca sem acentos, cancelamento |
| | `PersistenciaCadastrosTests` · `PersistenciaPassagensTests` · `AuditoriaPersistenciaTests` | 13 · 10 · 4 | Mapeamentos, TPH, chaves e restrições |
| | `EsquemaMigrationTests` · `MigracaoBuscaTests` · `ProvisionamentoDemonstracaoTests` | 4 · 4 · 9 | Esquema das migrations, migração da busca, seed idempotente |
| | `UnidadeTrabalhoEfTests` | 12 | Gravações simultâneas, 409 e falhas que continuam 500 |
| | `ProvedorPrevisaoSimuladoTests` · `ProvedorOlhoVivoTests` | 22 · 32 | Grade simulada em datas variadas; adaptador Olho Vivo com HTTP falso, token fora dos logs |
| API | `ConsultasPublicasHttpTests` · `PrevisoesHttpTests` · `AlternativasResumoHttpTests` | 44 · 21 · 19 | Respostas iguais aos exemplos do contrato, erros, indisponibilidade, cancelamento |
| | `AdministracaoAutorizacaoTests` · `AdministracaoCadastrosTests` · `ObservacoesPassagemHttpTests` · `AdministracaoFluxoCompletoTests` | 26 · 42 · 36 · 1 | Matriz de chave, 401, 403, 409, cadastros, janela temporal, fluxo do operador até a consulta pública |
| | `InfraestruturaHttpTests` · `HealthEndpointTests` · `InjecaoDependenciaTests` · `MigracaoAutomaticaTests` | 18 · 3 · 6 · 6 | 500 genérico, CORS, OpenAPI igual ao exportado, Scalar, atalhos da documentação, `/health`, contêiner de DI, banco criado em Development |

Filtros úteis:

```bash
dotnet test --filter-namespace "PontoVoz.Tests.Infrastructure"
```

```bash
dotnet test --filter-class "*Alternativas*"
```

```bash
dotnet test --report-xunit-trx --results-directory TestResults
```

O último comando gera um relatório TRX com o resultado de cada caso. Na validação final
([`docs/evidencias/B11-validacao.md`](docs/evidencias/B11-validacao.md)), uma execução teve 1
falha em 816 que não pôde ser identificada (a saída não guardou o nome do teste); depois dela, a
suíte passou em 28 execuções completas seguidas, inclusive duas com relatório TRX. Os fatores de
risco encontrados (esperas reais curtas no teste de paralelismo do resumo, testes de cancelamento
que esperam um log, limpeza global do pool de conexões do SQLite) estão registrados nessa
evidência como hipóteses.

## Dados simulados e limitações

- **Dados de demonstração:** o seed e as previsões são fictícios. Os registros do seed usam o
  prefixo `SIM-` e saem com `dadosSimulados: true`, `origem: "Simulado"` e o aviso
  `DadosSimulados`; nenhum deles representa uma parada ou linha real. A origem de cada dado está em
  [`../docs/dados.md`](../docs/dados.md).
- **Passagem recente:** as passagens do seed mantêm o horário do provisionamento e ficam
  desatualizadas com o tempo; uma passagem recente vem do registro manual pela rota administrativa.
- **Previsões:** vêm do provedor simulado. O adaptador Olho Vivo está desligado e nunca foi
  validado com credenciais oficiais da SPTrans.
- **Alternativas:** só linhas diretas com percurso estimado cadastrado (simulado), sem baldeação nem
  trânsito em tempo real.
- **`origemRegistro`:** o padrão `^[a-z0-9-]{3,40}$` recusa nomes com espaço ou maiúscula, e-mails e
  telefones formatados, mas não impede toda informação pessoal (uma sequência só de dígitos passa).
  Use apenas rótulos técnicos, como `operador-local`.
- **Chave administrativa:** única e compartilhada, sem usuários, auditoria nem limite de
  requisições; em Development sem chave, as rotas ficam abertas para a demonstração local, o que não
  é adequado para uma API exposta.
- **Ambiente:** testado no Windows 11 com o .NET SDK 10.0.403; Linux, macOS e o Visual Studio 2026
  não foram testados.
