# Registro de uso de inteligência artificial

Este arquivo registra o uso de ferramentas de IA no desenvolvimento do PontoVoz,
conforme as orientações acadêmicas da FIAP. Cada integrante registra o próprio uso
em uma seção da sua disciplina.

## Como este registro é feito

- **Prompts:** transcritos de forma fiel, exatamente como foram enviados à ferramenta.
- **Respostas da IA:** não são transcritas, porque são longas. Cada uma tem um
  **resumo**, identificado como tal.
- **Revisão humana:** todo código e documento gerado com apoio de IA é revisado pelo
  integrante responsável antes de ser aceito, bloco a bloco.

---

## C# Software Development — Backend

| Item | Informação |
|---|---|
| Ferramenta | Claude Code (aplicativo desktop), modelo Claude Opus 5.5, da Anthropic |
| Integrante responsável | Pendente (nome e RM) |
| Forma de uso | Diagnóstico do ambiente, planejamento da arquitetura e execução de blocos de implementação autorizados um a um |

### Prompt 01 — Diagnóstico do ambiente local (06/11/2026)

**Texto original:**

````text
Estou iniciando um novo projeto acadêmico de backend em C#/.NET para a Global Solution da FIAP.

Antes de desenvolver qualquer coisa, preciso que você faça um diagnóstico do meu ambiente local.

Verifique:
1. O caminho completo da pasta de trabalho atual.
2. Se a pasta está vazia ou contém algum arquivo.
3. Se o Git está instalado, informando sua versão.
4. Se o .NET SDK está instalado, informando as versões disponíveis.
5. Se o ambiente tem os recursos básicos necessários para desenvolver e testar uma ASP.NET Core Web API.

Se alguma ferramenta estiver faltando, explique o que preciso instalar, usando fontes oficiais, mas não instale nada ainda.

IMPORTANTE: não crie projetos, não altere arquivos, não instale ferramentas e não execute comandos que modifiquem o sistema. Quero somente o diagnóstico, com os resultados efetivamente verificados, e suas recomendações.
````

**Resumo da resposta (não é transcrição):** a IA executou apenas comandos de leitura.
Constatou pasta vazia, Git 2.44.0, .NET SDK 8.0.421 como único SDK, templates Web API
disponíveis, NuGet acessível, certificado HTTPS de desenvolvimento existente mas não
confiável, VS Code sem extensão C#, `dotnet-ef` e Docker ausentes. Recomendou confiar
no certificado, instalar o C# Dev Kit e, se exigido, o SDK do .NET 10.

### Prompt 02 — Verificação do .NET 10 SDK (06/11/2026)

**Texto original:**

````text
Instalei o .NET 10 SDK no meu computador. Verifique, usando apenas comandos de leitura, se a instalação foi reconhecida corretamente. Execute dotnet --list-sdks e dotnet --version, confira se os templates ASP.NET Core Web API continuam disponíveis e informe se podemos usar net10.0 no projeto PontoVoz da Global Solution. Não crie arquivos, não instale dependências e não inicie a implementação. Quero apenas a confirmação técnica do ambiente.
````

**Resumo da resposta (não é transcrição):** a IA confirmou os SDKs 8.0.421 e 10.0.403,
com `dotnet --version` igual a 10.0.403 na pasta do projeto, runtime ASP.NET Core 10.0.14
e o template `webapi` presente no pacote de templates do .NET 10. Concluiu que `net10.0`
pode ser usado.

### Prompt 03 — Etapa 01: planejamento e arquitetura (06/11/2026)

**Texto original:**

````text
# GLOBAL SOLUTION FIAP 2026.2 — PONTOVOZ
## ETAPA 01 — PLANEJAMENTO E ARQUITETURA DE C# SOFTWARE DEVELOPMENT

Você atuará como um **arquiteto de software sênior especialista em C#, ASP.NET Core, Entity Framework Core e engenharia de software**, responsável pelo planejamento técnico de um projeto acadêmico da FIAP.

Esta etapa é exclusivamente de análise e planejamento. **Não implemente código, não crie arquivos e não instale dependências.**

### 1. Contexto da Global Solution

Estamos desenvolvendo a Global Solution do segundo semestre de 2026, da graduação em Engenharia de Software da FIAP.

O tema oficial é **CitizenTech**, que propõe desenvolver soluções tecnológicas para problemas reais enfrentados pelos cidadãos.

Nossa equipe possui três integrantes, e todos trabalharão em diferentes disciplinas sobre uma única solução integrada.

A solução deve resolver um problema concreto, ser funcional, acessível e demonstrável. Precisamos garantir que cada componente tenha ligação direta com a proposta principal.

### 2. Ideia definitiva do projeto

**Nome provisório:** PontoVoz — Totem Inteligente de Mobilidade Urbana.

**Local de aplicação:** São Paulo, SP.

**Problema:** usuários do transporte público enfrentam dificuldades para obter informações acessíveis e contextualizadas sobre a chegada dos ônibus e as alternativas de deslocamento.

**Solução:** um totem digital instalado nos pontos de ônibus, permitindo que os passageiros interajam por voz ou por uma tela e recebam respostas sobre o transporte público.

A ideia principal inclui:

1. Consultar quantos minutos faltam para determinada linha chegar.
2. Informar a última passagem registrada do ônibus, quando houver dados confiáveis.
3. Apresentar alternativas de deslocamento que possam ser mais rápidas.
4. Permitir interação conversacional com uma inteligência artificial.
5. Proporcionar uma experiência simples e acessível, inclusive para pessoas que não possuem smartphone.

Essas funcionalidades representam o conceito do produto completo. Não necessariamente todas serão implementadas pelo backend de C#.

**Sua responsabilidade será propor exclusivamente a parte do sistema necessária à disciplina C#**, preservando a possibilidade de integração com os outros componentes.

### 3. Requisitos oficiais do professor de C#

A aplicação deverá utilizar C#/.NET para implementar a camada de backend integrada à proposta CitizenTech.

Os conceitos exigidos são:

- Programação Orientada a Objetos.
- Interfaces.
- Injeção de Dependência.
- Entity Framework Core.
- LINQ.
- APIs REST.
- Programação Assíncrona.
- Testes Unitários.
- Boas práticas e qualidade de código.

**Critérios de pontuação, totalizando 10 pontos:**

| Critério | Pontuação |
|---|---:|
| Programação Orientada a Objetos | 3,0 |
| Web API | 2,5 |
| Entity Framework Core + LINQ | 1,5 |
| Async/Await | 1,0 |
| Qualidade do código | 2,0 |

**Entregáveis obrigatórios:**

- Projeto C# funcional.
- Web API.
- Utilização efetiva do Entity Framework Core.
- README detalhado, incluindo descrição da solução e instruções de execução.
- Documentação dos endpoints usando Swagger, Scalar ou equivalente.
- Evidências de funcionamento da aplicação.
- Repositório GitHub com o código completo.

O envio será realizado exclusivamente pelo link do GitHub no Microsoft Teams, até 19/11/2026, às 23h55.

Não ignore nenhum conceito exigido, ainda que não tenha pontuação separada na rubrica.

### 4. Ambiente já verificado

O diagnóstico anterior confirmou:

- Windows.
- Diretório: `C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`.
- Pasta vazia.
- Git instalado.
- .NET SDK 10.0.403 instalado e funcionando.
- ASP.NET Core 10 disponível.
- Templates ASP.NET Core Web API disponíveis.
- Acesso ao NuGet funcional.
- VS Code instalado.
- `dotnet-ef` ainda não instalado.
- Certificado HTTPS de desenvolvimento ainda não confiável.

**Adote .NET 10 e target framework `net10.0`.** Planeje a utilização de versões compatíveis do Entity Framework Core e suas ferramentas.

Não é necessário instalar Docker nem utilizar serviços pagos.

### 5. Responsabilidade específica do backend

O projeto C# deverá disponibilizar serviços e endpoints que possam ser utilizados pelo totem e, futuramente, pelos componentes desenvolvidos pelos demais integrantes.

Analise os seguintes módulos candidatos:

**A. Pontos de ônibus:** cadastro e consulta de paradas, localização e suas associações com linhas.

**B. Linhas de ônibus:** identificação, descrição e consulta das linhas disponíveis em uma parada.

**C. Previsão de chegada:** serviço responsável por recuperar e organizar previsões de chegada de ônibus, incluindo fonte, horário da consulta e tratamento de indisponibilidade.

**D. Histórico de passagem:** caso viável, persistência de observações verificáveis. Não confundir uma previsão com a confirmação de passagem efetiva.

**E. Alternativas de deslocamento:** proposta de comparação de trajetos com base em critérios mensuráveis e dados disponíveis. Não inventar rotas reais ou tempos exatos.

Não precisamos criar todos esses módulos se isso tornar o projeto excessivamente grande. Selecione um MVP coerente e justifique quaisquer funcionalidades adicionais.

A API Olho Vivo da SPTrans é uma possível fonte externa a investigar, mas não presuma que temos credenciais ou acesso autorizado. Planeje uma abstração para o provedor de dados, permitindo substituir um mock identificado por uma integração real.

Os dados demonstrativos devem ser explicitamente identificados como simulados.

**A interação por voz e a IA conversacional poderão ser implementadas por outros componentes.** Não desenvolva esses recursos agora. O backend deve oferecer contratos claros e dados estruturados que permitam essa integração.

### 6. Arquitetura e boas práticas

Proponha uma arquitetura adequada ao prazo e à avaliação, considerando:

- ASP.NET Core Web API.
- C# / .NET 10.
- Programação Orientada a Objetos com responsabilidades bem definidas.
- Encapsulamento, abstração, interfaces e polimorfismo onde façam sentido.
- Injeção de Dependência.
- Entity Framework Core com banco relacional, preferencialmente SQLite para facilitar a execução local.
- Consultas reais utilizando LINQ.
- Uso correto de `async/await` para operações assíncronas.
- DTOs para entrada e saída de dados.
- Validações e tratamento de exceções.
- Respostas HTTP e códigos de status adequados.
- Swagger, Scalar ou equivalente.
- Testes unitários com framework apropriado.
- Configurações e segredos protegidos.
- Código limpo, organizado e compreensível.
- Integração futura com frontend e outras disciplinas.

Evite implementar padrões complexos ou excesso de camadas sem justificativa. A prioridade é demonstrar domínio dos conceitos exigidos, com qualidade e funcionamento verificável.

### 7. O que você deverá apresentar

Produza um documento de planejamento na própria resposta, contendo as seguintes seções:

**Seção A — Visão técnica do projeto**

Explique o problema, o público beneficiado, a solução e a responsabilidade do backend.

**Seção B — Arquitetura recomendada**

Apresente as tecnologias escolhidas, os componentes, suas responsabilidades e um fluxo de comunicação.

**Seção C — Estrutura do projeto**

Apresente uma árvore de pastas proposta e explique o propósito de cada projeto ou camada.

**Seção D — Modelagem do domínio**

Defina as entidades, propriedades, relacionamentos, interfaces e regras de negócio necessárias.

**Seção E — Banco de dados**

Proponha o DbContext, as tabelas, os relacionamentos, a estratégia de migrations e exemplos concretos de operações LINQ.

**Seção F — Contratos REST**

Crie uma tabela contendo os endpoints, verbos HTTP, finalidade, parâmetros, exemplos de respostas e possíveis erros.

**Seção G — POO e injeção de dependência**

Explique como aplicaremos concretamente os conceitos exigidos pelo professor. Relacione cada conceito a uma classe, interface ou responsabilidade proposta.

**Seção H — Programação assíncrona**

Identifique operações que precisarão utilizar `async/await` e explique sua necessidade real.

**Seção I — Testes unitários**

Defina o que será testado e quais cenários positivos e negativos deverão ser contemplados.

**Seção J — Aderência à rubrica**

Crie uma matriz de conformidade relacionando os cinco critérios de avaliação do professor com os componentes que serão implementados e as evidências que poderemos apresentar.

**Seção K — Plano de desenvolvimento**

Divida o projeto em blocos sequenciais pequenos, com dependências, critérios de conclusão e verificações de cada etapa.

**Seção L — Riscos e decisões**

Identifique dificuldades técnicas, dependências externas, suposições, limites do escopo e riscos de integração.

**Seção M — Checklist da entrega final**

Apresente todos os itens que deverão estar prontos antes do envio do link do GitHub ao professor.

### 8. Restrições obrigatórias

- Não desenvolver funcionalidades sem relação com o PontoVoz.
- Não criar artificialmente complexidade para preencher requisitos.
- Não afirmar que uma integração real funciona sem testá-la.
- Não inventar dados reais de ônibus.
- Não usar informações pessoais reais.
- Não misturar esta entrega com a de Testing, Compliance & Quality Assurance.
- Não desenvolver o aplicativo mobile ou interface do totem nesta etapa.
- Não alterar componentes dos demais integrantes.
- Não criar arquivos nem instalar pacotes durante este planejamento.
- Não considerar requisitos atendidos apenas por menção a uma tecnologia.
- Não avançar automaticamente para a próxima etapa.

### 9. Resultado esperado

Quero um planejamento técnico completo, mas proporcional a um projeto acadêmico realizado por três estudantes, com prazo até 19/11/2026.

Todas as decisões devem ter justificativa e contribuir para a proposta CitizenTech e para os critérios do professor.

**Ao concluir, pare e aguarde minha aprovação. Não inicie a programação.**

A implementação será conduzida posteriormente, em blocos, com revisão técnica a cada etapa.
````

**Resumo da resposta (não é transcrição):** a IA apresentou o planejamento nas seções
A a M: Web API em .NET 10 com Controllers, EF Core 10 com SQLite, um projeto de API
organizado em pastas e um projeto de testes xUnit. Propôs entidades de parada, linha,
associação e observação de passagem, um provedor de previsões trocável (simulado ou
Olho Vivo), cerca de 17 endpoints, matriz da rubrica, blocos B0 a B9 e riscos, como
a limitação do SQLite com `DateTimeOffset`.

### Prompt 04 — Etapa 01.1: revisão e consolidação do planejamento (06/11/2026)

**Texto original:**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
ETAPA 01.1 — REVISÃO E CONSOLIDAÇÃO DO PLANEJAMENTO C#
Analise novamente o planejamento técnico que você acabou de elaborar para o backend do PontoVoz.
O planejamento inicial foi revisado e considerado bem estruturado, mas precisamos realizar alguns ajustes antes de autorizar a implementação.
IMPORTANTE: ainda não crie arquivos, não instale dependências, não execute comandos que alterem o ambiente e não comece a desenvolver código. Estamos finalizando a arquitetura.
Mantenha as decisões técnicas que já estão corretas, especialmente .NET 10, ASP.NET Core Web API, EF Core 10, SQLite, Controllers, injeção de dependência, testes com xUnit e documentação OpenAPI/Scalar.
Implemente conceitualmente as seguintes correções no planejamento:
1. Preservar a proposta original do PontoVoz
Nossa ideia definitiva consiste em um totem inteligente para pontos de ônibus, capaz de fornecer:

1. Previsões de chegada de ônibus.
2. Informações sobre a última passagem registrada.
3. Alternativas de deslocamento.
4. Integração com interação por voz e inteligência artificial, implementadas pelos componentes responsáveis.

Você classificou as alternativas de deslocamento como opcionais.
Reavalie essa decisão.
Não precisamos desenvolver um sistema complexo de roteamento urbano, mas o produto final deve ter uma demonstração coerente dessa funcionalidade.
Proponha uma implementação mínima e tecnicamente justificável ou um contrato REST documentado para integração com o componente responsável.
Não considere uma linha mais rápida apenas porque seu ônibus chega primeiro, sem levar em conta o destino e as informações disponíveis.
Diferencie claramente o escopo obrigatório do backend C# das funcionalidades que dependem de outros integrantes.
2. Separar operações públicas e administrativas
O sistema será conceitualmente utilizado em espaços públicos.
As consultas de paradas, linhas e previsões são públicas.
Já operações de cadastro, atualização, desativação e registro de observações não devem ficar livremente disponíveis em uma eventual implantação pública.
Proponha uma estratégia simples para:

* Permitir a demonstração local das operações administrativas.
* Restringir essas operações fora do ambiente Development.
* Evitar a implementação desnecessária de um sistema complexo de autenticação.
* Documentar claramente as limitações de segurança do MVP.

Não adicione dependências desnecessárias.
3. Corrigir a justificativa de programação assíncrona
O SQLite possui limitações de I/O assíncrono no provedor Microsoft.Data.Sqlite.
Continue planejando o uso das APIs assíncronas do EF Core, incluindo ToListAsync, FirstOrDefaultAsync, SaveChangesAsync e CancellationToken.
Entretanto, não afirme que todas essas chamadas proporcionam I/O não bloqueante real no SQLite.
Explique a diferença entre utilizar métodos assíncronos do EF Core e realizar I/O genuinamente assíncrono.
Proponha como o adaptador de integração HTTP poderá demonstrar o uso correto de async/await, com testes adequados e sem criar latência artificial.
4. Preparar a integração com o grupo
Ainda não está definido se o repositório remoto será criado por mim ou por outro integrante.
Portanto:

* Não crie nem publique um repositório remoto automaticamente.
* Planeje o backend de maneira que possa ser integrado em uma pasta backend/ de um repositório compartilhado.
* Preserve a possibilidade de executar a solução .NET isoladamente.
* Defina como disponibilizar os contratos REST aos demais integrantes desde o início.
* Evite esperar o final do desenvolvimento para compartilhar o contrato OpenAPI.

O sistema de voz e a interface do totem deverão conseguir consumir essa API sem acoplamento desnecessário.
5. Incluir documentação geral da CitizenTech
Acrescente ao plano os seguintes elementos:

* Ficha da CitizenTech no início do README.
* Nome da solução.
* Cidade e bairro do projeto, mantendo o bairro como pendente até a definição de um local real.
* Cidadãos beneficiados.
* Problema identificado.
* Funcionamento da solução.
* Contribuição específica do backend.
* Indicadores de impacto mensuráveis.
* ODS 10 e 11, com justificativas.
* Integrantes da equipe.
* Arquivo prompts.md para registro do uso de IA.
* Documentação clara sobre dados reais, dados públicos e dados simulados.

Não invente entrevistas, resultados de impacto nem dados de cidadãos.
O uso de IA será documentado e deverá respeitar as orientações acadêmicas.
6. Refinar a configuração do ambiente
Mantenha .NET 10 e EF Core 10.
Para o global.json, considere utilizar:

* Versão mínima do SDK: 10.0.100.
* Política rollForward: latestFeature.

Isso permitirá utilizar o SDK 10.0.403 que já está instalado sem exigir exatamente esse patch de todos os integrantes.
Para a estratégia de seeding do EF Core, planeje corretamente UseSeeding e UseAsyncSeeding, garantindo compatibilidade com as ferramentas de migração.
7. Refinar a semântica da última passagem
Diferencie:

* Previsão de passagem.
* Observação simulada.
* Registro manual de observação.
* Passagem efetivamente verificada por fonte confiável.

O backend não poderá apresentar observações sem comprovação como se fossem registros oficiais da SPTrans.
Defina os campos e os avisos necessários nos contratos de resposta.
8. Revisar o tamanho do MVP
O planejamento inicial possui aproximadamente 16 a 17 endpoints.
Avalie se todos são necessários para atender à rubrica de C# e demonstrar o PontoVoz.
Priorize:

* Endpoints diretamente relacionados ao cidadão.
* Persistência real com EF Core.
* POO bem aplicada.
* Consultas LINQ relevantes.
* Programação assíncrona.
* Testes automatizados.
* Boa documentação.

Não elimine endpoints necessários para demonstrar conceitos, mas evite funcionalidades administrativas redundantes.
9. Resultado esperado
Apresente:

1. Lista das decisões corrigidas.
2. Arquitetura consolidada.
3. Escopo definitivo do MVP de C#.
4. Estrutura de pastas revisada.
5. Lista final de entidades.
6. Lista final de endpoints.
7. Matriz final de aderência à rubrica, com os cinco critérios e suas evidências previstas.
8. Backlog atualizado por blocos de implementação, com critérios de aceite.
9. Dependências e decisões que precisam ser alinhadas com a equipe.
10. Checklist final para autorizar o Bloco B0.

Não é necessário repetir integralmente todas as explicações do planejamento anterior que continuarem corretas. Concentre-se nas alterações e no plano final consolidado.
Não execute o Bloco B0. Pare após apresentar a revisão e aguarde minha aprovação.
O projeto deve continuar rigorosamente alinhado à Global Solution CitizenTech e à disciplina C# Software Development, conforme as exigências do professor da FIAP.
````

**Resumo da resposta (não é transcrição):** a IA consolidou o plano. Alternativas de
deslocamento entraram no MVP, por linhas diretas, ordenadas pelo tempo total estimado
(espera + percurso), com as entidades `Destino` e `PercursoEstimado`. Rotas
administrativas passaram a ficar em `/api/v1/admin`, protegidas por chave em cabeçalho.
A justificativa assíncrona foi corrigida, com o I/O real demonstrado no adaptador HTTP.
Foram definidos a estrutura `backend/` + `contracts/`, quatro níveis de confirmação de
passagem, 13 endpoints mais `/health`, a matriz da rubrica, os blocos B0 a B10 e o
checklist para autorizar o B0.

### Prompt 05 — Bloco B0: inicialização do backend (06/11/2026)

**Texto original:**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B0 — INICIALIZAÇÃO DO BACKEND C#/.NET 10
O planejamento técnico da Etapa 01.1 foi revisado e aprovado.
Agora você está autorizado a executar exclusivamente o Bloco B0, criando a estrutura inicial do projeto PontoVoz no ambiente local.
Não avance automaticamente para B1, B2 ou qualquer outra etapa.
1. CONTEXTO E DIRETÓRIO
Estamos desenvolvendo o PontoVoz, uma solução CitizenTech da FIAP para pontos de ônibus, com interação por voz/tela, previsão de chegada, histórico de passagens e alternativas de deslocamento.
Minha disciplina é C# Software Development.
O ambiente já possui:

* .NET SDK 10.0.403.
* Git instalado.
* NuGet acessível.
* Windows.
* Pasta do projeto: `C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`.

Trabalhe exclusivamente dentro dessa pasta.
Antes de executar qualquer alteração, confirme o diretório atual e examine seu conteúdo.
Se encontrar arquivos inesperados, alterações preexistentes ou um repositório Git já inicializado, não sobrescreva nada. Informe o que encontrou.
2. OBJETIVO DO BLOCO B0
Criar uma solução ASP.NET Core Web API em C#/.NET 10, pronta para receber posteriormente:

* Entidades e regras de domínio.
* Services e interfaces.
* Injeção de dependência.
* Entity Framework Core.
* SQLite.
* Consultas LINQ.
* Métodos assíncronos.
* Controllers REST.
* Testes automatizados.

Neste bloco, devemos criar apenas a infraestrutura inicial, sem implementar ainda as regras de negócio sobre transporte público.
A estrutura deve permitir que o backend seja futuramente integrado ao repositório compartilhado do grupo.
3. ESTRUTURA DE PASTAS
Crie uma estrutura coerente com o planejamento aprovado:
Raiz:

* README.md.
* prompts.md.
* .gitignore.
* docs/dados.md.
* contracts/README.md.
* backend/.

Dentro de backend/:

* PontoVoz.slnx.
* global.json.
* .editorconfig.
* .config/dotnet-tools.json.
* README.md.
* src/PontoVoz.Api/.
* tests/PontoVoz.Tests/.
* docs/evidencias/.

Crie outras pastas somente quando necessárias. Evite arquivos vazios e classes artificiais apenas para preencher a estrutura.
A solução .NET deve funcionar a partir do diretório backend/.
4. CONFIGURAÇÃO DO .NET
Utilize:

* ASP.NET Core Web API.
* .NET 10.
* Target Framework net10.0.
* Controllers, não Minimal APIs como abordagem principal da aplicação.
* Nullable reference types habilitados.
* Implicit usings habilitados.

Configure global.json com:

* SDK version: 10.0.100.
* rollForward: latestFeature.

Confirme que o SDK 10.0.403 instalado é reconhecido e utilizado.
Não atualize globalmente as ferramentas do Windows.
5. PROJETO DA API
Crie PontoVoz.Api com organização inicial para:

* Controllers.
* Domain.
* Application.
* Infrastructure.
* Security.
* Middleware.

Configure:

* AddControllers e MapControllers.
* OpenAPI nativo do ASP.NET Core.
* Interface Scalar.AspNetCore.
* Injeção de dependência nativa.
* Configurações por ambiente.
* LaunchSettings com portas HTTP 5080 e HTTPS 7080, desde que disponíveis.

A documentação OpenAPI deve ser acessível em Development em:
`/openapi/v1.json`
A interface Scalar deve estar disponível em:
`/scalar`
Crie um endpoint simples GET /health que permita conferir se o processo da API está ativo.
Neste bloco, o health check poderá validar apenas a execução da aplicação. Não afirme que ele verifica o banco de dados antes de implementar essa funcionalidade.
Não crie ainda os Controllers de paradas, linhas, previsões, passagens, destinos ou alternativas.
6. ENTITY FRAMEWORK CORE
Instale dependências compatíveis com .NET 10, considerando:

* Microsoft.EntityFrameworkCore.Sqlite 10.0.x.
* Microsoft.EntityFrameworkCore.Design 10.0.x.

Configure a ferramenta dotnet-ef 10.0.x como ferramenta LOCAL do projeto, com manifesto versionável.
Antes de instalar, verifique as versões estáveis disponíveis e utilize versões compatíveis entre si.
Verifique a instalação da ferramenta com:
`dotnet ef --version`
Não crie ainda DbContext, entidades, migrations ou banco de dados.
Esses elementos pertencem aos próximos blocos.
7. PROJETO DE TESTES
Crie o projeto PontoVoz.Tests utilizando xUnit compatível com net10.0.
Prepare as referências para testar as classes da API.
Adicione as dependências de testes aprovadas no planejamento somente quando forem necessárias e compatíveis.
Não crie testes fictícios como Assert.True(true) apenas para aparentar cobertura.
É aceitável que ainda não existam testes de regras de negócio, pois as entidades e serviços não foram implementados.
Verifique que o projeto de testes compila e que dotnet test funciona, diferenciando explicitamente ausência de testes de testes aprovados.
8. DOCUMENTAÇÃO INICIAL
Crie README.md na raiz contendo:

* Nome PontoVoz.
* Identificação da Global Solution FIAP 2026.2.
* Tema CitizenTech.
* Problema de mobilidade urbana.
* Público beneficiado.
* Explicação da proposta do totem.
* Componentes previstos.
* Contribuição específica do backend C#.
* ODS 10 e 11, com justificativas.
* Campos pendentes para cidade/bairro específico, integrantes, RMs e indicadores que ainda dependerão de validação.
* Status real do desenvolvimento.

Não invente pesquisas de campo, entrevistas, dados de impacto ou nomes de integrantes.
Crie backend/README.md com:

* Requisitos de ambiente.
* Versão do .NET.
* Instruções de restore, build, test e run.
* Como abrir o Scalar.
* Como acessar /health.
* Explicação do estágio atual de implementação.
* Lista das próximas etapas.

Crie docs/dados.md para documentar a futura separação entre dados públicos, dados reais e dados simulados.
Crie contracts/README.md explicando a arquitetura de integração e registrando que os endpoints definitivos serão construídos posteriormente.
Não gere nem publique um contrato OpenAPI que afirme existir endpoint ainda não implementado.
Crie prompts.md para registrar o uso do Claude Code na Global Solution.
Registre os prompts recebidos nesta sessão de forma fiel quando o texto original estiver disponível. Caso não esteja, não invente transcrições; registre uma referência ou resumo claramente identificado como tal.
9. GIT E SEGURANÇA
Inicialize um repositório Git somente LOCAL na raiz do projeto, se ainda não existir.
Configure o branch principal como main.
Não crie repositório remoto.
Não conecte ao GitHub.
Não execute git push.
Não altere configurações globais do Git sem necessidade e autorização.
Configure o .gitignore para impedir versionamento de:

* bin/.
* obj/.
* Arquivos locais de banco SQLite.
* Segredos e credenciais.
* Arquivos de ambiente sensíveis.
* Arquivos temporários.
* Arquivos de usuário e IDE.

Não inclua senhas, tokens ou chaves de API em nenhum arquivo versionável.
Não execute comandos destrutivos como git reset --hard ou limpeza indiscriminada de pastas.
Não faça commit automaticamente antes da minha revisão.
10. HTTPS E EXECUÇÃO
Para a verificação inicial, utilize preferencialmente:
`http://localhost:5080`
Não execute o comando `dotnet dev-certs https --trust` sem minha autorização específica, pois altera o repositório de certificados do Windows.
Não exponha a aplicação em 0.0.0.0 nem em interfaces públicas. Utilize apenas localhost neste bloco.
Inicie a aplicação localmente e verifique:

* Se o processo inicializa sem erros.
* Se GET /health responde com sucesso.
* Se GET /openapi/v1.json retorna um documento OpenAPI válido.
* Se a interface Scalar responde corretamente.

Se alguma verificação falhar, identifique a causa e tente corrigi-la dentro dos limites deste bloco.
Finalize o processo de teste após as verificações, evitando deixar servidores em execução desnecessariamente.
11. VERIFICAÇÕES OBRIGATÓRIAS
Execute os comandos apropriados para comprovar:

1. A versão do SDK utilizada.
2. Restore das dependências.
3. Build da solução.
4. Execução do comando dotnet test.
5. Instalação da ferramenta dotnet-ef local.
6. Inicialização real da API.
7. Resposta real do endpoint /health.
8. Disponibilidade do documento OpenAPI.
9. Disponibilidade da interface Scalar.
10. Ausência de arquivos sensíveis no Git.

Não declare testes aprovados sem executá-los.
Não declare endpoints funcionando com base apenas na leitura do código.
Se algum teste não puder ser executado, informe claramente o impedimento.
12. RELATÓRIO FINAL OBRIGATÓRIO
Quando concluir o Bloco B0, apresente:
A. Resumo do que foi criado.
B. Árvore de pastas e arquivos.
C. Tecnologias e versões instaladas.
D. Comandos executados e resultados observados.
E. Resultado do dotnet build.
F. Resultado do dotnet test, incluindo o número real de testes encontrados e executados.
G. Resultado do dotnet ef --version.
H. Resultado das chamadas HTTP a /health e /openapi/v1.json.
I. Confirmação do funcionamento do Scalar.
J. Estado atual do Git, incluindo a confirmação de que não existe remote configurado e de que não houve commit automático.
K. Problemas encontrados e correções realizadas.
L. Pendências para o próximo bloco.
M. Critérios específicos do professor de C# que já possuem implementação verificável e critérios que ainda estão pendentes.
13. RESTRIÇÃO FINAL
Este bloco é exclusivamente de inicialização e preparação.
Não implemente ainda:

* Entidades completas do domínio.
* DbContext e migrations.
* Repositórios.
* Services de negócio.
* Integração com a SPTrans.
* Previsões de chegada.
* Comparação de trajetos.
* IA conversacional.
* Interfaces do totem.
* Funcionalidades de Testing/QA.

Ao concluir o Bloco B0, pare. Não avance para B1 sem minha autorização.
Nosso objetivo é construir a Global Solution com máxima aderência aos critérios acadêmicos, comprovando cada etapa com resultados reais.
````

**Resumo da resposta (não é transcrição):** a IA criou a estrutura do B0: solução .NET 10
em `backend/`, projetos da API e de testes, pacotes EF Core 10.0.14 e Scalar,
`dotnet-ef` local, endpoint `/health`, documentação inicial e Git local sem remoto e
sem commit. O resultado das verificações está em
`backend/docs/evidencias/B0-inicializacao.md`.

### Prompt 06 — Bloco B0.1: auditoria final e primeiro commit local (06/11/2026)

**Texto original:**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B0.1 — AUDITORIA FINAL E PRIMEIRO COMMIT LOCAL
O relatório de execução do Bloco B0 foi analisado e aprovado, com alguns pontos finais de verificação.
Sua tarefa agora é realizar uma auditoria dos arquivos realmente criados e, se todas as verificações forem satisfatórias, registrar o primeiro commit LOCAL do projeto.
Não execute o Bloco B1 e não implemente funcionalidades de negócio.
1. Verificação dos arquivos
Examine os arquivos reais da solução e confirme:

* Os dois projetos .NET estão corretamente registrados em `PontoVoz.slnx`.
* Ambos utilizam `net10.0`.
* A aplicação usa Controllers.
* O OpenAPI e o Scalar estão configurados corretamente.
* As dependências são compatíveis.
* Não existem classes artificiais ou exemplos desnecessários do template.
* A estrutura permite integração futura com as demais disciplinas.

2. Segurança do Git
Revise o `.gitignore` da raiz, verificando também:

* Arquivos `.db`, `.db-wal`, `.db-shm` e `.db-journal`.
* Arquivos `.sqlite`, `.sqlite3` e seus arquivos auxiliares.
* Diretórios `bin/` e `obj/`.
* Arquivos `.env` e segredos.
* Credenciais e configurações locais sensíveis.
* Arquivos temporários e específicos da IDE.

Corrija o `.gitignore` somente se necessário.
Confirme que nenhum dado sensível será incluído no primeiro commit.
3. Documentação
Verifique se:

* O README principal apresenta corretamente a CitizenTech e a proposta PontoVoz.
* O README do backend contém comandos de execução válidos.
* Os ODS 10 e 11 estão contextualizados.
* Os campos ainda não definidos estão identificados como pendentes.
* O arquivo `prompts.md` registra corretamente o uso de IA.
* Resumos de prompts não são apresentados como transcrições literais.
* A documentação não afirma que existem funcionalidades ainda não desenvolvidas.

4. HealthController
Verifique se a implementação utiliza programação assíncrona de maneira apropriada.
Não crie operações assíncronas artificiais para demonstrar o requisito acadêmico.
O uso relevante de async/await será demonstrado nos próximos blocos por meio de serviços, EF Core e integração HTTP.
Faça qualquer ajuste necessário mantendo o endpoint funcional.
5. Validação final
Execute novamente, a partir de `backend/`:

* `dotnet restore`
* `dotnet build`
* `dotnet test`
* `dotnet tool restore`
* `dotnet ef --version`

Confirme novamente o funcionamento do endpoint `/health` e da documentação OpenAPI.
Não considere a ausência de testes como testes aprovados. Informe a quantidade real encontrada.
6. Primeiro commit
Se todas as verificações estiverem satisfatórias:

1. Confirme que o repositório Git local está na branch `main`.
2. Inspecione os arquivos que serão adicionados.
3. Adicione somente os arquivos pertencentes ao PontoVoz, sem segredos ou arquivos de compilação.
4. Crie um commit local com a mensagem:

`chore: initialize PontoVoz .NET 10 backend`

5. Confirme o hash do commit.
6. Verifique o estado do repositório após o commit.

Não crie um repositório remoto, não conecte ao GitHub e não execute git push.
7. Relatório final
Apresente:

1. Resultado da auditoria dos arquivos.
2. Ajustes realizados e justificativas.
3. Resultado final do build e dos testes.
4. Confirmação de funcionamento da API.
5. Quantidade de arquivos incluídos no commit.
6. Hash e mensagem do commit.
7. Estado do Git, branch e ausência de remoto.
8. Pendências reais, se houver.
9. Confirmação de que o B1 ainda não foi executado.

Caso encontre algum problema relevante que não consiga corrigir com segurança, não realize o commit e explique a situação.
Pare ao concluir esta tarefa e aguarde autorização para iniciar o próximo bloco.
````

**Resumo da resposta (não é transcrição):** a IA auditou os arquivos. Trocou o pacote
`xunit` 2.9.3, marcado como obsoleto no NuGet, pelo `xunit.v3` 4.0.2, ainda sem testes
escritos. Declarou a Microsoft.Testing.Platform no `global.json`, porque o .NET 10 SDK
recusou o modo antigo, e removeu o `coverlet.collector`, incompatível com ela. Acrescentou ao `.gitignore` os arquivos auxiliares do SQLite com extensão
`.sqlite`/`.sqlite3`. Marcou a contribuição do backend no README como escopo planejado
e trocou o exemplo de `/health` pela resposta real. Repetiu as validações e criou o
primeiro commit local, sem repositório remoto. Os resultados estão em
`backend/docs/evidencias/B0-inicializacao.md`.
