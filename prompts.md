# Uso de IA e perguntas de estudo — PontoVoz (C# Software Development)

Este arquivo atende à orientação da FIAP de registrar o uso de IA no projeto (página 21 do edital) e
reúne perguntas para estudar e debater os conteúdos da disciplina a partir do backend do PontoVoz.
Ele tem três partes:

1. [Como a IA foi usada no desenvolvimento](#como-a-ia-foi-usada-no-desenvolvimento): resumo fiel do
   que cada ferramenta fez.
2. [Registro histórico completo](#registro-histórico-completo): onde estão, com o texto integral, os
   prompts efetivamente usados.
3. [Perguntas de estudo e debate](#perguntas-de-estudo-e-debate): exemplos ilustrativos, organizados
   por assunto.

| Item | Informação |
|---|---|
| Disciplina | C# Software Development — backend (`backend/` e `contracts/`) |
| Equipe | Albert Katri (RM556544), Bruno Biletsky (RM554739), Guilherme Gomes Oliveira (RM554606) e Paulo Akira Okama (RM556840) |
| Ferramentas | ChatGPT e Claude Code (Anthropic) |

## Como a IA foi usada no desenvolvimento

- **ChatGPT:** apoiou o planejamento do projeto, a elaboração dos prompts enviados ao Claude Code e a
  revisão dos relatórios e dos pacotes de código produzidos em cada etapa.
- **Claude Code:** executou o trabalho técnico a partir desses prompts. Escreveu o código C# do
  backend (domínio, persistência com EF Core, migrations, seed, repositórios LINQ, services,
  controllers, tratamento de erros e segurança administrativa), os testes automatizados, o contrato
  REST, a documentação técnica e as evidências, e executou os comandos de build, testes, migrations,
  commits e pushes autorizados.
- **Equipe:** Paulo Akira Okama conduziu as sessões registradas: encaminhou as instruções, tomou as
  decisões apresentadas pela ferramenta e autorizou cada etapa e cada publicação. Não houve edição
  manual do código nas etapas registradas.

O primeiro commit do repositório contém um trailer `Co-Authored-By` do Claude Code, criado antes da
regra de não assinar commits com ferramentas. O histórico do Git não foi reescrito.

## Registro histórico completo

O texto integral de todos os prompts usados no desenvolvimento, em ordem cronológica, com o resumo
do resultado de cada etapa, está em
[`docs/prompts-registro-historico.md`](docs/prompts-registro-historico.md). Até 17/11/2026 esse
registro ficava neste arquivo; ele foi movido sem alteração das transcrições.

## Perguntas de estudo e debate

> **Exemplos ilustrativos.** As perguntas abaixo foram preparadas como material de estudo e debate,
> conforme a orientação do professor da disciplina. **Não são transcrições de conversas realizadas**:
> os prompts realmente usados estão no [registro histórico](docs/prompts-registro-historico.md).
> Cada pergunta indica onde a resposta pode ser conferida no código do PontoVoz. Os caminhos partem
> de `backend/src/PontoVoz.Api/`, e os de teste, de `backend/tests/PontoVoz.Tests/`.

### 1. Conceitos de Programação Orientada a Objetos

1. Quais classes do PontoVoz representam entidades, e quais representam objetos de valor? Que
   diferença isso faz na forma de compará-las?
   *Onde ver:* `Domain/Entities/EntidadeBase.cs` (`MesmaEntidade`, identidade pelo `Id`) e
   `Domain/ValueObjects/Coordenada.cs` (`record`, igualdade por valor).
2. Por que as entidades não têm construtor público e são criadas por métodos de fábrica como
   `Parada.CriarSimulada` e `Parada.CadastrarAdministrativa`? O que isso garante sobre a origem do
   dado?
   *Onde ver:* `Domain/Entities/Parada.cs`; testes em `Domain/ParadaTests.cs`.
3. Onde estão as regras de negócio que não pertencem a uma única entidade, e por que elas ficaram
   em classes estáticas separadas?
   *Onde ver:* `Domain/Rules/RegraTemporalPassagem.cs` e `Domain/Rules/RegrasCadastro.cs`.

### 2. Encapsulamento, herança, abstração e polimorfismo

1. Como o `private set` das propriedades de `Parada` impede que um controller altere o estado sem
   passar pelas validações? Quais métodos podem mudar esse estado?
   *Onde ver:* `Domain/Entities/Parada.cs` (`AtualizarDados`, `Ativar`, `Desativar`, `AssociarLinha`).
2. `RegistroPassagem` é abstrata e tem duas subclasses. O que cada subclasse sobrescreve, e por que
   `MinutosDesdeObservacao` e `EstaDesatualizada` não precisam ser sobrescritos?
   *Onde ver:* `Domain/Entities/RegistroPassagem.cs`, `PassagemSimulada.cs` e
   `PassagemRegistroManual.cs`; teste `Domain/RegistroPassagemTests.cs`
   (`Hierarquia_CadaSubclasseDefineNivelAvisoEOrigem_ENenhumaEVerificada`).
3. O service de últimas passagens trata todas as passagens como `RegistroPassagem`. Como o
   polimorfismo faz cada resposta sair com o nível de confirmação e o aviso corretos?
   *Onde ver:* `Application/Services/UltimasPassagensService.cs` (`Montar`).
4. Por que `IProvedorPrevisaoChegada` com três implementações (simulado, Olho Vivo e seletor) é um
   exemplo de polimorfismo por interface, e não por herança?
   *Onde ver:* `Application/Interfaces/IPrevisoes.cs` e `Infrastructure/Providers/`.

### 3. Interfaces e injeção de dependência

1. Por que os services dependem de `IParadaRepository` e não de `ParadaRepository`? O que isso
   permite nos testes?
   *Onde ver:* `Application/Services/ConsultaServices.cs` e `Application/DublesRepositorio.cs` (testes).
2. Onde as dependências são registradas, e por que os repositórios e services usam o tempo de vida
   *scoped*, enquanto o `TimeProvider` é *singleton*?
   *Onde ver:* `Program.cs`, `Application/AplicacaoExtensions.cs` e
   `Infrastructure/Persistence/PersistenciaExtensions.cs`; teste `Api/InjecaoDependenciaTests.cs`.
3. Como o `SeletorProvedorPrevisao` decide, por configuração, qual provedor responde, sem que o
   service saiba qual implementação está em uso?
   *Onde ver:* `Infrastructure/Providers/SeletorProvedorPrevisao.cs` e `ProvedoresExtensions.cs`.

### 4. Organização em camadas

1. Quais são as responsabilidades de `Domain`, `Application`, `Infrastructure` e dos controllers?
   Por que o domínio não referencia o EF Core nem o ASP.NET Core?
   *Onde ver:* READMEs de cada camada (`Domain/README.md`, `Application/README.md` e outros).
2. Por que os controllers não têm LINQ nem regra de negócio? O que acontece se uma validação for
   colocada no controller em vez do service?
   *Onde ver:* `Controllers/ParadasController.cs`.
3. As camadas são pastas de um mesmo projeto. Quais são as vantagens e as desvantagens disso em
   comparação com projetos separados?
   *Onde ver:* `backend/README.md`, seção "Arquitetura".

### 5. APIs REST

1. Por que as rotas são organizadas em torno do recurso parada (`/api/v1/paradas/{id}/previsoes`,
   `/ultimas-passagens`, `/alternativas`, `/resumo`)? Que papel tem o prefixo `/v1`?
   *Onde ver:* `contracts/api-v1.md`, seções 1.9 e 9.
2. Por que a associação de linha à parada usa `PUT` idempotente e devolve 204 mesmo quando repetida?
   *Onde ver:* `Controllers/Administracao/AdminParadasController.cs`; contrato, seção 5.5.
3. O projeto não tem `DELETE`: a parada é desativada com `PUT` e `"ativa": false`. Em que situação a
   exclusão lógica é preferível à exclusão física?
   *Onde ver:* contrato, seção 5.3; `RESTRICT` nas chaves estrangeiras
   (`Infrastructure/Persistence/Configuracoes/`).

### 6. Status HTTP

1. Qual a diferença entre 400 e 422 no PontoVoz? Dê um exemplo de cada.
   *Onde ver:* contrato, seção 7; `Application/Validacao/` e `Domain/Exceptions/RegraNegocioException.cs`.
2. Por que `/previsoes` responde 503 quando o provedor cai, mas `/alternativas` e `/resumo` continuam
   respondendo 200?
   *Onde ver:* `Application/Services/PrevisoesService.cs`, `AlternativasService.cs` e
   `ResumoParadaService.cs`; teste `Api/AlternativasResumoHttpTests.cs`.
3. Quando a API devolve 401 e quando devolve 403 nas rotas administrativas?
   *Onde ver:* `Security/ChaveAdministrativaFilter.cs`; teste `Api/AdministracaoAutorizacaoTests.cs`.

### 7. Entity Framework Core

1. Como os relacionamentos N:N entre parada e linha e a chave composta de `PercursosEstimados` são
   configurados com a Fluent API?
   *Onde ver:* `Infrastructure/Persistence/Configuracoes/ParadaLinhaConfiguration.cs` e
   `PercursoEstimadoConfiguration.cs`.
2. Como a herança `RegistroPassagem` é mapeada para uma única tabela (TPH), e como o EF Core sabe
   qual subclasse reconstruir ao ler do banco?
   *Onde ver:* `Infrastructure/Persistence/Configuracoes/RegistroPassagemConfiguration.cs`.
3. Por que os instantes são gravados como ticks UTC em vez de `DateTimeOffset`?
   *Onde ver:* `Infrastructure/Persistence/Conversores/InstanteUtcParaTicksConverter.cs`.

### 8. Migrations e seed

1. O que fazem as migrations `InitialCreate` e `AddSearchColumns`, e por que a segunda precisa de uma
   etapa em C# para preencher a coluna de busca?
   *Onde ver:* `Infrastructure/Persistence/Migrations/` e `Busca/PreenchimentoTextoBusca.cs`.
2. Por que a API aplica as migrations ao iniciar só em Development? Que risco haveria em fazer isso
   em produção?
   *Onde ver:* `PersistenciaExtensions.AplicarMigracoesEmDesenvolvimentoAsync`; teste
   `Api/MigracaoAutomaticaTests.cs`.
3. Como o seed garante que reiniciar a API não duplica nem renova os dados de demonstração?
   *Onde ver:* `Infrastructure/Persistence/Seed/ProvisionamentoDemonstracao.cs`; teste
   `Infrastructure/ProvisionamentoDemonstracaoTests.cs`.

### 9. LINQ

1. Como `ListarUltimasPorLinhaAsync` escolhe a última passagem de cada linha com `GroupBy`, e por que
   é importante que isso seja traduzido para um único comando SQL?
   *Onde ver:* `Infrastructure/Repositories/RegistroPassagemRepository.cs`; SQL gerado em
   `backend/docs/evidencias/B4-consultas-linq.md`.
2. Por que a paginação faz `CountAsync` e depois `Skip`/`Take` no banco, e não em memória?
   *Onde ver:* `Infrastructure/Repositories/ConsultaExtensions.cs` (`PaginarAsync`).
3. Como a busca sem acentos funciona, se o `LIKE` do SQLite não ignora acentos?
   *Onde ver:* `Infrastructure/Persistence/Busca/TextoBusca.cs` e `ConsultaExtensions.ContendoTexto`.
4. Em `AlternativasService.Ordenar`, o LINQ roda em memória. Por que isso é aceitável nesse caso?
   *Onde ver:* `Application/Services/AlternativasService.cs`.

### 10. Async/await

1. Por que os métodos dos repositórios são assíncronos (`ToListAsync`, `FirstOrDefaultAsync`), e o
   que a thread da requisição faz enquanto o banco responde?
   *Onde ver:* `Infrastructure/Repositories/`.
2. No resumo, o provedor e a consulta de passagens rodam juntos com `Task.WhenAll`. Por que as outras
   consultas ao banco continuam sequenciais?
   *Onde ver:* `Application/Services/ResumoParadaService.cs`; teste
   `Application/ResumoParadaServiceTests.cs`
   (`ProvedorEPassagensEmParalelo_SemDuasOperacoesDeBancoAoMesmoTempo`).
3. Por que as alternativas não usam `Task.WhenAll`?
   *Onde ver:* `Application/Services/AlternativasService.cs`; `backend/README.md`, seção "Async/Await".

### 11. CancellationToken

1. Como o `CancellationToken` da requisição chega do controller até o EF Core, e o que acontece
   quando o cliente desiste da requisição?
   *Onde ver:* `Controllers/ParadasController.cs` e os repositórios; teste
   `Api/AlternativasResumoHttpTests.cs` (`Cancelamento_NaoViraErroNemRespostaParcial`).
2. Por que um cancelamento não deve virar erro 500?
   *Onde ver:* `Middleware/README.md`; teste `Api/InfraestruturaHttpTests.cs`.

### 12. Testes unitários

1. Como `FakeTimeProvider` permite testar regras que dependem do horário, como a desatualização de
   uma passagem depois de 60 minutos?
   *Onde ver:* `Domain/RegraTemporalPassagemTests.cs` e `Application/UltimasPassagensServiceTests.cs`.
2. Para que servem os dublês de repositório, e como eles provam que uma requisição inválida não
   consulta o banco?
   *Onde ver:* `Application/DublesRepositorio.cs` e `Application/ServicosConsultaTests.cs`.
3. Que casos de teste garantem o ranking das alternativas (desempate por espera e código, linhas sem
   previsão no fim)?
   *Onde ver:* `Application/AlternativasServiceTests.cs`.

### 13. Testes de integração

1. Por que os testes de integração usam SQLite real e isolado em vez do provedor InMemory do EF Core?
   *Onde ver:* `Infrastructure/BancoSqliteTeste.cs`.
2. Como `WebApplicationFactory` sobe a API inteira em memória, e como os testes comparam as
   respostas com os exemplos JSON do contrato?
   *Onde ver:* `Api/ApiTeste.cs` e `Api/ConsultasPublicasHttpTests.cs`.
3. Como o teste do OpenAPI garante que a documentação exportada não fica desatualizada?
   *Onde ver:* `Api/InfraestruturaHttpTests.cs` (`OpenApiExportado_EstaIgualAoDocumentoGeradoPelaApi`).

### 14. Tratamento de erros

1. Como uma exceção do domínio vira uma resposta ProblemDetails com `codigo` e `traceId`, sem que
   os controllers tenham `try/catch`?
   *Onde ver:* `Middleware/TratadorExcecoes.cs` e `Middleware/ProblemasHttp.cs`.
2. Por que o erro 500 devolve uma mensagem genérica, e onde fica o detalhe real do erro?
   *Onde ver:* `Middleware/TratadorExcecoes.cs`; teste
   `Api/InfraestruturaHttpTests.cs` (`ErroInesperado_Responde500GenericoSemDadosSensiveis`).
3. Por que todos os erros de validação de um corpo são devolvidos juntos, em vez de um de cada vez?
   *Onde ver:* `Application/Validacao/ColetorErros.cs`.

### 15. Documentação de endpoints

1. Qual a relação entre o contrato escrito (`contracts/api-v1.md`), o documento OpenAPI gerado pela
   API e a interface Scalar?
   *Onde ver:* `Documentacao/DocumentacaoOpenApi.cs` e `backend/scripts/exportar-openapi.ps1`.
2. Como os comentários XML dos controllers aparecem na documentação do Scalar?
   *Onde ver:* `Controllers/ParadasController.cs` e o Scalar em `http://localhost:5080/scalar`.

### 16. Segurança de APIs

1. Por que a chave administrativa é comparada em tempo constante, e que ataque isso evita?
   *Onde ver:* `Security/ChaveAdministrativaFilter.cs` (`ChavesIguais`).
2. Por que a chave fica em user-secrets ou em variável de ambiente, e nunca no `appsettings.json`?
   *Onde ver:* `backend/README.md`, seção "Segurança".
3. O que muda entre Development e Production quando não há chave configurada? Por que o modo de
   demonstração local não é adequado para uma API exposta?
   *Onde ver:* contrato, seção 5.1; teste `Api/AdministracaoAutorizacaoTests.cs`.
4. Que limitação o padrão de `origemRegistro` tem em relação a dados pessoais?
   *Onde ver:* `docs/dados.md`, seção "Dados pessoais".

### 17. Qualidade e manutenção de código

1. Como o verificador do contrato ajuda a manter os exemplos JSON coerentes com a documentação?
   *Onde ver:* `contracts/verificador/VerificarContrato.cs`.
2. Por que ler o relógio uma única vez por requisição evita inconsistências entre as partes de uma
   resposta?
   *Onde ver:* `Application/Services/ResumoParadaService.cs`; teste
   `Application/ResumoParadaServiceTests.cs` (`Completo_UmInstanteParaTodasAsPartes`).
3. Na validação final, uma execução da suíte teve uma falha intermitente não identificada. Que
   práticas ajudam a investigar testes que dependem de tempo real ou de paralelismo?
   *Onde ver:* `backend/docs/evidencias/B11-validacao.md`, seção "Falha intermitente observada".

### 18. Aplicação ao contexto do PontoVoz

1. Por que a alternativa mais rápida é a de menor tempo total (espera + percurso) e não a que chega
   primeiro ao ponto?
   *Onde ver:* `Domain/ValueObjects/AlternativaDeslocamento.cs` (`Calcular`) e o exemplo do contrato,
   seção 4.7.
2. Como a API deixa claro, em cada resposta, que os dados são simulados e que nenhuma passagem é
   oficial da SPTrans?
   *Onde ver:* `Application/Avisos/CatalogoAvisos.cs`; `docs/dados.md`.
3. O que seria necessário para trocar o provedor simulado por dados reais da API Olho Vivo, e por que
   isso não foi validado?
   *Onde ver:* `Infrastructure/Providers/ProvedorOlhoVivo.cs`; `docs/dados.md`, seção "Dados públicos".
4. Como o resumo atende a um totem que precisa mostrar algo útil mesmo quando as previsões estão
   indisponíveis?
   *Onde ver:* `Application/Services/ResumoParadaService.cs`; exemplo
   `contracts/exemplos/resumo-provedor-indisponivel.json`.
