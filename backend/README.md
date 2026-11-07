# PontoVoz — Backend C# (ASP.NET Core / .NET 10)

Backend da Global Solution FIAP 2026.2 (CitizenTech) para a disciplina
**C# Software Development**. Visão geral do projeto: [../README.md](../README.md).

## Estágio atual

**Bloco B2 — domínio e testes de domínio.** Hoje o backend:

- inicia localmente e responde em `GET /health`, que verifica **apenas se o processo está
  ativo** (ainda não há banco de dados para verificar);
- publica o documento OpenAPI e a interface Scalar em Development;
- tem o **modelo de domínio** do PontoVoz em `src/PontoVoz.Api/Domain/` (entidades, objetos de
  valor, enums, exceções e a regra temporal das passagens), coberto por **278 testes
  unitários** (veja [Domínio](#domínio-b2)).

Ainda **não** existem `DbContext`, migrations, banco de dados, repositórios, services nem
endpoints de transporte público. Os endpoints de `/api/v1` descritos em
[`../contracts/api-v1.md`](../contracts/api-v1.md) são **planejados**. Os pacotes do EF Core já
estão instalados para o B3.

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
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.14 | EF Core com SQLite (uso a partir do B3) |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.14 | Suporte às migrations (uso a partir do B3) |
| `dotnet-ef` (ferramenta **local**) | 10.0.14 | Comandos de migrations, declarado em `.config/dotnet-tools.json` |
| `xunit.v3` | 4.0.2 | Testes automatizados (o projeto de testes é um executável, como exige o xUnit v3) |
| `xunit.runner.visualstudio` e `Microsoft.NET.Test.Sdk` | 4.0.1 e 17.14.1 | Compatibilidade com o Test Explorer das IDEs |
| `Microsoft.Extensions.TimeProvider.Testing` | 10.11.0 | `FakeTimeProvider`, relógio controlado nos testes das regras temporais (só no projeto de testes) |

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

```bash
dotnet run --project src/PontoVoz.Api --launch-profile http
```

A API fica disponível em `http://localhost:5080` (somente localhost).

### Endereços em Development

| Recurso | Endereço |
|---|---|
| Saúde do processo | http://localhost:5080/health |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Interface Scalar | http://localhost:5080/scalar |

Resposta real de `GET /health`, registrada nas evidências do B0:

```json
{"status":"Healthy","escopo":"processo","verificadoEm":"2026-11-07T01:39:15.6929158+00:00","verificacoes":[]}
```

`escopo: "processo"` e `verificacoes` vazia indicam que só o processo da API foi
verificado. A verificação do banco de dados entra quando o banco existir (bloco B3).

### Testes

O projeto usa xUnit v3 sobre a Microsoft.Testing.Platform, declarada em
`global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`), como o
.NET 10 SDK exige para essa plataforma.

Desde o B2, `dotnet test` executa os testes de domínio e termina com código de saída **0**
quando todos passam. Para ver cada teste:

```bash
dotnet test --output detailed
```

Para rodar só os casos J1 a J5 e T1 a T7 da regra temporal do contrato (seção 4.6.3):

```bash
dotnet test --filter-method "*CasosDoContrato"
```

A evidência da execução está em [Domínio](#evidências-dos-testes).

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

Deve mostrar `10.0.14`. Ainda não há migrations para aplicar.

## Estrutura

```
backend/
├─ PontoVoz.slnx                 solução (formato .slnx do .NET 10)
├─ global.json                   versão mínima do SDK e plataforma de testes
├─ .config/dotnet-tools.json     ferramenta local dotnet-ef
├─ .editorconfig                 padrão de estilo do código
├─ docs/evidencias/              evidências de funcionamento por bloco
├─ src/PontoVoz.Api/
│  ├─ Program.cs                 composição da aplicação e injeção de dependência
│  ├─ Controllers/               controllers REST (hoje só HealthController)
│  ├─ Domain/                    modelo de domínio (B2)
│  │  ├─ Entities/               entidades persistentes e associações
│  │  ├─ ValueObjects/           objetos de valor imutáveis
│  │  ├─ Enums/                  enums do contrato v1
│  │  ├─ Exceptions/             exceções de domínio com o código de erro do contrato
│  │  └─ Rules/                  regras compartilhadas (temporal e de cadastro)
│  ├─ Application/               services, interfaces e DTOs (B5)
│  ├─ Infrastructure/            EF Core, repositórios e provedores (B3, B4, B8)
│  ├─ Security/                  proteção das rotas administrativas (B7)
│  └─ Middleware/                tratamento global de erros (B6)
└─ tests/PontoVoz.Tests/         testes xUnit (referencia o projeto da API)
   └─ Domain/                    testes de domínio (B2)
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

### Decisões para o B3 (EF Core e SQLite)

- **Instantes:** o domínio usa `DateTimeOffset` sempre em UTC (`...EmUtc`). O provedor SQLite
  não ordena nem compara `DateTimeOffset` em consultas, então o B3 precisa de um conversor,
  para `DateTime` UTC ou para ticks, garantindo `ORDER BY` e o filtro
  `observadaEm <= consultadoEm`.
- **Hierarquia de passagens:** TPH (uma tabela, com discriminador). `ChaveSeed` e
  `OrigemRegistro` ficam como colunas anuláveis. `NivelConfirmacao`, `CodigoAviso` e
  `Verificada` são calculados e não são mapeados. `DadosSimulados` do registro manual é
  guardado pelo campo `_dadosSimulados`, e `OrigemRegistro` pelo campo `_origemRegistro`;
  os dois exigem configuração explícita do campo de apoio.
- **Chaves compostas:** `ParadaLinha` usa (`ParadaId`, `LinhaId`), e `PercursoEstimado` usa
  (`ParadaId`, `LinhaId`, `DestinoId`), com chave estrangeira composta para `ParadaLinha`.
- **Índices únicos:** `Parada.CodigoExterno`, `Linha.Codigo` e `PassagemSimulada.ChaveSeed`.
- **Coleção:** `Parada.LinhasAssociadas` usa o campo `_linhasAssociadas`.
- **Coordenada:** objeto de valor mapeado como tipo complexo ou *owned*, com as colunas
  anuláveis.
- **Enums:** guardados como texto, para coincidir com o contrato.
- **Busca:** a coluna normalizada (requisito R7 do contrato) entra junto com o mapeamento.
- **Seed:** usa as fábricas `CriarSimulada` e `CriarNoProvisionamento`, só no provisionamento
  de um banco vazio (contrato, seção 4.6.2).

## Próximas etapas

| Bloco | Conteúdo |
|---|---|
| B1 | Contrato v1 em `../contracts/` (concluído) |
| B2 | Entidades e regras de domínio, com testes (concluído, aguardando revisão) |
| B3 | `DbContext`, migration inicial e seed de dados simulados |
| B4 | Repositórios com consultas LINQ, com testes |
| B5 | Services, DTOs e injeção de dependência, com testes |
| B6 | Endpoints públicos, tratamento de erros e exportação do contrato OpenAPI |
| B7 | Endpoints administrativos com chave de acesso |
| B8 | Provedores de previsão (simulado e adaptador HTTP Olho Vivo) |
| B9 | Alternativas de deslocamento e resumo da parada |
| B10 | Documentação final, evidências e entrega |
