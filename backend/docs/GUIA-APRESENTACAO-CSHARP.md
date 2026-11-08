# Guia de apresentação — backend C# do PontoVoz

Guia para estudar e explicar o backend na apresentação da disciplina **C# Software Development**.
Cada conceito aponta um arquivo e uma classe ou método reais. Os caminhos partem de
`backend/src/PontoVoz.Api/`, salvo indicação. O código foi escrito com o Claude Code a partir dos
prompts registrados em [`../../prompts.md`](../../prompts.md); este guia serve para que cada
integrante entenda o que está entregando e consiga demonstrá-lo.

## Roteiro de cinco minutos

| Tempo | O que dizer | O que mostrar |
|---|---|---|
| 0:00–0:45 | **Problema:** no ponto de ônibus de São Paulo, quem não tem smartphone não sabe quando a linha chega nem qual opção é melhor. O PontoVoz é um totem que responde por voz ou tela; este backend é a API que o totem e a IA consultam. | README da raiz |
| 0:45–1:45 | **Arquitetura:** controllers recebem o HTTP, services aplicam as regras, o domínio guarda as invariantes e a infraestrutura fala com o SQLite pelo EF Core. Tudo é ligado por injeção de dependência no `Program.cs`. | `Program.cs` e a pasta `src/PontoVoz.Api/` |
| 1:45–3:15 | **Demonstração:** no Scalar, consultar a parada `SIM-P001`, as previsões, as alternativas até o Terminal Norte e o resumo. Mostrar que a linha mais rápida é a de menor tempo total, não a que chega primeiro, e que tudo aparece como **simulado**. | `http://localhost:5080/scalar` |
| 3:15–4:15 | **Qualidade:** 805 testes (domínio, aplicação, SQLite real e HTTP), erros padronizados em ProblemDetails, chave administrativa, falha do provedor tratada (503 nas previsões, resumo parcial). | `dotnet test` e `B10-validacao-final.md` |
| 4:15–5:00 | **Limitações:** dados de demonstração fictícios, Olho Vivo desligado e não validado, interface do totem e voz em outros componentes. | `docs/dados.md` |

## Conceitos e onde estão no código

### 1. Qual problema o PontoVoz resolve

Informação de transporte acessível no próprio ponto, sem depender de smartphone: previsão de
chegada, última passagem registrada e alternativas até um destino. O backend entrega essas
informações por uma API REST, sempre dizendo de onde vem cada dado (`origem`, `dadosSimulados`,
`avisos`). Veja o [README da raiz](../../README.md).

### 2. Como o backend funciona

Uma requisição percorre: controller → service → repositório ou provedor → domínio → DTO.
Exemplo: `GET /api/v1/paradas/{id}/alternativas` entra em `ParadasController.ObterAlternativas`
(`Controllers/ParadasController.cs`), que chama `IAlternativasService.ObterAsync`. O
`AlternativasService` (`Application/Services/AlternativasService.cs`) valida o `destinoId`, busca
a parada, o destino e os percursos (`IDestinoRepository.ListarPercursosDiretosAsync`), consulta o
provedor de previsões e devolve um `AlternativasDeslocamentoDto`.

### 3. O papel do Domain

`Domain/` guarda as entidades e as regras, sem depender de ASP.NET Core nem de EF Core.
Exemplos: `Entities/Parada.cs` (métodos `CadastrarAdministrativa`, `AtualizarDados`,
`AssociarLinha`), `Rules/RegraTemporalPassagem.cs` (`MinutosDesdeObservacao`,
`EstaDesatualizada`, `JaOcorreu`) e `ValueObjects/AlternativaDeslocamento.cs` (`Calcular`).
Uma regra violada lança `ValidacaoDominioException` (400) ou `RegraNegocioException` (422).

### 4. O papel do Application

`Application/` coordena cada caso de uso: valida os parâmetros (`Validacao/ValidacaoParametros.cs`,
`Validacao/ValidacaoCorpo.cs`), chama os repositórios pelas interfaces (`Interfaces/`), aplica as
regras e monta os DTOs (`Dtos/Dtos.cs`) no formato do contrato. Os avisos ficam em
`Avisos/CatalogoAvisos.cs`.

### 5. O papel do Infrastructure

`Infrastructure/` implementa o acesso a dados e a serviços externos:
`Persistence/PontoVozDbContext.cs` (EF Core), `Persistence/Configuracoes/` (mapeamentos),
`Persistence/Migrations/`, `Persistence/Seed/` (dados fictícios), `Repositories/` (consultas
LINQ) e `Providers/` (provedor simulado, adaptador Olho Vivo e o seletor).

### 6. O papel da API

`Controllers/` recebe o HTTP e devolve o DTO, sem LINQ nem regra de negócio. `Middleware/`
converte erros em ProblemDetails (`TratadorExcecoes.TryHandleAsync`), `Security/` protege as rotas
administrativas e `Documentacao/DocumentacaoOpenApi.cs` ajusta o documento OpenAPI.

### 7. O que é uma interface no projeto

Uma interface é um contrato de métodos sem implementação. Exemplo:
`Application/Interfaces/IPrevisoes.cs` declara `IProvedorPrevisaoChegada` (`ConsultarAsync`,
`OrigemPara`). Três classes a implementam: `ProvedorPrevisaoSimulado`, `ProvedorOlhoVivo` e
`SeletorProvedorPrevisao` (`Infrastructure/Providers/`). O service não sabe qual delas está em uso.
Outras interfaces: `IParadaRepository`, `ILinhaRepository`, `IDestinoRepository`,
`IRegistroPassagemRepository`, `IUnidadeTrabalho` e as dos services.

### 8. Como funciona a injeção de dependência

O `Program.cs` registra tudo no contêiner do ASP.NET Core: `AddPersistenciaPontoVoz`,
`AddProvedoresPrevisaoPontoVoz`, `AddAplicacaoPontoVoz`, `AddSegurancaAdministrativa` e
`AddSingleton(TimeProvider.System)`. Em `Application/AplicacaoExtensions.cs`, por exemplo,
`services.AddScoped<IAlternativasService, AlternativasService>()`. As classes recebem as
dependências pelo construtor primário, como em
`AlternativasService(IParadaRepository paradas, ..., TimeProvider relogio)`. O teste
`InjecaoDependenciaTests` (`tests/PontoVoz.Tests/Api/`) confirma que o contêiner resolve todos os
services.

### 9. Onde existem herança e polimorfismo

- **Herança:** `Parada`, `Linha`, `Destino` e `RegistroPassagem` herdam de `EntidadeBase`
  (`Domain/Entities/EntidadeBase.cs`, abstrata). `PassagemSimulada` e `PassagemRegistroManual`
  herdam de `RegistroPassagem` (abstrata). `AdministracaoControllerBase` é a base abstrata dos
  controllers administrativos e aplica a eles o filtro da chave.
- **Polimorfismo:** `RegistroPassagem` declara `abstract NivelConfirmacao`, `CodigoAviso` e
  `DadosSimulados`; cada subclasse faz `override` (`PassagemSimulada` sempre devolve
  `DadosSimulados => true`). O service de últimas passagens trata todas como `RegistroPassagem`, e
  a resposta sai certa para cada tipo. Teste:
  `RegistroPassagemTests.Hierarquia_CadaSubclasseDefineNivelAvisoEOrigem_ENenhumaEVerificada`.
- **Polimorfismo por interface:** o mesmo `IProvedorPrevisaoChegada` responde pelo provedor
  simulado ou pelo Olho Vivo.

### 10. Como o EF Core se conecta ao SQLite

`Infrastructure/Persistence/PersistenciaExtensions.cs` (`AddPersistenciaPontoVoz` e
`UsarSqlitePontoVoz`) registra o `PontoVozDbContext` com a connection string
`ConnectionStrings:PontoVoz` (`Data Source=pontovoz.db`). O comando
`dotnet ef database update --project src/PontoVoz.Api` aplica as migrations
(`20261107211456_InitialCreate` e `20261107222533_AddSearchColumns`) e roda o seed pelo gancho
`UseSeeding`. Os instantes são gravados como ticks UTC
(`Persistence/Conversores/InstanteUtcParaTicksConverter.cs`).

### 11. Onde o LINQ aparece

Nos repositórios (`Infrastructure/Repositories/`), traduzido para SQL pelo EF Core:

- `DestinoRepository.ListarPercursosDiretosAsync`: `Where` (parada, destino, cadastros ativos),
  `Include`, `OrderBy(minutos)` e `ThenBy(código)`;
- `RegistroPassagemRepository.ListarUltimasPorLinhaAsync`: `GroupBy` por linha com
  `OrderByDescending(...).First()` dentro do grupo, num único comando SQL;
- `ConsultaExtensions.ContendoTexto` (busca sem acentos) e `PaginarAsync` (`CountAsync`, `Skip`,
  `Take`).

LINQ em memória também aparece em `AlternativasService.Ordenar` (`Where`, `OrderBy`, `ThenBy`,
`Select`). O SQL gerado está em `docs/evidencias/B4-consultas-linq.md`.

### 12. Onde o Async/Await aparece

Toda operação com I/O é assíncrona: `ToListAsync`, `FirstOrDefaultAsync` e `CountAsync` nos
repositórios, `SaveChangesAsync` em `Infrastructure/Repositories/UnidadeTrabalhoEf.cs`; `await` nos services (`ResumoParadaService.ObterAsync`); o
adaptador Olho Vivo usa `HttpClient` de forma assíncrona. No resumo, o provedor e a consulta de
últimas passagens rodam juntos com `Task.WhenAll`. As outras consultas ao banco são sequenciais,
porque um `DbContext` não aceita duas operações ao mesmo tempo. Teste:
`ResumoParadaServiceTests.ProvedorEPassagensEmParalelo_SemDuasOperacoesDeBancoAoMesmoTempo`.

### 13. O que é CancellationToken

É o sinal de que o cliente desistiu da requisição. O ASP.NET Core o entrega ao controller
(parâmetro `CancellationToken cancellationToken` em `ParadasController.ObterAlternativas`), e ele
é repassado ao service, ao repositório e ao EF Core (`ToListAsync(cancellationToken)`). Uma
consulta cancelada é interrompida e não vira erro 500. Há testes de cancelamento em cada bloco
(por exemplo, `AlternativasResumoHttpTests.Cancelamento_NaoViraErroNemRespostaParcial`).

### 14. Como funciona o TimeProvider

`TimeProvider` é a abstração de relógio do .NET. O `Program.cs` registra `TimeProvider.System`, e
os services leem o instante com `relogio.GetUtcNow()`, **uma vez** por requisição (veja
`ResumoParadaService.ObterAsync`). Nos testes, `FakeTimeProvider` congela o relógio em
10/11/2026 às 14h32, e por isso as respostas podem ser comparadas com os exemplos do contrato.
Teste: `ResumoParadaServiceTests.Completo_UmInstanteParaTodasAsPartes`.

### 15. Como são geradas as previsões simuladas

`Infrastructure/Providers/ProvedorPrevisaoSimulado.cs`: cada linha de demonstração tem intervalo
e defasagem fixos (`LinhasDemonstracao`, por exemplo `SIM-L004` a cada 15 minutos, com defasagem
de 10). `ProximasChegadas` calcula a próxima viagem a partir de uma âncora fixa
(`DateTimeOffset.UnixEpoch`) e do instante da consulta, sem gravar nada e sem data fixa no código.
Linhas sem configuração (como `SIM-L001`) ficam sem previsão. Regra completa em
[`../../docs/dados.md`](../../docs/dados.md).

### 16. Como funciona o ranking das alternativas

`AlternativaDeslocamento.Calcular` (domínio) calcula, para cada linha direta:
`tempoTotal = espera da próxima chegada + percurso estimado`. `AlternativasService.Ordenar` põe
primeiro as linhas com previsão, pelo menor tempo total, depois menor espera e depois o código; as
posições são 1, 2, 3... As linhas sem previsão vêm por último, com `posicao: null`. No exemplo do
contrato, a `SIM-L002` chega em 1 minuto, mas leva 40 (41 no total); a `SIM-L004` chega em 8 e
leva 20 (28): a `SIM-L004` fica em 1º. Teste:
`AlternativasServiceTests.Exemplo_TempoTotalVenceQuemChegaPrimeiro_ESemPrevisaoVaiPorUltimo`.

### 17. Como é montado o resumo

`ResumoParadaService.ObterAsync`: lê a parada (404 se não existir), lê o relógio uma vez, lê as
linhas, consulta o provedor e as últimas passagens com `Task.WhenAll` e monta as três partes com os
mesmos métodos das rotas individuais (`PrevisoesService.Montar` e `UltimasPassagensService.Montar`).
Se o provedor estiver indisponível, a resposta continua 200, com `situacao: "Parcial"` e
`partesIndisponiveis: ["previsoes"]`.

### 18. Como funciona a chave administrativa

`Security/ChaveAdministrativaFilter.cs` (`OnAuthorizationAsync`) roda antes de qualquer rota
`/api/v1/admin`: compara o cabeçalho `X-PontoVoz-Admin-Key` com `Admin:Chave` em tempo constante
(`CryptographicOperations.FixedTimeEquals`, em `ChavesIguais`). Sem chave ou com chave errada:
401 `ChaveAdministrativaInvalida`. Fora de Development e sem chave configurada: 403
`OperacoesAdministrativasDesabilitadas`. Em Development sem chave, a demonstração local é
permitida, com aviso no log. A chave nunca vai para o Git: fica em user-secrets ou numa variável
de ambiente. Testes: `AdministracaoAutorizacaoTests`.

### 19. Como os testes comprovam as regras

São 805 testes em `backend/tests/PontoVoz.Tests/`:

- **Domain:** regras e invariantes sem banco (`RegraTemporalPassagemTests`, `ParadaTests`);
- **Application:** services com dublês de repositório (`DublesRepositorio`) e relógio controlado;
- **Infrastructure:** SQLite real e isolado por teste (migrations, consultas, provedores);
- **Api:** a API inteira em memória (`WebApplicationFactory`), com as respostas comparadas campo a
  campo com os exemplos JSON do contrato.

Comando: `dotnet test` (a partir de `backend/`).

### 20. Por que os dados não podem ser chamados de oficiais

Os dados da demonstração (seed e previsões) são fictícios: códigos `SIM-`, `dadosSimulados: true`,
`origem: "Simulado"` e o aviso `DadosSimulados`. Os cadastros feitos pelo operador declaram a sua
natureza e nunca são verificados (aviso `CadastroNaoVerificado`). As passagens nunca saem com `verificada: true` nem `oficialSptrans: true`
(`RegistroPassagem.OficialSptrans => false`). O adaptador Olho Vivo está desligado
(`Previsoes:OlhoVivo:Habilitado = false`) e nunca foi testado contra a API real da SPTrans, por
falta de credenciais. Veja [`../../docs/dados.md`](../../docs/dados.md).

## Perguntas que o professor pode fazer

| Pergunta | Resposta sustentada pelo código |
|---|---|
| Onde está o encapsulamento? | As propriedades das entidades têm `private set`; o estado só muda por métodos que validam, como `Parada.AtualizarDados` e `Parada.AssociarLinha`. Não há construtor público: a criação passa por fábricas como `Parada.CadastrarAdministrativa`. |
| Onde está o polimorfismo? | `RegistroPassagem` tem membros `abstract`, e `PassagemSimulada` e `PassagemRegistroManual` os sobrescrevem; também `IProvedorPrevisaoChegada`, com três implementações. |
| Por que usar interfaces nos repositórios? | Os services dependem de `IParadaRepository` e das outras interfaces, não do EF Core. Isso permite testar os services com dublês (`DublesRepositorio`) e trocar a implementação sem mudar a regra. |
| O LINQ roda no banco ou em memória? | Nos repositórios, no banco: o EF Core traduz para SQL (registrado em `B4-consultas-linq.md`). Em `AlternativasService.Ordenar`, em memória, sobre poucas linhas já carregadas. |
| Por que não usar `Task.WhenAll` em tudo? | Um `DbContext` não aceita duas operações simultâneas. Só o provedor (que não usa o banco) roda em paralelo com uma consulta; o teste com repositórios instrumentados confirma no máximo uma operação de banco por vez. |
| Para que serve o `CancellationToken`? | Para interromper a consulta quando o cliente desiste; ele vai do controller até o `ToListAsync`. |
| Como vocês testam regras que dependem da hora? | Com `TimeProvider` injetado e `FakeTimeProvider` nos testes, que fixa o relógio. |
| O que acontece se o provedor de previsões cair? | `/previsoes` responde 503 com `Retry-After: 30`; `/alternativas` responde 200 `SemComparacao`; `/resumo` responde 200 `Parcial`. Nada é inventado para cobrir a falha. |
| Como a rota administrativa é protegida? | Pelo `ChaveAdministrativaFilter`, aplicado na classe base `AdministracaoControllerBase`, com comparação em tempo constante e 401/403 em ProblemDetails. |
| Como o erro chega ao cliente? | `TratadorExcecoes` converte as exceções em ProblemDetails com `codigo` e `traceId`; erros inesperados viram 500 com mensagem genérica, e o detalhe fica só no log. |
| Como a migration e o seed são criados? | `dotnet ef database update` aplica `InitialCreate` e `AddSearchColumns`; o seed roda pelo `UseSeeding` e não duplica dados num banco já provisionado. |
| Os dados são da SPTrans? | Não. Os dados da demonstração são fictícios e marcados como simulados; a integração Olho Vivo está desligada e não foi validada. |
| Como foi usada a IA? | Está registrado em `prompts.md`: o ChatGPT apoiou o planejamento, os prompts e as revisões, e o Claude Code escreveu o código, os testes e a documentação, com cada etapa autorizada pelo integrante. |
