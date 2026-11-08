# Registro de uso de inteligência artificial — PontoVoz

Registro dos prompts usados no desenvolvimento do backend C# do PontoVoz, na disciplina
**C# Software Development** da Global Solution FIAP 2026.2 (CitizenTech), conforme a orientação
do material oficial da FIAP de registrar os prompts utilizados e compreender o código do projeto.

| Item | Informação |
|---|---|
| Disciplina | C# Software Development — backend (`backend/` e `contracts/`) |
| Ferramentas | ChatGPT (planejamento, elaboração dos prompts e apoio às revisões) e Claude Code, da Anthropic, no aplicativo desktop (implementação). O Claude Code se identificou na sessão como modelo Claude Opus 5.5; a versão do ChatGPT não está registrada. |
| Integrante responsável | Pendente (nome e RM) |
| Período | 06/11/2026 em diante |
| Prompts registrados | 32 |

## Como a IA foi usada

Ferramentas de IA participaram de **todas** as etapas registradas abaixo, do diagnóstico do
ambiente à publicação de cada bloco no GitHub. Duas ferramentas foram usadas, com papéis
diferentes.

**ChatGPT:**

- auxiliou no planejamento do projeto e na elaboração dos prompts registrados neste arquivo;
- ajudou a definir os critérios de revisão de cada etapa;
- auxiliou na auditoria dos materiais enviados a ele: relatórios e arquivos produzidos pelo
  Claude Code. As auditorias citadas nos prompts 11 e 13 ("Realizei uma auditoria
  independente...") foram feitas com esse apoio.

**Claude Code:**

- recebeu os prompts registrados abaixo e executou o trabalho técnico de todas as etapas;
- diagnosticou o ambiente e propôs a arquitetura (prompts 01 a 04);
- escreveu o contrato REST, os exemplos JSON e o verificador do contrato (prompts 08 a 11);
- escreveu todo o código C# do backend: domínio, persistência com EF Core, migrations, seed,
  repositórios, consultas LINQ, services, controllers, tratamento de erros e a proteção das
  rotas administrativas;
- escreveu os testes automatizados, executou-os e fez testes de mutação para conferir se eles
  detectam regressões;
- preparou a documentação técnica (READMEs e evidências) e este registro;
- executou os comandos (`dotnet`, `dotnet ef`, `git`), incluindo os commits e os pushes, só quando
  autorizados no prompt e com a identidade Git do integrante (e-mail *noreply* do GitHub).

**Integrante:**

- encaminhou as instruções ao Claude Code e forneceu arquivos às ferramentas;
- definiu preferências, como a apresentação do repositório e as regras de commit (prompt 08);
- tomou decisões pontuais quando a ferramenta apresentou uma escolha, como o uso do e-mail
  *noreply* nos commits (prompt 07);
- autorizou cada etapa e cada publicação, com a mensagem de commit definida no prompt.

Não houve edição manual do código pelo integrante nas etapas registradas. A verificação técnica de
cada etapa foi executada pelo Claude Code, com os comandos registrados nas evidências
(`backend/docs/evidencias/`).

**Commits:** o primeiro commit (`f7a8ccf`) contém um trailer `Co-Authored-By` do Claude Code, criado
antes da diretriz do prompt 08. A partir dela, por decisão do integrante, os commits não levam
assinatura de ferramenta, e o uso da IA fica registrado neste arquivo. O histórico não foi
reescrito.

## Como ler este registro

- **Instrução original:** o texto integral do prompt, exatamente como foi enviado. A numeração
  deste arquivo é sequencial; alguns prompts têm numeração própria no texto (por exemplo,
  "PROMPT 05 — BLOCO B3"), mantida sem alteração.
- **Resultado:** resumo do que o Claude Code entregou, **escrito pelo próprio Claude Code** ao fim
  da etapa. Não é uma transcrição da resposta, que era longa. Nos resumos, "a IA" e "a ferramenta"
  se referem ao Claude Code.
- **Tipo:** diagnóstico, planejamento, implementação, revisão e correção, ou auditoria e
  publicação.
- **Commit:** o commit publicado ao fim da etapa, quando houve.

## Índice

| Nº | Data | Etapa | Tipo | Commit |
|---|---|---|---|---|
| [01](#prompt-01) | 06/11/2026 | Diagnóstico do ambiente local | Diagnóstico | — |
| [02](#prompt-02) | 06/11/2026 | Verificação do .NET 10 SDK | Diagnóstico | — |
| [03](#prompt-03) | 06/11/2026 | Etapa 01: planejamento e arquitetura | Planejamento | — |
| [04](#prompt-04) | 06/11/2026 | Etapa 01.1: revisão e consolidação do planejamento | Planejamento (revisão) | — |
| [05](#prompt-05) | 06/11/2026 | Bloco B0: inicialização do backend | Implementação | — |
| [06](#prompt-06) | 06/11/2026 | Bloco B0.1: auditoria final e primeiro commit local | Auditoria e commit local | `f7a8ccf` (local) |
| [07](#prompt-07) | 06/11/2026 | Etapa B0.2: conexão e publicação no GitHub | Publicação | `f7a8ccf` |
| [08](#prompt-08) | 07/11/2026 | Bloco B1: contratos REST e diretriz permanente | Documentação (contrato REST) e diretriz | — |
| [09](#prompt-09) | 07/11/2026 | Bloco B1.1: revisão técnica final dos contratos REST | Revisão e correção | — |
| [10](#prompt-10) | 07/11/2026 | Bloco B1.2: consolidação final dos contratos | Revisão e correção | — |
| [11](#prompt-11) | 07/11/2026 | Bloco B1.3: correções finais e publicação do contrato | Revisão, correção e publicação | `370fab4` |
| [12](#prompt-12) | 07/11/2026 | Bloco B2: implementação do domínio e Programação Orientada a Objetos | Implementação e testes | — |
| [13](#prompt-13) | 07/11/2026 | Bloco B2.1: correções de domínio e testes de regressão | Revisão, correção e testes | — |
| [14](#prompt-14) | 07/11/2026 | Bloco B2.2: auditoria final e publicação do domínio C# | Auditoria e publicação | `f53fe2d` |
| [15](#prompt-15) | 07/11/2026 | Bloco B3: Entity Framework Core, SQLite, migrations e persistência | Implementação e testes | — |
| [16](#prompt-16) | 07/11/2026 | Bloco B3.1: correções finais, validação e publicação do EF Core | Revisão, correção, testes e publicação | `988a5fc` |
| [17](#prompt-17) | 07/11/2026 | Bloco B4: repositórios, interfaces e consultas LINQ | Implementação e testes | — |
| [18](#prompt-18) | 07/11/2026 | Bloco B4.1: auditoria final e publicação das consultas LINQ | Auditoria, correção e publicação | `38d2dae` |
| [19](#prompt-19) | 07/11/2026 | Bloco B5: serviços de negócio, DTOs, validações e testes | Implementação, testes e documentação (inclui diretriz do prompts.md) | — |
| [20](#prompt-20) | 07/11/2026 | Bloco B5.1: auditoria final, correção documental e publicação | Auditoria, correção documental e publicação | `feat: implement application services and DTO validation` |
| [21](#prompt-21) | 07/11/2026 | Bloco B6: controllers REST públicos, ProblemDetails, OpenAPI e testes HTTP | Implementação, testes e documentação | — |
| [22](#prompt-22) | 07/11/2026 | Bloco B6.1: auditoria final e publicação da Web API pública | Auditoria, correção e publicação | `feat: implement public REST API and OpenAPI documentation` |
| [23](#prompt-23) | 07/11/2026 | Bloco B7: API administrativa, autorização, cadastros e observações | Implementação e testes (interrompida por limite de uso) | — |
| [24](#prompt-24) | 08/11/2026 | Bloco B7 (09.1): retomada e conclusão após a interrupção | Continuação: testes, documentação e evidências | — |
| [25](#prompt-25) | 08/11/2026 | Bloco B7.2: pacote ZIP para auditoria independente do código | Preparação para auditoria | — |
| [26](#prompt-26) | 08/11/2026 | Bloco B7.3: validação final e publicação do B7 | Validação e publicação | `feat: implement secured administrative REST API` |
| [27](#prompt-27) | 08/11/2026 | Bloco B8: previsões de chegada, provedor simulado, adaptador Olho Vivo e testes | Implementação, testes e documentação | — |
| [28](#prompt-28) | 08/11/2026 | Bloco B8.1: pacote ZIP para auditoria independente do código | Preparação para auditoria | — |
| [29](#prompt-29) | 08/11/2026 | Bloco B8.2: validação final e publicação do B8 | Validação e publicação | `feat: implement simulated arrival predictions and Olho Vivo adapter` |
| [30](#prompt-30) | 08/11/2026 | Bloco B9: alternativas de deslocamento, ranking e resumo agregado do totem | Implementação, testes e documentação | — |
| [31](#prompt-31) | 08/11/2026 | Bloco B9.1: pacote ZIP para auditoria independente do código | Preparação para auditoria | — |
| [32](#prompt-32) | 08/11/2026 | Bloco B9.2: correções documentais, validação final e publicação do B9 | Correção, validação e publicação | `feat: implement travel alternatives and stop summary` |

---

## Ambiente e planejamento

<a id="prompt-01"></a>

### Prompt 01 — Diagnóstico do ambiente local

**Data:** 06/11/2026 · **Tipo:** Diagnóstico · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

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

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA executou apenas comandos de leitura.
Constatou pasta vazia, Git 2.44.0, .NET SDK 8.0.421 como único SDK, templates Web API
disponíveis, NuGet acessível, certificado HTTPS de desenvolvimento existente mas não
confiável, VS Code sem extensão C#, `dotnet-ef` e Docker ausentes. Recomendou confiar
no certificado, instalar o C# Dev Kit e, se exigido, o SDK do .NET 10.

<a id="prompt-02"></a>

### Prompt 02 — Verificação do .NET 10 SDK

**Data:** 06/11/2026 · **Tipo:** Diagnóstico · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
Instalei o .NET 10 SDK no meu computador. Verifique, usando apenas comandos de leitura, se a instalação foi reconhecida corretamente. Execute dotnet --list-sdks e dotnet --version, confira se os templates ASP.NET Core Web API continuam disponíveis e informe se podemos usar net10.0 no projeto PontoVoz da Global Solution. Não crie arquivos, não instale dependências e não inicie a implementação. Quero apenas a confirmação técnica do ambiente.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA confirmou os SDKs 8.0.421 e 10.0.403,
com `dotnet --version` igual a 10.0.403 na pasta do projeto, runtime ASP.NET Core 10.0.14
e o template `webapi` presente no pacote de templates do .NET 10. Concluiu que `net10.0`
pode ser usado.

<a id="prompt-03"></a>

### Prompt 03 — Etapa 01: planejamento e arquitetura

**Data:** 06/11/2026 · **Tipo:** Planejamento · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

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

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA apresentou o planejamento nas seções
A a M: Web API em .NET 10 com Controllers, EF Core 10 com SQLite, um projeto de API
organizado em pastas e um projeto de testes xUnit. Propôs entidades de parada, linha,
associação e observação de passagem, um provedor de previsões trocável (simulado ou
Olho Vivo), cerca de 17 endpoints, matriz da rubrica, blocos B0 a B9 e riscos, como
a limitação do SQLite com `DateTimeOffset`.

<a id="prompt-04"></a>

### Prompt 04 — Etapa 01.1: revisão e consolidação do planejamento

**Data:** 06/11/2026 · **Tipo:** Planejamento (revisão) · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

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

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA consolidou o plano. Alternativas de
deslocamento entraram no MVP, por linhas diretas, ordenadas pelo tempo total estimado
(espera + percurso), com as entidades `Destino` e `PercursoEstimado`. Rotas
administrativas passaram a ficar em `/api/v1/admin`, protegidas por chave em cabeçalho.
A justificativa assíncrona foi corrigida, com o I/O real demonstrado no adaptador HTTP.
Foram definidos a estrutura `backend/` + `contracts/`, quatro níveis de confirmação de
passagem, 13 endpoints mais `/health`, a matriz da rubrica, os blocos B0 a B10 e o
checklist para autorizar o B0.

---

## Bloco B0 — inicialização e publicação do repositório

<a id="prompt-05"></a>

### Prompt 05 — Bloco B0: inicialização do backend

**Data:** 06/11/2026 · **Tipo:** Implementação · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

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

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA criou a estrutura do B0: solução .NET 10
em `backend/`, projetos da API e de testes, pacotes EF Core 10.0.14 e Scalar,
`dotnet-ef` local, endpoint `/health`, documentação inicial e Git local sem remoto e
sem commit. O resultado das verificações está em
`backend/docs/evidencias/B0-inicializacao.md`.

<a id="prompt-06"></a>

### Prompt 06 — Bloco B0.1: auditoria final e primeiro commit local

**Data:** 06/11/2026 · **Tipo:** Auditoria e commit local · **Commit:** `f7a8ccf` (local)

**Instrução original (texto integral, como foi enviada):**

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

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA auditou os arquivos. Trocou o pacote
`xunit` 2.9.3, marcado como obsoleto no NuGet, pelo `xunit.v3` 4.0.2, ainda sem testes
escritos. Declarou a Microsoft.Testing.Platform no `global.json`, porque o .NET 10 SDK
recusou o modo antigo, e removeu o `coverlet.collector`, incompatível com ela. Acrescentou ao `.gitignore` os arquivos auxiliares do SQLite com extensão
`.sqlite`/`.sqlite3`. Marcou a contribuição do backend no README como escopo planejado
e trocou o exemplo de `/health` pela resposta real. Repetiu as validações e criou o
primeiro commit local, sem repositório remoto. Os resultados estão em
`backend/docs/evidencias/B0-inicializacao.md`.

<a id="prompt-07"></a>

### Prompt 07 — Etapa B0.2: conexão e publicação no GitHub

**Data:** 06/11/2026 · **Tipo:** Publicação · **Commit:** `f7a8ccf`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
ETAPA B0.2 — CONEXÃO E PUBLICAÇÃO NO GITHUB
Concluímos a inicialização e auditoria do backend C# do PontoVoz.
Criei um repositório GitHub dentro da organização oficial da equipe.
Repositório: https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
URL remota: https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp.git
Diretório local: C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz
Sua tarefa é conectar o repositório Git local ao GitHub e publicar o commit inicial existente, respeitando rigorosamente as instruções abaixo.
1. Verificação do projeto local
Antes de qualquer alteração, execute as verificações necessárias para confirmar:

* Estamos no diretório correto.
* O repositório Git local já existe.
* A branch atual é main.
* O commit inicial `0c0bf4a7af967abf8f9dbe2b7f08a47c0f2ad043` está presente.
* A árvore de trabalho está limpa.
* Os arquivos do backend, contratos e documentação estão preservados.

Não inicialize outro repositório Git.
2. Auditoria de segurança antes da publicação
Examine os arquivos rastreados e o histórico que será enviado.
Confirme que não existem:

* Tokens ou chaves de API.
* Senhas e credenciais.
* Arquivos de ambiente sensíveis.
* Bancos de dados locais.
* Arquivos binários ou de compilação desnecessários.
* Informações pessoais ou privadas indevidas, inclusive dentro do prompts.md.
* Conteúdo acadêmico ou documentação marcada incorretamente como concluída.

O repositório pode ser público posteriormente, portanto considere também a exposição futura do histórico Git.
Se encontrar algum problema relevante, interrompa a publicação e informe o motivo. Não reescreva o histórico automaticamente.
3. Conexão ao GitHub
Confirme que o repositório remoto informado corresponde à organização PONTOVOZ-GLOBAL-SOLUTION.
Verifique se já existe um remote chamado origin.
Se não existir, configure:
git remote add origin https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp.git
Depois confirme a configuração com:
git remote -v
Confira também se o repositório remoto está vazio, evitando conflitos com históricos existentes.
Não utilize force push.
4. Publicação
Se todas as verificações forem satisfatórias, publique a branch main utilizando:
git push -u origin main
Se o GitHub solicitar autenticação, utilize o fluxo oficial de login do Git ou Git Credential Manager.
Não solicite que eu compartilhe senhas ou tokens diretamente na conversa.
Se a autenticação falhar, explique o que aconteceu e interrompa a operação, sem tentar soluções inseguras.
5. Verificação após publicação
Após o push, confirme:

1. Se a branch main foi publicada.
2. Se o remote origin está configurado corretamente.
3. Se o commit inicial aparece no GitHub.
4. Se os arquivos README.md, prompts.md, backend/ e contracts/ foram enviados.
5. Se o repositório local continua com a árvore limpa.
6. Se não houve alterações inesperadas.
7. Se o endereço do repositório está correto.

6. Relatório final
Apresente:

* URL do repositório publicado.
* Hash do commit remoto.
* Resultado real do git push.
* Branch local e remota.
* Confirmação dos arquivos publicados.
* Eventuais problemas encontrados.
* Próxima etapa recomendada.

Importante: não implemente nenhuma funcionalidade nova. Não avance para B1, não modifique a arquitetura e não crie commits adicionais sem necessidade.
Esta etapa é exclusivamente para conectar e publicar o backend já desenvolvido.
Ao concluir, pare e aguarde minha revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA conferiu o repositório local e o remoto, que
estava vazio e já era público, e auditou os arquivos antes de publicar. Encontrou o e-mail pessoal
do integrante nos metadados do commit e perguntou como prosseguir. O integrante escolheu o e-mail
*noreply* do GitHub. A IA configurou esse e-mail só no repositório local e refez o autor do commit,
que ainda não tinha sido publicado, sem alterar nenhum arquivo. Depois conectou o `origin`,
executou `git push -u origin main` sem force push e conferiu no GitHub que os 25 arquivos
publicados eram iguais aos locais. Commit publicado: `f7a8ccf`.

---

## Bloco B1 — contrato REST

<a id="prompt-08"></a>

### Prompt 08 — Bloco B1: contratos REST e diretriz permanente

**Data:** 07/11/2026 · **Tipo:** Documentação (contrato REST) e diretriz · **Commit:** —

O prompt foi enviado em uma única mensagem, com duas partes.

**Instrução original (texto integral) — parte 1:**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B1 — DEFINIÇÃO E DOCUMENTAÇÃO DOS CONTRATOS REST
O Bloco B0 foi concluído e publicado com sucesso no GitHub.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
A arquitetura definida na Etapa 01.1 foi aprovada.
Agora você deverá executar exclusivamente o Bloco B1 — Contratos da API, preparando a documentação técnica que será utilizada pelo backend C# e pelos demais integrantes da Global Solution.
Não implemente funcionalidades de negócio nesta etapa.
1. Contexto obrigatório
O PontoVoz é uma solução CitizenTech para mobilidade urbana em São Paulo.
A proposta consiste em um totem inteligente instalado em pontos de ônibus, permitindo interação por voz e tela.
O cidadão poderá:

1. Consultar previsões de chegada.
2. Consultar a última passagem registrada.
3. Obter alternativas de deslocamento.
4. Conversar com um assistente de IA que utilizará informações do backend.

A disciplina C# Software Development exige POO, Web API, EF Core, LINQ, async/await, interfaces, injeção de dependência, testes unitários e qualidade de código.
A documentação produzida neste bloco deve preservar a arquitetura aprovada e orientar a implementação futura desses conceitos.
2. Verificação inicial
Antes de editar, confirme:

* A pasta de trabalho correta.
* A presença da solução ASP.NET Core.
* O estado atual do Git.
* A branch main.
* O remote origin.
* A existência do commit publicado.
* Os documentos atuais em contracts/.

Não sobrescreva alterações preexistentes sem inspecioná-las.
Se houver diferenças inesperadas no repositório, interrompa e informe.
3. Contratos REST a documentar
Mantenha os endpoints definidos na arquitetura consolidada.
Consultas públicas:

1. GET /api/v1/paradas
2. GET /api/v1/paradas/{id}
3. GET /api/v1/linhas
4. GET /api/v1/destinos
5. GET /api/v1/paradas/{id}/previsoes
6. GET /api/v1/paradas/{id}/ultimas-passagens
7. GET /api/v1/paradas/{id}/alternativas
8. GET /api/v1/paradas/{id}/resumo

Operações administrativas:

9. POST /api/v1/admin/paradas
10. PUT /api/v1/admin/paradas/{id}
11. POST /api/v1/admin/linhas
12. PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}
13. POST /api/v1/admin/observacoes-passagem

Verificação de saúde:
GET /health
O endpoint /health já existe no projeto.
Os demais endpoints estão planejados e NÃO devem ser descritos como implementados.
Caso identifique inconsistências na lista, explique-as antes de fazer alterações de escopo.
4. Documentação obrigatória de cada endpoint
Para cada operação, documente:

* Método HTTP.
* Rota completa.
* Finalidade.
* Quem utilizará o endpoint.
* Parâmetros de rota.
* Query parameters.
* Corpo da requisição, quando aplicável.
* Tipos de dados.
* Campos obrigatórios e opcionais.
* Exemplo JSON de sucesso.
* Códigos HTTP esperados.
* Exemplos de erros.
* Regras de validação.
* Restrições de acesso.
* Dependências de outros módulos.

Os exemplos devem representar contratos planejados, não resultados de execuções que ainda não aconteceram.
Utilize JSON válido e convenções consistentes.
5. Padronização dos contratos
Defina convenções claras para:

* JSON em camelCase.
* Nomes dos campos em português.
* IDs internos e códigos externos de linhas e paradas.
* Datas em ISO 8601 e tratamento correto de UTC e fuso de São Paulo.
* Valores nulos.
* Paginação.
* Filtros e ordenação.
* Enums representados como texto.
* ProblemDetails para erros.
* Campos de origem e confiabilidade dos dados.
* Versionamento da API.
* Cancelamento e indisponibilidade de serviços externos.

As convenções devem ser fáceis de consumir por um frontend ou assistente de IA.
6. Regras específicas de negócio
Previsões de chegada
As respostas deverão distinguir:

* Previsão disponível.
* Ausência de previsão.
* Provedor indisponível.
* Dados simulados.

Inclua origem dos dados, horário da consulta e aviso quando necessário.
Não invente dados da SPTrans.
Última passagem
Diferencie:

* Observação simulada.
* Registro manual sem verificação.
* Passagem confirmada por fonte confiável, ainda não implementada.

A resposta deve deixar claro o nível de confirmação e nunca apresentar uma observação simulada como registro oficial.
Alternativas de deslocamento
O MVP considera somente linhas diretas entre a parada e um destino cadastrado.
A comparação deve utilizar:
tempo total estimado = espera prevista + tempo estimado de percurso.
Não classifique como mais rápida uma alternativa sem dados suficientes.
Se não houver previsão, defina explicitamente como a alternativa será representada, incluindo valores nulos e sua posição na ordenação.
Não utilize rotas inventadas como se fossem reais.
Resumo do totem
O endpoint de resumo deve permitir que a interface obtenha em uma chamada:

* Informações da parada.
* Linhas disponíveis.
* Previsões de chegada.
* Últimas passagens registradas.
* Avisos e estados de disponibilidade.

O resumo deverá continuar útil mesmo quando o provedor de previsões estiver indisponível.
7. Operações administrativas
Documente as operações administrativas separadamente das consultas públicas.
O planejamento prevê um cabeçalho:
X-PontoVoz-Admin-Key
A implementação futura deverá distinguir ambientes Development e ambientes externos, conforme a estratégia de segurança já aprovada.
Não coloque nenhuma chave real nos exemplos.
Utilize placeholders claramente identificados.
8. Arquivos a produzir
Organize a documentação em contracts/, preferencialmente com:

* contracts/README.md
* contracts/api-v1.md
* contracts/exemplos/

Crie exemplos JSON úteis para as funcionalidades principais, incluindo:

* Listagem de paradas.
* Previsões de chegada.
* Última passagem.
* Alternativas de deslocamento.
* Resumo para o totem.
* Uma requisição administrativa.
* Erro no padrão ProblemDetails.

Não crie dezenas de arquivos desnecessários. Use uma estrutura objetiva e fácil de consultar pelos colegas.
Os exemplos deverão ser válidos e coerentes entre si.
Se ainda não existir o OpenAPI completo gerado pela API, não fabrique um arquivo afirmando que o contrato já está implementado.
O documento OpenAPI gerado automaticamente pela aplicação somente poderá representar endpoints efetivamente implementados.
9. Compatibilidade com a equipe
A documentação deverá explicar como outros integrantes poderão consumir os endpoints.
Inclua:

* URL base local da API.
* Padrões de respostas.
* Como consultar uma parada.
* Como consultar previsões.
* Como localizar um destino.
* Como obter alternativas.
* Como utilizar o resumo.
* Como interpretar avisos e dados simulados.

Forneça um exemplo simples da jornada:
"O cidadão está na parada X, pergunta pelo ônibus e recebe a previsão e uma possível alternativa."
Explique quais operações pertencem ao backend e quais dependem do componente de IA ou interface.
Não implemente a IA conversacional.
10. Conferência de consistência
Antes de concluir:

* Confirme que as rotas não entram em conflito.
* Confirme que os exemplos JSON são válidos.
* Verifique os relacionamentos entre IDs.
* Verifique os códigos de status.
* Confirme que os campos de data são coerentes.
* Confirme que a documentação diferencia recursos planejados dos implementados.
* Confira que nenhuma funcionalidade foge da proposta PontoVoz.

Estabeleça uma política coerente para respostas vazias, recursos inexistentes e serviços indisponíveis.
Se alguma decisão do planejamento anterior precisar ser corrigida, documente a divergência e a justificativa. Não faça alterações silenciosas.
11. Registro e Git
Atualize prompts.md com o prompt recebido nesta etapa, preservando o registro do uso da IA.
Não modifique o código-fonte da API, exceto se houver uma correção documental estritamente necessária em seus arquivos de configuração. Nesse caso, explique antes.
Não realize commit automaticamente.
Não execute git push.
O repositório é público, e quero revisar a documentação antes de publicá-la.
12. Relatório final obrigatório
Ao finalizar, apresente:

1. Resumo da documentação criada.
2. Árvore dos novos arquivos.
3. Relação completa dos endpoints.
4. Principais decisões contratuais.
5. Exemplos JSON produzidos.
6. Resultado das validações.
7. Compatibilidade com a arquitetura C#.
8. Compatibilidade com a proposta CitizenTech.
9. Divergências identificadas e soluções propostas.
10. Estado atual do Git.
11. Pendências para aprovação.
12. Checklist para iniciar o Bloco B2 — Domínio e POO.

Não avance para o B2.
Pare ao concluir a documentação e aguarde minha aprovação.
Nosso objetivo é entregar uma Web API altamente coerente com o PontoVoz, tecnicamente consistente e alinhada à rubrica da FIAP.
````

**Instrução original (texto integral) — parte 2:**

````text
DIRETRIZ PERMANENTE — PADRONIZAÇÃO DO CÓDIGO E DOS COMMITS
A partir desta etapa, adote as seguintes diretrizes em todo o desenvolvimento do PontoVoz:

1. Código profissional: todos os arquivos devem seguir as boas práticas de desenvolvimento C#/.NET, com nomes claros, comentários técnicos pertinentes e organização consistente.
2. Sem marcas automáticas de ferramentas: não adicione cabeçalhos, comentários, banners, badges ou mensagens promocionais como "Generated by Claude", "AI Generated" ou equivalentes nos arquivos de código.
3. Mensagens de commit: utilize mensagens convencionais, técnicas e objetivas, como `feat: add bus stop domain entities` ou `fix: validate arrival predictions`. Não acrescente textos automáticos de divulgação ou assinaturas de ferramentas às mensagens.
4. Autoria e histórico: mantenha os metadados de autoria corretos, preserve o histórico existente e não reescreva commits para ocultar informações. Antes de qualquer commit, apresente a mensagem proposta para revisão.
5. Transparência acadêmica: preserve o arquivo `prompts.md` e registre o uso de IA conforme as orientações da FIAP, sem inserir referências redundantes nos demais arquivos.
6. Documentação: o README deve priorizar o problema CitizenTech, a arquitetura, as funcionalidades, as tecnologias, as instruções de execução e as evidências técnicas.
7. Aplicação permanente: considere estas diretrizes em todas as próximas etapas do projeto, mantendo-as compatíveis com os requisitos acadêmicos.

Neste Bloco B1, não realize commit nem push. Aguarde minha aprovação após a auditoria da documentação.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA escreveu o rascunho do contrato v1 em
`contracts/api-v1.md` e o guia da equipe em `contracts/README.md`. Os documentos trazem as
convenções, a política de respostas, os 13 endpoints planejados e o `/health` implementado, os
catálogos de erros e avisos, a conferência das rotas e as divergências em relação à Etapa 01.1.
Gerou 12 exemplos JSON fictícios e coerentes entre si em `contracts/exemplos/` e os validou com
um script. Nenhum código-fonte da API foi alterado, e nenhum commit foi feito.

<a id="prompt-09"></a>

### Prompt 09 — Bloco B1.1: revisão técnica final dos contratos REST

**Data:** 07/11/2026 · **Tipo:** Revisão e correção · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B1.1 — REVISÃO TÉCNICA FINAL DOS CONTRATOS REST
O relatório do Bloco B1 foi analisado.
A documentação foi considerada bem estruturada, mas precisamos corrigir e validar alguns aspectos antes de aprovar os contratos e avançar para a implementação do domínio C#.
Sua tarefa é revisar os arquivos reais existentes em contracts/, preservar o trabalho correto e realizar exclusivamente os ajustes necessários.
Não implemente endpoints, não modifique o código C#, não crie commits e não execute git push.
1. Semântica dos códigos HTTP
Reavalie o comportamento do endpoint:
GET /api/v1/paradas/{id}/previsoes
Adote uma política consistente:

* 200 quando houver previsões disponíveis.
* 200 com status SemPrevisoes quando a consulta funcionar, mas não houver previsão.
* 503 quando a dependência de previsões estiver indisponível e a consulta não puder ser atendida.
* 404 quando a parada não existir.
* 422 quando a linha informada não atender à parada, conforme as regras do domínio.

Para:
GET /api/v1/paradas/{id}/resumo
Mantenha 200 quando for possível retornar informações parciais úteis, mesmo com o provedor de previsões indisponível.
Inclua avisos estruturados e status explícitos nas respostas parciais.
Ajuste a documentação e os exemplos para eliminar divergências.
2. Datas e simulação
Confirme que os horários fixos de 10/11/2026 pertencem apenas aos exemplos documentais.
A implementação futura do provedor simulado deverá calcular horários relativos ao TimeProvider.
Isso permitirá executar a aplicação em qualquer data sem apresentar previsões vencidas.
Não modifique código da aplicação nesta etapa. Registre essa regra como requisito para os próximos blocos.
Revise também a estratégia de registros de passagem simulados, evitando que o seed produza um histórico que se torne enganoso após alguns dias.
3. Identificação de parada pelo totem
O totem poderá conhecer o codigoExterno da parada, como SIM-P001, sem conhecer o ID interno do banco.
Garanta que GET /api/v1/paradas?codigoExterno=... permita identificar a parada corretamente.
Documente a sequência completa:

1. Totem conhece o código externo.
2. Consulta o endpoint de busca.
3. Obtém o identificador interno.
4. Consulta previsões, últimas passagens, alternativas ou resumo.

Confirme a consistência dos exemplos JSON.
4. Registros de passagem
Revise a mudança no limite temporal de sete dias para 24 horas.
Diferencie claramente:

* Validação temporal de um novo registro manual.
* Persistência histórica no banco de dados.
* Consulta da última passagem registrada.
* Indicação de registro antigo ou desatualizado.
* Ausência de registros.

Defina uma regra simples, coerente e testável.
Não apresente previsões nem observações manuais como passagens oficialmente confirmadas.
Documente as decisões no contrato, inclusive os casos sem registros.
5. Alternativas de deslocamento
Verifique:

* Uso do tempo total estimado de viagem.
* Cálculo da espera somada ao percurso.
* Ordenação das alternativas efetivamente comparáveis.
* Tratamento de linhas sem previsão.
* Uso de null quando um valor não puder ser determinado.
* Semântica de comparacaoCompleta.
* Status e avisos quando nenhuma alternativa puder ser comparada.

Não declare que determinada linha é a mais rápida em termos absolutos se os dados forem incompletos.
O MVP permanece restrito a trajetos diretos cadastrados, sem baldeações.
Os tempos simulados devem ser identificados explicitamente.
6. Revisão das dez divergências
No relatório B1 você informou dez divergências, D1 a D10.
Revisite todas elas individualmente.
Apresente uma matriz contendo:

* Identificador da divergência.
* Comportamento anterior.
* Comportamento consolidado.
* Justificativa técnica.
* Arquivo de documentação afetado.
* Impacto na implementação futura.

Não faça mudanças silenciosas.
7. Consistência e reprodutibilidade
Valide novamente os arquivos de contracts/.
Confira:

* Sintaxe dos JSONs.
* Coerência de IDs e códigos externos.
* Datas, fusos e cálculos.
* Status HTTP.
* Avisos e erros.
* Tipos, campos opcionais e valores nulos.
* Paginação.
* Nomes de enums.
* Relação entre rotas e funcionalidades do PontoVoz.
* Coerência com o plano C# aprovado.

Informe exatamente quais verificações foram executadas e quais resultados foram obtidos.
Se a validação depender de um script temporário, considere preservar um verificador simples e reproduzível dentro de contracts/, com instruções de execução. Evite dependências adicionais desnecessárias.
Não apresente a quantidade de verificações como garantia absoluta de correção.
8. Escopo acadêmico
Confirme que todos os contratos continuam relacionados à ideia do totem inteligente da Global Solution CitizenTech.
Não acrescente funcionalidades desvinculadas da mobilidade urbana.
Preserve os requisitos da disciplina C#:

* POO.
* Interfaces.
* Injeção de dependência.
* Web API.
* Entity Framework Core e LINQ.
* Async/Await.
* Testes unitários.
* Qualidade de código.

Não confunda contratos planejados com endpoints já implementados.
9. Documentação e Git
Atualize o arquivo prompts.md com o registro desta solicitação.
Preserve o padrão profissional do repositório e as diretrizes acadêmicas já estabelecidas.
Não altere o histórico de commits.
Não faça commit.
Não execute push.
10. Relatório final
Ao concluir, apresente:

1. Arquivos revisados e modificados.
2. Correções realizadas.
3. Tabela completa das divergências D1 a D10.
4. Política final dos códigos HTTP.
5. Regras finais para dados simulados e última passagem.
6. Resultado real das validações.
7. Estado atual do Git.
8. Pendências, caso existam.
9. Checklist para autorizar o B2.

Não inicie B2.
Pare e aguarde minha revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA revisou os arquivos de `contracts/`. Mudou o
endpoint de previsões para devolver 503 quando o provedor está indisponível, mantendo 200 com
status explícito no resumo e nas alternativas. Renomeou o status para `SemPrevisoes` e definiu
regras separadas para validar, guardar, consultar e marcar como desatualizada uma passagem.
Informou que o limite de 7 dias do planejamento tinha sido trocado por 24 horas no B1 sem
registro e documentou a decisão. Acrescentou ao resumo `situacao` e `partesIndisponiveis`,
detalhou a identificação da parada pelo código externo e registrou os requisitos R1 a R8 para
os próximos blocos. Consolidou a matriz de divergências D1 a D15, gerou 15 exemplos e criou um
verificador em C# (`contracts/verificador/VerificarContrato.cs`), testado também com defeitos
plantados de propósito. Nenhum código da API foi alterado, e nenhum commit foi feito.

<a id="prompt-10"></a>

### Prompt 10 — Bloco B1.2: consolidação final dos contratos

**Data:** 07/11/2026 · **Tipo:** Revisão e correção · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B1.2 — CONSOLIDAÇÃO FINAL DOS CONTRATOS
A revisão B1.1 foi analisada. A arquitetura contratual foi aprovada, com um único ajuste obrigatório relacionado às passagens simuladas.
As seguintes decisões estão APROVADAS e não devem ser alteradas:

1. HTTP 503 no endpoint exclusivo de previsões quando o provedor estiver indisponível.
2. HTTP 200 em alternativas e resumo quando houver informações parciais úteis, acompanhadas de status e avisos explícitos.
3. Validação de novas observações manuais entre 24 horas no passado e dois minutos no futuro.
4. Marcação `desatualizada` para registros de passagem com mais de 60 minutos.
5. Comparação de alternativas pelo tempo total estimado, apenas quando os dados forem suficientes.
6. Preservação da distinção entre previsão, observação manual, simulação e passagem verificada.

1. Ajuste obrigatório — Dados simulados
Revise as regras R3 e R4 do contrato.
Não quero que o seed atualize automaticamente os horários de passagens persistidas toda vez que a aplicação iniciar.
Adote a seguinte abordagem:

* O seed deverá ser idempotente, criando os dados demonstrativos necessários sem duplicar registros.
* Registros históricos de passagem não devem ter seus horários modificados automaticamente.
* As previsões simuladas devem ser calculadas dinamicamente utilizando `TimeProvider`.
* As observações de passagem devem manter seus horários originais.
* Registros antigos devem permanecer identificados como desatualizados.
* Para demonstrar uma passagem recente, podemos utilizar a operação administrativa de registro manual, com fonte e nível de confirmação claramente identificados.
* Nenhuma observação simulada ou manual pode ser apresentada como passagem oficialmente verificada.
* Não gerar eventos históricos fictícios automaticamente como consequência de reiniciar a API.

Os exemplos JSON documentais podem continuar usando horários fixos, desde que identificados como exemplos fictícios.
2. Revisão da documentação
Atualize somente os trechos afetados em:

* contracts/api-v1.md
* contracts/README.md
* contracts/exemplos/, caso necessário

Preserve as demais decisões aprovadas.
Se a correção gerar uma nova divergência, registre-a no histórico de decisões.
3. Verificação
Execute novamente o verificador de contratos.
Confirme:

* JSON válido.
* Consistência dos horários.
* Códigos HTTP.
* Estados de indisponibilidade.
* Relação entre os exemplos.
* Regras de registros de passagem.
* Ausência de dados sensíveis.

Se o verificador não contemplar uma regra importante, amplie-o de maneira objetiva.
4. Registro acadêmico
Atualize prompts.md com esta solicitação.
Preserve os registros de uso da IA exigidos pela FIAP, sem adicionar referências promocionais de ferramentas ao código ou à documentação técnica.
5. Git
Não execute commit nem push nesta etapa.
Não altere o histórico existente.
6. Relatório final
Apresente:

1. Correções realizadas.
2. Arquivos modificados.
3. Política definitiva de dados simulados.
4. Resultados efetivos do verificador.
5. Estado do Git.
6. Confirmação de que o código da API permaneceu inalterado.
7. Pendências restantes para o B2.

Não implemente código de negócio e não inicie o B2.
Pare após concluir esta revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA revisou as regras R3 a R5. Removeu a opção
que renovava as observações simuladas a cada inicialização e definiu um seed idempotente: as
observações simuladas são criadas uma única vez, mantêm o horário original e ficam marcadas
como desatualizadas com o tempo. Reiniciar a API não cria nem altera passagens. A passagem
recente da demonstração passou a vir do registro manual administrativo. Acrescentou a seção
4.6.2 ao `api-v1.md`, registrou a mudança como divergência D16 e atualizou o README. Ajustou
os exemplos de passagem, resumo e operação administrativa. Ampliou o verificador com um grupo
de regras para dados simulados e o testou com defeitos plantados. Nenhum código da API foi
alterado, e nenhum commit foi feito.

<a id="prompt-11"></a>

### Prompt 11 — Bloco B1.3: correções finais e publicação do contrato

**Data:** 07/11/2026 · **Tipo:** Revisão, correção e publicação · **Commit:** `370fab4`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B1.3 — CORREÇÕES FINAIS E PUBLICAÇÃO DO CONTRATO
Realizei uma auditoria independente dos arquivos reais da Global Solution, incluindo os documentos em contracts/, os 15 exemplos JSON, o verificador e a estrutura do backend.
O Bloco B1.2 está aprovado em sua arquitetura e no conjunto principal de contratos, condicionado à correção de três detalhes técnicos.
Execute apenas as correções abaixo. Não amplie o escopo e não implemente funcionalidades de negócio.
1. Resolver a contradição R3/R4
O requisito R3 estabelece que o seed cria dados demonstrativos ausentes, incluindo registros fictícios de passagem.
O requisito R4 afirma que iniciar a API não cria passagens.
Esclareça a distinção entre:

* Primeiro provisionamento de um banco vazio, quando o seed poderá criar os registros iniciais.
* Inicialização posterior de um banco já provisionado, quando não se deve criar novos eventos de passagem nem atualizar horários existentes.
* Registro manual de uma nova observação durante a demonstração.

Preserve a idempotência e a rastreabilidade histórica.
Atualize R3, R4 e as seções relacionadas, sem modificar o comportamento geral aprovado.
2. Corrigir a classificação de dados fictícios administrativos
O exemplo POST /api/v1/admin/paradas utiliza um cadastro fictício, com código TESTE-P003, mas a resposta o identifica com dadosSimulados: false.
Isso precisa ser corrigido.
Diferencie:

* Método de cadastro: como a informação entrou no sistema.
* Natureza do dado: fictício ou não fictício.
* Verificação: confirmado ou não confirmado por uma fonte confiável.

Não considere automaticamente que um registro administrativo contém dados reais ou verificados.
Proponha e documente o menor ajuste necessário nos DTOs e exemplos para permitir que cadastros fictícios sejam explicitamente identificados como simulados, mesmo quando criados por uma operação administrativa.
Garanta que a resposta pública não perca essa informação.
O contrato também menciona o aviso CadastroNaoVerificado. Defina exatamente como esse aviso será representado nos endpoints públicos, ou ajuste a redação para corresponder ao formato realmente definido.
Não acrescente novos endpoints.
3. Corrigir o tratamento temporal de observações futuras
O contrato aceita observadaEm até dois minutos depois do relógio do servidor como tolerância.
Defina de forma inequívoca:

* Como calcular minutosDesdeObservacao sem valores negativos.
* Como lidar com registros aceitos por tolerância de relógio que ainda apresentem horário futuro.
* Como evitar apresentar uma observação futura como uma passagem já confirmada.
* Como calcular desatualizada usando o intervalo efetivamente transcorrido.
* Como testar os limites de 0, 60 e 61 minutos, além dos casos imediatamente acima de 60 minutos e dentro da tolerância futura.

Preserve a janela de 24 horas no passado e dois minutos no futuro.
Não implemente a lógica no backend agora; documente a regra definitiva para os blocos de domínio e serviços.
4. Revisar os exemplos e o verificador
Atualize apenas os arquivos afetados em contracts/.
Garanta que os exemplos continuem coerentes entre si.
Amplie o verificador somente na medida necessária para cobrir as decisões corrigidas.
Execute novamente o comando de verificação.
Informe a quantidade de exemplos analisados, os grupos de regras, as falhas e o código de saída real.
Não considere que o verificador comprova a execução da API.
5. Atualizar a documentação
Registre as novas decisões e eventuais divergências de forma objetiva.
Atualize a identificação da etapa B1 nos documentos pertinentes.
Preserve os requisitos FIAP e o registro acadêmico de prompts em prompts.md.
Não altere código de negócio, projetos .NET ou arquivos desnecessários.
6. Git e publicação
Antes do commit:

* Verifique o estado de main e origin/main.
* Inspecione o diff completo.
* Verifique a ausência de segredos, dados pessoais indevidos, arquivos temporários e binários.
* Não inclua alterações fora dos documentos do B1.
* Não reescreva os commits anteriores.

Se todas as verificações estiverem satisfatórias, está autorizada a publicação desta etapa usando a mensagem de commit previamente definida:
docs: consolidate PontoVoz REST API v1 contract
Não adicione assinaturas automáticas ou mensagens promocionais de ferramentas ao commit.
Preserve o registro acadêmico de IA em prompts.md.
Execute o commit e o push normal para origin/main. Nunca utilize force push.
Se alguma validação falhar ou aparecer uma alteração inesperada, interrompa a publicação e explique o problema.
7. Relatório obrigatório
Ao final, apresente:

1. As três correções e decisões técnicas adotadas.
2. Os arquivos efetivamente alterados.
3. O resultado do verificador.
4. A confirmação de que o backend permaneceu inalterado.
5. O resultado real do commit e do push, caso executados.
6. O hash final.
7. O estado de main e origin/main.
8. As pendências para iniciar o B2.

Não execute B2.
Pare após concluir esta etapa e aguarde minha revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA separou, no contrato, o provisionamento
inicial de um banco vazio (único momento em que o seed cria observações simuladas), a
inicialização posterior (que nunca cria nem altera passagens) e o registro manual durante a
demonstração. Acrescentou `dadosSimulados` obrigatório ao corpo dos cadastros administrativos
de parada e linha, corrigiu o exemplo `TESTE-P003`, separou método de cadastro, natureza do
dado e verificação, e definiu em quais listas `avisos` o aviso `CadastroNaoVerificado` aparece.
Definiu a regra temporal das observações: minutos nunca negativos, desatualização pelo
intervalo exato, registros com horário futuro fora da consulta até o horário chegar e casos de
teste nos limites. Registrou as divergências D17 a D19, ampliou o verificador, que foi testado
também com defeitos plantados, e publicou a etapa com commit e push normais. Nenhum código da
API foi alterado.

---

## Bloco B2 — domínio e Programação Orientada a Objetos

<a id="prompt-12"></a>

### Prompt 12 — Bloco B2: implementação do domínio e Programação Orientada a Objetos

**Data:** 07/11/2026 · **Tipo:** Implementação e testes · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B2 — IMPLEMENTAÇÃO DO DOMÍNIO E PROGRAMAÇÃO ORIENTADA A OBJETOS
Você atuará como engenheiro de software sênior especialista em C#/.NET 10, Programação Orientada a Objetos, arquitetura de domínio e testes unitários.
Os blocos B0 e B1 foram concluídos, revisados e publicados no GitHub.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Você está autorizado a executar exclusivamente o Bloco B2 — Domínio e POO.
Não avance para B3 ou para qualquer outro bloco.
1. CONTEXTO DA GLOBAL SOLUTION
O PontoVoz é uma CitizenTech criada para melhorar o acesso a informações sobre transporte público em São Paulo.
O produto final é um totem inteligente instalado em pontos de ônibus, com interação por voz e tela.
O cidadão poderá consultar previsões de chegada, últimas passagens registradas e alternativas de deslocamento.
O backend em C# fornece os dados e as regras de negócio para os demais componentes do sistema.
A solução deve continuar integrada à proposta CitizenTech da FIAP.
2. CRITÉRIOS DO PROFESSOR
A disciplina C# Software Development exige:

* Programação Orientada a Objetos — 3,0 pontos.
* Web API — 2,5 pontos.
* Entity Framework Core + LINQ — 1,5 ponto.
* Async/Await — 1,0 ponto.
* Qualidade do código — 2,0 pontos.

Além disso, são exigidas interfaces, injeção de dependência, testes unitários e documentação.
O foco deste bloco é demonstrar POO de maneira tecnicamente correta, com testes automatizados e regras reais do PontoVoz.
Não implemente funcionalidades irrelevantes apenas para demonstrar herança ou polimorfismo.
3. VERIFICAÇÃO OBRIGATÓRIA ANTES DE IMPLEMENTAR
Examine os arquivos reais do projeto:

* backend/PontoVoz.slnx.
* backend/src/PontoVoz.Api/.
* backend/tests/PontoVoz.Tests/.
* contracts/api-v1.md.
* contracts/README.md.
* contracts/exemplos/.
* backend/global.json.
* prompts.md.

Confirme também o estado do Git.
O commit esperado do B1.3 é:
`370fab4342c2f0ff3847b37114405a835e8e35f9`
Não sobrescreva alterações preexistentes.
O contrato B1.3 é a referência obrigatória. Não crie um domínio incompatível com ele.
Se encontrar ambiguidades, proponha a solução mais simples e coerente, explicando a decisão. Se houver risco de quebrar o contrato, interrompa a alteração correspondente e informe o impedimento.
4. ESTRUTURA DO DOMÍNIO
Implemente as classes e objetos de valor necessários em:
`backend/src/PontoVoz.Api/Domain/`
Organize de maneira clara:

* Entities/.
* ValueObjects/.
* Enums/.
* Exceptions/.
* Rules/, apenas se houver necessidade de regras compartilhadas.

Evite criar classes sem responsabilidade real.
4.1 EntidadeBase
Crie uma classe base abstrata adequada para as entidades persistentes.
Considere:

* Id.
* CriadoEmUtc.
* Encapsulamento.
* Construtores apropriados.
* Compatibilidade futura com EF Core.

Não invente identificadores de banco durante a criação das entidades. A persistência e geração de IDs serão configuradas no B3.
4.2 Parada
Represente um ponto de ônibus.
Propriedades principais:

* CodigoExterno.
* Nome.
* Endereco.
* Coordenada.
* Ativa.
* Origem.
* DadosSimulados.
* Associações com linhas.

Implemente invariantes e métodos de domínio apropriados.
Exemplos:

* AtualizarDados.
* Ativar.
* Desativar.
* AssociarLinha.

Regras:

* Código obrigatório e válido.
* Nome com comprimento válido.
* Origem Simulado implica DadosSimulados verdadeiro.
* Cadastro administrativo informa explicitamente a natureza do dado.
* DadosSimulados não muda depois da criação.
* Não permitir associação duplicada.
* Não permitir associação de linhas inativas.
* Não permitir associação quando a parada estiver inativa.

Cuide da diferença entre objetos ainda não persistidos e entidades com IDs gerados pelo banco.
4.3 Linha
Represente uma linha de ônibus em determinado sentido.
Propriedades:

* Codigo.
* Letreiro.
* TerminalPrincipal.
* TerminalSecundario.
* Sentido.
* Ativa.
* Origem.
* DadosSimulados.

Implemente:

* Validação de código.
* Validação dos campos obrigatórios.
* Métodos de atualização, quando justificados.
* Ativação e desativação.
* Regras de proteção ao prefixo SIM-.

Cada sentido de linha corresponde a um cadastro próprio, conforme o contrato.
4.4 ParadaLinha
Represente a associação entre uma parada e uma linha.
Considere:

* Parada.
* Linha.
* AssociadaEmUtc.
* Identificadores que serão persistidos posteriormente.

A associação não pode ser duplicada.
Mantenha compatibilidade com o futuro relacionamento N configurado no Entity Framework Core.
Não implemente banco nesta etapa.
4.5 Destino
Represente os destinos consultáveis pelo totem.
Propriedades:

* Nome.
* TipoDestino.
* Ativo.
* Origem.
* DadosSimulados.

No MVP, os destinos serão demonstrativos e cadastrados pelo seed.
Não crie funcionalidades administrativas de destino que não existam no contrato.
4.6 PercursoEstimado
Represente um percurso direto de uma linha que atende uma parada até determinado destino.
Campos relevantes:

* Parada.
* Linha.
* Destino.
* MinutosEstimados.
* Origem e natureza do dado, conforme contrato.

Regras:

* O tempo estimado deve ficar entre 1 e 240 minutos.
* O percurso precisa estar associado à linha, à parada e ao destino.
* Não permitir um relacionamento logicamente inconsistente.
* Preparar a futura chave composta e restrição de unicidade para o B3.

A comparação das alternativas será implementada posteriormente nos services. Não desenvolva o algoritmo completo agora.
4.7 RegistroPassagem
Implemente a hierarquia de domínio planejada:

* RegistroPassagem, classe abstrata.
* PassagemSimulada.
* PassagemRegistroManual.

Use herança e polimorfismo de maneira coerente.
O registro deverá permitir:

* Parada e linha.
* ObservadaEmUtc.
* RegistradaEmUtc.
* PrefixoVeiculo.
* NivelConfirmacao.
* Indicação de verificação.
* Natureza simulada quando aplicável.

PassagemSimulada:

* Deve possuir uma chave interna estável para o seed.
* Não pode ser apresentada como oficial.
* Seu horário não deve ser renovado automaticamente.
* A chave interna não precisa aparecer nos DTOs públicos.

PassagemRegistroManual:

* Deve possuir OrigemRegistro.
* Deve validar o formato técnico do rótulo.
* Não pode ser apresentada como oficialmente verificada.
* Sua criação respeita a janela temporal definida no contrato.

Não crie ainda o seed. Apenas prepare a modelagem que permitirá implementá-lo no B3.
5. OBJETOS DE VALOR
Implemente os objetos de valor que fizerem sentido para a modelagem aprovada.
Considere:
Coordenada

* Latitude entre -90 e 90.
* Longitude entre -180 e 180.
* Igualdade por valor.
* Imutabilidade.

PrevisaoChegada

* Representa uma previsão, nunca uma passagem confirmada.
* Horário previsto.
* Referência da linha.
* Identificação opcional do veículo.
* Regras pertinentes ao cálculo de minutos.

ResultadoProvedor

* Representa o resultado do provedor.
* Status, origem, horário da consulta e previsões.

AlternativaDeslocamento

* Representa um resultado calculado.
* Não é entidade persistente.
* Não implementa ainda a ordenação completa do serviço.

Use records e outros recursos modernos do C# quando adequados.
Não crie abstrações excessivas.
6. ENUMS
Implemente os enums efetivamente exigidos pelo contrato B1.3, incluindo:

* OrigemDado.
* TipoDestino.
* SentidoLinha.
* NivelConfirmacao.
* StatusPrevisao.
* SituacaoAlternativas.
* SituacaoAlternativa.
* SituacaoResumo.

Use exatamente os valores documentados em `contracts/api-v1.md`.
Não renomeie valores sem justificativa e não acrescente membros desnecessários.
7. REGRAS TEMPORAIS — OBRIGATÓRIAS
Siga rigorosamente a seção 4.6.3 do contrato.
Para a criação de um registro manual:

* Aceitar observações até 24 horas no passado.
* Aceitar tolerância de até 2 minutos no futuro.
* Rejeitar observações fora desses limites.

Para cálculos de passagem:

* minutosDesdeObservacao nunca pode ser negativo.
* Utilizar minutos inteiros arredondados para baixo.
* desatualizada é verdadeira quando o intervalo exato ultrapassa 60 minutos.
* Um registro com horário futuro não pode ser tratado como passagem já ocorrida.
* A consulta deverá filtrar registros futuros no bloco apropriado.

Use TimeProvider ou um instante explícito fornecido pelo componente de aplicação. Não dependa de DateTime.Now dentro das entidades.
Mantenha cálculos e comparações em UTC.
Planeje corretamente a conversão entre o modelo de domínio e a persistência futura em SQLite.
8. PROGRAMAÇÃO ORIENTADA A OBJETOS
Garanta demonstração real dos seguintes conceitos:
Encapsulamento: propriedades protegidas contra alterações arbitrárias e métodos que preservam invariantes.
Abstração: classes abstratas e interfaces quando houver responsabilidade concreta.
Herança: EntidadeBase e a hierarquia de registros de passagem.
Polimorfismo: comportamento específico das subclasses, por exemplo em nível de confirmação e avisos.
Composição: relacionamentos entre entidades e objetos de valor.
SOLID: aplicação proporcional ao projeto, priorizando responsabilidade única e dependências claras.
Não crie interfaces para cada entidade apenas para aumentar a quantidade de arquivos.
As interfaces de serviços e repositórios serão definidas nos blocos apropriados, quando suas operações forem implementadas.
9. TESTES UNITÁRIOS REAIS
Este bloco deverá criar os primeiros testes efetivos do PontoVoz.
Utilize a infraestrutura já instalada:

* xUnit v3.
* Microsoft Testing Platform.
* .NET 10.

Se necessário, adicione Microsoft.Extensions.TimeProvider.Testing em versão compatível para controlar o relógio nos testes.
Não migre novamente o framework de testes sem necessidade.
Crie testes positivos e negativos para as regras de domínio.
Inclua, entre outros:

* Construção válida de parada.
* Código de parada inválido.
* Nome obrigatório e comprimento inválido.
* Coordenadas nos limites aceitos e rejeitados.
* Criação válida de linha.
* Associação duplicada.
* Associação de linha inativa.
* Desativação e reativação.
* DadosSimulados consistente com a origem.
* Imutabilidade de propriedades essenciais.
* Percurso com 1 e 240 minutos.
* Percurso com 0 e 241 minutos.
* RegistroPassagem com polimorfismo.
* Registro manual com origemRegistro válida e inválida.
* Registro de passagem sem confirmação oficial.
* Cálculo temporal de passagens.

Execute todos os casos J1 a J5 e T1 a T7 da seção 4.6.3 do contrato.
Utilize valores variados de entrada para provar as regras, não somente os números dos exemplos.
Não crie testes artificiais como Assert.True(true).
O objetivo é que dotnet test execute testes reais, com resultado verificável.
10. COMPATIBILIDADE COM ENTITY FRAMEWORK CORE
O B3 implementará DbContext, migrations e mapeamento do banco.
Portanto:

* Prepare entidades persistíveis por EF Core.
* Mantenha construtores e modificadores de acesso compatíveis com materialização.
* Evite regras que impeçam o EF de reconstruir entidades.
* Não introduza dependência do Domain em Infrastructure.
* Não adicione atributos de persistência sem necessidade.
* Não implemente EF Core neste bloco.

Caso o futuro mapeamento de uma hierarquia ou chave composta exija uma decisão especial, documente essa decisão para o B3.
11. DOCUMENTAÇÃO E EVIDÊNCIAS
Atualize backend/README.md para descrever os conceitos de POO efetivamente implementados.
Inclua:

* Lista das entidades.
* Relacionamentos fundamentais.
* Invariantes.
* Exemplos de aplicação de herança e polimorfismo.
* Instruções para executar os testes.
* Evidências dos testes executados.
* O que continua pendente para o B3.

Se for útil, crie um diagrama Mermaid simples de classes no README.
Não afirme que os endpoints REST estão implementados.
Preserve o README geral e o contrato REST publicado.
Registre este prompt em prompts.md conforme as exigências acadêmicas.
12. PADRONIZAÇÃO DO DESENVOLVIMENTO
Mantenha as diretrizes estabelecidas:

* Código limpo, profissional e legível.
* Comentários técnicos apenas quando agregarem valor.
* Sem banners promocionais, marcas automáticas de ferramentas ou comentários de geração.
* Registro acadêmico de uso de IA preservado em prompts.md.
* Sem informações sensíveis no repositório.
* Sem mudanças na autoria ou no histórico Git existente.
* Não acrescente assinaturas automáticas de ferramentas a futuras mensagens de commit.

Nenhum commit está autorizado neste bloco antes da minha revisão.
13. VALIDAÇÃO OBRIGATÓRIA
Ao finalizar a implementação, execute:

1. dotnet restore.
2. dotnet build.
3. dotnet test.

Registre:

* Número exato de testes descobertos.
* Número de testes aprovados.
* Número de falhas.
* Número de testes ignorados.
* Código de saída do comando.
* Avisos e erros de compilação.

Verifique também que a API ainda compila.
Não altere o comportamento do `/health`, OpenAPI ou Scalar sem necessidade.
Se houver falhas, analise, corrija e execute novamente.
Não afirme sucesso se a execução não comprovar o resultado.
14. GIT
Antes de programar, confirme:

* Branch main.
* Estado local do Git.
* Último commit.
* Conexão com origin/main.

Durante a implementação:

* Trabalhe somente nos arquivos necessários.
* Não execute git reset --hard.
* Não execute force push.
* Não sobrescreva arquivos de outros integrantes.
* Não faça commit.
* Não faça push.

Deixe todas as alterações locais para nossa auditoria.
15. RELATÓRIO FINAL OBRIGATÓRIO
Apresente:
A. Resumo da implementação: o que foi desenvolvido e o que ficou para depois.
B. Árvore dos arquivos criados e modificados.
C. Entidades, propriedades e relacionamentos implementados.
D. Regras de negócio e invariantes.
E. Demonstração dos conceitos de POO: classe ou método correspondente a encapsulamento, abstração, herança, polimorfismo e composição.
F. Resultado dos testes unitários: números reais, incluindo os testes temporais J1 a J5 e T1 a T7.
G. Resultado do build.
H. Compatibilidade com o contrato B1.3: diferenças, se existirem.
I. Preparação para EF Core no B3: decisões e possíveis dificuldades.
J. Estado do Git.
K. Pendências, riscos e próximos passos.
L. Matriz dos critérios FIAP: o que já está implementado de forma verificável e o que ainda está pendente.
16. RESTRIÇÃO FINAL
Não implemente:

* DbContext.
* Migrations.
* Repositórios EF Core.
* Controllers de negócio.
* Integração com SPTrans.
* Serviços de previsão.
* Comparador completo de alternativas.
* Interface do totem.
* Inteligência artificial conversacional.
* Funcionalidades da disciplina Testing/QA.

Execute somente o Bloco B2. Ao finalizar, pare e aguarde minha auditoria antes de qualquer commit ou avanço para B3.
O objetivo é construir um domínio C# tecnicamente sólido, com POO bem demonstrada, aderência integral ao PontoVoz e testes unitários reais.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA implementou o domínio em
`backend/src/PontoVoz.Api/Domain/`:
- `EntidadeBase` e as entidades `Parada`, `Linha`, `ParadaLinha`, `Destino` e `PercursoEstimado`;
- a hierarquia `RegistroPassagem`, com `PassagemSimulada` e `PassagemRegistroManual`;
- os objetos de valor `Coordenada`, `ReferenciaLinha`, `PrevisaoChegada`, `ResultadoProvedor` e
  `AlternativaDeslocamento`;
- os nove enums do contrato;
- exceções de domínio com os códigos de erro do contrato;
- a regra temporal da seção 4.6.3 em `RegraTemporalPassagem`.

As entidades usam métodos de fábrica, propriedades com `private set` e construtores sem
parâmetros não públicos, para que o EF Core possa materializá-las. Foram criados 200 testes
unitários com xUnit v3 e `FakeTimeProvider`, incluindo os casos J1 a J5 e T1 a T7. Todos
passaram, com build sem avisos, e uma mutação intencional da regra temporal confirmou que os
testes detectam o erro. O `backend/README.md` passou a documentar entidades, invariantes,
conceitos de POO, as evidências e as decisões para o B3. Não houve commit, banco de dados nem
endpoints novos.

<a id="prompt-13"></a>

### Prompt 13 — Bloco B2.1: correções de domínio e testes de regressão

**Data:** 07/11/2026 · **Tipo:** Revisão, correção e testes · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B2.1 — CORREÇÕES DE DOMÍNIO E TESTES DE REGRESSÃO
Realizei uma auditoria independente dos arquivos C# efetivamente produzidos no Bloco B2, incluindo entidades, objetos de valor, regras, contratos e testes unitários.
O trabalho foi considerado adequado, mas foram identificadas três inconsistências que precisam ser resolvidas antes da aprovação para commit.
Execute somente as correções deste prompt. Não avance para B3.
1. Contexto e verificações iniciais
Confirme o estado do Git, a branch main e o commit publicado `370fab4342c2f0ff3847b37114405a835e8e35f9`.
Preserve todas as alterações locais ainda não commitadas do B2.
Examine o código real, especialmente:

* Domain/Rules/RegrasCadastro.cs
* Domain/Entities/PassagemRegistroManual.cs
* Domain/Entities/PassagemSimulada.cs
* Domain/ValueObjects/PrevisaoChegada.cs
* Domain/ValueObjects/ResultadoProvedor.cs
* Domain/ValueObjects/AlternativaDeslocamento.cs
* Os testes dessas classes
* contracts/api-v1.md

Não reescreva arquivos completos sem necessidade.
2. Correção 1 — Prefixo de veículo e natureza simulada
O contrato estabelece que dados de veículos simulados utilizam o prefixo SIM-.
A classe PassagemSimulada valida o prefixo, mas a classe PassagemRegistroManual permite cadastrar um veículo sem esse prefixo mesmo quando o registro depende de uma parada ou linha fictícia.
Corrija o comportamento de maneira consistente com o contrato.
Regras:

* `prefixoVeiculo` continua opcional.
* Se estiver presente e o registro representar dados simulados, sua identificação deverá respeitar o padrão estabelecido para veículos fictícios.
* Não confunda `OrigemDado.CadastroAdministrativo` com a natureza fictícia do cadastro.
* Cadastros administrativos reais não devem ser obrigados a utilizar o prefixo SIM-.
* Preserve a expressão regular geral de formato e os limites documentados.
* Não altere a API REST, pois ela ainda não foi implementada.

Revise os testes existentes que usam, por exemplo, `V-12` em uma parada e linha simuladas.
Adicione testes positivos e negativos para os cenários relevantes.
Se identificar uma ambiguidade contratual entre o prefixo reservado e os dados fictícios administrativos, resolva-a com a menor mudança coerente e explique a decisão.
3. Correção 2 — Previsões vencidas
O contrato estabelece que previsões anteriores ao instante `consultadoEm` não devem ser devolvidas ao consumidor.
Atualmente, ResultadoProvedor.Respondido permite armazená-las e AlternativaDeslocamento.Calcular pode classificá-las como comparáveis.
Corrija isso preservando a arquitetura planejada.
Verifique especialmente:

* Previsão anterior ao instante da consulta.
* Previsão exatamente no instante da consulta.
* Previsão futura.
* Resultado contendo somente previsões vencidas.
* Resultado misto, com previsões vencidas e futuras.
* Alternativa construída com uma previsão vencida.
* Coerência entre `esperaMinutos`, `tempoTotalEstimadoMinutos` e `chegadaEstimadaAoDestinoUtc`.

Uma previsão vencida nunca deve ser apresentada como alternativa comparável.
Preserve a semântica do contrato: uma previsão exatamente em `consultadoEm` pode representar ônibus chegando, com espera de zero minutos.
Se a filtragem pertencer ao ResultadoProvedor, implemente-a nesse ponto. Se houver proteção necessária em AlternativaDeslocamento, acrescente-a sem duplicar regras desnecessariamente.
Crie testes de regressão.
4. Correção 3 — Expressões regulares
Verifique as expressões regulares de:

* Código de parada e linha.
* Prefixo de veículo.
* OrigemRegistro.
* ChaveSeed.

As validações atuais utilizam `$`, que no mecanismo Regex do .NET pode aceitar uma posição anterior à quebra de linha final.
Substitua as âncoras quando necessário para exigir correspondência com a entrada completa, por exemplo utilizando `\A` e `\z` com a sintaxe correta de strings C#.
Adicione testes rejeitando:

* Quebra de linha no início.
* Quebra de linha no final.
* Quebra de linha no meio.
* Entradas com caracteres fora do formato.

Preserve os formatos e comprimentos estabelecidos pelo contrato B1.3.
Não acrescente bibliotecas de validação desnecessárias.
5. Preservação da modelagem
As seguintes decisões estão aprovadas:

* Associação idempotente de linha e parada.
* Parada fictícia sem endereço ou coordenada, conforme o contrato atual.
* Herança de RegistroPassagem.
* Níveis de confirmação distintos.
* Regras temporais J1 a J5 e T1 a T7.
* Uso de TimeProvider.
* Domínio independente da infraestrutura.

Não altere essas decisões durante esta correção.
6. Testes e qualidade
Após as correções:

1. Execute `dotnet restore`.
2. Execute `dotnet build`.
3. Execute `dotnet test`.

Confirme que os 200 testes existentes continuam aprovados e que os novos testes também passam.
Informe a contagem exata, sem assumir que continuará sendo 200.
Execute também os casos temporais do contrato.
Verifique que o projeto continua sem avisos e erros.
Faça testes negativos que comprovem que as novas validações realmente rejeitam entradas incorretas.
Se executar testes de mutação, restaure integralmente o código original após a verificação.
7. Documentação
Atualize backend/README.md e Domain/README.md apenas quando necessário para refletir as correções.
Não afirme que o EF Core, os services ou os endpoints de negócio já estão implementados.
Atualize prompts.md com este prompt e o resumo identificado da execução, conforme a exigência acadêmica da FIAP.
Preserve os padrões profissionais do código e da documentação.
Não insira assinaturas automáticas ou marcas promocionais de ferramentas.
8. Git
Não faça commit.
Não execute git push.
Não modifique commits existentes.
Mantenha todas as alterações locais para a próxima auditoria.
9. Relatório final
Apresente:

1. As três falhas identificadas e suas causas.
2. As correções feitas, com nomes dos arquivos e métodos.
3. Novos testes criados e quais cenários cobrem.
4. Total real de testes aprovados, falhos e ignorados.
5. Resultado de restore, build e test.
6. Confirmação de que o contrato B1.3 continua sendo respeitado.
7. Confirmação de que o código da Web API não foi alterado indevidamente.
8. Estado do Git.
9. Riscos que restarem para o mapeamento EF Core no B3.
10. Proposta de mensagem de commit, sem executá-lo.

Não inicie B3.
Pare após concluir e aguarde minha revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA corrigiu as três falhas.
- **Prefixo de veículo:** passou a seguir a natureza do registro (`RegrasCadastro.PrefixoVeiculo`
  com `dadosSimulados`). Registros fictícios usam `SIM-`, inclusive em cadastros administrativos
  fictícios, e registros reais não podem usá-lo. O prefixo segue opcional.
- **Previsões vencidas:** criou `PrevisaoChegada.VencidaEm`. Com ela, `ResultadoProvedor.Respondido`
  descarta as previsões vencidas antes de definir o status, e `AlternativaDeslocamento.Calcular`
  trata uma previsão vencida como ausente. A previsão exatamente no instante da consulta continua
  válida, com espera zero.
- **Expressões regulares:** passaram a usar `\A` e `\z`.

Também ajustou o teste que usava `V-12` em dados simulados e criou 78 testes de regressão. Ao
todo, 278 testes passaram, com build sem avisos. Mutações temporárias, depois restauradas,
confirmaram que cada correção é detectada pelos testes. Não houve commit.

<a id="prompt-14"></a>

### Prompt 14 — Bloco B2.2: auditoria final e publicação do domínio C#

**Data:** 07/11/2026 · **Tipo:** Auditoria e publicação · **Commit:** `f53fe2d`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B2.2 — AUDITORIA FINAL E PUBLICAÇÃO DO DOMÍNIO C#
Os Blocos B2 e B2.1 foram analisados. A implementação do domínio e das correções de regressão está aprovada para publicação, desde que todas as verificações finais sejam satisfatórias.
Sua tarefa é realizar a conferência final e publicar o domínio C# já desenvolvido no repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Não implemente novas funcionalidades e não avance para B3.
1. Verificação inicial
Confirme:

* Diretório correto do PontoVoz.
* Branch main.
* Commit atual `370fab4342c2f0ff3847b37114405a835e8e35f9`.
* Sincronização entre main e origin/main.
* Arquivos alterados e novos.
* Ausência de conflitos ou alterações inesperadas.

Não execute reset, rebase ou qualquer reescrita do histórico publicado.
2. Conferência do escopo
Revise o diff e confirme que as alterações correspondem somente ao B2/B2.1:

* Entidades e objetos de valor.
* Enums e exceções de domínio.
* Regras de cadastro e de passagem.
* Testes unitários e testes de regressão.
* Documentação técnica do backend.
* Registro acadêmico em prompts.md.

Confirme que os contratos REST publicados continuam intactos e que Program.cs, Controllers, appsettings e demais configurações da API não foram modificados indevidamente.
Não inclua bin/, obj/, bancos SQLite, arquivos de ambiente, segredos ou backups temporários.
3. Validação obrigatória
Execute novamente, a partir de backend/:

* dotnet restore
* dotnet build
* dotnet test

Confirme a quantidade real de testes.
O relatório anterior apontou 278 testes aprovados, nenhuma falha e nenhum aviso no build.
Caso esse resultado mude, explique a diferença.
Verifique se os testes J1 a J5 e T1 a T7 continuam aprovados.
Confirme também que não restaram alterações temporárias dos testes de mutação.
Se alguma validação falhar, interrompa a publicação.
4. Auditoria de segurança e qualidade
Inspecione os arquivos a serem versionados.
Procure:

* Chaves, senhas e tokens.
* Dados pessoais indevidos.
* Arquivos temporários.
* Binários desnecessários.
* Código incompleto ou comentários enganosos.
* Testes fictícios.
* Alterações não relacionadas ao domínio.

Preserve o registro acadêmico de utilização de IA exigido pela FIAP em prompts.md.
Não insira marcas promocionais ou assinaturas automáticas de ferramentas no código ou na mensagem de commit.
5. Commit
Se a auditoria passar, está autorizado a criar um commit com a seguinte mensagem:
feat: implement PontoVoz domain model with unit tests
Mantenha a mensagem objetiva.
Não acrescente corpo promocional, trailer automático de ferramenta ou linha Co-Authored-By.
Preserve os metadados legítimos de autoria, usando o e-mail noreply já configurado localmente.
Antes do push, inspecione a mensagem real gravada no commit com git log -1 --format=%B.
Se algum trailer automático indesejado tiver sido inserido, corrija apenas a mensagem do novo commit local antes de publicá-lo, preservando os arquivos e a autoria. Não altere commits anteriores.
6. Publicação
Publique a branch main com push normal para origin/main.
Não utilize force push.
Depois do envio, confirme:

* Hash do commit.
* Mensagem efetivamente publicada.
* Branch main e origin/main sincronizadas.
* Árvore local limpa.
* Arquivos do domínio publicados.
* Testes e documentação publicados.
* Ausência de arquivos indevidos.

7. Relatório final obrigatório
Apresente:

1. Resultado da auditoria.
2. Arquivos publicados.
3. Total real de testes aprovados, falhos e ignorados.
4. Resultado do restore, build e test.
5. Hash do commit.
6. Mensagem do commit.
7. Resultado real do push.
8. Estado final do Git.
9. Pendências técnicas para B3.
10. Confirmação de que B3 não foi iniciado.

Registre este prompt em prompts.md antes do commit.
Não desenvolva DbContext, migrations, repositórios ou seeds nesta etapa.
Pare depois da publicação e aguarde minha análise.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA conferiu o estado do Git e o escopo do diff,
que continha apenas domínio, testes, documentação do backend e este registro. Confirmou que
`contracts/` e a configuração da API não tinham mudado e que não restavam alterações
temporárias dos testes de mutação. Executou `dotnet restore`, `dotnet build` (0 avisos e 0 erros)
e `dotnet test` (278 testes aprovados, incluindo os 17 casos J1 a J5 e T1 a T7). Fez a auditoria
de segredos, dados pessoais, binários e marcas de ferramenta e publicou o commit
`feat: implement PontoVoz domain model with unit tests`, sem trailer, com push normal para
`origin/main`. O B3 não foi iniciado.

---

## Bloco B3 — persistência com EF Core e SQLite

<a id="prompt-15"></a>

### Prompt 15 — Bloco B3: Entity Framework Core, SQLite, migrations e persistência

**Data:** 07/11/2026 · **Tipo:** Implementação e testes · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 05 — BLOCO B3: ENTITY FRAMEWORK CORE, SQLITE, MIGRATIONS E PERSISTÊNCIA
Atue como engenheiro de software sênior especializado em C#, .NET 10 e Entity Framework Core.
Estamos desenvolvendo o backend C# do PontoVoz, projeto CitizenTech da FIAP.
Repositório: https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local: `C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Os blocos B0, B1 e B2 foram concluídos, auditados e publicados.
O último commit esperado é:
`f53fe2d743554d0ef1ef1d7266d63bae5643cc47`
Sua tarefa é executar somente o Bloco B3 — Persistência com EF Core e SQLite.
Não avance para B4 e não execute commit ou push.
1. PRIORIDADE ABSOLUTA: EXIGÊNCIAS DO PROFESSOR
O enunciado oficial da disciplina C# Software Development exige:

* Programação Orientada a Objetos — 3,0 pontos.
* Web API — 2,5 pontos.
* Entity Framework Core + LINQ — 1,5 ponto.
* Async/Await — 1,0 ponto.
* Qualidade de código — 2,0 pontos.

Também exige interfaces, injeção de dependência, APIs REST, testes unitários, README, documentação dos endpoints, evidências da aplicação e repositório GitHub.
O escopo deste bloco é exclusivamente a persistência das entidades com Entity Framework Core.
LINQ será explorado no B4, serviços e DI nos blocos seguintes, e os endpoints REST nos blocos posteriores.
Não tente implementar todos os critérios neste bloco.
Todo código desenvolvido deve ter relação real com a proposta PontoVoz.
Não adicione funcionalidades, ferramentas, integrações ou abstrações que não sejam necessárias para atender ao planejamento e à rubrica.
2. INSPEÇÃO OBRIGATÓRIA
Antes de modificar arquivos:

1. Confirme o diretório local correto.
2. Confirme a branch main.
3. Verifique o commit atual e a sincronização com origin/main.
4. Confirme que a árvore de trabalho está limpa.
5. Examine a solução e os projetos .NET existentes.
6. Leia os arquivos atuais de `Domain/`.
7. Leia `contracts/api-v1.md` e `contracts/README.md`.
8. Examine os 278 testes existentes e as configurações de teste.
9. Confira as dependências já instaladas do Entity Framework Core.

O estado real do código e o contrato B1.3 prevalecem sobre qualquer suposição.
Se encontrar inconsistências entre a modelagem e o contrato, explique o problema e faça apenas o ajuste mínimo seguro.
Não reestruture o projeto inteiro.
3. IMPLEMENTAÇÃO DO DBCONTEXT
Crie um `PontoVozDbContext` em:
`backend/src/PontoVoz.Api/Infrastructure/Persistence/`
Configure o contexto para representar as entidades persistentes do domínio:

* Parada.
* Linha.
* ParadaLinha.
* Destino.
* PercursoEstimado.
* RegistroPassagem.
* PassagemSimulada.
* PassagemRegistroManual.

Utilize os recursos nativos do Entity Framework Core 10.
Preserve a arquitetura já aprovada.
O domínio não deve depender de Infrastructure.
Configure os DbSets necessários e os mapeamentos por Fluent API.
Não exponha propriedades mutáveis públicas nas entidades apenas para facilitar o EF Core.
4. MAPEAMENTO DAS ENTIDADES
Configure os relacionamentos conforme o domínio real.
Parada

* Identificador interno.
* Código externo.
* Nome.
* Endereço opcional.
* Coordenada opcional.
* Ativa.
* Origem.
* DadosSimulados.
* Data de criação.
* Relacionamento com linhas.

Linha

* Identificador interno.
* Código.
* Letreiro.
* Terminais.
* Sentido.
* Ativa.
* Origem.
* DadosSimulados.
* Data de criação.

ParadaLinha

* Relação entre parada e linha.
* Chave composta por ParadaId e LinhaId.
* Data da associação.
* Integridade referencial.

Destino

* Identificador.
* Nome.
* Tipo.
* Ativo.
* Origem.
* DadosSimulados.

PercursoEstimado

* Chave composta por ParadaId, LinhaId e DestinoId.
* Tempo estimado de percurso.
* Origem e natureza do dado.
* Relação válida com uma associação ParadaLinha e com Destino.

RegistroPassagem

* Identificador.
* ParadaId e LinhaId.
* ObservadaEmUtc.
* RegistradaEmUtc.
* PrefixoVeiculo.
* Dados específicos de suas subclasses.

Respeite todos os campos existentes no código real.
Não invente propriedades sem verificar sua necessidade.
5. HERANÇA E POLIMORFISMO NO EF CORE
A hierarquia de passagens foi implementada no B2:

* RegistroPassagem, abstrata.
* PassagemSimulada.
* PassagemRegistroManual.

Configure o mapeamento TPH (Table Per Hierarchy).
Utilize um discriminador apropriado para identificar o tipo de passagem.
Mapeie corretamente:

* ChaveSeed, exclusiva das passagens simuladas.
* OrigemRegistro, exclusiva das passagens manuais.
* DadosSimulados conforme a implementação atual.
* Campos de apoio privados que precisem ser persistidos.

Não mapeie propriedades derivadas ou calculadas que não sejam efetivamente persistentes, como:

* CodigoAviso.
* Verificada.
* OficialSptrans.
* NivelConfirmacao, se for calculado pelo tipo concreto.

Confirme que, após salvar e carregar registros do banco, a subclasse correta é reconstruída e seu comportamento polimórfico permanece funcional.
6. DATAS UTC E SQLITE
Este é um ponto crítico.
O domínio utiliza DateTimeOffset em UTC.
O SQLite possui limitações para ordenação e comparação direta de DateTimeOffset por meio do EF Core.
Implemente uma estratégia consistente de persistência que:

* Preserve a representação UTC.
* Permita salvar e recuperar instantes corretamente.
* Permita ordenar passagens no banco de dados.
* Permita filtrar passagens anteriores a um instante de referência.
* Evite depender de comparação em memória quando a consulta deva ocorrer no SQL.
* Preserve a precisão necessária às regras temporais do domínio.

Avalie um ValueConverter para representação UTC como inteiro de ticks ou outra estratégia comprovadamente compatível com o SQLite.
Justifique brevemente a escolha.
Não altere toda a API pública do domínio para resolver um detalhe de armazenamento.
Crie testes com instantes anteriores, posteriores e iguais ao limite, incluindo fronteiras temporais.
7. COORDENADAS E ENUMS
Mapeie Coordenada de maneira compatível com a sua definição de objeto de valor e com EF Core 10.
Como Endereco e Coordenada podem ser nulos, teste explicitamente esses casos.
Configure os enums persistidos como texto quando previsto pela arquitetura.
Verifique se os valores são recuperados corretamente.
Não altere os nomes de enums definidos no contrato.
8. ÍNDICES, CHAVES E INTEGRIDADE
Crie os índices e restrições necessários.
Inclua, quando aplicável:

* Índice único para Parada.CodigoExterno.
* Índice único para Linha.Codigo.
* Índice único para PassagemSimulada.ChaveSeed, tratando corretamente as outras subclasses.
* Chave composta em ParadaLinha.
* Chave composta em PercursoEstimado.
* Relacionamentos com chaves estrangeiras reais.
* Índices úteis para consultas por parada, linha e horário.

Teste a integridade referencial com SQLite real.
Evite exclusões em cascata que apaguem passagens históricas indevidamente.
A aplicação utiliza ativação/desativação de cadastros em vez de excluir seu histórico.
Não crie dezenas de índices sem justificativa.
9. CONFIGURAÇÃO E INJEÇÃO DE DEPENDÊNCIA
Configure o DbContext na infraestrutura e registre-o no contêiner nativo de injeção de dependência do ASP.NET Core.
Utilize:

* EF Core SQLite.
* Connection string de desenvolvimento em configuração adequada.
* Arquivo de banco local ignorado pelo Git.
* TimeProvider já registrado, quando necessário.

Preserve o funcionamento de:

* GET /health.
* OpenAPI.
* Scalar.

Não crie endpoints de negócio nesta etapa.
Não afirme que o health check verifica o banco de dados se ele continua verificando apenas o processo.
Não introduza credenciais ou segredos no repositório.
10. MIGRATION INICIAL
Crie uma migration inicial real usando a ferramenta dotnet-ef já instalada no projeto.
Por exemplo:
`dotnet ef migrations add InitialCreate`
Use os parâmetros corretos para a estrutura real do projeto.
Examine a migration gerada antes de aplicá-la.
Depois execute:
`dotnet ef database update`
Confirme que:

* O banco SQLite foi criado.
* As tabelas esperadas existem.
* As chaves e índices estão presentes.
* As relações estão corretas.
* A migration foi registrada.
* Reexecutar o update não destrói os dados.

Não utilize EnsureCreated como substituto de migrations.
Não inclua o arquivo .db no Git.
Não apague bancos já existentes sem confirmar sua origem e sem necessidade.
Se precisar testar um banco vazio, utilize um arquivo temporário de teste.
11. SEED DE DADOS SIMULADOS
Implemente o seed mínimo necessário para o MVP, conforme o contrato aprovado.
O objetivo é permitir uma demonstração real do backend com dados fictícios identificados.
Utilize os dados e relacionamentos coerentes com os exemplos documentados em contracts/exemplos/.
Crie somente o necessário:

* Paradas simuladas.
* Linhas simuladas.
* Associações entre paradas e linhas.
* Destinos simulados.
* Percursos estimados.
* Registros simulados de passagem no primeiro provisionamento.

Preserve os códigos SIM- e a consistência entre entidades.
Regras obrigatórias do seed:

1. No primeiro provisionamento de um banco vazio, criar os dados demonstrativos.
2. Não duplicar registros em execuções posteriores.
3. Não renovar automaticamente os horários de passagens.
4. Não criar novas passagens ao reiniciar a API com banco já provisionado.
5. Não apagar ou sobrescrever registros manuais.
6. Não simular passagens oficialmente verificadas.
7. Armazenar a chave estável de PassagemSimulada.
8. Calcular os horários iniciais em relação ao instante do provisionamento, apenas uma vez.
9. Manter toda observação identificada como fictícia e não oficial.

Utilize um mecanismo de seed compatível com EF Core 10 e com migrations, incluindo os fluxos síncronos e assíncronos exigidos pela abordagem escolhida.
Evite duplicar lógica entre esses fluxos.
Não associe a execução do seed a cada requisição HTTP.
Não modifique o histórico apenas para que uma passagem pareça recente durante a apresentação.
12. TESTES DE INTEGRAÇÃO DO BANCO
Não utilize EF Core InMemory para fingir que SQLite foi testado.
Crie testes de integração usando o provedor relacional SQLite real, em memória com conexão mantida aberta ou em bancos temporários isolados.
Preserve os 278 testes do B2.
Adicione testes de integração pertinentes, incluindo:

* Criação de esquema a partir da migration.
* Persistência de parada.
* Persistência de linha.
* Associação parada-linha.
* Persistência de destino.
* Persistência de percurso.
* Persistência de registro manual.
* Persistência de registro simulado.
* Materialização da hierarquia TPH.
* ChaveSeed única.
* Código de parada duplicado rejeitado pelo banco.
* Código de linha duplicado rejeitado pelo banco.
* Associação duplicada rejeitada.
* Relacionamento inválido rejeitado.
* Recuperação correta de enums.
* Recuperação de coordenada nula.
* Recuperação de dados fictícios.
* Datas UTC preservadas.
* Ordenação por ObservadaEmUtc.
* Filtro por instante de referência executado no SQLite.
* Seed executado duas vezes sem duplicação.
* Reinicialização sem criar ou renovar passagens.
* Preservação de registros manuais.
* Reaplicação da migration sem perda de dados.

Os testes devem verificar resultados concretos, e não somente que métodos foram chamados.
Evite testes repetitivos sem valor.
Mantenha o banco de testes isolado do banco de desenvolvimento.
13. QUALIDADE DO CÓDIGO
Siga os critérios acadêmicos e a arquitetura existente.
Exijo:

* Código organizado e legível.
* Responsabilidades bem separadas.
* Configurações Fluent API específicas quando isso melhorar a clareza.
* Tratamento correto de nulidade.
* Uso adequado de async/await para operações de persistência.
* CancellationToken quando aplicável.
* Sem acesso ao banco dentro das entidades de domínio.
* Sem SQL manual desnecessário.
* Sem novas dependências sem justificativa.

Não implemente um repositório genérico artificial.
Os repositórios específicos e consultas LINQ serão trabalhados no B4.
14. DOCUMENTAÇÃO E EVIDÊNCIAS
Atualize backend/README.md para explicar:

* Como configurar o SQLite.
* Como executar a migration.
* Como criar o banco.
* Como aplicar o seed.
* Como executar os testes.
* Quais tabelas e relações foram criadas.
* Como os dados simulados são identificados.
* Qual é a estratégia de armazenamento UTC.
* O que já funciona e o que ainda está planejado.

Registre evidências reais, identificadas como B3, em backend/docs/evidencias/.
As evidências devem mostrar resultados efetivamente verificados, como:

* Migração criada e aplicada.
* Esquema do banco.
* Testes executados.
* Confirmação da idempotência do seed.

Não fabrique capturas, saídas de terminal ou resultados.
Registre integralmente este prompt em prompts.md, mantendo o histórico acadêmico exigido pela FIAP.
Preserve a apresentação profissional do código e as diretrizes de commits já estabelecidas.
15. VALIDAÇÕES OBRIGATÓRIAS
Ao concluir, execute a partir de backend/:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef database update

Execute também os testes de integração criados e confirme os resultados reais.
Informe:

* Quantidade total de testes.
* Testes aprovados.
* Testes falhos.
* Testes ignorados.
* Código de saída.
* Avisos e erros de compilação.
* Migration aplicada.
* Quantidade de registros criados pelo seed.
* Resultado do segundo provisionamento.
* Se o banco de dados foi efetivamente consultado.

Verifique que a API ainda inicia e que `/health`, OpenAPI e Scalar continuam funcionando.
Se houver falhas, tente corrigi-las sem alterar o escopo. Registre os problemas e as soluções.
16. GIT E CONTROLE DE ALTERAÇÕES
Não execute commit.
Não execute git push.
Não altere o histórico.
Não utilize force push ou reset --hard.
Confirme que somente arquivos relacionados ao B3, seus testes e sua documentação foram alterados.
Atenção à presença de bancos .db, arquivos temporários, binários e logs.
Deixe o trabalho local aguardando minha revisão.
17. RELATÓRIO FINAL OBRIGATÓRIO
Ao concluir, apresente:
A. Resumo: o que foi desenvolvido, em linguagem clara.
B. Arquivos: árvore das pastas criadas e modificadas.
C. Entidades persistidas: tabelas, relacionamentos, índices e chaves.
D. Configuração EF Core: DbContext, Fluent API e DI.
E. Migrations: comando utilizado e resultado real.
F. SQLite: estratégia de UTC, conversões e limitações resolvidas.
G. Seed: dados criados e comprovação de idempotência.
H. Testes: total de testes, resultados, testes de integração e casos críticos.
I. Evidências: arquivos e verificações disponíveis.
J. Compatibilidade: confirmação de aderência ao contrato B1.3 e à CitizenTech.
K. Rubrica FIAP: critérios atendidos e ainda pendentes, sem declarar concluído o que não foi implementado.
L. Git: branch, alterações locais e confirmação de que não houve commit ou push.
M. Riscos e pendências: possíveis problemas para o B4.
18. RESTRIÇÃO FINAL
NÃO implemente nesta etapa:

* Repositórios e consultas completas de LINQ (B4).
* Serviços de negócio do totem (B5).
* Controllers REST de negócio (B6/B7).
* Integração real com SPTrans (B8).
* Algoritmo completo de alternativas (B9).
* Assistente de IA.
* Interface do totem.
* Funcionalidades de outras disciplinas.
* Sistemas de autenticação novos ou recursos não previstos no contrato.

Execute somente o B3. Ao terminar, pare e aguarde minha auditoria antes do commit e do B4.
O objetivo é demonstrar a utilização real de Entity Framework Core no PontoVoz, com banco SQLite, migrations, persistência e testes de integração, de maneira rigorosamente alinhada aos critérios oficiais da disciplina C# Software Development.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA criou a persistência em
`Infrastructure/Persistence/`:
- `PontoVozDbContext` e um mapeamento Fluent API por entidade;
- a hierarquia TPH de passagens, com a coluna `TipoPassagem` e os campos privados do registro
  manual mapeados;
- `Coordenada` como tipo complexo opcional e enums gravados como texto;
- chaves compostas, chaves estrangeiras compostas para `ParadasLinhas`, índices únicos (inclusive
  o de `ChaveSeed`, filtrado) e todas as exclusões como `RESTRICT`;
- todo `DateTimeOffset` gravado como ticks UTC, para que ordenação e filtros rodem no SQLite.

O seed idempotente usa os ganchos `UseSeeding` e `UseAsyncSeeding` do EF Core e reaproveita a
mesma montagem de dados nos dois fluxos. No primeiro provisionamento cria 21 registros
fictícios coerentes com os exemplos do contrato; num banco já provisionado não faz nada. O
contexto foi registrado na injeção de dependência. A migration `InitialCreate` foi gerada,
revisada e aplicada com `dotnet ef`, e o esquema real foi inspecionado. Repetir o `update` deixou
o banco idêntico. O snapshot do modelo, gravado pelo `dotnet-ef` numa pasta errada, foi movido
para junto da migration. Foram criados 35 testes de integração com SQLite real, num total de 313
testes aprovados; mutações temporárias confirmaram que eles detectam regressões. O `/health`,
o OpenAPI e o Scalar continuam funcionando, sem verificação de banco no `/health`. As evidências
estão em `backend/docs/evidencias/B3-persistencia.md`. Não houve commit.

<a id="prompt-16"></a>

### Prompt 16 — Bloco B3.1: correções finais, validação e publicação do EF Core

**Data:** 07/11/2026 · **Tipo:** Revisão, correção, testes e publicação · **Commit:** `988a5fc`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B3.1 — CORREÇÕES FINAIS, VALIDAÇÃO E PUBLICAÇÃO DO EF CORE
O relatório do Bloco B3 foi analisado.
A implementação de Entity Framework Core, SQLite, migration inicial, seed e testes de integração está aprovada em princípio, mas precisamos concluir uma auditoria final antes de publicá-la no GitHub.
Você é o responsável pela execução técnica completa do backend C# do PontoVoz. O usuário não realizará alterações manuais no código.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base esperado:
`f53fe2d743554d0ef1ef1d7266d63bae5643cc47`
Execute somente B3.1. Não avance para B4.
1. PRINCÍPIO OBRIGATÓRIO — SEGUIR O PROFESSOR
A fonte principal de requisitos é o enunciado oficial de C# Software Development da FIAP.
A implementação final deverá demonstrar:

* POO: 3,0 pontos.
* Web API: 2,5 pontos.
* EF Core + LINQ: 1,5 ponto.
* Async/Await: 1,0 ponto.
* Qualidade de código: 2,0 pontos.

Também são exigidos interfaces, injeção de dependência, testes unitários, documentação de endpoints, README, evidências da aplicação e integração com a CitizenTech.
Não invente funcionalidades acadêmicas nem elimine requisitos do professor.
Neste bloco, concentre-se exclusivamente em completar e publicar a persistência B3.
2. INSPEÇÃO INICIAL
Examine o estado real do repositório.
Confirme:

* Branch main.
* Sincronização com origin/main.
* Alterações não commitadas do B3.
* Estrutura do DbContext.
* Mappings Fluent API.
* Migration inicial e snapshot.
* Seed.
* Testes de integração.
* Registro no Program.cs.
* Connection string.
* Compatibilidade com contracts/api-v1.md.

Preserve os arquivos existentes e evite refatorações desnecessárias.
3. PENDÊNCIA 1 — VERIFICAÇÃO DO BANCO NO HEALTH CHECK
O contrato REST, seção 1.10, estabelece que o endpoint `/health` deverá sinalizar falha no banco quando a verificação de persistência for incorporada no B3.
Atualmente `/health` verifica somente o processo.
Implemente a menor alteração necessária para incluir uma verificação real de conectividade com SQLite.
Requisitos:

* Quando o processo e o banco estiverem saudáveis, devolver HTTP 200.
* Quando o banco estiver inacessível, sinalizar falha real por HTTP 503.
* Preservar o formato do endpoint na medida compatível com o contrato.
* Identificar claramente quais verificações foram executadas.
* Não expor detalhes internos, connection strings ou informações sensíveis.
* Manter CancellationToken e programação assíncrona adequada.
* Não aplicar migration nem recriar o banco automaticamente dentro do health check.
* Não criar um novo endpoint sem necessidade.

Crie testes que comprovem o funcionamento tanto com banco disponível quanto indisponível, utilizando ambiente de teste isolado.
Atualize a documentação pertinente.
4. PENDÊNCIA 2 — DADOS SIMULADOS DAS PASSAGENS
O relatório B3 informa que a coluna DadosSimulados pode ficar NULL para PassagemSimulada.
Investigue o mapeamento real e confirme se isso é intencional e seguro.
O contrato exige que passagens simuladas sejam identificadas como fictícias.
Verifique:

* Persistência da subclasse PassagemSimulada.
* Materialização correta após leitura do SQLite.
* Valor de DadosSimulados observado no objeto reconstruído.
* Comportamento de futuras consultas LINQ.
* Uso do discriminador TPH.
* Possibilidade de filtros incorretos por causa de NULL.

Se existir inconsistência funcional, corrija-a com a menor mudança possível.
Se o valor NULL for uma decisão válida de armazenamento, documente a razão e teste a interpretação correta, especialmente para o B4.
Não introduza uma propriedade ou coluna redundante sem justificativa técnica.
5. PREPARAÇÃO PARA A BUSCA SEM ACENTOS
O contrato exige que a busca encontre termos independentemente de acentos.
O R7 prevê suporte de normalização nos blocos B3/B4.
Verifique se o banco atual permite implementar essa regra corretamente no B4.
Se a solução exigir uma coluna normalizada e uma nova migration, documente o plano preciso para o B4.
Não implemente agora os repositórios completos nem as consultas de negócio.
Evite alterar desnecessariamente a migration inicial já validada.
6. VALIDAÇÃO DO BANCO
Confirme novamente:

* As seis tabelas.
* As chaves e índices.
* Relacionamentos N.
* Hierarquia TPH.
* Conversão de datas para ticks UTC.
* Consultas de comparação e ordenação executadas no SQLite.
* Restrições de integridade referencial.
* Persistência de enums.
* Idempotência do seed.
* Preservação das observações antigas.
* Ausência de duplicações ou reinserção de passagens ao reiniciar.
* Ausência de alterações inesperadas nas classes do domínio.

Revise também a restrição única de Destinos.Nome, verificando sua coerência com o contrato.
Não crie recursos adicionais desnecessários.
7. TESTES E EVIDÊNCIAS
Execute novamente, a partir de backend/:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

Teste também:

* GET /health com banco disponível.
* Comportamento de saúde quando o banco está indisponível, sem danificar o banco de desenvolvimento.
* GET /openapi/v1.json.
* Interface Scalar.

O relatório anterior apresentou 313 testes aprovados. Todos devem continuar passando, além dos novos testes efetivamente necessários.
Atualize backend/docs/evidencias/B3-persistencia.md com resultados reais.
Não invente resultados nem declare que testes foram executados se não tiverem sido.
8. README PARA EXECUÇÃO EM OUTRO COMPUTADOR
O professor precisa conseguir executar o projeto a partir do GitHub.
Verifique se o README descreve, com comandos corretos:

1. Pré-requisitos.
2. Restauração das ferramentas e dependências.
3. Criação do banco através das migrations.
4. Execução da API.
5. Acesso ao Swagger/Scalar.
6. Execução dos testes.
7. Origem e limitações dos dados simulados.

Considere que o professor clonará um repositório que NÃO contém o banco SQLite local.
Não dependa de configurações exclusivas do computador atual.
9. REGISTRO ACADÊMICO E PADRÃO PROFISSIONAL
Registre esta solicitação em prompts.md, conforme o padrão existente.
Preserve a documentação acadêmica do uso de IA exigida pela FIAP.
Não adicione comentários promocionais, banners ou assinaturas automáticas de ferramentas.
Não altere commits antigos.
Utilize os dados legítimos de autoria já configurados no Git local.
10. AUTORIZAÇÃO CONDICIONAL PARA COMMIT E PUSH
Se todas as verificações passarem, está autorizado a publicar o B3.
Mensagem de commit:
`feat: add EF Core persistence and SQLite integration tests`
Antes de publicar:

* Inspecione o diff completo.
* Confirme que somente alterações pertinentes ao B3 serão versionadas.
* Confirme ausência de senhas, tokens, arquivos .db, arquivos temporários e binários.
* Verifique a mensagem real do commit.
* Não acrescente trailer Co-Authored-By ou assinatura automática de ferramenta.
* Não utilize force push.

Execute commit e push normal para origin/main.
Se encontrar falha relevante que não consiga corrigir com segurança, NÃO publique; explique o bloqueio.
11. RELATÓRIO FINAL
Apresente objetivamente:
A. O que foi corrigido.
B. Estado final da verificação de saúde.
C. Resultado da auditoria de DadosSimulados.
D. Confirmação das migrations e integridade do banco.
E. Quantidade real de testes aprovados, falhos e ignorados.
F. Resultado do build e dos comandos EF Core.
G. Evidências registradas.
H. Comandos para o professor executar o backend a partir do GitHub.
I. Hash e mensagem do commit, se publicado.
J. Resultado do push e sincronização de main com origin/main.
K. Pendências específicas para B4.
L. Critérios oficiais da FIAP já implementados e ainda pendentes.
Não implemente repositórios LINQ, novos endpoints de negócio, integração externa ou serviços do totem.
Execute B3.1, valide, publique somente se estiver correto e pare. Não inicie B4.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** - **Verificação do banco:** a IA criou o `BancoDadosHealthCheck`. Ele confere se o SQLite está
  acessível e se as migrations foram aplicadas, sem criar o banco nem aplicar migrations, e foi
  registrado junto com a persistência. O `/health` manteve o formato e passou a responder 200
  com o banco saudável e 503 com o banco indisponível ou sem migrations.
- **Testes do `/health`:** três testes pela API real (`WebApplicationFactory`), com bancos
  temporários isolados, comprovam esses cenários.
- **`DadosSimulados` das passagens:** a auditoria concluiu que o `NULL` nas passagens simuladas é
  uma decisão de armazenamento válida, porque o tipo, pelo discriminador, as identifica como
  fictícias. Testes mostram a materialização correta e que o filtro pela propriedade calculada
  não é traduzido, o que evita resultados errados. Também foi testado o filtro traduzível que o
  B4 deve usar.
- **Busca sem acentos:** um teste comprovou que o `LIKE` do SQLite não ignora acentos, e o plano
  de colunas de busca normalizadas para o B4 foi documentado.
- **Documentação:** foram atualizados o contrato (seção 6 e histórico), o `backend/README.md`
  (com um passo a passo para executar a partir do GitHub) e as evidências do B3.
- **Validação:** 320 testes aprovados; todos os comandos `dotnet ef` passaram; o banco de
  desenvolvimento ficou idêntico.
- **Publicação:** a etapa foi publicada com o commit
  `feat: add EF Core persistence and SQLite integration tests`, sem trailer, e push normal.

---

## Bloco B4 — repositórios e consultas LINQ

<a id="prompt-17"></a>

### Prompt 17 — Bloco B4: repositórios, interfaces e consultas LINQ

**Data:** 07/11/2026 · **Tipo:** Implementação e testes · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 06 — BLOCO B4: REPOSITÓRIOS, INTERFACES E CONSULTAS LINQ
Atue como engenheiro de software sênior especializado em C#, ASP.NET Core 10, Entity Framework Core e LINQ.
Estamos desenvolvendo o backend do PontoVoz, projeto CitizenTech da FIAP.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Os blocos B0, B1, B2 e B3 estão concluídos e publicados.
Commit esperado:
`988a5fce736cc01fe1d0be4ab72f8b624c2f80e8`
Sua responsabilidade é executar integralmente a parte técnica deste bloco, incluindo implementação, correção, testes, validação e documentação.
Execute exclusivamente o B4. Não avance para B5 e não faça commit ou push antes da auditoria.
1. PRIORIDADE ABSOLUTA — ENUNCIADO DA FIAP
O professor exige:

* Programação Orientada a Objetos: 3,0 pontos.
* Web API: 2,5 pontos.
* Entity Framework Core e LINQ: 1,5 ponto.
* Async/Await: 1,0 ponto.
* Qualidade de código: 2,0 pontos.

Também exige interfaces, injeção de dependência, APIs REST, testes unitários, código organizado, README, documentação e evidências.
Neste bloco, nosso objetivo é demonstrar consultas LINQ reais ao banco SQLite, utilizando EF Core, interfaces e injeção de dependência.
Não acrescente ferramentas ou funcionalidades desnecessárias.
Não retire nem simplifique requisitos obrigatórios.
O contrato REST aprovado deve ser respeitado.
2. INSPEÇÃO INICIAL
Antes de modificar qualquer arquivo:

1. Confirme a pasta de trabalho.
2. Confirme o estado de main e origin/main.
3. Verifique o commit esperado.
4. Confirme que não há alterações locais inesperadas.
5. Leia os arquivos reais do Domain.
6. Leia o PontoVozDbContext, os mapeamentos e migrations.
7. Leia `contracts/api-v1.md`.
8. Leia `contracts/README.md`.
9. Leia os exemplos JSON e as regras R1 a R8.
10. Examine os 320 testes existentes.

Não suponha nomes de propriedades ou métodos sem conferir o código.
O contrato já publicado é nossa referência funcional.
Não crie novos endpoints nem modifique a proposta CitizenTech.
3. ARQUITETURA DOS REPOSITÓRIOS
Implemente os componentes necessários para consultar as entidades persistidas.
A estrutura sugerida é:
`Application/Interfaces/`
`Infrastructure/Repositories/`
Crie interfaces específicas para as operações reais de consulta, evitando um repositório genérico sem utilidade.
Contemple, conforme as necessidades do contrato:

* Consultas de paradas.
* Consultas de linhas.
* Consultas de destinos.
* Consultas de associações entre paradas e linhas.
* Consultas de percursos estimados.
* Consultas de registros de passagem.

Não é obrigatório criar uma classe para cada item se uma organização mais simples atender integralmente às responsabilidades.
As interfaces devem abstrair as operações de acesso aos dados.
As implementações devem utilizar EF Core e LINQ.
Não coloque SQL ou dependências do EF Core nas entidades de domínio.
4. CONSULTAS DE PARADAS
Implemente consultas necessárias para os futuros endpoints:

* GET /api/v1/paradas
* GET /api/v1/paradas/{id}

Considere as regras do contrato:

* Listagem de paradas ativas.
* Paginação.
* Busca por nome.
* Busca por código externo.
* Consulta por identificador interno.
* Ordenação determinística.
* Identificação de dados fictícios.
* Exclusão de cadastros inativos das consultas públicas.
* Retorno vazio quando não houver resultados.

Respeite exatamente os filtros, nomes e limites definidos no contrato.
Não altere a semântica da API.
5. BUSCA SEM ACENTOS — R7
O contrato exige buscas capazes de localizar nomes independentemente de acentos e diferenças entre maiúsculas e minúsculas.
O B3 demonstrou que o LIKE padrão do SQLite não atende completamente a esse requisito.
Implemente uma solução simples, correta e testável.
O planejamento B3 prevê:

* Normalização textual consistente.
* Campos de busca normalizados no banco.
* Nova migration `AddSearchColumns`, caso necessária.
* Atualização dos campos em novos cadastros e em alterações futuras.
* Preenchimento dos campos para registros já existentes.

Antes de implementar, examine a estrutura atual e confirme a solução de menor complexidade que preserva o contrato.
Atenção:

1. Normalizar texto Unicode, inclusive letras acentuadas e formas de composição equivalentes.
2. Evitar diferenças de comportamento entre gravação e consulta.
3. Não carregar a tabela inteira na memória para filtrar os resultados.
4. Não utilizar funções C# em consultas LINQ que o SQLite não consiga traduzir.
5. Não perder dados existentes durante a migration.
6. Testar a atualização de registros anteriores à migration.
7. Verificar se a busca funciona depois de alterar o nome de uma entidade.
8. Verificar que a normalização não modifica o nome original exibido ao usuário.
9. Não afirmar que um índice acelera todos os padrões de busca sem verificar a consulta real.

Os testes devem incluir:

* demonstracao
* Demonstração
* DEMONSTRAÇÃO
* Texto com espaços adicionais
* Texto com acentos combinados
* Busca inexistente

Se o preenchimento dos registros antigos exigir uma etapa de migração de dados separada, implemente-a de forma reproduzível, segura e documentada.
Não altere a migration InitialCreate.
6. CONSULTAS DE LINHAS
Prepare as operações de consulta necessárias para:

* Listagem de linhas.
* Busca por código.
* Consulta das linhas associadas a uma parada.
* Identificação do sentido da linha.
* Filtro de linhas ativas.
* Ordenação determinística.
* Dados simulados corretamente identificados.

As consultas devem respeitar o relacionamento N existente em ParadaLinha.
Não considere que uma linha atende determinada parada apenas porque elas existem separadamente.
A associação deve estar persistida e válida.
7. CONSULTAS DE DESTINOS E PERCURSOS
Prepare as consultas necessárias para:

* GET /api/v1/destinos
* Identificação de um destino existente.
* Listagem e busca paginada de destinos ativos.
* Busca dos percursos diretos cadastrados para uma parada e destino.
* Consulta dos minutos estimados.
* Confirmação de que a linha atende à parada.
* Exclusão de associações e cadastros inativos.

Não implemente o algoritmo completo de comparação de alternativas agora. Ele pertence ao B9.
O B4 deve apenas obter os dados corretos para esse cálculo futuro.
8. ÚLTIMAS PASSAGENS REGISTRADAS
Esta é uma consulta importante do PontoVoz.
Implemente acesso aos registros de passagem necessários ao futuro endpoint:
GET /api/v1/paradas/{id}/ultimas-passagens
Requisitos:

* Buscar os registros de uma parada.
* Identificar a última passagem por linha.
* Não considerar registros com ObservadaEmUtc posterior ao instante de consulta.
* Permitir filtro por uma linha específica, conforme o contrato.
* Preservar a distinção entre PassagemSimulada e PassagemRegistroManual.
* Interpretar corretamente DadosSimulados na hierarquia TPH.
* Respeitar o estado ativo dos cadastros.
* Não transformar uma previsão de chegada em passagem observada.
* Não declarar uma observação manual ou simulada como passagem oficialmente verificada.
* Deixar os cálculos de desatualização e a apresentação dos avisos para os serviços apropriados, quando não forem responsabilidades da consulta.

A seleção da última passagem de cada linha deve ocorrer por meio de LINQ traduzido para SQL, sem carregar todos os registros para agrupá-los em memória.
Utilize uma ordenação determinística para desempates por horário, caso existam.
Confirme o SQL gerado e teste a execução com SQLite real.
9. LINQ E EF CORE — DEMONSTRAÇÃO ACADÊMICA
O professor deve conseguir identificar claramente o uso de Entity Framework Core e LINQ.
Empregue, quando fizer sentido:

* Where.
* Select.
* OrderBy / ThenBy.
* Skip / Take.
* AnyAsync.
* FirstOrDefaultAsync.
* GroupBy, quando a tradução e o resultado forem adequados.
* AsNoTracking para consultas somente leitura.
* Include ou projeções, quando necessário para relacionamentos.
* Consultas por múltiplos filtros.
* Cancelamento com CancellationToken.

Não utilize todos esses operadores artificialmente apenas para demonstrar quantidade.
Priorize consultas corretas, claras e eficientes.
Garanta que:

* Paginação seja executada no banco.
* Filtros sejam aplicados antes de materializar os resultados.
* Não ocorram consultas N+1 desnecessárias.
* Não haja consultas ilimitadas sem justificativa.
* Não existam ordenações não determinísticas em resultados paginados.
* O acesso aos dados permaneça isolado na infraestrutura.

Não use `ToList()` antes dos filtros quando o objetivo for executar a consulta no SQL.
Não exponha IQueryable diretamente aos futuros Controllers.
10. ASYNC/AWAIT E INJEÇÃO DE DEPENDÊNCIA
Implemente operações assíncronas de consulta por meio dos métodos de EF Core.
Utilize:

* Task ou Task<T>.
* async/await.
* CancellationToken.
* Interfaces específicas.
* Registro das implementações no contêiner de DI do ASP.NET Core.

Os serviços futuros devem conseguir receber essas interfaces por injeção de dependência.
Não crie tarefas artificiais apenas para satisfazer a rubrica.
Não utilize Task.Run para disfarçar operações síncronas de banco.
Considere que o provedor SQLite tem particularidades de execução assíncrona; documente o uso real de async/await sem alegar paralelismo ou desempenho que não tenha sido demonstrado.
11. TESTES UNITÁRIOS E DE INTEGRAÇÃO
Preserve todos os 320 testes existentes.
Adicione testes reais para as consultas implementadas.
Utilize SQLite, como no B3.
Teste pelo menos:

1. Listagem de paradas ativas.
2. Exclusão de paradas inativas.
3. Busca por código externo.
4. Busca por nome.
5. Busca com e sem acentos.
6. Paginação e ordenação.
7. Parada inexistente.
8. Linhas associadas a uma parada.
9. Linha inativa.
10. Destinos cadastrados.
11. Destino inexistente.
12. Percursos diretos.
13. Ausência de percurso.
14. Última passagem por linha.
15. Várias passagens da mesma linha.
16. Passagens com mesmo horário e desempate.
17. Passagem com horário futuro.
18. Passagem simulada.
19. Passagem manual com DadosSimulados true.
20. Passagem manual com DadosSimulados false.
21. Consulta com CancellationToken cancelado.
22. Filtros e paginação executados no banco.
23. Migration dos campos de busca.
24. Preservação de registros anteriores à migration.
25. Atualização de nome e atualização da normalização.
26. Consultas em banco com dados reais de teste e relacionamento N.

Não utilize mocks para afirmar que uma consulta LINQ foi executada no SQLite.
Não crie testes artificiais.
Inclua testes que comprovem que uma regressão relevante seria detectada.
Não precisa inflar a quantidade de testes. A prioridade é cobrir as regras.
12. MIGRATION E PRESERVAÇÃO DE DADOS
Se criar a migration dos campos normalizados:

* Não altere a InitialCreate já publicada.
* Gere uma nova migration real.
* Revise o conteúdo gerado.
* Aplique-a a uma cópia isolada de banco existente.
* Confirme a preservação de dados.
* Verifique a atualização dos valores normalizados anteriores.
* Execute novamente a aplicação da migration para confirmar ausência de alterações inesperadas.
* Verifique índices e integridade referencial.
* Confirme que o seed continua idempotente.
* Confirme que registros manuais e simulados não são renovados nem apagados.

Não apague o banco de desenvolvimento.
Não versionar arquivos SQLite.
13. COMPATIBILIDADE COM O CONTRATO
Leia as seções relevantes do contrato antes de decidir nomes, parâmetros e comportamentos.
Os futuros endpoints deverão continuar exatamente como planejado.
Não implemente Controllers neste bloco.
Não implemente ainda:

* Cálculo completo de previsões.
* Integração SPTrans.
* Comparador de alternativas.
* Resumo agregado.
* IA conversacional.
* Interface do totem.

Se encontrar contradição real entre o contrato e a estrutura persistida, explique-a e faça somente a correção indispensável.
Não crie um novo escopo sem necessidade.
14. DOCUMENTAÇÃO E EVIDÊNCIAS
Atualize backend/README.md explicando:

* Repositórios e interfaces implementados.
* Exemplos reais de LINQ utilizados.
* Paginação.
* Busca sem acentos.
* Consulta da última passagem.
* Como executar os testes.
* Como aplicar a nova migration, se houver.
* O que foi implementado e o que ainda está pendente.

Registre evidências efetivas do B4 em:
`backend/docs/evidencias/B4-consultas-linq.md`
Inclua:

* Exemplos de consultas reais.
* SQL traduzido pelo EF Core.
* Resultados de testes.
* Comandos de migrations, caso aplicáveis.
* Evidência de que os filtros e a paginação foram executados no banco.

Atualize prompts.md conforme a orientação acadêmica da FIAP.
Não insira assinaturas automáticas de ferramentas ou mensagens promocionais no código.
15. VALIDAÇÃO FINAL
Execute:

* dotnet restore.
* dotnet build.
* dotnet test.
* dotnet ef migrations list.
* dotnet ef database update.
* dotnet ef migrations has-pending-model-changes.

Confirme:

* Número real de testes.
* Falhas e testes ignorados.
* Código de saída.
* Ausência de erros e avisos de compilação.
* Banco atualizado.
* Seed preservado.
* `/health` funcionando.
* OpenAPI e Scalar funcionando.
* Nenhum endpoint de negócio implementado antecipadamente.

Teste também a execução em ambiente limpo ou banco de teste novo, sem depender de arquivos exclusivos do computador atual.
Se uma verificação falhar, investigue e corrija antes de considerar o bloco concluído.
16. GIT E CONTROLE
Confirme branch main e sincronização com origin/main.
Não faça commit.
Não execute git push.
Não utilize force push.
Não altere o histórico.
Não apague ou sobrescreva trabalhos existentes.
Deixe as alterações locais para auditoria antes da publicação.
17. RELATÓRIO FINAL
Apresente:
A. Resumo simples: o que foi construído.
B. Interfaces e repositórios criados.
C. Consultas LINQ implementadas.
D. Estratégia da busca sem acentos.
E. Migration, se criada.
F. Consulta da última passagem por linha.
G. Integração com EF Core e DI.
H. Testes e resultados reais.
I. Evidências registradas.
J. Compatibilidade com o contrato.
K. Matriz dos critérios do professor: o que está implementado e o que falta.
L. Estado do Git.
M. Riscos e pendências para B5.
Não faça commit e não avance para B5.
Pare ao concluir o B4 e aguarde minha auditoria.
Nosso objetivo é demonstrar com clareza e rigor o uso real de EF Core, LINQ, interfaces, injeção de dependência e operações assíncronas, cumprindo exatamente o que o professor exige para a Global Solution.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a IA criou:
- quatro interfaces de repositório em `Application/Interfaces` (paradas; linhas e associações;
  destinos e percursos; passagens), os tipos `Paginacao` e `Pagina<T>` e as implementações com EF
  Core e LINQ em `Infrastructure/Repositories`, registradas na injeção de dependência;
- a busca sem acentos (R7), com uma coluna `TextoBusca` normalizada por tabela, mantida pelo
  `DbContext` ao salvar, e a migration `AddSearchColumns`. Uma etapa idempotente preenche os
  registros antigos depois das migrations;
- a consulta da última passagem por linha, traduzida para SQL num único comando.

Durante os testes foram encontrados e corrigidos três problemas:
- a `Paginacao` era uma `record struct`, e `new Paginacao()` zerava os campos;
- o EF Core não aceita `Include` depois de `GroupBy`;
- o seed gerava ids de linhas diferentes dos exemplos do contrato.

Foram criados 67 testes, num total de 387 aprovados, e mutações temporárias confirmaram que eles
detectam regressões. A migration foi aplicada a uma cópia do banco e ao banco de
desenvolvimento, sem perda de dados, e o fluxo foi repetido em ambiente limpo. O SQL gerado e os
planos de execução foram registrados em `backend/docs/evidencias/B4-consultas-linq.md`. Não houve
commit.

<a id="prompt-18"></a>

### Prompt 18 — Bloco B4.1: auditoria final e publicação das consultas LINQ

**Data:** 07/11/2026 · **Tipo:** Auditoria, correção e publicação · **Commit:** `38d2dae`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B4.1 — AUDITORIA FINAL E PUBLICAÇÃO DAS CONSULTAS LINQ
O relatório do Bloco B4 foi analisado.
A implementação de repositórios, interfaces, injeção de dependência, consultas LINQ, normalização textual e migration de busca está aprovada em princípio.
Sua responsabilidade agora é realizar a auditoria técnica final e publicar o B4 no GitHub, desde que todas as verificações sejam satisfatórias.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base esperado:
`988a5fce736cc01fe1d0be4ab72f8b624c2f80e8`
Não inicie B5. Não acrescente novas funcionalidades.
1. PRINCÍPIO DE EXECUÇÃO
Você é responsável pela execução técnica integral do projeto C#.
O objetivo é atender rigorosamente ao enunciado oficial de C# Software Development da FIAP.
Critérios de avaliação:

* POO: 3,0 pontos.
* Web API: 2,5 pontos.
* EF Core + LINQ: 1,5 ponto.
* Async/Await: 1,0 ponto.
* Qualidade do código: 2,0 pontos.

Requisitos adicionais: interfaces, injeção de dependência, testes unitários, documentação, README, evidências e integração com a proposta CitizenTech.
Não adicione nem remova requisitos por iniciativa própria.
Neste bloco, concentre-se apenas na finalização e publicação do B4.
2. INSPEÇÃO DO CÓDIGO
Examine os arquivos reais e confirme:

* Branch main sincronizada com origin/main.
* Ausência de alterações inesperadas.
* Interfaces em Application.
* Implementações em Infrastructure.
* Consultas LINQ assíncronas.
* Registro dos repositórios na injeção de dependência.
* Migration AddSearchColumns.
* Atualização dos campos normalizados.
* Consultas de última passagem.
* Testes do B4.
* README e evidências.

Verifique que os contratos REST permanecem respeitados e que não foram implementados Controllers de negócio antecipadamente.
3. ATENÇÃO ESPECIAL — IDENTIFICADORES DO SEED
O relatório B4 informou que a ordem de inserção das linhas foi modificada para aproximar os IDs internos dos exemplos JSON.
Isso exige uma revisão.
Os IDs numéricos são identificadores internos gerados pelo banco. Não devem ser considerados valores estáveis entre bancos diferentes.
Confirme que:

1. A API e os futuros services não dependerão de IDs fixos do seed.
2. Os testes funcionais não exigem que SIM-L001, SIM-L002, SIM-L003 ou SIM-L004 tenham IDs numéricos específicos.
3. Os exemplos JSON são explicitamente ilustrativos.
4. A identificação externa das paradas e linhas permanece estável por meio dos códigos.
5. Bancos já provisionados continuam funcionando mesmo com IDs diferentes.
6. A ordem de inserção não é utilizada como substituto de uma regra de negócio.

Se houver testes ou documentação que tratem o ID numérico como uma garantia, corrija-os.
Não reorganize novamente o seed apenas para produzir IDs específicos.
Preserve os relacionamentos e a idempotência.
4. VALIDAÇÃO DA BUSCA SEM ACENTOS
Confirme novamente que:

* A busca normaliza Unicode de modo consistente.
* A pesquisa funciona com e sem acentos.
* Maiúsculas e minúsculas são tratadas corretamente.
* A normalização não modifica os nomes exibidos.
* Uma alteração de nome atualiza o campo normalizado.
* Registros anteriores à migration recebem os valores normalizados.
* Os filtros são executados no SQLite, não sobre tabelas carregadas inteiramente na memória.
* A migration não apaga dados.
* O seed continua sem duplicar ou renovar passagens.

A decisão de não criar índice para `LIKE '%termo%'` está aceita, desde que continue documentada e tecnicamente justificada.
Não acrescente índices sem necessidade.
5. VERIFICAÇÃO DOS REPOSITÓRIOS
Revise os resultados reais das consultas:

* Paradas ativas e paginadas.
* Busca por código externo.
* Linhas associadas a paradas.
* Destinos ativos.
* Percursos diretos.
* Última passagem por linha.
* Desempate determinístico por horário.
* Exclusão de observações futuras.
* Distinção entre passagens simuladas e manuais.
* Filtros e paginação executados no SQL.
* Operações assíncronas com CancellationToken.

Verifique especialmente que a paginação nunca retorna tamanho zero indevidamente.
Confirme que não existem métodos com `ToList()` antes dos filtros que deveriam ser executados no banco.
Não modifique o escopo funcional.
6. EXECUÇÃO EM AMBIENTE LIMPO
Repita as verificações obrigatórias:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

O último relatório apontou 387 testes aprovados, com zero falhas e zero avisos.
Informe os números reais encontrados agora.
Confirme que o projeto funciona em uma instalação limpa, com banco novo, e que o banco existente mantém seus dados.
Verifique também:

* GET /health.
* OpenAPI.
* Scalar.

Não declare aprovação se algum comando tiver falhado.
7. SEGURANÇA E DOCUMENTAÇÃO
Inspecione os arquivos que serão publicados.
Não inclua:

* Arquivos SQLite.
* Segredos.
* Tokens.
* Credenciais.
* Binários desnecessários.
* Arquivos temporários de diagnóstico.
* Logs sensíveis.
* Código de teste artificial.

Atualize as evidências somente com resultados efetivamente observados.
Registre este prompt em prompts.md conforme a exigência acadêmica.
Preserve o padrão profissional dos arquivos.
Não adicione marcas promocionais ou assinaturas automáticas de ferramentas.
8. PUBLICAÇÃO AUTORIZADA
Se todas as verificações passarem, faça o commit com a seguinte mensagem:
`feat: implement EF Core LINQ repositories and search`
Confirme a mensagem efetivamente gravada antes do push.
Mantenha a autoria legítima e o e-mail noreply já configurado.
Não acrescente trailer automático Co-Authored-By.
Não altere commits anteriores.
Publique com push normal para origin/main.
Não utilize force push.
Caso encontre um problema que não consiga resolver com segurança, interrompa a publicação e apresente o impedimento.
9. RELATÓRIO FINAL
Apresente:
A. Resumo das verificações e correções.
B. Resultado da conferência dos IDs internos.
C. Interfaces e repositórios efetivamente publicados.
D. Resultado da busca sem acentos e da migration.
E. Quantidade real de testes aprovados, falhos e ignorados.
F. Resultado de build e dos comandos EF Core.
G. Confirmação da execução em ambiente limpo.
H. Resultado dos endpoints existentes.
I. Hash e mensagem do commit.
J. Resultado do push e sincronização main/origin/main.
K. Pendências específicas para B5.
L. Matriz atualizada da rubrica FIAP, sem confundir implementação com nota garantida.
Não inicie B5. Pare após concluir esta auditoria e a publicação autorizada.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** - **Ids internos:** a IA confirmou que repositórios e regras não dependem deles. Removeu o teste
  do B4 que exigia ids específicos, trocou a busca do destino por id pela busca pelo nome e
  ajustou os comentários do seed, sem reorganizá-lo.
- **Testes novos:** um teste que prova o funcionamento com ids diferentes dos exemplos e 15
  testes unitários de paginação. Uma mutação confirmou que esses testes detectariam a volta do
  problema da `record struct`.
- **Documentação:** README e evidências foram corrigidos para não tratar ids como garantia.
- **Validação:** restore, build sem avisos, 402 testes aprovados e os comandos `dotnet ef`, no
  banco de desenvolvimento (dados preservados) e em ambiente limpo.
- **Publicação:** commit `feat: implement EF Core LINQ repositories and search`, sem trailer, e
  push normal.

---

## Bloco B5 — serviços de aplicação

<a id="prompt-19"></a>

### Prompt 19 — Bloco B5: serviços de negócio, DTOs, validações e testes

**Data:** 07/11/2026 · **Tipo:** Implementação, testes e documentação (inclui diretriz do prompts.md) · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 07 — BLOCO B5: SERVIÇOS DE NEGÓCIO, DTOs, VALIDAÇÕES E TESTES
Atue como engenheiro de software sênior especializado em C#, .NET 10, arquitetura de aplicações, ASP.NET Core e testes automatizados.
Estamos desenvolvendo o backend C# do PontoVoz, solução CitizenTech da FIAP para consultas de transporte público em um totem inteligente.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Último commit esperado:
`38d2dae398d595bd26c830732341a709d44ae033`
Os blocos B0, B1, B2, B3 e B4 foram concluídos e publicados.
Sua responsabilidade nesta etapa é executar integralmente a implementação técnica, os testes, a correção de problemas, a documentação e as validações do B5.
Execute exclusivamente o Bloco B5. Não implemente B6 e não faça commit ou push.
1. PRIORIDADE ABSOLUTA — REQUISITOS OFICIAIS DA FIAP
O enunciado de C# Software Development exige:

* Programação Orientada a Objetos — 3,0 pontos.
* Web API — 2,5 pontos.
* Entity Framework Core + LINQ — 1,5 ponto.
* Async/Await — 1,0 ponto.
* Qualidade do código — 2,0 pontos.

Também exige interfaces, injeção de dependência, APIs REST, testes unitários, documentação dos endpoints, README, evidências e integração com a proposta CitizenTech.
O contrato `contracts/api-v1.md` é a referência funcional do projeto.
Não invente funcionalidades nem retire requisitos obrigatórios.
O objetivo deste bloco é transformar as consultas existentes em serviços de aplicação capazes de preparar respostas corretas para os futuros Controllers REST.
2. INSPEÇÃO INICIAL OBRIGATÓRIA
Antes de modificar arquivos:

1. Confirme o diretório correto.
2. Confirme a branch main e o último commit.
3. Confira a sincronização com origin/main.
4. Verifique que a árvore de trabalho está limpa.
5. Leia o contrato REST e seus exemplos JSON.
6. Examine as entidades e regras do Domain.
7. Examine os repositórios LINQ do B4 e suas interfaces.
8. Examine o DbContext, a injeção de dependência e a paginação.
9. Confira a estrutura dos testes existentes.
10. Identifique possíveis ambiguidades antes de implementar.

Não suponha nomes de propriedades, métodos ou campos sem conferir o código real.
Preserve os contratos e os arquivos publicados.
3. ESCOPO DESTE BLOCO
Implemente os serviços necessários para:

* Listagem e consulta de paradas.
* Listagem e consulta de linhas.
* Listagem e consulta de destinos.
* Consulta das últimas passagens registradas por linha.
* Validação de parâmetros de entrada.
* Montagem de DTOs.
* Padronização de erros de aplicação.
* Montagem de avisos relacionados à origem e à confiabilidade dos dados.

Não implemente neste bloco:

* Controllers REST de negócio.
* Operações administrativas de escrita.
* Provedor de previsões.
* Integração real com SPTrans.
* Comparador completo de alternativas.
* Resumo agregado que depende de previsões.
* Interface do totem.
* Assistente conversacional.

4. ARQUITETURA DA CAMADA APPLICATION
Utilize a estrutura existente em:
`backend/src/PontoVoz.Api/Application/`
Organize somente os componentes necessários:

* Interfaces de serviços.
* Implementações de serviços.
* DTOs.
* Validações.
* Mapeamentos.
* Erros de aplicação.

As interfaces devem representar responsabilidades concretas.
A camada Application deverá depender das interfaces dos repositórios, não diretamente do DbContext.
Não crie abstrações artificiais apenas para demonstrar conceitos acadêmicos.
Preserve a separação entre Domain, Application e Infrastructure.
5. SERVIÇOS DE PARADAS
Implemente a lógica necessária aos futuros endpoints:
GET /api/v1/paradas
GET /api/v1/paradas/{id}
Requisitos:

* Validar os parâmetros.
* Aplicar os limites de paginação do contrato.
* Validar termo de pesquisa e código externo.
* Utilizar os repositórios existentes.
* Excluir paradas inativas das consultas públicas.
* Preservar a identificação dos dados simulados.
* Produzir respostas paginadas.
* Produzir os DTOs previstos no contrato.
* Tratar listas vazias.
* Distinguir entrada inválida de recurso inexistente.
* Preservar o fluxo de identificação do totem pelo codigoExterno.

Não crie novos endpoints.
6. SERVIÇOS DE LINHAS
Implemente a lógica necessária a:
GET /api/v1/linhas
Inclua:

* Filtros definidos no contrato.
* Paginação, quando prevista.
* Informações de sentido.
* Informações dos terminais.
* Identificação de dados simulados.
* Exclusão de linhas inativas.
* Ordenação determinística.
* DTOs compatíveis com os exemplos documentados.

Não implemente operações administrativas.
7. SERVIÇOS DE DESTINOS
Implemente a lógica necessária a:
GET /api/v1/destinos
Requisitos:

* Pesquisa por nome.
* Busca sem acentos utilizando os recursos do B4.
* Paginação conforme o contrato.
* Exclusão de destinos inativos.
* Identificação de dados simulados.
* Listas vazias quando apropriado.
* Mapeamento para DTOs corretos.

Não implemente agora a comparação de alternativas de deslocamento.
8. SERVIÇO DE ÚLTIMAS PASSAGENS
Implemente a lógica para o futuro endpoint:
GET /api/v1/paradas/{id}/ultimas-passagens
O serviço deverá:

1. Validar o identificador da parada.
2. Confirmar a existência de uma parada ativa.
3. Validar o filtro opcional de linha.
4. Confirmar que a linha atende à parada.
5. Consultar as linhas ativas da parada.
6. Consultar a última passagem registrada de cada linha.
7. Excluir observações com horário futuro.
8. Identificar as linhas sem registros.
9. Calcular minutosDesdeObservacao.
10. Calcular desatualizada.
11. Diferenciar registros simulados e manuais.
12. Preservar origemRegistro e nivelConfirmacao.
13. Produzir avisos estruturados.
14. Incluir consultadoEm corretamente.

A desatualização deve respeitar a regra do domínio: intervalo exato superior a 60 minutos.
Leia o instante atual usando TimeProvider, preferencialmente uma única vez por operação.
Nenhuma observação fictícia ou manual poderá ser apresentada como oficialmente confirmada.
Não confunda previsão de chegada com passagem observada.
9. VALIDAÇÕES E ERROS DE APLICAÇÃO
Implemente validações consistentes para:

* Identificadores inválidos.
* Parâmetros ausentes.
* Termos de busca fora dos limites.
* Código externo inválido.
* Paginação inválida.
* Parada inexistente.
* Linha inexistente.
* Linha não associada à parada.
* Destino inexistente quando necessário.
* Ausência de registros.

Os serviços devem produzir erros de aplicação com códigos estáveis, permitindo que os futuros Controllers ou middleware convertam esses erros para ProblemDetails.
Respeite a política de HTTP 400, 404 e 422 do contrato.
Não misture regras de domínio com detalhes de HTTP.
Não transforme exceções inesperadas em erros de validação.
O tratamento HTTP completo será implementado no B6.
10. DTOs E MAPEAMENTO
Crie os DTOs necessários para os serviços deste bloco.
Respeite exatamente as estruturas previstas no contrato.
Verifique:

* Nomes de campos.
* Tipos de dados.
* Valores nulos.
* Listas vazias.
* Paginação.
* Enums.
* Avisos com codigo e mensagem.
* Datas e fusos.
* Origem dos dados.
* Indicadores de dados simulados.
* IDs internos e códigos externos.

Não crie respostas estáticas copiando os exemplos JSON.
Os DTOs devem ser construídos a partir dos dados reais retornados pelos repositórios.
Prepare a futura serialização JSON em camelCase sem implementar os Controllers antecipadamente.
11. ASYNC/AWAIT E INJEÇÃO DE DEPENDÊNCIA
Utilize:

* Task e Task<T>.
* async/await em operações reais.
* CancellationToken.
* Interfaces específicas.
* Injeção de dependência por construtor.
* Registro dos serviços no contêiner nativo do ASP.NET Core.
* TimeProvider injetado.

Não utilize Task.Run artificialmente.
Não acesse o SQLite diretamente na camada Application.
Documente as operações que realmente executam I/O assíncrono.
12. TESTES OBRIGATÓRIOS
Preserve os 402 testes existentes.
Crie testes unitários e de integração para os novos serviços.
Cubra:

* Consulta de parada válida.
* Parada inexistente ou inativa.
* Código externo válido e inválido.
* Termo de busca válido e inválido.
* Paginação válida e inválida.
* Resposta paginada.
* Lista vazia.
* Linhas e destinos ativos.
* Busca sem acentos.
* Última passagem registrada.
* Várias linhas numa parada.
* Linha sem passagem.
* Passagem simulada.
* Passagem manual.
* Registro desatualizado.
* Registro exatamente com 60 minutos.
* Registro com 60 minutos e um segundo.
* Passagem futura excluída.
* Linha que não atende à parada.
* Avisos e níveis de confirmação.
* TimeProvider controlado.
* CancellationToken cancelado.
* Compatibilidade dos DTOs com o contrato.

Utilize doubles de teste quando apropriado para regras isoladas e SQLite real quando for necessário verificar consultas e integração.
Não crie testes artificiais.
Não busque uma quantidade específica de testes. A prioridade é cobrir os comportamentos exigidos.
13. COMPATIBILIDADE COM OS PRÓXIMOS BLOCOS
Preserve a divisão:

* B6: Controllers REST públicos e tratamento HTTP.
* B7: operações administrativas.
* B8: provedores de previsão.
* B9: alternativas e resumo.
* B10: validação final e entrega.

Não antecipe esses blocos.
14. DIRETRIZ PERMANENTE — ORGANIZAÇÃO DO ARQUIVO prompts.md
Esta diretriz também deverá ser observada nas próximas etapas.
O arquivo `prompts.md` é parte da documentação acadêmica da Global Solution.
O material oficial da FIAP, na página 21, orienta registrar os prompts utilizados e compreender o código do projeto.
Desejo que esse arquivo tenha uma apresentação profissional, objetiva, organizada e fácil de consultar pelo professor.
Faça uma revisão estrutural do arquivo atual, respeitando as seguintes regras:
14.1. Organização

* Crie um título e uma introdução objetiva.
* Organize os registros por etapa ou finalidade.
* Utilize um índice de navegação quando for útil.
* Padronize títulos, datas disponíveis e referências.
* Diferencie claramente prompts de implementação, revisão, testes e documentação.
* Evite resumos redundantes e informações repetidas sem necessidade.

14.2. Fidelidade do histórico

* Preserve o conteúdo original dos prompts registrados, podendo reorganizá-los em seções ou em uma área de histórico integral.
* Não invente contribuições humanas que não ocorreram.
* Não descreva a utilização da IA como limitada a algumas etapas se ela também participou das demais.
* Não elimine registros relevantes com o objetivo de ocultar a extensão do uso da ferramenta.
* Identifique corretamente os resumos e as instruções originais.

14.3. Clareza acadêmica
Na introdução, explique de maneira objetiva como a IA foi utilizada no processo de desenvolvimento, revisão, implementação e testes.
O documento deverá distinguir o trabalho efetivamente realizado pela ferramenta das decisões e verificações realizadas pelos integrantes.
Não atribua aos integrantes atividades técnicas que não tenham executado.
14.4. Apresentação
Evite textos promocionais, mensagens automáticas sem valor documental e informações repetitivas que não sejam necessárias para compreender o registro.
O objetivo é um documento acadêmico legível, não uma transcrição desorganizada de terminal.
14.5. Preservação
Não modifique o histórico Git para esconder versões anteriores.
Não altere retroativamente o significado das solicitações feitas.
Se reorganizar o arquivo, apresente no relatório exatamente o que mudou.
14.6. Aplicação permanente
Nas próximas etapas, mantenha o arquivo organizado no mesmo padrão, incluindo os novos prompts e distinguindo as instruções dos resultados obtidos.
Não adicione marcas promocionais de ferramentas ao código-fonte ou aos commits.
Essa organização documental não altera os requisitos técnicos do projeto.
15. DOCUMENTAÇÃO E EVIDÊNCIAS
Atualize backend/README.md com:

* Serviços implementados.
* Interfaces utilizadas.
* Validações.
* DTOs.
* Integração com os repositórios.
* Operações assíncronas.
* Instruções de execução dos testes.
* Funcionalidades pendentes para B6.

Crie evidências reais em:
`backend/docs/evidencias/B5-servicos.md`
Registre somente verificações efetivamente executadas.
A documentação não poderá afirmar que os Controllers REST de negócio já estão implementados.
16. VALIDAÇÃO FINAL
Execute:

* dotnet restore.
* dotnet build.
* dotnet test.
* dotnet ef migrations list.
* dotnet ef migrations has-pending-model-changes.

Confirme também:

* Os testes anteriores continuam passando.
* Os novos testes passam.
* O build não apresenta avisos ou erros.
* Não houve alteração inesperada no banco.
* `/health` continua funcional.
* OpenAPI e Scalar continuam funcionando.
* Nenhum endpoint de negócio foi criado antecipadamente.
* Os serviços estão coerentes com os contratos.

Informe os resultados reais, sem inventar execuções.
17. GIT
Não faça commit.
Não execute git push.
Não altere o histórico publicado.
Não utilize force push.
Não sobrescreva arquivos não relacionados ao B5.
As alterações deverão permanecer locais para nossa auditoria.
18. RELATÓRIO FINAL OBRIGATÓRIO
Apresente:
A. Resumo simples do que foi desenvolvido.
B. Arquivos criados e modificados.
C. Serviços e interfaces implementados.
D. DTOs e compatibilidade com o contrato.
E. Validações e erros de aplicação.
F. Regras de últimas passagens.
G. Uso de async/await e injeção de dependência.
H. Número real de testes aprovados, falhos e ignorados.
I. Resultado do build e das verificações EF Core.
J. Evidências disponíveis.
K. Matriz dos requisitos oficiais da FIAP: implementados e pendentes.
L. Estado do Git.
M. Alterações realizadas em prompts.md, preservando a fidelidade documental.
N. Pendências específicas para B6.
Inclua uma explicação didática, de até cinco minutos, abordando o que é um service, como ele utiliza um repository, por que existem interfaces e como as informações serão entregues ao totem quando os Controllers estiverem prontos.
Execute somente o B5. Não faça commit, não faça push e não inicie B6.
Pare ao concluir e aguarde revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a ferramenta criou a camada de aplicação em `Application/`:
- **Services:** quatro services de consulta (paradas, linhas, destinos e últimas passagens), com
  interfaces, registrados na injeção de dependência.
- **Contrato:** DTOs no formato do contrato, a validação de parâmetros (que devolve todos os erros
  juntos), erros de aplicação com os códigos do catálogo (400, 404 e 422) e o catálogo de avisos.
- **Últimas passagens:** o instante é lido uma vez do `TimeProvider`; o service calcula os minutos
  e a desatualização pelas regras do domínio e não apresenta nenhuma passagem como verificada.
- **Testes:** 61 testes novos, unitários com dublês de repositório e de integração com SQLite real.
  Eles comparam as respostas, campo a campo, com seis exemplos JSON do contrato, e mutações
  temporárias confirmaram que detectam regressões. No total, 463 testes passaram.
- **`prompts.md`:** reorganizado por etapa, com introdução sobre o uso da IA, índice e o texto
  integral de todos os prompts preservado.
- **Controllers:** nenhum controller de negócio foi criado.
- **Git:** não houve commit.

<a id="prompt-20"></a>

### Prompt 20 — Bloco B5.1: auditoria final, correção documental e publicação

**Data:** 07/11/2026 · **Tipo:** Auditoria, correção documental e publicação · **Commit:** `feat: implement application services and DTO validation`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B5.1 — AUDITORIA FINAL, CORREÇÃO DOCUMENTAL E PUBLICAÇÃO
Os arquivos reais do Bloco B5 foram analisados, incluindo services, interfaces, DTOs, validações, testes e documentação.
A implementação técnica está aprovada em princípio, condicionada à validação final e a uma correção na atribuição das atividades descritas em prompts.md.
Sua tarefa é concluir o B5 e publicá-lo no GitHub.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base esperado:
`38d2dae398d595bd26c830732341a709d44ae033`
Não implemente B6 nesta etapa.
1. Exigências obrigatórias da FIAP
Preserve integralmente os requisitos oficiais de C# Software Development:

* POO — 3,0.
* Web API — 2,5.
* EF Core + LINQ — 1,5.
* Async/Await — 1,0.
* Qualidade de código — 2,0.

Também devem estar contemplados interfaces, injeção de dependência, testes unitários, documentação, README, evidências e aderência à CitizenTech.
Não amplie o escopo além do necessário.
2. Conferência técnica
Antes de publicar, confirme:

* Branch main e origin/main sincronizadas.
* Alterações locais exclusivamente relacionadas ao B5.
* Quatro serviços de aplicação funcionando.
* Interfaces e injeção de dependência corretas.
* Application independente de Infrastructure e EF Core.
* DTOs compatíveis com contracts/api-v1.md.
* Validação de parâmetros antes da consulta.
* Erros de aplicação com códigos estáveis.
* Últimas passagens com cálculo correto dos minutos e da desatualização.
* Registros simulados e manuais corretamente diferenciados.
* CancellationToken propagado.
* TimeProvider utilizado corretamente.
* Ausência de novos endpoints de negócio antecipados.

Preserve os contratos e a infraestrutura existente.
3. Correção obrigatória do prompts.md
A reorganização do arquivo está adequada, mas a introdução contém atribuições que precisam ser corrigidas.
O documento afirma que o integrante escreveu todos os prompts e realizou pessoalmente as auditorias técnicas.
O processo efetivo foi:

* O ChatGPT auxiliou no planejamento, na elaboração dos prompts, na definição de critérios de revisão e na auditoria dos materiais enviados.
* O Claude Code realizou a implementação técnica, a execução de testes, a preparação dos documentos e as operações Git autorizadas.
* O integrante encaminhou instruções, forneceu arquivos, definiu preferências, tomou decisões pontuais e autorizou etapas e publicações.
* Não houve edição manual do código pelo integrante nas etapas registradas.

Atualize somente a introdução e os demais trechos de atribuição que precisem refletir esses fatos.
Regras obrigatórias:

1. Preserve os prompts originais.
2. Preserve a cronologia e o índice.
3. Preserve os resumos identificados como resumos.
4. Não invente participação ou atividades técnicas humanas.
5. Não afirme que a IA foi utilizada somente em pontos específicos.
6. Não altere o histórico Git anterior.
7. Não remova informações relevantes para ocultar a participação das ferramentas.
8. Mantenha o documento objetivo, profissional e adequado à orientação acadêmica da FIAP.

Se a identificação do modelo exato da ferramenta não estiver comprovada, use apenas o nome confirmado da ferramenta, sem inventar versões.
Não reorganize novamente o arquivo inteiro sem necessidade.
4. Validação obrigatória
Execute novamente a partir de backend/:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef migrations has-pending-model-changes

O relatório anterior apontou:

* 463 testes aprovados.
* 0 falhas.
* 0 testes ignorados.
* 0 avisos e 0 erros no build.

Verifique os resultados reais.
Teste também:

* GET /health.
* GET /openapi/v1.json.
* /scalar/.
* Um endpoint de negócio ainda não implementado, confirmando que o B6 não foi antecipado.

Verifique que os testes de integração com SQLite real continuam aprovados e que os arquivos da aplicação respeitam o contrato.
5. Segurança e qualidade
Revise os arquivos que serão publicados.
Não inclua:

* Senhas, tokens ou chaves reais.
* Banco SQLite.
* Arquivos temporários.
* Binários de compilação.
* Logs sensíveis.
* Arquivos de diagnóstico.
* Testes artificiais.

Não adicione assinaturas promocionais ou trailers automáticos de ferramentas às mensagens de commit.
Preserve os registros acadêmicos de uso de IA.
6. Atualização das evidências
Revise backend/docs/evidencias/B5-servicos.md.
Confirme que o relatório distingue resultados realmente executados de funcionalidades apenas planejadas.
Não declare que os Controllers públicos já funcionam.
Atualize a evidência somente se alguma verificação real tiver mudado.
7. Commit e push
Se todas as verificações passarem, está autorizado a criar o commit:
`feat: implement application services and DTO validation`
Antes do push:

* Confira o diff.
* Confira os arquivos preparados.
* Confira a mensagem gravada.
* Confira a autoria legítima, usando o e-mail noreply já configurado.
* Não altere commits anteriores.

Publique com push normal em origin/main.
Não utilize force push.
Se alguma validação falhar e não puder ser corrigida com segurança, não publique.
8. Relatório final
Apresente:
A. Resumo simples da conclusão do B5.
B. Resultado da auditoria técnica.
C. Correção feita na introdução de prompts.md.
D. Quantidade real de testes aprovados, falhos e ignorados.
E. Resultado do build e dos comandos EF Core.
F. Confirmação da compatibilidade com o contrato.
G. Arquivos publicados.
H. Hash e mensagem do commit.
I. Resultado do push e estado de main/origin/main.
J. Pendências específicas para B6.
K. Matriz dos requisitos oficiais da FIAP, separando critérios implementados e pendentes.
Não implemente B6, não crie novos endpoints e não adicione funcionalidades fora do escopo.
Conclua esta etapa e aguarde minha revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code conferiu o estado do Git e
a implementação do B5:
- quatro services, com interfaces e injeção de dependência;
- a camada Application sem dependência de Infrastructure nem do EF Core;
- validação antes da consulta, erros com códigos estáveis, `CancellationToken` e `TimeProvider`;
- nenhum endpoint de negócio.

Corrigiu a introdução deste arquivo para registrar os papéis do ChatGPT (planejamento, elaboração
dos prompts, critérios de revisão e apoio às auditorias), do Claude Code (implementação, testes,
documentação e operações Git) e do integrante (instruções, arquivos, preferências, decisões e
autorizações), sem alterar os prompts nem os resumos. Repetiu `dotnet restore`, `dotnet build`,
`dotnet test` e os comandos `dotnet ef`, verificou `/health`, OpenAPI, Scalar e um endpoint de
negócio ainda inexistente, e publicou o B5 com o commit
`feat: implement application services and DTO validation`, sem trailer, e push normal.

---

## Bloco B6 — Web API pública

<a id="prompt-21"></a>

### Prompt 21 — Bloco B6: controllers REST públicos, ProblemDetails, OpenAPI e testes HTTP

**Data:** 07/11/2026 · **Tipo:** Implementação, testes e documentação · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 08 — BLOCO B6: CONTROLLERS REST PÚBLICOS, PROBLEMDETAILS, OPENAPI E TESTES HTTP
Atue como engenheiro de software sênior especializado em C#, ASP.NET Core 10, APIs REST, testes de integração, documentação OpenAPI e boas práticas de arquitetura.
Você é responsável pela execução técnica do backend C# do PontoVoz, incluindo implementação, correção de problemas, testes e documentação.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Último commit esperado:
`314cb27523bf22c26e5b0184f05c04b071efc4eb`
Os blocos B0 a B5 foram concluídos e publicados.
Execute exclusivamente o Bloco B6. Não avance para B7, B8 ou B9. Não faça commit nem push antes da auditoria.
1. PRIORIDADE ABSOLUTA — ENUNCIADO OFICIAL DA FIAP
A disciplina C# Software Development exige:

* Programação Orientada a Objetos — 3,0 pontos.
* Web API — 2,5 pontos.
* Entity Framework Core + LINQ — 1,5 ponto.
* Async/Await — 1,0 ponto.
* Qualidade de código — 2,0 pontos.

Também são obrigatórios, conforme o enunciado, interfaces, injeção de dependência, APIs REST, testes unitários, organização do código, README, documentação dos endpoints e evidências de funcionamento.
O backend deve estar claramente integrado à CitizenTech PontoVoz.
Não invente funcionalidades, não retire requisitos e não amplie a arquitetura sem necessidade.
O arquivo `contracts/api-v1.md` determina os contratos funcionais.
Os arquivos existentes em Domain, Application e Infrastructure determinam a arquitetura técnica já implementada.
2. INSPEÇÃO INICIAL
Antes de editar:

1. Confirme a pasta correta do projeto.
2. Confirme main sincronizada com origin/main.
3. Confirme o commit esperado e a árvore de trabalho limpa.
4. Leia o contrato REST, especialmente as seções 1, 2, 3, 4, 6, 7 e 9.
5. Examine os DTOs implementados no B5.
6. Examine as interfaces e os quatro services do B5.
7. Examine `ErroAplicacaoException`.
8. Examine `JsonContrato`.
9. Examine `Program.cs` e `HealthController`.
10. Examine os testes existentes e a configuração de WebApplicationFactory.

Não duplique funcionalidades já existentes.
Não modifique as regras dos services para adaptá-las arbitrariamente aos controllers.
3. ESCOPO EXATO DO B6
Implemente somente os cinco endpoints públicos cujos services já estão prontos:

1. GET /api/v1/paradas
2. GET /api/v1/paradas/{id}
3. GET /api/v1/linhas
4. GET /api/v1/destinos
5. GET /api/v1/paradas/{id}/ultimas-passagens

Preserve também o endpoint existente:
GET /health
Os endpoints restantes do contrato continuam planejados:

* Previsões: B8.
* Alternativas: B9.
* Resumo agregado: B9, com atenção à referência de implementação parcial no planejamento B6. Não crie um endpoint fictício ou incompleto apenas para antecipar a rota. Documente a pendência.
* Operações administrativas: B7.

Não declare implementadas funcionalidades que ainda não existem.
4. CONTROLLERS REST
Implemente Controllers ASP.NET Core, utilizando a infraestrutura já existente.
Organize os Controllers de maneira objetiva, por exemplo:

* ParadasController.
* LinhasController.
* DestinosController.

O endpoint de últimas passagens pode pertencer ao ParadasController, caso essa seja a organização mais coerente.
Requisitos:

* Usar controllers e atributos apropriados do ASP.NET Core.
* Utilizar rotas e métodos HTTP exatamente como documentados.
* Injetar as interfaces dos services pelo construtor.
* Não acessar DbContext diretamente nos controllers.
* Não colocar consultas LINQ nos controllers.
* Não duplicar regras de validação que já pertencem aos services.
* Receber CancellationToken e propagá-lo aos services.
* Retornar os DTOs efetivamente calculados.
* Usar HTTP 200 nas consultas bem-sucedidas.
* Respeitar os comportamentos de listas vazias e paginação.

Os controllers devem permanecer pequenos e fáceis de explicar ao professor.
5. ROTAS E PARÂMETROS
Siga os contratos reais:
GET /api/v1/paradas
Query parameters:

* termo.
* codigoExterno.
* pagina.
* tamanhoPagina.

GET /api/v1/paradas/{id}
Parâmetro:

* id.

GET /api/v1/linhas
Query parameters:

* termo.
* pagina.
* tamanhoPagina.

GET /api/v1/destinos
Query parameters:

* termo.
* pagina.
* tamanhoPagina.

GET /api/v1/paradas/{id}/ultimas-passagens
Parâmetros:

* id na rota.
* linhaId opcional na query.

Preserve os valores padrão e limites especificados no contrato.
Respeite a distinção entre identificadores internos e códigos externos.
Use restrições de rota para identificadores inteiros positivos, conforme a seção 9 do contrato.
Verifique as respostas esperadas para valores inválidos em rota e em query.
6. SERIALIZAÇÃO JSON
Utilize o `JsonContrato` já implementado no B5.
Configure a serialização do ASP.NET Core MVC para respeitar:

* Campos em camelCase.
* Enums como texto.
* Inclusão explícita dos campos com valor null.
* Listas vazias como [].
* Datas no formato previsto.
* Avisos estruturados.
* Identificação de dados simulados.

Não crie um segundo padrão incompatível de serialização.
Teste o JSON efetivamente devolvido por HTTP, não apenas a serialização isolada dos DTOs.
7. PROBLEMDETAILS E TRATAMENTO DE ERROS
Implemente o tratamento HTTP das exceções de aplicação e de domínio, utilizando os recursos apropriados do ASP.NET Core.
Converta os erros esperados em ProblemDetails conforme o catálogo do contrato.
400 — RequisicaoInvalida

* Entradas com formato ou faixa inválidos.
* Campos de validação agrupados no objeto `errors`.
* Mensagens coerentes com o contrato.

404 — Não encontrado

* Parada inexistente ou inativa.
* Linha inexistente ou inativa, quando aplicável.
* Rota inválida que não corresponde ao padrão estabelecido.

422 — LinhaNaoAtendeParada

* Linha existente que não atende à parada solicitada.

500 — ErroInterno

* Exceções inesperadas.
* Mensagem genérica para o consumidor.
* Nenhuma stack trace, connection string ou dado sensível na resposta.

O ProblemDetails deve contemplar, conforme o contrato:

* type.
* title.
* status.
* detail.
* instance.
* codigo.
* traceId.
* errors apenas quando aplicável à validação.

Utilize o Content-Type adequado a ProblemDetails.
A política de tratamento deve funcionar também para erros de model binding, incluindo valores inválidos nos parâmetros de query.
Não dependa exclusivamente do comportamento padrão do ASP.NET Core se ele gerar respostas incompatíveis com o contrato.
O tratamento deve centralizar a conversão de erros, evitando duplicações nos controllers.
Não converta exceções inesperadas em erros 400.
Cancelamento
Se a requisição for cancelada pelo cliente, propague o CancellationToken e não trate esse cancelamento como erro interno 500.
8. DOCUMENTAÇÃO OPENAPI E SCALAR
O professor exige documentação dos endpoints por Swagger, Scalar ou equivalente.
O projeto já utiliza OpenAPI nativo e Scalar.
Preserve essas ferramentas.
Ao concluir B6, a documentação gerada deverá listar:

* GET /health.
* GET /api/v1/paradas.
* GET /api/v1/paradas/{id}.
* GET /api/v1/linhas.
* GET /api/v1/destinos.
* GET /api/v1/paradas/{id}/ultimas-passagens.

Documente corretamente:

* Métodos.
* Rotas.
* Parâmetros.
* Respostas 200.
* Respostas 400, 404 e 422 quando previstas.
* Tipos de DTO.
* Padrões de erro.

Utilize anotações ou recursos nativos de documentação quando necessário.
Evite instalar pacotes adicionais sem justificativa.
O Scalar deve permitir visualizar e testar os endpoints implementados.
Não apresente os 13 endpoints planejados como se já estivessem funcionando.
9. EXPORTAÇÃO DO OPENAPI
O contrato prevê a exportação do documento OpenAPI gerado pela API para:
`contracts/openapi/pontovoz-v1.json`
Implemente uma maneira simples e reproduzível de exportar o documento real.
A exportação deve:

* Ser gerada a partir da API em execução.
* Conter somente os endpoints efetivamente implementados.
* Produzir JSON válido.
* Não conter segredos ou informações locais indevidas.
* Ser reproduzível por outro integrante.
* Estar documentada no README.

Não fabrique manualmente operações inexistentes para completar o documento.
O arquivo representa o estado implementado até B6 e será atualizado quando os endpoints B7 a B9 estiverem prontos.
10. CORS
O contrato prevê origens permitidas configuráveis em:
`Cors:OrigensPermitidas`
Prepare a configuração de CORS necessária à integração futura com o frontend do totem.
Regras:

* Usar uma lista explícita de origens permitidas.
* Não liberar arbitrariamente todas as origens.
* Não inventar qual será a URL definitiva do frontend.
* Manter o comportamento seguro quando nenhuma origem estiver configurada.
* Documentar como a equipe deverá informar as origens.
* Não adicionar autenticação às rotas públicas.

Teste uma origem permitida e uma não permitida em ambiente isolado.
Não habilite credenciais de forma indiscriminada.
11. TESTES HTTP OBRIGATÓRIOS
Preserve todos os 463 testes existentes.
Crie testes de integração HTTP usando WebApplicationFactory e SQLite isolado.
Testes mínimos:

1. GET /health com banco saudável.
2. GET /api/v1/paradas retorna 200.
3. Busca de parada pelo codigoExterno.
4. Busca de parada sem acentos.
5. Paginação padrão.
6. Paginação inválida retorna 400.
7. Página além do total retorna 200 com lista vazia.
8. GET /api/v1/paradas/{id} existente.
9. GET /api/v1/paradas/{id} inexistente retorna 404.
10. ID inválido na rota não provoca 500.
11. GET /api/v1/linhas retorna 200.
12. Filtro de linhas funcionando.
13. GET /api/v1/destinos retorna 200.
14. Filtro de destinos funcionando.
15. GET /api/v1/paradas/{id}/ultimas-passagens retorna 200.
16. Linha sem registro.
17. Passagem simulada.
18. Passagem manual.
19. Registro desatualizado.
20. Filtro linhaId válido.
21. Filtro linhaId inválido retorna 400.
22. Linha inexistente retorna 404.
23. Linha existente que não atende a parada retorna 422.
24. Enums serializados como texto.
25. Campos null presentes na resposta.
26. Content-Type do JSON de sucesso.
27. Content-Type do ProblemDetails.
28. Campo codigo nos erros.
29. Campo traceId nos erros.
30. Agrupamento de erros de validação.
31. Erro inesperado convertido em 500 sem dados sensíveis.
32. Cancelamento não transformado em 500.
33. OpenAPI lista somente as operações implementadas.
34. Scalar carrega corretamente.
35. CORS de origem permitida e não permitida.
36. Nenhum endpoint administrativo ou de previsão aparece como implementado antecipadamente.

Os testes devem consultar a API de verdade.
Não substitua os testes HTTP por chamadas diretas aos services.
Utilize bancos de teste isolados e não altere o banco local de desenvolvimento.
Não crie testes apenas para aumentar a contagem.
12. VALIDAÇÃO DO FORMATO DAS RESPOSTAS
Compare as respostas HTTP com os exemplos do contrato.
Utilize como referência:

* paradas-listagem.json.
* paradas-busca-codigo.json.
* parada-detalhe.json.
* destinos-busca.json.
* ultimas-passagens.json.
* erro-validacao.json.
* erro-parada-nao-encontrada.json.
* Exemplo de GET /linhas no api-v1.md.

Considere que IDs internos podem variar entre bancos.
Não altere silenciosamente o contrato para fazer o código passar.
Se houver diferenças legítimas de IDs ou horários dinâmicos, normalize apenas esses campos nas comparações de teste, documentando a justificativa.
Verifique especialmente se os nomes e tipos das propriedades permanecem idênticos.
13. VALIDAÇÕES FINAIS
Execute:

* dotnet restore.
* dotnet build.
* dotnet test.
* dotnet ef migrations list.
* dotnet ef migrations has-pending-model-changes.

Confirme:

* Quantidade real de testes.
* Testes aprovados.
* Testes falhos.
* Testes ignorados.
* Código de saída.
* Avisos e erros da compilação.
* Banco de dados preservado.
* Compatibilidade com o contrato.
* Funcionamento da API real.

Teste via HTTP:

* GET /health.
* GET /api/v1/paradas.
* GET /api/v1/linhas.
* GET /api/v1/destinos.
* GET /api/v1/paradas/{id}.
* GET /api/v1/paradas/{id}/ultimas-passagens.
* GET /openapi/v1.json.
* /scalar/.

Utilize IDs obtidos do próprio banco ou da resposta da API, sem supor IDs numéricos fixos.
Teste também ao menos um cenário de erro HTTP.
Se alguma verificação falhar, investigue e corrija antes de declarar a etapa concluída.
14. DOCUMENTAÇÃO E EVIDÊNCIAS
Atualize backend/README.md com:

* Lista dos endpoints B6.
* Comandos para rodar a API.
* URL base local.
* URL do Scalar.
* Exemplos de requisições.
* Como identificar a parada pelo codigoExterno.
* Como configurar CORS.
* Como reproduzir a exportação OpenAPI.
* Como executar os testes.
* O que já funciona e o que ainda está pendente.

Crie:
`backend/docs/evidencias/B6-web-api.md`
Inclua somente evidências reais:

* Respostas HTTP.
* Erros tratados.
* OpenAPI gerado.
* Interface Scalar funcionando.
* Testes automatizados.
* Resultados de build.

Registre este prompt em prompts.md.
Preserve o registro fiel do uso de IA exigido pelo documento geral da Global Solution.
Não altere o histórico para atribuir implementação humana inexistente.
Não insira banners promocionais ou assinaturas automáticas de ferramentas no código.
15. CONTROLE DO GIT
Não faça commit.
Não execute git push.
Não reescreva histórico.
Não utilize force push.
Não modifique arquivos que não sejam necessários ao B6.
Deixe todas as alterações locais aguardando auditoria.
16. RELATÓRIO FINAL OBRIGATÓRIO
Apresente:
A. Resumo simples do que foi implementado.
B. Arquivos criados e alterados.
C. Controllers e rotas implementadas.
D. Mapeamento dos erros HTTP e ProblemDetails.
E. Configuração de JSON e CORS.
F. Resultado dos testes HTTP, incluindo a contagem total.
G. Resultado do build.
H. Evidências do OpenAPI e Scalar.
I. Resultado da comparação com os exemplos do contrato.
J. Comandos utilizados para executar a API.
K. Estado do Git.
L. Critérios da FIAP implementados e ainda pendentes.
M. Problemas identificados e soluções.
N. Pendências para B7.
Não implemente operações administrativas, provedores de previsão ou alternativas.
Não faça commit nem push.
Execute somente o B6 e pare aguardando a próxima auditoria.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a ferramenta criou a camada
HTTP pública do B6:
- **Controllers:** `ParadasController`, `LinhasController` e `DestinosController`, com as rotas
  `GET /api/v1/paradas`, `/paradas/{id}`, `/linhas`, `/destinos` e
  `/paradas/{id}/ultimas-passagens`. Eles recebem as interfaces dos services, repassam o
  `CancellationToken` e não têm LINQ, `DbContext` nem validação repetida; os ids da rota usam
  `int:min(1)`.
- **Erros:** tratamento central em `Middleware/` (`IExceptionHandler` e uma tabela única de
  conversão), com ProblemDetails 400, 404, 422 e 500 em `application/problem+json`, `codigo` e
  `traceId`; erros de model binding também saem como 400 `RequisicaoInvalida`; o 500 não expõe
  detalhes internos; o cancelamento pelo cliente não vira 500.
- **JSON, CORS e OpenAPI:** o `JsonContrato` do B5 nas respostas; CORS só para as origens de
  `Cors:OrigensPermitidas` (lista vazia por padrão, só `GET`, sem credenciais); OpenAPI e Scalar
  com os seis endpoints implementados, e o script `backend/scripts/exportar-openapi.ps1`, que
  gerou `contracts/openapi/pontovoz-v1.json` a partir da API em execução.
- **Testes:** 56 testes HTTP com `WebApplicationFactory` e SQLite isolado, que comparam as
  respostas com os exemplos do contrato (sem os ids internos e o `traceId`). No total, 519 testes
  passaram. A API também foi verificada pelo `curl` e pelo Scalar no banco de desenvolvimento, sem
  alterá-lo.
- **Documentação:** README do backend, `contracts/api-v1.md` e `contracts/README.md` (estado das
  rotas e conferências do B6) e `backend/docs/evidencias/B6-web-api.md`. O resumo ficou
  registrado como pendente para o B9, sem rota antecipada.
- **Git:** não houve commit.

<a id="prompt-22"></a>

### Prompt 22 — Bloco B6.1: auditoria final e publicação da Web API pública

**Data:** 07/11/2026 · **Tipo:** Auditoria, correção e publicação · **Commit:** `feat: implement public REST API and OpenAPI documentation`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B6.1 — AUDITORIA FINAL E PUBLICAÇÃO DA WEB API PÚBLICA
O relatório do B6 foi analisado.
A implementação está aprovada em princípio, mas precisamos realizar três verificações finais e publicar o código somente se todas as validações passarem.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base esperado:
`314cb27523bf22c26e5b0184f05c04b071efc4eb`
Você é responsável por executar as verificações, correções pontuais, testes, documentação e publicação.
Não inicie B7.
1. Prioridade absoluta
Preserve os requisitos oficiais da disciplina C# Software Development:

* POO: 3,0 pontos.
* Web API: 2,5 pontos.
* EF Core + LINQ: 1,5 ponto.
* Async/Await: 1,0 ponto.
* Qualidade do código: 2,0 pontos.

Também preserve interfaces, injeção de dependência, testes unitários, documentação dos endpoints, README, evidências e integração com CitizenTech.
Não implemente funcionalidades fora do escopo aprovado.
2. Conferência inicial
Verifique:

* Diretório correto.
* Branch main.
* Sincronização com origin/main.
* Alterações locais do B6.
* Cinco endpoints públicos implementados.
* HealthController preservado.
* Tratamento centralizado de erros.
* JSON e CORS.
* OpenAPI e Scalar.
* Testes HTTP.
* Evidências.

Não sobrescreva arquivos sem necessidade.
3. Correção do README principal
O relatório informa que o README da raiz ainda afirma que só existe o endpoint /health.
Atualize essa informação para refletir o estado real do B6.
O README deverá distinguir:
Implementado:

* Paradas.
* Detalhe de parada.
* Linhas.
* Destinos.
* Últimas passagens.
* Health check.
* Documentação OpenAPI/Scalar.

Planejado para os próximos blocos:

* Operações administrativas.
* Previsões de chegada.
* Alternativas de deslocamento.
* Resumo agregado do totem.

Preserve a ficha CitizenTech, as instruções relevantes e a estrutura já existente.
Não invente nomes, RMs ou informações da equipe.
4. Auditoria do ProblemDetails e código dos erros
O relatório informa que rotas inexistentes ou incompatíveis com a restrição de rota devolvem 404 sem o campo `codigo`.
Leia o contrato completo, o catálogo de erros e o tratamento centralizado existente.
Confirme se esse comportamento respeita a política global de ProblemDetails aprovada no B1.
Se o contrato exigir `codigo` em todos os erros, corrija o tratamento das rotas não encontradas, utilizando um código estável e documentado, sem expor informações internas.
Se o contrato definir expressamente outra política para rotas sem correspondência, preserve-a e justifique a decisão.
Não altere silenciosamente a semântica do contrato.
Teste os casos:

* /api/v1/paradas/abc
* /api/v1/paradas/0
* /api/v1/rota-inexistente
* Parada inexistente com ID válido
* Erro de validação de query

Confirme status, Content-Type e campos efetivamente devolvidos.
5. Auditoria das alterações contratuais do B6
Examine todas as modificações realizadas em:

* contracts/api-v1.md
* contracts/README.md
* contracts/openapi/pontovoz-v1.json

Compare com a versão publicada no B5.
Confirme que:

* Nenhuma funcionalidade foi removida.
* Nenhum endpoint planejado foi apresentado como implementado.
* Nenhum campo obrigatório foi alterado sem justificativa.
* As atualizações de CORS e ProblemDetails refletem o comportamento real.
* O OpenAPI exportado corresponde exatamente à API em execução.
* As rotas futuras permanecem documentadas como planejadas.

Caso encontre divergência, aplique apenas a correção necessária e registre-a.
6. Testes e validação
Execute novamente:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef migrations has-pending-model-changes

O B6 informou 519 testes aprovados. Confirme o total real.
Execute testes HTTP de:

* GET /health.
* GET /api/v1/paradas.
* GET /api/v1/linhas.
* GET /api/v1/destinos.
* GET /api/v1/paradas/{id}.
* GET /api/v1/paradas/{id}/ultimas-passagens.
* GET /openapi/v1.json.
* Interface Scalar.
* Pelo menos um caso de 400, 404, 422 e 500 em ambiente de testes isolado.

Confirme também:

* Nenhum banco de desenvolvimento foi alterado indevidamente.
* Nenhum endpoint B7, B8 ou B9 foi antecipado.
* As respostas JSON continuam compatíveis com o contrato.
* O verificador de contratos continua passando.
* Nenhum segredo aparece nas respostas de erro.

7. Documentação e registro acadêmico
Atualize a evidência B6 somente com verificações realmente executadas.
Registre esta solicitação em prompts.md, preservando o histórico fiel e a organização já aprovada.
Não atribua implementação manual a integrantes que não a realizaram.
Não adicione assinaturas promocionais de ferramentas ao código ou aos commits.
8. Auditoria de segurança
Inspecione o diff completo.
Não publique:

* Arquivos de banco SQLite.
* Senhas ou tokens.
* Credenciais.
* Arquivos temporários.
* Binários.
* Logs sensíveis.
* Capturas contendo dados privados.

9. Commit e push autorizados
Se todas as verificações passarem, crie o commit:
`feat: implement public REST API and OpenAPI documentation`
Confirme a mensagem gravada antes do push.
Preserve a autoria legítima e o e-mail noreply configurado.
Não acrescente trailer automático de ferramenta.
Não reescreva commits anteriores.
Execute push normal para origin/main.
Não utilize force push.
Se encontrar uma falha relevante que não possa ser corrigida com segurança, interrompa a publicação e explique o bloqueio.
10. Relatório final obrigatório
Apresente objetivamente:

1. Resultado das três conferências finais.
2. Correções realizadas.
3. Endpoints efetivamente publicados.
4. Total real de testes aprovados.
5. Resultado de build, EF Core e verificador.
6. Confirmação do funcionamento HTTP e Scalar.
7. Hash e mensagem do commit.
8. Resultado do push.
9. Estado final de main e origin/main.
10. Requisitos da FIAP já atendidos e pendentes.
11. Pendências para o B7.

Não implemente o B7. Pare após publicar o B6 e aguarde a próxima revisão.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code fez as três
conferências pedidas:
- **README da raiz:** o status, que ainda descrevia o B0, passou a separar o que está
  implementado (paradas, detalhe, linhas, destinos, últimas passagens, health check e
  OpenAPI/Scalar) do que está planejado (operações administrativas, previsões, alternativas e
  resumo). A ficha CitizenTech e os campos de integrantes ficaram como estavam.
- **`codigo` nos erros:** a seção 7 do contrato define todo erro com `codigo`, e o 404 de rota
  sem correspondência e o 405 saíam sem ele. Foram acrescentados ao catálogo
  `RotaNaoEncontrada` (404) e `MetodoNaoPermitido` (405), com teste e registro no histórico do
  contrato; nenhum código existente mudou.
- **Contrato e OpenAPI:** a comparação com o B5 não encontrou endpoint removido, campo alterado
  nem rota planejada apresentada como implementada; os exemplos e o verificador não mudaram. A
  linha de versão do `api-v1.md` foi corrigida para citar os códigos novos. O OpenAPI gerado pela
  API é igual ao arquivo exportado.

Repetiu `dotnet restore`, `dotnet build`, `dotnet test` (521 testes aprovados), os comandos
`dotnet ef` e o verificador do contrato; consultou a API real e o Scalar sem alterar o banco de
desenvolvimento; atualizou a evidência do B6 e publicou o B6 com o commit `feat: implement public REST API and OpenAPI documentation`, sem trailer,
e push normal.

---

## Bloco B7 — operações administrativas

<a id="prompt-23"></a>

### Prompt 23 — Bloco B7: API administrativa, autorização, cadastros e observações de passagem

**Data:** 07/11/2026 · **Tipo:** Implementação e testes (interrompida por limite de uso) · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 09 — BLOCO B7: API ADMINISTRATIVA, AUTORIZAÇÃO, CADASTROS E OBSERVAÇÕES DE PASSAGEM
Atue como engenheiro de software sênior especializado em C#, ASP.NET Core 10, Entity Framework Core, APIs REST, segurança de aplicações e testes automatizados.
Você é responsável pela execução técnica completa do backend C# do PontoVoz.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Último commit esperado:
`4ffef65d3965b1f31779f4be6e5a1598dfb94dea`
Os blocos B0 a B6 foram concluídos e publicados.
Sua tarefa é implementar exclusivamente o Bloco B7 — Operações Administrativas.
Não implemente B8, B9 ou B10. Não faça commit nem push antes da auditoria.
1. PRIORIDADE ABSOLUTA — ENUNCIADO OFICIAL DA FIAP
O projeto deve atender aos requisitos de C# Software Development:

* POO — 3,0 pontos.
* Web API — 2,5 pontos.
* EF Core + LINQ — 1,5 ponto.
* Async/Await — 1,0 ponto.
* Qualidade do código — 2,0 pontos.

Também devem estar presentes interfaces, injeção de dependência, testes unitários, documentação de endpoints, README, evidências e integração com a proposta CitizenTech.
Não adicione funcionalidades desnecessárias e não retire requisitos.
O contrato REST aprovado é a referência funcional.
Implemente somente o que estiver previsto para B7.
2. INSPEÇÃO INICIAL OBRIGATÓRIA
Antes de modificar arquivos:

1. Confirme o diretório de trabalho.
2. Confirme branch main.
3. Confirme HEAD no commit esperado.
4. Confirme sincronização com origin/main.
5. Confirme árvore de trabalho limpa.
6. Leia `contracts/api-v1.md`, especialmente as seções 1.8, 2, 4.6, 5, 7, 8, 9 e 11.
7. Leia os exemplos JSON das operações administrativas.
8. Examine o domínio de Parada, Linha, ParadaLinha e PassagemRegistroManual.
9. Examine os mapeamentos EF Core, o DbContext, os repositórios e a injeção de dependência.
10. Examine os services existentes do B5.
11. Examine os Controllers, o tratamento de ProblemDetails e a configuração JSON do B6.
12. Examine os testes e a exportação OpenAPI existentes.

O contrato efetivamente publicado em main prevalece sobre qualquer pressuposição deste prompt.
Se houver alguma divergência real, documente-a e preserve a solução já aprovada, fazendo somente os ajustes necessários.
3. ESCOPO EXATO DO B7
Implemente estas cinco operações:

1. POST /api/v1/admin/paradas
2. PUT /api/v1/admin/paradas/{id}
3. POST /api/v1/admin/linhas
4. PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}
5. POST /api/v1/admin/observacoes-passagem

Essas operações são utilizadas por um operador, não pelo passageiro, pelo totem ou diretamente pela IA.
Não crie endpoints administrativos adicionais.
Não implemente:

* DELETE.
* Autenticação com usuários ou JWT.
* Cadastro de destinos administrativos.
* Consulta administrativa adicional.
* Atualização de linhas.
* Integração SPTrans.
* Previsões de chegada.
* Alternativas de deslocamento.
* Resumo agregado.
* Interface visual administrativa.

4. AUTORIZAÇÃO ADMINISTRATIVA
O contrato, seção 5.1, determina:
Cabeçalho:
`X-PontoVoz-Admin-Key`
Configuração:
`Admin:Chave`
Implemente `ChaveAdministrativaFilter` ou um componente equivalente que preserve exatamente o comportamento previsto.
Matriz obrigatória
Development, sem chave configurada:

* Operações permitidas.
* Emitir aviso no log a cada inicialização informando que as operações administrativas estão sem proteção por chave.

Development, com chave configurada:

* Exigir cabeçalho correto.
* Sem chave ou com chave incorreta: 401.

Ambientes diferentes de Development, sem chave configurada:

* Bloquear operações administrativas.
* Devolver 403 `OperacoesAdministrativasDesabilitadas`.

Ambientes diferentes de Development, com chave configurada:

* Exigir cabeçalho correto.
* Sem chave ou com chave incorreta: 401 `ChaveAdministrativaInvalida`.

Requisitos de segurança

* A chave nunca poderá ser incluída em arquivos versionados.
* Utilizar user-secrets ou variável de ambiente, conforme a configuração .NET.
* Não registrar o valor da chave em logs.
* Não devolver a chave em erros ou evidências.
* Utilizar `CryptographicOperations.FixedTimeEquals` ou mecanismo de comparação em tempo constante.
* Quando devolver 401, incluir o cabeçalho `WWW-Authenticate` no formato previsto no contrato.
* Os erros devem usar ProblemDetails com `codigo` e `traceId`.
* Não liberar as operações administrativas em ambientes não Development sem chave configurada.

ATENÇÃO À ORDEM DE EXECUÇÃO:
A verificação da autorização deve acontecer antes da validação do corpo da requisição.
Um POST sem autorização e com JSON inválido deve devolver 401 ou 403, e não 400.
Verifique o ciclo de execução do ASP.NET Core: um Action Filter comum pode executar depois do model binding. Utilize uma abordagem que efetivamente garanta a prioridade da autorização.
Teste a ordem real por HTTP.
Não altere o comportamento das rotas públicas.
5. POST /api/v1/admin/paradas
Implemente o cadastro administrativo de parada.
Campos obrigatórios e opcionais

* `codigoExterno`: obrigatório, padrão e limite do contrato.
* `nome`: obrigatório, de 3 a 120 caracteres.
* `endereco`: opcional, até 200 caracteres.
* `coordenada`: opcional.
* `dadosSimulados`: booleano obrigatório, sem valor padrão presumido.

Sucesso:

* HTTP 201.
* Resposta `ParadaAdministrativa`.
* Cabeçalho `Location: /api/v1/paradas/{id}`.

Regras

* A parada nasce ativa.
* A origem deve ser `CadastroAdministrativo`.
* `dadosSimulados` corresponde ao valor declarado pelo operador.
* Um cadastro administrativo não se torna oficialmente verificado.
* O código não pode usar o prefixo reservado `SIM-`.
* Dados fictícios não podem receber endereço ou coordenadas reais, conforme a regra do domínio.
* Código já cadastrado deve devolver 409 `CodigoJaCadastrado`.
* Validações incorretas devem devolver 400 `RequisicaoInvalida`.

Use o Domain existente e não duplique suas regras.
A gravação deve ser real no SQLite.
O cadastro criado precisa aparecer imediatamente nas consultas públicas quando estiver ativo.
6. PUT /api/v1/admin/paradas/{id}
Implemente a atualização completa dos campos editáveis da parada.
Corpo

* `nome`: obrigatório.
* `endereco`: opcional.
* `coordenada`: opcional.
* `ativa`: booleano obrigatório.

Sucesso: HTTP 200, `ParadaAdministrativa`.
Regras

* Atualizar nome, endereço e coordenada quando válidos.
* Permitir ativar e desativar.
* Permitir atualizar uma parada já inativa.
* Não alterar `codigoExterno`.
* Não alterar `origem`.
* Não alterar `dadosSimulados`.
* Não apagar associações ou passagens anteriores.
* Parada realmente inexistente: 404 `ParadaNaoEncontrada`.
* Corpo inválido: 400 `RequisicaoInvalida`.

A atualização deve preservar as regras de domínio para cadastros fictícios.
IMPORTANTE: a busca sem acentos implementada no B4 deve continuar funcionando após a atualização do nome.
Teste a atualização do campo normalizado `TextoBusca`.
Após desativar, a parada deve deixar de aparecer nas consultas públicas, mas continuar disponível para uma atualização administrativa posterior.
Não implemente exclusão física.
7. POST /api/v1/admin/linhas
Implemente o cadastro administrativo de linha.
Corpo

* `codigo`: obrigatório, padrão definido no contrato.
* `letreiro`: obrigatório, de 1 a 30 caracteres.
* `terminalPrincipal`: obrigatório, de 3 a 120 caracteres.
* `terminalSecundario`: obrigatório, de 3 a 120 caracteres.
* `sentido`: enum obrigatório.
* `dadosSimulados`: booleano obrigatório.

Sucesso:

* HTTP 201.
* Resposta Linha com os campos administrativos previstos.
* Sem Location, conforme contrato.

Regras

* A linha nasce ativa.
* `origem` deve ser `CadastroAdministrativo`.
* A natureza fictícia é definida por `dadosSimulados`.
* Código e letreiro não podem usar o prefixo reservado `SIM-`.
* Código duplicado: 409 `CodigoJaCadastrado`.
* Sentido inválido: 400.
* Dados ausentes ou fora dos limites: 400.
* Não converter automaticamente um cadastro administrativo em dado oficial.

A nova linha deve aparecer na consulta pública de linhas.
Preserve a regra de que cada sentido é representado por um cadastro próprio.
Não implemente atualização de linha, pois não está prevista neste bloco.
8. PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}
Implemente a associação entre uma parada e uma linha.
Sucesso: HTTP 204, sem corpo.
Regras

* Confirmar a existência da parada.
* Confirmar a existência da linha.
* A parada deve estar ativa.
* A linha deve estar ativa.
* A associação deve ser idempotente.
* Repetir a mesma chamada deve devolver 204.
* Não criar relações duplicadas.
* Não apagar relações existentes.
* Parada inexistente: 404 `ParadaNaoEncontrada`.
* Linha inexistente: 404 `LinhaNaoEncontrada`.
* Parada inativa: 422 `ParadaInativa`.
* Linha inativa: 422 `LinhaInativa`.

Utilize a relação N `ParadaLinha` e suas restrições persistidas.
Teste a chamada repetida, inclusive em contextos diferentes, para garantir que a idempotência não dependa somente de objetos carregados em memória.
9. POST /api/v1/admin/observacoes-passagem
Implemente o registro manual de uma passagem observada.
Corpo

* `paradaId`: obrigatório.
* `linhaId`: obrigatório.
* `observadaEm`: date-time obrigatório com indicação de fuso.
* `prefixoVeiculo`: opcional, formato definido no contrato.
* `origemRegistro`: obrigatório, identificador técnico de 3 a 40 caracteres, composto por letras minúsculas, números e hífen.

Sucesso:

* HTTP 201.
* Resposta Passagem com `paradaId`, no formato contratado.
* Sem Location.

Regras

* A parada deve existir e estar ativa.
* A linha deve existir e estar ativa.
* A linha deve atender à parada.
* A observação deve estar entre 24 horas antes e 2 minutos depois do horário atual do servidor, inclusive.
* Fora da janela: HTTP 400.
* Utilizar TimeProvider para obter o instante de referência.
* Respeitar a regra temporal já implementada no domínio.
* Preservar o instante UTC internamente.
* Converter datas na resposta para o formato previsto.
* Calcular corretamente `minutosDesdeObservacao`.
* Calcular corretamente `desatualizada`.
* O nível de confirmação deve ser `RegistroManual`.
* `verificada` deve ser false.
* `oficialSptrans` deve ser false.
* A identificação de dados simulados deve respeitar o domínio e a origem dos registros.
* O prefixo do veículo deve respeitar a natureza simulada ou não simulada do registro.
* `origemRegistro` não pode conter nome, e-mail ou telefone pessoal.

A observação persistida deverá aparecer no endpoint público de últimas passagens, quando seu horário observado já tiver ocorrido.
Caso a observação esteja dentro da tolerância futura de 2 minutos, ela será aceita no cadastro, mas não aparecerá como passagem ocorrida até chegar seu horário.
Não renove as passagens antigas do seed.
Não crie uma API de verificação oficial de passagens.
10. CAMADAS DA APLICAÇÃO
Organize a implementação respeitando as camadas existentes.
Domain
Reutilize entidades, objetos de valor e regras do B2.
Evite modificações desnecessárias.
Application
Crie somente:

* Interfaces administrativas necessárias.
* Services administrativos.
* DTOs de entrada e resposta.
* Validações que não estejam cobertas pelo domínio.
* Erros de aplicação apropriados.

Infrastructure
Implemente operações de persistência necessárias à escrita.
Respeite EF Core, as migrations, as chaves únicas e as restrições de relacionamento.
API
Crie os Controllers administrativos e o filtro de autorização.
Mantenha os Controllers pequenos.
Use:

* Interfaces.
* DI.
* async/await.
* CancellationToken.
* ProblemDetails.
* JSON no formato do contrato.

Não crie um repositório genérico artificial.
Não exponha DbContext diretamente nos Controllers.
11. TRATAMENTO DE ERROS
Preserve o tratamento centralizado criado no B6.
Erros administrativos esperados:

* 400 `RequisicaoInvalida`.
* 401 `ChaveAdministrativaInvalida`.
* 403 `OperacoesAdministrativasDesabilitadas`.
* 404 `ParadaNaoEncontrada`.
* 404 `LinhaNaoEncontrada`.
* 409 `CodigoJaCadastrado`.
* 422 `LinhaNaoAtendeParada`.
* 422 `ParadaInativa`.
* 422 `LinhaInativa`.
* 500 `ErroInterno` somente para falhas inesperadas.

Todos os erros devem respeitar a política global de ProblemDetails.
Inclua `codigo` e `traceId`.
Não exponha stack traces ou dados sensíveis.
Para códigos únicos, verifique a existência previamente quando necessário para obter mensagens claras, mas preserve também as restrições do SQLite.
Se houver colisão de código durante a gravação, a API deve retornar 409 de maneira segura, sem converter a violação de unicidade em erro 500.
Não transforme toda DbUpdateException em 409; diferencie violação de unicidade de erros inesperados.
Não exponha mensagens internas do SQLite.
12. VALIDAÇÃO DE CORPOS JSON
Os campos obrigatórios precisam ser realmente obrigatórios.
Cuidado especial com booleanos e enums:
Se `dadosSimulados` ou `ativa` estiverem ausentes, a API deve devolver 400.
Não aceite silenciosamente `false` apenas porque esse é o valor padrão de um bool não anulável em C#.
Se `sentido` estiver ausente ou indefinido, a API deve devolver 400.
Valide:

* JSON ausente.
* JSON malformado.
* Campos obrigatórios omitidos.
* Tipos incorretos.
* Enum inválido.
* Datas sem fuso.
* Datas fora da janela temporal.
* Valores fora das faixas previstas.
* Formatos de código inválidos.

Os erros de validação devem continuar no padrão ProblemDetails do B6, com `errors` apropriado.
Não permita que entradas inválidas provoquem exceção HTTP 500.
13. TESTES AUTOMATIZADOS
Preserve os 521 testes existentes.
Adicione testes unitários e de integração HTTP relevantes para B7.
Autorização
Teste as quatro combinações de ambiente e chave do contrato:

* Development sem chave.
* Development com chave.
* Outro ambiente sem chave.
* Outro ambiente com chave.

Confirme ainda:

* Chave correta.
* Chave incorreta.
* Chave ausente.
* Requisição com corpo inválido e sem autorização.
* 401 com WWW-Authenticate.
* 403 com o código correto.
* Nenhuma informação de chave vazando nos logs ou erros.
* Rotas públicas continuam funcionando sem chave administrativa.

Cadastro de paradas
Teste:

* Cadastro válido 201.
* Location correto.
* Campo dadosSimulados ausente.
* Cadastro fictício declarado.
* Código reservado SIM-.
* Código duplicado 409.
* Coordenada inválida.
* Atualização válida.
* Desativação e reativação.
* Atualização de parada inativa.
* Preservação do código e da natureza.
* Busca sem acentos após renomear.
* Preservação do histórico.

Cadastro de linhas
Teste:

* Cadastro válido 201.
* dadosSimulados ausente.
* Sentido ausente ou inválido.
* Código duplicado.
* Prefixo reservado.
* Linha criada visível na consulta pública.

Associação
Teste:

* Associação válida 204.
* Associação repetida 204.
* Ausência de duplicação no SQLite.
* Parada inexistente.
* Linha inexistente.
* Parada inativa.
* Linha inativa.

Passagens
Teste:

* Cadastro manual válido 201.
* Últimas passagens atualizadas.
* Linha não associada à parada.
* Janela de 24 horas.
* Tolerância futura de 2 minutos.
* Fronteiras exatas da janela.
* Prefixo de veículo.
* OrigemRegistro inválida.
* Dados simulados e não simulados.
* Nível RegistroManual.
* Verificada false.
* OficialSptrans false.
* Registro futuro aceito mas temporariamente excluído da consulta pública.
* Registro visível após o horário ocorrer.
* Desatualização e minutos corretamente calculados.

Utilize FakeTimeProvider ou ferramenta equivalente já presente no projeto.
Os testes HTTP devem utilizar WebApplicationFactory e SQLite isolado.
Não altere o banco de desenvolvimento durante os testes.
Não utilize mocks de banco para afirmar que a persistência real foi verificada.
Não busque um número arbitrário de testes. O objetivo é cobrir integralmente as regras do contrato.
14. TESTE DE FLUXO COMPLETO
Crie pelo menos um teste de integração que execute o fluxo real:

1. Configurar autorização administrativa conforme contrato.
2. Cadastrar uma parada administrativa fictícia.
3. Cadastrar uma linha administrativa fictícia.
4. Associar a linha à parada.
5. Registrar uma observação manual válida.
6. Consultar o endpoint público de últimas passagens.
7. Confirmar que o registro aparece com dados corretos.
8. Confirmar que nunca é apresentado como verificado.
9. Desativar a parada.
10. Confirmar que ela desaparece da consulta pública.
11. Confirmar que os registros permanecem persistidos.

Esse teste deve usar HTTP e SQLite real.
Não utilize IDs numéricos fixos.
15. OPENAPI E SCALAR
Atualize a documentação automática para incluir as cinco operações administrativas.
Ao final do B7, a API deverá apresentar:

* Os cinco endpoints públicos do B6.
* Os cinco endpoints administrativos do B7.
* GET /health.

Total esperado: 11 operações implementadas, sem contar a interface Scalar e a rota que serve o documento OpenAPI.
As três operações públicas restantes do contrato continuarão pendentes para B8 e B9.
Documente:

* Métodos e rotas.
* DTOs de requisição.
* DTOs de resposta.
* Códigos HTTP.
* Campos obrigatórios.
* Cabeçalho de chave administrativa.
* ProblemDetails.
* Comportamento dos dados simulados.

Se for apropriado, documente a chave como mecanismo de segurança no OpenAPI, sem incluir nenhum valor real.
Preserve Scalar.
Execute novamente o script:
`pwsh ./scripts/exportar-openapi.ps1`
Confirme que o OpenAPI gerado corresponde ao documento exportado.
Não documente rotas futuras como implementadas.
16. CONFIGURAÇÃO E SEGREDOS
Documente de forma clara como configurar `Admin:Chave`.
Utilize os recursos nativos de configuração do .NET.
Não coloque chaves reais no appsettings.json.
Não coloque segredos no prompts.md.
Não coloque chaves reais nos testes publicados, evidências ou exemplos de console.
Se utilizar valores fictícios nos testes, identifique-os como dados de teste.
Explique no README que o modo Development sem chave é permitido pelo contrato para demonstração local, mas não equivale a segurança adequada para uma API exposta publicamente.
Não altere a política do contrato por conta própria.
17. DOCUMENTAÇÃO ACADÊMICA
Atualize:

* backend/README.md.
* README principal do repositório, se houver informações de status.
* Documentação da camada Application.
* Documentação de Infrastructure quando necessário.
* contracts/README.md.
* contracts/api-v1.md, apenas para marcar como implementadas as operações realmente funcionais.
* contracts/openapi/pontovoz-v1.json.
* prompts.md.

Crie:
`backend/docs/evidencias/B7-admin.md`
Registre evidências reais de:

* Autorização 401 e 403.
* Cadastro 201.
* Atualização 200.
* Associação 204.
* Observação manual 201.
* Persistência no SQLite.
* Integração com consultas públicas.
* Testes.
* OpenAPI.
* Scalar.

Não registre segredos nas capturas.
Preserve a fidelidade acadêmica do prompts.md e a identificação correta do uso de IA.
Não reescreva o histórico para ocultar as ferramentas utilizadas.
18. VALIDAÇÃO FINAL
Execute:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

Confirme os resultados reais.
Teste a API em execução:

* GET /health.
* GET /api/v1/paradas.
* POST /api/v1/admin/paradas.
* PUT /api/v1/admin/paradas/{id}.
* POST /api/v1/admin/linhas.
* PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}.
* POST /api/v1/admin/observacoes-passagem.
* GET /api/v1/paradas/{id}/ultimas-passagens.
* GET /openapi/v1.json.
* /scalar/.

Use banco isolado nas operações que modificam dados.
Não altere o histórico da base de desenvolvimento somente para produzir evidências.
Confirme:

* Ausência de erros e avisos de compilação.
* Preservação dos testes anteriores.
* Novos testes aprovados.
* 11 operações implementadas.
* Contrato verificado.
* OpenAPI atualizado.
* Segurança das operações administrativas.
* Ausência de segredos versionados.
* Ausência de alterações inesperadas no banco.
* Funcionalidades de B8/B9 não antecipadas.

Se encontrar falhas, corrija-as dentro do escopo antes de considerar o bloco concluído.
19. GIT
Não faça commit.
Não execute git push.
Não utilize force push.
Não reescreva o histórico.
Não altere arquivos não relacionados ao B7 sem necessidade.
Deixe as alterações locais para auditoria antes da publicação.
20. RELATÓRIO FINAL OBRIGATÓRIO
Apresente:
A. Resumo simples do que foi desenvolvido.
B. Arquivos criados e modificados.
C. Cinco rotas administrativas e seus comportamentos.
D. Estratégia de autorização e matriz de ambientes.
E. DTOs e regras de validação.
F. Persistência, idempotência e tratamento de duplicidades.
G. Funcionamento do cadastro de observações.
H. Integração real entre rotas administrativas e públicas.
I. Resultados dos testes, com totais aprovados, falhos e ignorados.
J. Resultado do build, migrations e validação do contrato.
K. OpenAPI, Scalar e evidências.
L. Confirmação de ausência de segredos.
M. Critérios oficiais da FIAP já atendidos e pendentes.
N. Estado do Git.
O. Problemas encontrados e correções aplicadas.
P. Pendências para o B8.
Inclua uma explicação didática, breve, para que o integrante consiga apresentar:

* O que são as rotas administrativas.
* Por que elas exigem autorização.
* Como o cadastro chega ao SQLite.
* Como as novas informações aparecem nas consultas públicas.

Execute somente B7. Não faça commit, não faça push e não inicie B8.
Pare ao concluir e aguarde a próxima auditoria.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a ferramenta implementou as cinco
rotas administrativas, o filtro `ChaveAdministrativaFilter` (autorização antes da leitura do corpo,
matriz de ambiente e chave, comparação em tempo constante, 401 com `WWW-Authenticate` e 403), os
services, DTOs e validações administrativos, a gravação com EF Core (repositórios de escrita e
unidade de trabalho que converte só a violação de unicidade do SQLite) e a documentação OpenAPI
das rotas. Escreveu os testes HTTP de autorização, cadastros, associação, observações e o fluxo
completo. A sessão foi **interrompida pelo limite de uso** da ferramenta antes dos testes dos
services e de concorrência, da exportação do OpenAPI, da documentação, das evidências e deste
registro. No momento da interrupção, 626 de 627 testes passavam; a única falha era o arquivo
OpenAPI exportado, ainda desatualizado. Não houve commit.

<a id="prompt-24"></a>

### Prompt 24 — Bloco B7 (09.1): retomada e conclusão após a interrupção por limite de uso

**Data:** 08/11/2026 · **Tipo:** Continuação: testes, documentação e evidências · **Commit:** —

Continuação do prompt 23, enviada depois da interrupção. O trabalho local não commitado foi
preservado e conferido antes de continuar.

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 09.1 — RETOMADA E CONCLUSÃO DO BLOCO B7 INTERROMPIDO POR LIMITE DE USO
Estamos retomando o desenvolvimento do B7 após a interrupção por limite de créditos do Claude Code.
ATENÇÃO: NÃO RECOMECE O B7 DO ZERO.
O trabalho anterior permanece na pasta local e deve ser preservado.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Último commit publicado:
`4ffef65d3965b1f31779f4be6e5a1598dfb94dea`
O B7 foi parcialmente implementado, mas ainda não foi commitado nem publicado.
Sua responsabilidade é continuar exatamente do ponto em que parou, concluir as pendências, corrigir eventuais problemas e apresentar um relatório completo para auditoria.
Não inicie B8.
Não faça commit nem push.
1. PRESERVAÇÃO DO TRABALHO EXISTENTE
Antes de modificar qualquer arquivo:

1. Confirme o diretório correto.
2. Examine `git status`.
3. Confirme a branch main.
4. Confirme o último commit publicado.
5. Liste os arquivos modificados e ainda não versionados.
6. Identifique os componentes B7 já implementados.
7. Preserve integralmente o trabalho não commitado.

É proibido executar `git reset --hard`, `git clean -fd`, descartar alterações ou recriar o projeto.
Não sobrescreva arquivos já implementados sem necessidade comprovada.
Não repita etapas que já foram concluídas corretamente.
2. ESTADO ANTERIOR À INTERRUPÇÃO
O relatório anterior informou:
Implementado:

* As cinco rotas administrativas previstas na seção 5 do contrato.
* Chave administrativa e matriz de autorização.
* Validação de campos obrigatórios.
* Tratamento de códigos duplicados.
* Validação de datas e fusos.
* Testes HTTP administrativos.
* Fluxo integrado de cadastro e consulta pública.
* OpenAPI gerado pela aplicação com 11 operações.

Testes no momento da interrupção:

* 627 testes.
* 626 aprovados.
* Uma falha: o arquivo OpenAPI exportado ainda não havia sido atualizado.
* Build com zero erros e zero avisos.

Pendências declaradas:

1. Testes unitários dos services administrativos.
2. Testes de concorrência e violação de unicidade do SQLite.
3. Exportação atualizada do OpenAPI.
4. Atualização dos contratos e READMEs.
5. Evidências do B7.
6. Registro do prompt em prompts.md.
7. Validação final de todos os comandos.
8. Relatório técnico completo.

Confira o estado real antes de assumir que esses números permanecem iguais.
3. FINALIZE OS TESTES PENDENTES
Conclua os testes unitários dos services administrativos.
Cubra, conforme necessário:

* Validação antes de operações de escrita.
* Cadastro de parada.
* Atualização de parada.
* Cadastro de linha.
* Associação idempotente.
* Registro de observação.
* Tratamento de erros esperados.
* CancellationToken.
* TimeProvider.
* Respeito às regras de domínio.

Conclua também os testes de concorrência e restrições SQLite:

* Dois cadastros com o mesmo código.
* Violação real do índice único.
* Conversão correta de violação de unicidade em HTTP 409.
* Exceções de banco não relacionadas à unicidade não convertidas indevidamente em 409.
* Associação repetida sem duplicação.
* Preservação da integridade referencial.

Use SQLite real nos testes de persistência.
Não crie testes artificiais apenas para aumentar a quantidade.
4. VALIDAÇÃO FINAL DA SEGURANÇA
Revise os quatro cenários do contrato:

* Development sem chave.
* Development com chave.
* Ambiente não Development sem chave.
* Ambiente não Development com chave.

Confirme:

* Autorização antes de validação do corpo.
* 401 e 403 conforme o contrato.
* WWW-Authenticate nos 401.
* Chave comparada em tempo constante.
* Ausência de vazamento de segredos.
* Nenhuma chave real versionada.
* Rotas públicas continuam acessíveis.
* Erros em ProblemDetails com codigo e traceId.

Confirme também que a configuração de user-secrets funciona.
O `UserSecretsId` no projeto não é um segredo e pode ser versionado, desde que o valor real de Admin não seja publicado.
5. QUESTÃO DO origemRegistro
O relatório anterior apontou que a expressão regular do contrato aceita uma sequência numérica que poderia coincidir com um telefone.
A orientação acadêmica é não utilizar informações pessoais nesse campo.
Não altere silenciosamente a regra contratual.
Preserve o formato técnico já definido para origemRegistro.
Documente claramente que o campo deve conter apenas um identificador técnico impessoal, nunca nomes, telefones ou e-mails reais.
Não afirme que a expressão regular impede absolutamente a entrada de dados pessoais, porque isso não seria verdade.
Se considerar indispensável ampliar a validação, registre a proposta e seu impacto no contrato como pendência para auditoria, sem mudar unilateralmente o requisito publicado.
6. EXPORTAÇÃO OPENAPI
O arquivo OpenAPI exportado está desatualizado.
Execute o script existente de exportação, respeitando as instruções reais do projeto:
`pwsh ./scripts/exportar-openapi.ps1`
Confirme que:

* O documento contém 11 operações implementadas.
* As cinco rotas administrativas estão presentes.
* As cinco rotas públicas continuam presentes.
* `/health` permanece presente.
* O esquema de segurança administrativa está documentado.
* Os campos obrigatórios estão corretamente identificados.
* Campos opcionais não aparecem indevidamente como obrigatórios.
* Não há valores de chaves reais.
* As operações B8 e B9 não aparecem como implementadas.

Execute novamente o teste que falhava por divergência do OpenAPI.
Confirme que o arquivo exportado é idêntico ao documento gerado pela API real.
7. COMPLETE A DOCUMENTAÇÃO
Atualize os arquivos relevantes:

* contracts/api-v1.md.
* contracts/README.md.
* backend/README.md.
* README.md da raiz.
* Application/README.md.
* Infrastructure/README.md.
* Security/README.md, conforme o caminho efetivamente utilizado.
* Middleware/README.md, se necessário.
* contracts/openapi/pontovoz-v1.json.

A documentação deve explicar:

* As cinco operações administrativas.
* Os códigos HTTP.
* O cabeçalho X-PontoVoz-Admin-Key.
* A matriz de segurança.
* A configuração Admin usando user-secrets ou variável de ambiente.
* O comportamento das validações.
* O uso de SQLite.
* Os dados simulados.
* As restrições do registro manual.
* Como executar os testes.
* Como testar as operações no Scalar.

Não registre valores secretos.
Não modifique os requisitos já aprovados sem necessidade.
8. EVIDÊNCIAS REAIS
Crie:
`backend/docs/evidencias/B7-admin.md`
Inclua somente evidências realmente verificadas:

* 201 para cadastro de parada.
* 200 para atualização.
* 201 para cadastro de linha.
* 204 para associação.
* 201 para observação manual.
* 401 com chave inválida ou ausente.
* 403 quando operações administrativas estiverem desabilitadas.
* 409 para código duplicado.
* 422 para regras de negócio.
* Persistência real no SQLite.
* Fluxo administrativo até consulta pública.
* Resultado dos testes.
* OpenAPI e Scalar.
* Resultado do build.

Não use dados pessoais reais ou segredos.
9. REGISTRO ACADÊMICO
Atualize `prompts.md` com o prompt original do B7, caso ainda não esteja registrado, e identifique esta solicitação como continuação após uma interrupção por limite de uso.
Preserve o histórico verdadeiro dos prompts e as atribuições já corrigidas.
Não invente atividades humanas nem omita o uso efetivo das ferramentas.
Não altere o histórico Git antigo.
Mantenha a organização profissional do documento.
10. VALIDAÇÃO FINAL COMPLETA
Execute:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

Confirme os resultados reais.
Execute também:

* Verificador dos contratos.
* Testes HTTP das operações administrativas.
* Testes de segurança.
* Testes dos services.
* Testes de integração SQLite.
* Teste de igualdade entre OpenAPI exportado e OpenAPI gerado.
* Teste da API real.
* Verificação de Scalar.

Reproduza o fluxo administrativo completo em um banco de teste isolado:

1. Cadastrar parada.
2. Cadastrar linha.
3. Associá-las.
4. Registrar observação manual.
5. Consultar as últimas passagens na API pública.
6. Desativar a parada.
7. Confirmar ausência nas consultas públicas.
8. Confirmar preservação dos registros no SQLite.

Não use IDs internos fixos.
Não altere o banco de desenvolvimento para produzir evidências.
Se algum teste falhar, identifique a causa e corrija apenas o necessário.
11. RUBRICA OFICIAL DA FIAP
Verifique a implementação considerando os critérios obrigatórios:

* POO — 3,0 pontos.
* Web API — 2,5 pontos.
* EF Core + LINQ — 1,5 ponto.
* Async/Await — 1,0 ponto.
* Qualidade do código — 2,0 pontos.

Confirme também interfaces, injeção de dependência, testes unitários, documentação de endpoints, README, evidências e alinhamento com CitizenTech.
Não confunda testes aprovados com garantia de nota.
12. GIT
Não faça commit.
Não faça push.
Não execute force push.
Não descarte modificações.
Não reescreva histórico.
Deixe o B7 concluído localmente para auditoria.
13. RELATÓRIO FINAL
Apresente:
A. Resumo simples do B7 completo.
B. O que já estava implementado antes da interrupção.
C. O que foi concluído nesta retomada.
D. Arquivos criados e modificados.
E. Cinco rotas administrativas implementadas.
F. Segurança e matriz de ambientes.
G. Validações e erros HTTP.
H. Resultados da persistência e testes de concorrência.
I. Fluxo completo administrativo → SQLite → API pública.
J. Total real de testes aprovados, falhos e ignorados.
K. Resultado de build, EF Core e verificador do contrato.
L. Estado do OpenAPI e Scalar.
M. Evidências criadas.
N. Confirmação de ausência de segredos.
O. Critérios FIAP implementados e pendentes.
P. Estado do Git.
Q. Problemas encontrados e soluções.
R. Pendências para B8.
Conclua integralmente o B7, mas não publique ainda. Não inicie B8. Aguarde a auditoria.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code conferiu o estado
local (branch, commit, arquivos do B7 e testes) sem descartar nada e concluiu o B7:
- **Testes:** 23 testes dos services administrativos com dublês e 12 testes de concorrência e
  restrições no SQLite real (código repetido, associação simultânea, chave estrangeira que não
  vira 409). No total, 663 testes passaram.
- **OpenAPI:** exportado com as 11 operações; números passaram a ser só números
  (`NumberHandling.Strict`), e o documento marca os campos obrigatórios corretamente.
- **Documentação:** contrato, READMEs (raiz, backend, contracts, Application, Infrastructure,
  Security e Middleware) e `backend/docs/evidencias/B7-admin.md`, com respostas reais em banco
  isolado, conferência do user-secrets (valor temporário, removido em seguida) e captura do Scalar.
- **`origemRegistro`:** a frase do contrato que dizia que telefones não passam no padrão foi
  corrigida; a regra não mudou, e a eventual ampliação ficou como pendência para a auditoria.
- **Git:** não houve commit.

<a id="prompt-25"></a>

### Prompt 25 — Bloco B7.2: pacote ZIP para auditoria independente do código

**Data:** 08/11/2026 · **Tipo:** Preparação para auditoria · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
B7.2 — PREPARAÇÃO PARA AUDITORIA INDEPENDENTE DO CÓDIGO
O relatório final do B7 foi recebido.
O B7 está aprovado em princípio, com 663 testes aprovados, mas antes de autorizar o commit e o push faremos uma auditoria independente do código-fonte.
Sua tarefa agora é preparar um ZIP completo e atualizado do projeto para essa revisão.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base publicado:
`4ffef65d3965b1f31779f4be6e5a1598dfb94dea`
1. PRESERVE O ESTADO ATUAL
Não recomece o B7.
Não implemente novas funcionalidades.
Não faça commit ou push.
Não descarte arquivos modificados ou ainda não versionados.
Confirme que todas as alterações locais do B7 continuam presentes.
2. GERE O ZIP PARA AUDITORIA
Crie um arquivo chamado:
`PontoVoz-B7-Auditoria.zip`
Salve-o fora da pasta controlada pelo Git, preferencialmente no diretório pai do projeto.
O arquivo deve conter a versão atual e completa de:

* README.md da raiz.
* prompts.md.
* contracts/, inclusive exemplos e OpenAPI.
* backend/src/, incluindo Domain, Application, Infrastructure, Controllers, Middleware e Security.
* backend/tests/.
* backend/docs/evidencias/.
* Arquivos de configuração, solução e projetos .NET.
* Scripts necessários à execução.

Inclua também todos os arquivos novos do B7 que ainda não foram commitados.
Não utilize somente `git archive HEAD`, pois isso excluiria as alterações locais ainda não publicadas.
3. NÃO INCLUA INFORMAÇÕES SENSÍVEIS OU ARQUIVOS GERADOS
Exclua do ZIP:

* .git/
* bin/
* obj/
* *.db
* *.db-shm
* *.db-wal
* .vs/
* .idea/
* Arquivos temporários.
* Arquivos de user-secrets.
* Tokens, chaves e senhas reais.
* Logs com informações privadas.

Preserve os exemplos fictícios e as evidências acadêmicas apropriadas.
4. VERIFIQUE O ZIP
Antes de concluir:

1. Confirme que o ZIP abre corretamente.
2. Liste a quantidade de arquivos incluídos.
3. Confirme que as cinco rotas administrativas e seus testes estão presentes.
4. Confirme que os arquivos novos do B7 não ficaram de fora.
5. Confirme ausência de bancos e segredos.
6. Calcule o SHA-256 do ZIP.
7. Informe o caminho completo do arquivo.
8. Informe o estado do Git e o número de arquivos modificados.

Não altere o código somente para gerar o ZIP.
5. RELATÓRIO FINAL
Apresente:
A. Caminho completo do ZIP.
B. Tamanho do arquivo.
C. Quantidade de arquivos.
D. SHA-256.
E. Confirmação da presença de todos os arquivos do B7.
F. Confirmação de ausência de segredos e bancos SQLite.
G. Estado do Git.
H. Confirmação de que não houve commit nem push.
Depois de gerar o ZIP, pare e aguarde a auditoria. Não inicie B8.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code gerou
`PontoVoz-B7-Auditoria.zip` na pasta acima do repositório, a partir da árvore de trabalho
(arquivos versionados e os novos do B7 ainda não commitados), sem `.git`, binários, bancos
SQLite, logs nem segredos. Conferiu a integridade, os 183 arquivos, a presença e a igualdade byte
a byte dos 51 arquivos alterados ou novos do B7, a ausência de bancos e chaves, e informou o
SHA-256. Nenhum código foi alterado e não houve commit.

<a id="prompt-26"></a>

### Prompt 26 — Bloco B7.3: validação final e publicação do B7

**Data:** 08/11/2026 · **Tipo:** Validação e publicação · **Commit:** `feat: implement secured administrative REST API`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B7.3 — VALIDAÇÃO FINAL E PUBLICAÇÃO AUTORIZADA
O ZIP `PontoVoz-B7-Auditoria.zip` foi recebido e o código-fonte foi auditado.
A auditoria verificou as cinco rotas administrativas, o filtro de autorização, as regras de negócio, a persistência com EF Core, o tratamento de erros e os testes.
O B7 está aprovado para publicação, condicionado à última validação automática.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base esperado:
`4ffef65d3965b1f31779f4be6e5a1598dfb94dea`
1. Preserve a implementação aprovada
Não recomece o B7.
Não adicione funcionalidades.
Não altere o contrato sem necessidade.
Não implemente B8.
Não descarte arquivos locais.
Confirme que a pasta de trabalho continua contendo exatamente as alterações aprovadas do B7.
2. Verificação final obrigatória
Execute a partir de backend/:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

Execute também o verificador dos contratos.
O último relatório informou 663 testes aprovados, sem falhas ou avisos.
Informe os resultados reais encontrados agora.
Confirme que:

* Todas as 11 operações da API permanecem funcionais.
* Os testes de autorização continuam passando.
* Os testes de persistência e concorrência continuam passando.
* O OpenAPI exportado continua igual ao documento gerado pela aplicação.
* O Scalar continua funcionando.
* Nenhum segredo está presente nos arquivos versionáveis.
* O banco de desenvolvimento não sofreu alterações de dados.
* Nenhum endpoint de B8 ou B9 foi antecipado.

3. Conferência dos requisitos acadêmicos
Confira novamente a cobertura de:

* POO.
* Web API.
* EF Core e LINQ.
* Async/Await.
* Qualidade do código.
* Interfaces.
* Injeção de dependência.
* Testes unitários.
* README.
* Documentação OpenAPI/Scalar.
* Evidências de funcionamento.

Não confunda critérios implementados com garantia de nota.
4. Controle de segurança
Verifique que:

* A chave administrativa não está no repositório.
* Os quatro cenários de autorização seguem o contrato.
* Requisições sem autorização não executam operações de escrita.
* 401 e 403 são tratados corretamente.
* A restrição de unicidade produz 409 quando aplicável.
* A associação repetida permanece idempotente.
* O registro manual continua não verificado.
* O histórico é preservado após desativar uma parada.

A limitação conhecida de `origemRegistro` está documentada. Não altere sua expressão regular nesta publicação.
5. Documentação
Confirme que os READMEs, contratos, evidências e prompts.md estão atualizados até o B7.
O arquivo prompts.md deve continuar fiel ao histórico real de utilização do ChatGPT e do Claude Code.
Não altere registros anteriores para ocultar o uso das ferramentas.
Registre esta instrução final de publicação de acordo com o padrão existente.
6. Commit autorizado
Se todas as verificações passarem, execute o commit com esta mensagem:
`feat: implement secured administrative REST API`
Antes de confirmar:

* Inspecione o diff.
* Confirme a lista de arquivos staged.
* Exclua bancos, binários, temporários e segredos.
* Preserve a autoria legítima configurada no Git.
* Não acrescente trailer automático Co-Authored-By.
* Não altere commits anteriores.

Confirme que a mensagem gravada é exatamente a autorizada.
7. Push autorizado
Depois de um commit válido e testes aprovados, execute push normal para origin/main.
Não utilize force push.
Confirme que main e origin/main apontam para o mesmo commit.
Confirme árvore de trabalho limpa.
Se encontrar falha relevante que não consiga corrigir com segurança, interrompa a publicação e relate o impedimento.
8. Relatório final
Informe:
A. Resumo simples da publicação.
B. Total de testes aprovados, falhos e ignorados.
C. Resultado do build e das migrations.
D. Resultado do verificador de contratos.
E. Confirmação de 11 operações no OpenAPI.
F. Confirmação da segurança administrativa.
G. Hash completo do commit.
H. Mensagem gravada.
I. Resultado do push.
J. Estado final do Git.
K. Pendências específicas para B8.
Não implemente B8. Publique somente o B7 validado e pare.
````

Durante a execução, o build falhou porque o executável da API estava bloqueado por uma instância
iniciada em um terminal do VS Code. A ferramenta interrompeu a publicação e pediu autorização para
encerrar esse processo. O integrante respondeu com a instrução complementar abaixo.

**Instrução complementar (texto integral, como foi enviada):**

````text
Autorizo você a encerrar o processo PID 39276, desde que confirme novamente que ele pertence exclusivamente à API PontoVoz e está bloqueando a compilação.
Tente primeiro um encerramento normal. Se necessário, finalize especificamente esse processo, sem interromper outros programas ou serviços.
Depois, continue automaticamente a execução do B7.3, exatamente de onde parou:

1. Execute novamente restore, build e todos os testes usando os binários recompilados.
2. Confirme os 663 testes ou a quantidade atual, sem falhas.
3. Valide migrations, contrato, 11 operações da API, segurança administrativa, OpenAPI e Scalar.
4. Confirme a ausência de segredos e alterações indevidas no banco.
5. Registre o B7.3 no prompts.md.
6. Se todas as verificações passarem, realize o commit e o push já autorizados para origin/main.
7. Apresente o relatório final com o hash do commit e o estado do Git.

Você tem autorização para resolver bloqueios operacionais semelhantes durante as próximas etapas, desde que confirme que os processos pertencem exclusivamente ao PontoVoz e não comprometa dados ou trabalhos de outras aplicações.
Não solicite autorização novamente para operações rotineiras necessárias à execução do projeto. Não descarte alterações, não reescreva o histórico e não execute force push.
Se encontrar um problema relevante que não consiga corrigir com segurança, interrompa a publicação e relate o impedimento.
Não inicie B8 até concluirmos o B7.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code confirmou que a
árvore de trabalho era idêntica ao ZIP auditado. Na primeira tentativa, o `dotnet build` falhou
porque o processo PID 39276 (a API do PontoVoz, iniciada pelo terminal do VS Code) bloqueava o
executável; a ferramenta não o encerrou sem autorização. Com a autorização, confirmou de novo que o
processo era só a API do PontoVoz, tentou o encerramento normal e, como a aplicação de console não
o aceita, finalizou apenas aquele PID. Depois repetiu restore, build completo (0 avisos e 0 erros),
os 663 testes com os binários recompilados, os comandos `dotnet ef` e o verificador do contrato;
conferiu na API real, sobre um banco isolado, as 11 operações, a matriz da chave, o 409, a
associação idempotente, o registro manual não verificado, o histórico após desativar, a ausência
das rotas do B8 e B9, o OpenAPI igual ao exportado e o Scalar, sem alterar o banco de
desenvolvimento. Publicou o B7 com o commit `feat: implement secured administrative REST API`, sem trailer, e push normal.

---

## Bloco B8 — previsões de chegada

<a id="prompt-27"></a>

### Prompt 27 — Bloco B8: previsões de chegada, provedor simulado, adaptador Olho Vivo e testes

**Data:** 08/11/2026 · **Tipo:** Implementação, testes e documentação · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 10 — BLOCO B8: PREVISÕES DE CHEGADA, PROVEDOR SIMULADO, ADAPTADOR OLHO VIVO E TESTES
Atue como engenheiro de software sênior especializado em C#/.NET 10, ASP.NET Core, integração de APIs, arquitetura de aplicações e testes automatizados.
Você é responsável pela execução técnica completa do backend C# do PontoVoz.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Último commit publicado esperado:
`f9661ba5ec8b374f963c987211315dae8899d2a8`
Os blocos B0 a B7 estão concluídos e publicados.
Sua tarefa é implementar exclusivamente o B8 — Previsões de Chegada, conforme o contrato REST já aprovado.
Não implemente B9 ou B10. Não faça commit ou push antes da auditoria.
1. PRIORIDADE ABSOLUTA — EXIGÊNCIAS OFICIAIS DA FIAP
A disciplina C# Software Development exige:

* POO — 3,0 pontos.
* Web API — 2,5 pontos.
* EF Core + LINQ — 1,5 ponto.
* Async/Await — 1,0 ponto.
* Qualidade de código — 2,0 pontos.

Também exige interfaces, injeção de dependência, testes unitários, README, documentação dos endpoints, evidências e integração com a proposta CitizenTech.
Todos esses requisitos devem continuar atendidos.
Não adicione recursos não previstos pelo contrato ou pelo planejamento.
O objetivo do B8 é implementar corretamente uma fonte intercambiável de previsões, um provedor fictício funcional e o endpoint público de previsões de chegada.
A integração real com a SPTrans não deve ser apresentada como funcionando sem credenciais, autorização e teste efetivo.
2. INSPEÇÃO INICIAL OBRIGATÓRIA
Antes de qualquer alteração:

1. Confirme o diretório correto.
2. Verifique `git status`.
3. Confirme branch main.
4. Confirme HEAD no commit esperado.
5. Confirme sincronização com origin/main.
6. Confirme árvore de trabalho limpa.
7. Leia `contracts/api-v1.md`, especialmente as seções 1.7, 1.8, 1.10, 2, 4.5, 7, 8, 9 e 11.
8. Leia `contracts/exemplos/previsoes.json`.
9. Leia `contracts/exemplos/erro-provedor-previsoes-indisponivel.json`.
10. Leia `docs/dados.md`.
11. Examine as classes `PrevisaoChegada`, `ResultadoProvedor`, `ReferenciaLinha`, os enums e os testes existentes.
12. Examine `CatalogoAvisos`, os DTOs, os services, os repositórios e os Controllers.
13. Examine o tratamento global de ProblemDetails.
14. Examine as configurações, o OpenAPI e os testes HTTP.
15. Confirme os testes existentes antes de desenvolver.

Não presuma propriedades, nomes, formatos ou comportamentos sem conferir o código real.
3. ESCOPO EXATO DO B8
Implemente o endpoint:
`GET /api/v1/paradas/{id}/previsoes`
Com filtro opcional:
`?linhaId=`
Inclua os componentes necessários:

* Interface do provedor de previsões.
* Provedor simulado padrão.
* Service de previsões.
* DTOs de resposta.
* Controller REST.
* Opção de simular indisponibilidade.
* Tratamento HTTP 503.
* Adaptador HTTP Olho Vivo preparado e desligado por padrão.
* Testes unitários, de integração e HTTP.
* OpenAPI e Scalar atualizados.
* Documentação e evidências.

Não implemente ainda:

* Alternativas de deslocamento.
* Resumo agregado.
* Interface ou voz do totem.
* Rastreamento de veículos.
* Mapas.
* Cadastro de linhas adicionais.
* Autenticação de passageiros.
* Novos endpoints não previstos.
* Persistência das previsões simuladas.

4. INTERFACE DO PROVEDOR
Implemente a interface `IProvedorPrevisaoChegada`, ou utilize a equivalente existente, respeitando as responsabilidades do contrato.
O objetivo é permitir a troca da origem das previsões sem modificar o serviço de aplicação.
Requisitos:

* Receber a parada e as linhas relevantes, usando os tipos e dados efetivamente disponíveis.
* Retornar um `ResultadoProvedor` coerente.
* Utilizar `CancellationToken`.
* Preservar o `OrigemDado`.
* Distinguir resposta sem previsões de indisponibilidade.
* Permitir que o B9 reutilize o serviço/provedor.
* Evitar acoplamento do domínio a HTTP e à SPTrans.
* Não expor `IQueryable` ou `DbContext` nas interfaces.

Aproveite as classes de domínio que já existem.
Não crie um framework genérico desnecessário.
5. PROVEDOR SIMULADO — R1 E R2
Implemente um provedor que calcule previsões fictícias a partir do horário atual, usando `TimeProvider`.
Regra obrigatória: as previsões simuladas devem ser calculadas no momento da consulta. Não devem ser gravadas no SQLite.
Cada linha simulada deverá ter uma configuração de demonstração com:

* Intervalo recorrente.
* Defasagem fixa.
* Quantidade de próximas chegadas, quando aplicável.
* Prefixos fictícios de veículos.
* Identificação pelo código estável da linha, nunca por IDs internos fixos.

A configuração precisa ser determinística.
Utilize a mesma regra de cálculo em datas diferentes, inclusive depois de reiniciar a aplicação.
Não coloque uma data fixa de novembro de 2026 no algoritmo.
Os horários específicos do exemplo JSON existem somente como referência para a comparação do contrato.
Compatibilidade com o exemplo
Com o relógio controlado em 10/11/2026 às 14h32, horário de São Paulo, o exemplo de `previsoes.json` mostra:

* SIM-L002: chegadas às 14h33 e 14h51.
* SIM-L004: chegada às 14h40.
* SIM-L001: sem previsão.

Escolha uma configuração recorrente de intervalos e defasagens que reproduza esse cenário sem codificar a data do exemplo.
A regra deve continuar funcionando nos dias seguintes, não apenas nesse instante.
Não simule previsões para linhas desconhecidas como se fossem linhas reais.
Regras temporais

* Nunca devolver previsões vencidas.
* Uma previsão exatamente no instante da consulta pode ter espera de 0 minuto.
* O número de minutos de espera nunca pode ser negativo.
* Preservar UTC internamente.
* Exibir horário de São Paulo na resposta.
* Utilizar o mesmo instante de referência para calcular as previsões, esperas e `consultadoEm`.
* Testar limites de segundos e minutos.
* Testar horários próximos de meia-noite.
* Testar mudança de dia e de mês.
* Não criar observações de passagem ao gerar previsões.

Preserve a implementação já existente de `PrevisaoChegada` e `ResultadoProvedor` sempre que possível.
6. FONTE E CONFIABILIDADE
O provedor simulado deve retornar:

* `origem: "Simulado"`.
* `dadosSimulados: true` quando houver dados fictícios utilizados.
* `nivelConfirmacao: "Previsao"` em cada chegada prevista.
* Aviso `DadosSimulados`, quando cabível.

Nenhuma previsão é uma passagem observada.
Nenhum resultado simulado pode ser apresentado como dado oficial ou confirmado pela SPTrans.
A consulta de previsões não deve modificar o histórico de últimas passagens.
Preserve a distinção entre:

* Método de obtenção do dado.
* Natureza fictícia ou não.
* Nível de confirmação.

Para paradas ou linhas administrativas, preserve a regra do aviso `CadastroNaoVerificado` prevista na seção 8 do contrato.
7. SERVICE DE PREVISÕES
Implemente a camada Application responsável por:

1. Validar os parâmetros.
2. Consultar a parada.
3. Confirmar se ela existe e está ativa.
4. Validar `linhaId`, quando informado.
5. Confirmar que a linha existe e está ativa.
6. Confirmar que a linha atende à parada.
7. Identificar as linhas ativas que serão consultadas.
8. Consultar o provedor.
9. Montar o DTO `PrevisoesParada`.
10. Produzir avisos estruturados.
11. Ordenar as previsões.
12. Identificar linhas sem previsão.

A ordem de validação deve seguir o contrato:
400 → 404 → 422 → consulta ao provedor.
Uma entrada inválida não deve consultar o provedor.
Se a parada não possuir linhas ativas associadas, retorne 200 com listas vazias e aviso `ParadaSemLinhas`, sem consultar o provedor.
Não crie consultas redundantes desnecessárias.
Use interfaces de repositórios e DI.
Não acesse o SQLite diretamente na camada Application.
8. RESPOSTA DO ENDPOINT
Implemente exatamente o DTO previsto na seção 4.5:
`PrevisoesParada`
Campos:

* `paradaId`
* `codigoExternoParada`
* `nomeParada`
* `status`
* `origem`
* `dadosSimulados`
* `consultadoEm`
* `previsoes`
* `linhasSemPrevisao`
* `avisos`

Cada item de `previsoes` deve conter:

* `linhaId`
* `codigoLinha`
* `letreiro`
* `previstaPara`
* `esperaMinutos`
* `prefixoVeiculo`
* `nivelConfirmacao`

Ordenação

* `previsoes`: por previstaPara crescente e depois codigoLinha.
* `linhasSemPrevisao`: por codigoLinha.

Inclua critério final determinístico de desempate quando necessário, sem alterar a ordem contratual.
Estados
A. Previsões para todas as linhas

* HTTP 200.
* `status: "Disponivel"`.
* `linhasSemPrevisao: []`.

B. Previsões para parte das linhas

* HTTP 200.
* `status: "Disponivel"`.
* `linhasSemPrevisao` preenchida.
* Aviso `PrevisaoParcial`.

C. Provedor respondeu sem previsões

* HTTP 200.
* `status: "SemPrevisoes"`.
* `previsoes: []`.
* Aviso `SemPrevisoes`.

D. Parada sem linhas

* HTTP 200.
* Listas vazias.
* Aviso `ParadaSemLinhas`.
* Não consultar o provedor.

E. Provedor indisponível

* HTTP 503.
* ProblemDetails.
* Código `ProvedorPrevisoesIndisponivel`.
* Cabeçalho `Retry-After: 30`.

O valor `ProvedorIndisponivel` não deve ser devolvido como uma resposta HTTP 200 nesse endpoint.
Esse status será usado dentro do resumo do B9.
9. OPÇÃO DE SIMULAR INDISPONIBILIDADE — R6
Implemente uma opção de configuração para simular a indisponibilidade do provedor.
Requisitos:

* Desligada por padrão.
* Utilizada nos testes e na demonstração do 503.
* Configurável sem modificar código-fonte.
* Documentada no README.
* Não exposta como rota pública para ativar falhas.
* Não altera o banco.
* Não remove os dados simulados existentes.
* Não interfere nas consultas de paradas, linhas, destinos ou últimas passagens.

Quando a opção estiver ativa e houver uma consulta válida que precise do provedor:

* O endpoint deverá devolver 503.
* O código será `ProvedorPrevisoesIndisponivel`.
* O cabeçalho deverá ser `Retry-After: 30`.

Uma parada sem linhas continua retornando 200 sem consultar o provedor, conforme o contrato.
10. ADAPTADOR HTTP OLHO VIVO — R8
O planejamento do projeto prevê um adaptador para a API Olho Vivo da SPTrans.
Implemente somente o adaptador necessário para permitir futura integração, sem ativá-lo por padrão.
Fonte de referência
Use a documentação oficial da SPTrans como base:
https://www.sptrans.com.br/desenvolvedores/api-do-olho-vivo-guia-de-referencia/documentacao-api
Também existe documentação da API v2.1 no portal de APIs da Prefeitura de São Paulo.
Confira a documentação oficial antes de definir URLs, autenticação e formato de resposta.
Não suponha que um endpoint ou host antigo continua disponível sem verificação.
Requisitos

* Utilizar HttpClient via injeção de dependência.
* Utilizar HTTPS.
* Configurar tempo limite explícito, documentado.
* Autenticação prevista pela documentação oficial.
* Credenciais fora do repositório.
* Não registrar tokens em logs ou erros.
* Não realizar chamadas reais sem configuração e autorização.
* Tratar corretamente falha de autenticação.
* Tratar timeout.
* Tratar falha de conexão.
* Tratar HTTP não bem-sucedido.
* Tratar JSON inválido ou inesperado.
* Interpretar previsões retornadas.
* Não incluir previsões já vencidas.
* Preservar o instante de referência e o fuso correto.
* Não inventar veículos ou horários em caso de resposta incompleta.
* Propagar CancellationToken.
* Distinguir cancelamento do cliente de indisponibilidade do provedor.

Proibição importante
O adaptador não deve enviar códigos SIM- à SPTrans.
Os códigos fictícios do seed não correspondem a identificadores oficiais de parada ou linha.
Para os dados SIM-, as previsões devem sempre utilizar o provedor simulado.
Não suponha que um código administrativo é automaticamente um código real e validado da SPTrans.
Sem correspondência oficial comprovada, não execute chamadas reais.
Testes do adaptador
Utilize um HttpMessageHandler falso ou mecanismo equivalente para simular respostas da API.
Cubra pelo menos estes cenários:

1. Autenticação bem-sucedida e resposta válida.
2. Autenticação recusada.
3. Timeout.
4. Erro HTTP ou de conexão.
5. JSON inválido ou campos obrigatórios ausentes.
6. Resposta válida sem previsões.

Também teste o bloqueio de códigos SIM- e a ausência de chamadas externas quando o adaptador estiver desabilitado.
Não afirme que esses testes simulados comprovam funcionamento contra a API real da SPTrans.
Se encontrar divergência entre diferentes versões da documentação oficial, documente o problema e limite o adaptador ao que for verificável, sem inventar uma integração completa.
11. SELEÇÃO DO PROVEDOR
O funcionamento padrão do PontoVoz deve permanecer:
Provedor simulado, sem necessidade de token externo.
Prepare uma seleção de provedor por configuração ou estratégia equivalente, mantendo o desenho simples.
Requisitos:

* Não precisar alterar Controllers para trocar o provedor.
* Não ativar Olho Vivo sem configuração explícita.
* Manter dados SIM- no provedor simulado.
* Não utilizar integração externa para a demonstração obrigatória.
* Não apresentar fallback fictício como se fosse previsão real após uma falha externa.
* Manter compatibilidade com os services de B9.

Não implemente múltiplas políticas complexas de fallback não previstas no contrato.
12. TIMEOUT, ERROS E CANCELAMENTO
O contrato determina um limite de tempo para o provedor externo.
Defina e documente um tempo limite razoável e configurável.
Quando o provedor falhar, estiver desligado ou não responder no prazo:

* A consulta de previsões deve devolver 503.
* A resposta deve usar `application/problem+json`.
* O código deve ser `ProvedorPrevisoesIndisponivel`.
* A mensagem deve ser a do contrato.
* O cabeçalho `Retry-After` deve ser 30.

Integre essa condição ao tratamento centralizado do B6 sem alterar indevidamente os outros erros.
Não converta qualquer exceção de aplicação em indisponibilidade do provedor.
Erros inesperados do banco continuam sendo erros internos, não falha da SPTrans.
Se o cliente cancelar a requisição, propague o cancelamento sem produzir artificialmente um 500 ou um 503.
Não exponha tokens, connection strings, stack traces ou dados internos.
13. TESTES UNITÁRIOS
Preserve os 663 testes existentes.
Crie testes adequados para:

* Provedor simulado.
* Regra de intervalos e defasagens.
* Horários futuros.
* Horário exatamente igual ao instante de consulta.
* Previsões vencidas descartadas.
* Espera em minutos.
* Troca de dia e mês.
* Repetição do padrão em datas diferentes.
* Linhas sem previsão.
* Várias previsões para a mesma linha.
* Ordenação determinística.
* Status Disponivel.
* Status SemPrevisoes.
* Provedor indisponível.
* TimeProvider controlado.
* CancellationToken.
* Origem e dados simulados.
* Ausência de gravação de previsões no SQLite.

Não crie testes baseados no relógio real que possam falhar dependendo da hora da execução.
Use FakeTimeProvider ou os utilitários de teste já presentes.
14. TESTES DE SERVICES
Teste:

1. Parada válida.
2. Parada inexistente.
3. Parada inativa.
4. Identificador de rota inválido.
5. linhaId inválido.
6. Linha inexistente.
7. Linha inativa.
8. Linha que não atende a parada.
9. Parada sem linhas.
10. Previsão para todas as linhas.
11. Previsão parcial.
12. Sem previsões.
13. Indisponibilidade.
14. Ordem dos avisos.
15. Dados administrativos não verificados.
16. Consulta ao provedor somente depois das validações.
17. Ausência de chamada ao provedor para parada sem linhas.
18. Propagação de CancellationToken.

Não duplique regras de domínio já implementadas.
15. TESTES HTTP E SQLITE REAL
Utilize WebApplicationFactory e SQLite isolado, preservando a estrutura de testes atual.
Teste o endpoint:
`GET /api/v1/paradas/{id}/previsoes`
Verifique:

* 200 com previsões.
* 200 com previsão parcial.
* 200 sem previsões.
* 200 para parada sem linhas.
* 400 para linhaId inválido.
* 404 para parada inexistente ou inativa.
* 404 para linha inexistente.
* 422 para linha não associada à parada.
* 503 para provedor indisponível.
* Retry-After: 30.
* Campos e enums JSON.
* Datas com fuso.
* Prefixos de veículo.
* Avisos corretos.
* Campos nulos presentes.
* Ordenação.
* Acessibilidade pública sem chave administrativa.
* Nenhuma alteração no histórico de passagens.
* Nenhuma previsão persistida no banco.
* Compatibilidade com as rotas B6 e B7.
* Cancelamento da requisição.
* Não vazamento de informações sensíveis.

Compare a resposta com `contracts/exemplos/previsoes.json` usando horário controlado, normalizando apenas IDs internos legítimos e campos realmente dinâmicos.
Compare o 503 com `erro-provedor-previsoes-indisponivel.json`.
Não altere os exemplos apenas para fazer o teste passar.
16. OPENAPI E SCALAR
Atualize a documentação automática para incluir a nova operação:
`GET /api/v1/paradas/{id}/previsoes`
Documente:

* Parâmetros.
* DTO de resposta.
* Códigos 200, 400, 404, 422 e 503.
* ProblemDetails.
* Retry-After.
* Origem simulada.
* Diferença entre previsão e passagem observada.

Ao concluir B8, o OpenAPI deverá conter:

* 5 operações públicas anteriores.
* 5 operações administrativas.
* 1 nova operação pública de previsões.
* GET /health.

Total esperado: 12 operações efetivamente implementadas.
Alternativas e resumo continuam pendentes para B9.
Reexecute:
`pwsh ./scripts/exportar-openapi.ps1`
Confirme que o documento exportado é igual ao gerado pela API real.
Preserve Scalar.
17. DOCUMENTAÇÃO E TRANSPARÊNCIA DOS DADOS
Atualize:

* README.md da raiz.
* backend/README.md.
* Infrastructure/README.md.
* Application/README.md, quando necessário.
* contracts/README.md.
* contracts/api-v1.md, somente no que for necessário para indicar o B8 implementado.
* contracts/openapi/pontovoz-v1.json.
* docs/dados.md.
* prompts.md.

Atenção especial a docs/dados.md: atualmente algumas seções ainda descrevem o projeto como estando no B0 ou B3. Atualize o estado para refletir o código real, sem alterar a origem dos dados.
O documento deve distinguir claramente:

* Provedor simulado funcional.
* Adaptador Olho Vivo preparado, mas desligado.
* Integração real não testada, caso não exista credencial.
* Dados fictícios utilizados na demonstração.
* Ausência de dados oficiais de chegada quando não houver integração real.

Não declare integração oficial funcionando com base apenas em testes com stubs.
Registre os prompts em prompts.md, preservando o histórico e a atribuição correta das ferramentas.
Não omita nem falsifique o uso de IA.
Não adicione marcas promocionais ou trailers automáticos de ferramentas.
18. EVIDÊNCIAS DO B8
Crie:
`backend/docs/evidencias/B8-previsoes.md`
Inclua evidências reais de:

* Provedor simulado.
* Resposta HTTP 200.
* Previsão parcial.
* Resposta sem previsões.
* Resposta 503.
* Cabeçalho Retry-After.
* Testes de horários.
* Testes do adaptador com HTTP simulado.
* Segurança das credenciais.
* Ausência de chamadas reais indevidas.
* OpenAPI e Scalar.
* Total real de testes.
* Build.

Não invente resultados nem imagens.
Não inclua chaves ou tokens nas evidências.
19. VALIDAÇÃO FINAL
Execute:

* dotnet restore
* dotnet build
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

Execute também:

* Verificador do contrato.
* Testes unitários do provedor.
* Testes de services.
* Testes HTTP.
* Testes do adaptador Olho Vivo com stubs.
* Teste de igualdade do OpenAPI.
* Teste da API real.
* Verificação do Scalar.

Confirme:

* Todos os testes antigos preservados.
* Novos testes aprovados.
* 0 avisos e 0 erros no build.
* API com 12 operações.
* Dados SIM- sempre simulados.
* Nenhuma previsão vencida.
* Nenhuma previsão falsamente oficial.
* Nenhum segredo versionado.
* Nenhuma alteração inesperada no banco.
* B9 não antecipado.

Se o executável estiver bloqueado por uma instância antiga da API, você já está autorizado a verificar e encerrar exclusivamente processos confirmados do PontoVoz, sem interromper outras aplicações.
Não utilize resultados de testes executados sobre binários antigos como validação final.
Corrija qualquer falha encontrada antes de concluir o bloco.
20. GIT
Não execute commit.
Não execute git push.
Não utilize force push.
Não altere commits anteriores.
Não descarte alterações.
Não versionar bancos, segredos ou arquivos temporários.
Deixe o B8 localmente implementado para auditoria.
21. RELATÓRIO FINAL OBRIGATÓRIO
Apresente:
A. Resumo simples do B8.
B. Arquivos criados e modificados.
C. Interface e provedores de previsão.
D. Algoritmo do provedor simulado.
E. Tratamento dos horários e do TimeProvider.
F. Service e DTOs.
G. Endpoint HTTP e respostas.
H. Condições 200, 400, 404, 422 e 503.
I. Opção de indisponibilidade.
J. Adaptador Olho Vivo: o que foi implementado, o que foi testado com stub e o que NÃO foi testado com a API real.
K. Resultados dos testes por grupo.
L. Total de testes aprovados, falhos e ignorados.
M. Resultado do build e dos comandos EF Core.
N. OpenAPI e Scalar.
O. Evidências registradas.
P. Confirmação de ausência de segredos e chamadas externas indevidas.
Q. Compatibilidade com o contrato.
R. Critérios oficiais da FIAP implementados e pendentes.
S. Estado do Git.
T. Problemas encontrados e soluções.
U. Pendências para B9.
Inclua uma explicação didática curta:

* Como o PontoVoz calcula uma chegada prevista.
* Por que os dados são simulados.
* Como o programa utiliza TimeProvider.
* Qual é a diferença entre previsão de chegada e última passagem registrada.
* Como o sistema reage quando o provedor está indisponível.

Execute somente B8. Não faça commit, não faça push e não inicie B9.
Conclua o bloco e aguarde a auditoria.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a ferramenta implementou
`GET /api/v1/paradas/{id}/previsoes`:
- **Provedores:** a interface `IProvedorPrevisaoChegada` (uma falha volta como indisponibilidade,
  não como exceção, para o B9 reutilizar), o provedor simulado padrão (grade de chegadas por linha,
  contada em minutos desde uma âncora fixa e calculada no instante da consulta, sem data no código e
  sem gravar nada), o adaptador Olho Vivo (v2.1, conferido na documentação oficial da SPTrans,
  desligado por padrão, só HTTPS, prazo de 5 s, sem enviar dados `SIM-`, com o token fora dos logs)
  e um seletor por configuração.
- **Service e HTTP:** validação 400 → 404 → 422 antes do provedor, parada sem linhas sem consultar o
  provedor, avisos do contrato, 503 `ProvedorPrevisoesIndisponivel` com `Retry-After: 30` pelo
  tratamento central e a opção `Previsoes:SimularIndisponibilidade` (R6).
- **Testes:** 94 novos (provedor simulado, adaptador com HTTP falso, service e HTTP); a resposta da
  API ficou igual a `previsoes.json` e ao exemplo do 503. Um teste encontrou um erro no adaptador
  (JSON com tipo inesperado viraria 500), corrigido. No total, 756 testes passaram. Nenhum teste
  acessou a API real da SPTrans, e a integração real **não** foi testada (sem credenciais).
- **Documentação:** OpenAPI com 12 operações, contrato, READMEs, `docs/dados.md` (antes ainda no
  estado do B0) e `backend/docs/evidencias/B8-previsoes.md`, com respostas reais em banco isolado.
- **Git:** não houve commit.

<a id="prompt-28"></a>

### Prompt 28 — Bloco B8.1: pacote ZIP para auditoria independente do código

**Data:** 08/11/2026 · **Tipo:** Preparação para auditoria · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B8.1 — PREPARAÇÃO DO CÓDIGO PARA AUDITORIA INDEPENDENTE
O relatório do B8 foi recebido.
A implementação está aprovada em princípio, mas, antes de autorizar commit e push, faremos uma auditoria independente dos arquivos reais do projeto.
Sua tarefa agora é preparar um ZIP completo e atualizado, preservando integralmente o trabalho realizado.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Commit-base publicado:
`f9661ba5ec8b374f963c987211315dae8899d2a8`
1. PRESERVAÇÃO DO PROJETO
Antes de qualquer operação:

1. Confirme o diretório correto.
2. Confirme a branch main.
3. Confirme HEAD e origin/main no commit esperado.
4. Execute git status.
5. Liste arquivos modificados e ainda não versionados.
6. Confirme que todas as alterações B8 permanecem presentes.

O relatório informou 21 arquivos modificados e 14 novos.
Não descarte alterações.
Não execute git reset --hard ou git clean.
Não recomece o B8.
Não implemente funcionalidades adicionais.
2. GERAR ZIP COMPLETO
Crie um arquivo chamado:
`PontoVoz-B8-Auditoria.zip`
Salve-o fora da pasta controlada pelo Git, preferencialmente em:
`C:\Users\paulo\Projetos\`
O ZIP deve conter o projeto completo no estado atual da pasta local, incluindo arquivos ainda não commitados.
Inclua obrigatoriamente:

* README.md da raiz.
* prompts.md.
* contracts/, incluindo exemplos, verificador e OpenAPI exportado.
* docs/, incluindo docs/dados.md.
* backend/src/, com todas as camadas do projeto.
* backend/tests/, incluindo os novos testes do B8.
* backend/docs/evidencias/.
* Scripts.
* Arquivos de solução e projetos .NET.
* Configurações versionáveis necessárias à execução.
* READMEs das camadas.

Confirme especialmente a presença de:

* IPrevisoes.cs.
* PrevisoesService.cs.
* ProvedorPrevisaoSimulado.cs.
* ProvedorOlhoVivo.cs.
* SeletorProvedorPrevisao.cs.
* OpcoesPrevisoes.cs.
* ProvedoresExtensions.cs.
* ProvedorPrevisoesIndisponivelException.cs.
* Todos os quatro grupos de testes novos.
* B8-previsoes.md.
* B8-scalar-previsoes.jpg.

Use os caminhos reais encontrados no projeto.
Não utilize somente git archive HEAD, porque isso excluiria os arquivos novos do B8 ainda não publicados.
3. EXCLUSÕES DE SEGURANÇA
Não inclua:

* .git/
* bin/
* obj/
* .vs/
* .idea/
* TestResults/
* Bancos SQLite.
* Arquivos -shm e -wal.
* Arquivos de user-secrets.
* Tokens reais.
* Chaves de API.
* Senhas reais.
* Logs privados.
* Arquivos temporários.
* Backups desnecessários.

Preserve os exemplos fictícios de testes e as evidências acadêmicas adequadas.
4. VERIFICAR INTEGRIDADE E SEGURANÇA
Após gerar o ZIP:

1. Teste sua integridade.
2. Conte os arquivos incluídos.
3. Confirme que todos os arquivos novos e alterados do B8 estão presentes.
4. Compare byte a byte os arquivos do ZIP com a pasta local.
5. Confirme a presença das 12 operações previstas no código e OpenAPI.
6. Confirme a inclusão dos testes do provedor simulado.
7. Confirme a inclusão dos testes do adaptador Olho Vivo.
8. Confirme que não há bancos, binários ou segredos.
9. Calcule o SHA-256.
10. Informe o caminho completo do arquivo.

Caso identifique alguma informação sensível nos documentos, não a publique nem a inclua no ZIP; relate o problema.
5. OBSERVAÇÕES PARA A AUDITORIA
Não altere o código nesta etapa.
A auditoria independente verificará especialmente:

* Algoritmo de previsões e TimeProvider.
* Separação entre dados simulados e oficiais.
* Tratamento de indisponibilidade.
* Resposta 503 e Retry-After.
* Segurança do adaptador Olho Vivo.
* Autenticação e tratamento de sessão do adaptador.
* Ausência de tokens em logs.
* Ausência de chamadas externas indevidas.
* Aderência ao contrato.
* Testes de integração.
* Documentação e evidências.

A integração real Olho Vivo deverá continuar identificada como não validada contra a API oficial, pois não foram executados testes reais.
6. RELATÓRIO FINAL
Apresente:
A. Caminho completo do ZIP.
B. Tamanho do arquivo.
C. Quantidade de arquivos.
D. SHA-256.
E. Confirmação de que todas as alterações locais B8 estão incluídas.
F. Confirmação da presença dos testes e evidências.
G. Resultado da verificação de segurança.
H. Estado do Git.
I. Confirmação de que não houve commit nem push.
Não inicie B9. Não faça commit ou push. Apenas prepare o ZIP e aguarde a auditoria.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code gerou
`PontoVoz-B8-Auditoria.zip` na pasta acima do repositório, a partir da árvore de trabalho
(arquivos versionados e os novos do B8 ainda não commitados), sem `.git`, binários, bancos SQLite,
logs nem segredos. Conferiu a integridade, os 197 arquivos, a presença e a igualdade byte a byte
dos 35 arquivos alterados ou novos do B8, as 12 operações nos controllers e no OpenAPI exportado,
os quatro grupos de testes novos, a ausência de bancos e chaves, e informou o SHA-256. Nenhum
código foi alterado e não houve commit.

<a id="prompt-29"></a>

### Prompt 29 — Bloco B8.2: validação final e publicação do B8

**Data:** 08/11/2026 · **Tipo:** Validação e publicação · **Commit:** `feat: implement simulated arrival predictions and Olho Vivo adapter`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B8.2 — VALIDAÇÃO FINAL E PUBLICAÇÃO AUTORIZADA
O ZIP PontoVoz-B8-Auditoria.zip foi recebido e auditado.
A auditoria confirmou a integridade dos 197 arquivos, o cálculo recorrente das previsões, a estrutura do provedor simulado, o tratamento de indisponibilidade, o adaptador Olho Vivo desligado por padrão, os testes e as 12 operações do OpenAPI.
O B8 está aprovado para publicação, condicionado à última validação automática.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base:
`f9661ba5ec8b374f963c987211315dae8899d2a8`
1. Verifique o estado do projeto
Antes de editar:

* Confirme o diretório e a branch main.
* Confirme HEAD e origin/main no commit esperado.
* Preserve todos os arquivos locais do B8.
* Compare o estado de trabalho com o ZIP auditado.
* Não descarte alterações.
* Não modifique a arquitetura sem necessidade.

2. Validação final
Execute a partir de backend/:

* dotnet restore
* dotnet build --no-incremental
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

Execute também o verificador de contrato.
O último relatório apontou 756 testes aprovados, 0 falhas, 0 ignorados e compilação sem avisos.
Confirme o resultado real atual, utilizando os binários recompilados.
3. Conferências específicas do B8
Verifique novamente:

* Provedor simulado como padrão.
* Algoritmo determinístico em datas diferentes.
* Nenhuma previsão vencida.
* TimeProvider utilizado corretamente.
* Dados fictícios sempre identificados como simulados.
* Nenhuma previsão apresentada como passagem observada.
* Endpoint GET /api/v1/paradas/{id}/previsoes.
* Respostas HTTP 200, 400, 404, 422 e 503.
* Retry-After: 30 na indisponibilidade.
* Nenhuma gravação de previsões no SQLite.
* Nenhuma chamada não autorizada à SPTrans.
* Nenhum token em logs, arquivos versionados ou respostas.
* Cancelamento propagado.
* OpenAPI com 12 operações.
* Documento OpenAPI exportado igual ao documento gerado.
* Scalar funcionando.

Não execute chamadas reais à Olho Vivo sem credenciais e autorização.
O adaptador ainda não foi validado com a API real. Preserve essa ressalva em toda a documentação.
4. Documentação acadêmica
Confirme que estão atualizados:

* README da raiz.
* README do backend.
* READMEs das camadas envolvidas.
* contracts/api-v1.md.
* contracts/README.md.
* contracts/openapi/pontovoz-v1.json.
* docs/dados.md.
* backend/docs/evidencias/B8-previsoes.md.
* prompts.md.

Registre integralmente esta instrução como próximo prompt do histórico.
Preserve a identificação fiel do uso do ChatGPT e do Claude Code.
Não altere a regra origemRegistro nesta etapa; a limitação já está documentada e não pertence ao escopo do B8.
5. Segurança e arquivos preparados
Antes do commit:

* Revise o diff.
* Confira todos os arquivos staged.
* Não inclua arquivos SQLite.
* Não inclua binários nem arquivos temporários.
* Não inclua chaves, tokens ou credenciais.
* Não inclua o ZIP de auditoria.
* Não inclua informações privadas nas evidências.
* Preserve os exemplos de teste fictícios.

6. Commit autorizado
Se todas as verificações passarem, faça o commit:
`feat: implement simulated arrival predictions and Olho Vivo adapter`
Confirme que a mensagem gravada é exatamente essa.
Use a autoria legítima e o e-mail noreply configurado.
Não acrescente trailer automático de ferramenta.
Não reescreva commits anteriores.
7. Push autorizado
Depois da validação e do commit correto, execute push normal para origin/main.
Não utilize force push.
Confirme main e origin/main sincronizadas e árvore limpa.
Se uma validação relevante falhar e não puder ser corrigida com segurança, não publique.
8. Relatório final
Apresente:
A. Resumo da publicação.
B. Total de testes aprovados, falhos e ignorados.
C. Resultado do build e EF Core.
D. Resultado do verificador de contratos.
E. Confirmação das 12 operações e Scalar.
F. Confirmação de que Olho Vivo continua desativado e não foi validado em produção.
G. Confirmação de ausência de segredos.
H. Hash completo do commit.
I. Mensagem gravada.
J. Resultado do push.
K. Estado final do Git.
L. Pendências para B9.
Não implemente B9. Publique somente o B8 e aguarde a próxima orientação.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code confirmou que a
árvore de trabalho era idêntica ao ZIP auditado e repetiu restore, build completo
(`--no-incremental`, 0 avisos e 0 erros), os 756 testes com os binários recompilados, os comandos
`dotnet ef` e o verificador do contrato, sem alterar o banco de desenvolvimento. Na API real, sobre
um banco isolado, conferiu as previsões simuladas (todas futuras), as respostas 200, 400, 404, 422
e 503 com `Retry-After: 30`, a ausência de gravação, nenhuma chamada à SPTrans, as 12 operações do
OpenAPI iguais ao arquivo exportado e o Scalar. O adaptador Olho Vivo continua desligado e não
validado contra a API real. Publicou o B8 com o commit `feat: implement simulated arrival predictions and Olho Vivo adapter`, sem trailer, e push normal.

## Bloco B9 — alternativas e resumo

<a id="prompt-30"></a>

### Prompt 30 — Bloco B9: alternativas de deslocamento, ranking e resumo agregado do totem

**Data:** 08/11/2026 · **Tipo:** Implementação, testes e documentação · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
PROMPT 11 — BLOCO B9: ALTERNATIVAS DE DESLOCAMENTO, RANKING E RESUMO AGREGADO DO TOTEM
Atue como engenheiro de software sênior especializado em C#, ASP.NET Core 10, Entity Framework Core, arquitetura de aplicações, APIs REST e testes automatizados.
Você é responsável por executar integralmente o desenvolvimento técnico do backend C# do PontoVoz.
Repositório oficial:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório local:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Último commit esperado:
`f35d1a2cee1823f380587de3eec761b26a7bddbe`
Os blocos B0 a B8 foram concluídos e publicados.
Sua tarefa é implementar exclusivamente o Bloco B9 — Alternativas de Deslocamento e Resumo do Totem.
Este bloco deverá completar as 13 rotas de negócio previstas no contrato, mantendo também `/health`.
Não implemente B10. Não faça commit nem push antes da auditoria.
1. EXIGÊNCIAS OFICIAIS DA FIAP
A entrega C# Software Development deve atender:

* POO: 3,0 pontos.
* Web API: 2,5 pontos.
* EF Core + LINQ: 1,5 ponto.
* Async/Await: 1,0 ponto.
* Qualidade de código: 2,0 pontos.

Também devem estar presentes:

* Interfaces.
* Injeção de dependência.
* Organização de camadas.
* API REST.
* Testes unitários.
* Persistência com EF Core.
* Consultas LINQ.
* Documentação OpenAPI/Scalar.
* README.
* Evidências de funcionamento.
* Alinhamento ao tema CitizenTech.

Não amplie o escopo nem remova requisitos.
A referência funcional obrigatória é `contracts/api-v1.md`, especialmente as seções 1, 2, 3, 4.7, 4.8, 7, 8, 9 e 11.
Os exemplos JSON existentes são parte do contrato aprovado.
2. INSPEÇÃO INICIAL OBRIGATÓRIA
Antes de editar qualquer arquivo:

1. Confirme o diretório correto.
2. Confirme a branch main.
3. Confirme HEAD no commit esperado.
4. Confirme main sincronizada com origin/main.
5. Confirme a árvore de trabalho limpa.
6. Leia integralmente as seções 4.7 e 4.8 do contrato.
7. Examine os quatro exemplos JSON:
   * `alternativas.json`.
   * `alternativas-sem-comparacao.json`.
   * `resumo.json`.
   * `resumo-provedor-indisponivel.json`.
8. Examine o catálogo de erros e avisos.
9. Examine os enums `SituacaoAlternativas`, `SituacaoAlternativa`, `CriterioAlternativas` e `SituacaoResumo`.
10. Examine a entidade `PercursoEstimado`.
11. Examine `IDestinoRepository` e `DestinoRepository`.
12. Examine o seed de percursos diretos.
13. Examine `IProvedorPrevisaoChegada`, `ResultadoProvedor` e o seletor de provedores do B8.
14. Examine `PrevisoesService`, `UltimasPassagensService`, `ParadaService` e seus DTOs.
15. Examine Controllers, ProblemDetails, DI e OpenAPI.
16. Examine a infraestrutura de testes e os exemplos de comparação contratual.

Aproveite as implementações existentes.
Não recrie `PercursoEstimado` ou seu repositório se já atenderem às necessidades.
Não presuma IDs internos fixos.
3. ESCOPO EXATO DO B9
Implemente exclusivamente:
Endpoint 1
`GET /api/v1/paradas/{id}/alternativas?destinoId={destinoId}`
Finalidade: comparar linhas diretas cadastradas da parada até determinado destino.
Endpoint 2
`GET /api/v1/paradas/{id}/resumo`
Finalidade: retornar, em uma única resposta, os dados da parada, as previsões de chegada e as últimas passagens.
Inclua:

* Services e interfaces necessários.
* DTOs do contrato.
* Validações.
* Mapeamentos.
* Avisos estruturados.
* Controllers públicos.
* Tratamento de indisponibilidade.
* Testes unitários e HTTP.
* Integração com SQLite.
* OpenAPI, Scalar e evidências.

Não implemente:

* Baldeações.
* Rotas por outros modais.
* Trajetos inventados.
* Mapas ou geolocalização em tempo real.
* Trânsito em tempo real.
* Interface visual do totem.
* Síntese de voz.
* Integração com IA generativa.
* Novas operações administrativas.
* Novas previsões ou registros fictícios persistidos.
* Integração real com SPTrans sem autorização.

4. ALTERNATIVAS — VALIDAÇÃO DE ENTRADA
Endpoint:
`GET /api/v1/paradas/{id}/alternativas`
Parâmetro obrigatório:
`destinoId`
Regras:

* `destinoId` ausente: 400 `RequisicaoInvalida`.
* `destinoId` com formato inválido: 400.
* `destinoId` menor que 1: 400.
* Parada inexistente ou inativa: 404 `ParadaNaoEncontrada`.
* Destino inexistente ou inativo: 404 `DestinoNaoEncontrado`.
* `id` inválido na rota: 404 `RotaNaoEncontrada`, conforme a restrição de rota já existente.

A validação deve ocorrer antes da consulta ao provedor.
Preserve a ordem contratual:
400 → 404 → consulta dos dados válidos.
Não retorne 422 simplesmente porque não existe uma linha direta até o destino.
Nesse caso, a resposta correta é 200 com `SemLinhaDireta`.
5. ALTERNATIVAS — IDENTIFICAÇÃO DAS LINHAS DIRETAS
Considere apenas linhas que:

1. Estejam ativas.
2. Atendam à parada solicitada.
3. Possuam um `PercursoEstimado` cadastrado para o destino.
4. Tenham o percurso associado à parada e à linha corretas.

Use `IDestinoRepository.ListarPercursosDiretosAsync` ou equivalente já implementado.
O cadastro dos percursos vem do seed existente.
Não calcule percursos novos a partir de distância geográfica.
Não infira que uma linha vai a um destino apenas por possuir nome de terminal parecido.
Não crie rotas que não estejam cadastradas.
No MVP:

* Somente linhas diretas.
* Nenhuma baldeação.
* `baldeacoes: 0`.
* Percurso estimado entre 1 e 240 minutos.
* `origemPercurso: "Simulado"`.

O percurso estimado deve continuar sendo apresentado como simulado, sem trânsito em tempo real.
6. ALTERNATIVAS — CÁLCULO DO RANKING
Para cada linha direta com previsão disponível, utilize a próxima chegada válida da linha.
Fórmulas obrigatórias:
`tempoTotalEstimadoMinutos = esperaMinutos + percursoEstimadoMinutos`
`chegadaEstimadaAoDestino = previstaPara + percursoEstimadoMinutos`
A espera deve utilizar o mesmo instante de referência da consulta.
Não utilize previsões vencidas.
Não confunda a hora de chegada do ônibus à parada com a hora de chegada do passageiro ao destino.
Ordenação das linhas comparáveis

1. Menor `tempoTotalEstimadoMinutos`.
2. Em empate, menor `esperaMinutos`.
3. Em novo empate, menor `codigoLinha`.

Atribua:
`posicao: 1, 2, 3...`
somente às linhas comparáveis.
Linhas sem previsão
Devem aparecer depois das comparáveis.
Campos obrigatoriamente nulos:

* `posicao`.
* `previstaPara`.
* `esperaMinutos`.
* `tempoTotalEstimadoMinutos`.
* `chegadaEstimadaAoDestino`.

O campo `percursoEstimadoMinutos` continua presente.
Ordenação:

1. Menor percurso estimado.
2. Menor codigoLinha.

Uma linha sem previsão nunca recebe posição e nunca é apresentada como a mais rápida.
Regra de comparação incompleta
Quando existir ao menos uma linha direta sem previsão:

* `comparacaoCompleta: false`.
* Incluir aviso `ComparacaoIncompleta`, quando houver alguma linha comparável.
* A posição 1 significa apenas a mais rápida entre as linhas com previsão.

A resposta não pode afirmar que encontrou a melhor opção absoluta quando faltam previsões.
7. EXEMPLO OBRIGATÓRIO DO RANKING
O exemplo `contracts/exemplos/alternativas.json` determina que, com o relógio controlado em 10/11/2026 às 14h32 de São Paulo:
SIM-L004

* Espera: 8 minutos.
* Percurso: 20 minutos.
* Total: 28 minutos.
* Posição: 1.

SIM-L002

* Espera: 1 minuto.
* Percurso: 40 minutos.
* Total: 41 minutos.
* Posição: 2.

SIM-L001

* Sem previsão.
* Percurso: 30 minutos.
* Posição: null.

O resultado deve ser exatamente essa ordem:
SIM-L004 → SIM-L002 → SIM-L001.
A comparação deverá ser incompleta.
Este exemplo comprova que a melhor alternativa considera o tempo total, e não somente qual ônibus chega primeiro.
Reproduza o exemplo com testes determinísticos e banco isolado.
Não codifique os valores exclusivamente para essa data.
8. ALTERNATIVAS — SITUAÇÕES GERAIS
Implemente os três estados previstos no contrato.
A. Comparadas
Quando existe pelo menos uma linha comparável:

* HTTP 200.
* `situacao: "Comparadas"`.
* `comparacaoCompleta: true` somente se todas as linhas forem comparáveis.
* Linhas sem previsão permanecem na lista, com posição null.
* `statusPrevisao` representa o resultado efetivo do provedor.

B. SemComparacao
Quando existem linhas diretas, mas nenhuma possui previsão utilizável:

* HTTP 200.
* `situacao: "SemComparacao"`.
* `comparacaoCompleta: false`.
* Todas as linhas com `posicao: null`.
* Nenhuma linha indicada como mais rápida.
* Aviso `SemComparacao`.

Se o provedor respondeu sem previsões:

* `statusPrevisao: "SemPrevisoes"`.
* Aviso `SemPrevisoes`.

Se o provedor estava indisponível:

* `statusPrevisao: "ProvedorIndisponivel"`.
* Aviso `ProvedorIndisponivel`.

Importante: a indisponibilidade do provedor não deve produzir HTTP 503 no endpoint de alternativas.
A resposta correta é 200 com informações parciais e avisos.
C. SemLinhaDireta
Quando não existe linha direta cadastrada até o destino:

* HTTP 200.
* `situacao: "SemLinhaDireta"`.
* `alternativas: []`.
* `comparacaoCompleta: false`.
* `statusPrevisao: null`.
* `origemPrevisao: null`.
* Aviso `SemLinhaDireta`.
* Não consultar o provedor de previsões.

Preserve também `LimitacaoLinhasDiretas`.
Se a parada não tiver linhas associadas, respeite a regra do aviso `ParadaSemLinhas`.
9. DTO ALTERNATIVASDESLOCAMENTO
Implemente os campos exatos da seção 4.7:

* `paradaId`.
* `codigoExternoParada`.
* `nomeParada`.
* `destino`: objeto `{ id, nome, tipo }`.
* `criterio`.
* `situacao`.
* `comparacaoCompleta`.
* `statusPrevisao`.
* `origemPrevisao`.
* `dadosSimulados`.
* `consultadoEm`.
* `alternativas`.
* `avisos`.

O critério do MVP deve ser:
`MenorTempoTotalEstimado`
Cada objeto `Alternativa` deve conter:

* `posicao`.
* `linhaId`.
* `codigoLinha`.
* `letreiro`.
* `situacao`.
* `previstaPara`.
* `esperaMinutos`.
* `percursoEstimadoMinutos`.
* `tempoTotalEstimadoMinutos`.
* `chegadaEstimadaAoDestino`.
* `baldeacoes`.
* `origemPercurso`.

Preserve enums como texto, campos null presentes e datas no horário de São Paulo.
Não crie propriedades adicionais fora do contrato.
10. AVISOS DE ALTERNATIVAS
Utilize o `CatalogoAvisos` existente e amplie-o apenas com os códigos previstos que ainda faltam.
Códigos relevantes:

* `DadosSimulados`.
* `CadastroNaoVerificado`.
* `LimitacaoLinhasDiretas`.
* `PercursoEstimado`.
* `ComparacaoIncompleta`.
* `SemComparacao`.
* `SemPrevisoes`.
* `ProvedorIndisponivel`.
* `SemLinhaDireta`.
* `ParadaSemLinhas`.

Preserve as mensagens e a ordem dos exemplos JSON.
O aviso `LimitacaoLinhasDiretas` deve sempre aparecer na resposta de alternativas.
Quando existir percurso, incluir `PercursoEstimado`.
O aviso `CadastroNaoVerificado` deve aparecer uma única vez quando a parada ou alguma linha incluída na resposta tiver origem `CadastroAdministrativo`.
Não confunda `CadastroNaoVerificado` com `DadosSimulados`.
Nenhum resultado deve ser apresentado como uma rota oficial validada pela SPTrans.
11. RESUMO — OBJETIVO
Implemente:
`GET /api/v1/paradas/{id}/resumo`
Essa rota deve reunir, numa única resposta:

1. Detalhe da parada.
2. Previsões de chegada.
3. Últimas passagens registradas.

O endpoint é público e não exige chave administrativa.
A resposta deverá ter formato `ResumoParada`.
Não inclua a lista de alternativas no resumo, pois ela depende de um destino e não faz parte do DTO da seção 4.8.
Não crie novas funcionalidades de previsão ou passagem.
Reutilize os services e regras já implementados.
12. DTO RESUMOPARADA
Implemente exatamente:

* `consultadoEm`.
* `dadosSimulados`.
* `situacao`.
* `partesIndisponiveis`.
* `parada`.
* `previsoes`.
* `ultimasPassagens`.
* `avisos`.

Os campos internos deverão manter o mesmo formato das respectivas respostas individuais:

* `parada`: mesmo formato do GET /paradas/{id}.
* `previsoes`: mesmo formato de GET /previsoes.
* `ultimasPassagens`: mesmo formato de GET /ultimas-passagens.

A interface do totem deverá conseguir reutilizar os mesmos componentes de leitura dos dados.
Não renomeie propriedades de DTOs existentes.
13. RESUMO — INSTANTE ÚNICO
Essa é uma regra essencial:
Toda a resposta de resumo deve usar um único instante de referência do TimeProvider.
Leia o relógio uma vez por requisição.
O mesmo instante deve valer para:

* `resumo.consultadoEm`.
* `previsoes.consultadoEm`.
* `ultimasPassagens.consultadoEm`.
* Esperas de previsões.
* Minutos desde observações.
* Desatualização de passagens.
* Cálculos temporais internos.

Se os services B5 e B8 atualmente consultarem o próprio relógio, faça o menor ajuste seguro necessário para permitir reutilizar a mesma referência temporal dentro do resumo.
Preserve o comportamento público anterior dos services.
Evite copiar e colar toda a lógica de previsões e passagens para criar o resumo.
Utilize interfaces ou métodos internos adequados, mantendo a separação das camadas.
Teste com relógio controlado e verifique explicitamente que o TimeProvider é lido uma única vez para o resumo.
14. RESUMO — CONSULTAS ASSÍNCRONAS
O contrato prevê paralelismo entre provedor de previsões e consultas ao banco quando for seguro.
Atenção: o mesmo DbContext do EF Core não permite operações simultâneas.
Implemente o paralelismo de forma segura:

* Consultas que compartilham o mesmo DbContext devem ser executadas em sequência.
* A chamada assíncrona independente ao provedor pode executar em paralelo com operações de banco quando não houver dependência e quando isso for seguro.
* Utilize `Task.WhenAll` onde o contrato pede paralelismo e as dependências permitirem.
* Não compartilhe o mesmo DbContext entre operações EF simultâneas.
* Não use Task.Run para disfarçar operações assíncronas.
* Não crie novos DbContexts desnecessariamente.
* Não consulte o provedor duas vezes para a mesma parte do resumo.

Documente brevemente essa decisão técnica.
Propague CancellationToken a todas as dependências.
15. RESUMO — SITUAÇÃO COMPLETA
Quando todas as partes forem obtidas:

* HTTP 200.
* `situacao: "Completo"`.
* `partesIndisponiveis: []`.
* `avisos: []`, quando não houver aviso geral adicional previsto.

Cada parte mantém seus próprios avisos.
Uma previsão parcial para algumas linhas não torna automaticamente o resumo inteiro parcial se o provedor respondeu normalmente.
A ausência legítima de previsões não é o mesmo que indisponibilidade do provedor.
Compare com:
`contracts/exemplos/resumo.json`
16. RESUMO — PROVEDOR INDISPONÍVEL
Quando o provedor de previsões estiver indisponível:
O resumo deve continuar respondendo HTTP 200.
Campos obrigatórios:

* `situacao: "Parcial"`.
* `partesIndisponiveis: ["previsoes"]`.
* Aviso geral `ResumoParcial`.

A parte `previsoes` deve existir com:

* `status: "ProvedorIndisponivel"`.
* `previsoes: []`.
* `linhasSemPrevisao` contendo as linhas aplicáveis.
* Aviso interno `ProvedorIndisponivel`.
* Campos de origem e simulação coerentes com o contrato.

As partes `parada` e `ultimasPassagens` devem continuar disponíveis e corretas.
Não transforme a indisponibilidade de previsão em HTTP 503 no resumo.
O 503 continua reservado ao endpoint específico `/previsoes`.
Compare a resposta com:
`contracts/exemplos/resumo-provedor-indisponivel.json`
Não invente informações ausentes.
Não transforme falhas inesperadas do banco em uma resposta parcial de sucesso. Erros internos reais devem continuar sendo tratados como erros internos.
17. RESUMO — AUSÊNCIA DE LINHAS E CADASTROS
Teste e preserve:

* Parada sem linhas associadas.
* Parada inexistente ou inativa.
* Linhas sem registro de passagem.
* Passagens desatualizadas.
* Registros manuais não verificados.
* Dados simulados.
* Paradas cadastradas administrativamente.
* Linhas cadastradas administrativamente.

Para parada inexistente ou inativa:
404 `ParadaNaoEncontrada`.
Para parada sem linhas:
200, com as listas vazias e os avisos adequados.
Não consultar o provedor se nenhuma linha precisar de previsão.
O resumo deve respeitar a natureza dos dados:

* Previsão não é passagem observada.
* Passagem registrada não é previsão.
* Cadastro administrativo não é dado oficial verificado.
* Dado fictício deve ser identificado como simulado.

18. REUTILIZAÇÃO DO PROVEDOR DO B8
Reutilize `IProvedorPrevisaoChegada` e o seletor existente.
Preserve:

* Provedor simulado como padrão.
* Olho Vivo desativado por padrão.
* Nenhum token real no repositório.
* Nenhuma chamada real não autorizada.
* Bloqueio de dados SIM- para chamadas externas.
* Regras de timeout e cancelamento.
* Ausência de previsões vencidas.

Quando houver indisponibilidade:

* `/previsoes`: 503.
* `/alternativas`: 200 com `SemComparacao`.
* `/resumo`: 200 com `Parcial`.

Teste explicitamente os três comportamentos com a mesma configuração de indisponibilidade.
Não altere o tratamento do endpoint B8 para fazê-lo parecer disponível.
19. TESTES UNITÁRIOS — ALTERNATIVAS
Preserve todos os 756 testes existentes.
Crie testes que cubram:

1. destinoId ausente.
2. destinoId inválido.
3. Parada inexistente.
4. Parada inativa.
5. Destino inexistente.
6. Destino inativo.
7. Parada sem linhas.
8. Parada com linhas, mas sem percurso direto.
9. Uma linha comparável.
10. Várias linhas comparáveis.
11. Ranking por menor tempo total.
12. Desempate por espera.
13. Desempate por código.
14. Escolha apenas da próxima previsão válida.
15. Linha sem previsão.
16. Comparação incompleta.
17. Comparação completa.
18. Sem comparação por ausência de previsões.
19. Sem comparação por indisponibilidade.
20. Sem linha direta.
21. Ordenação das linhas sem previsão.
22. Posições numéricas consecutivas.
23. Nenhuma posição atribuída a linha sem previsão.
24. Comparação vazia nunca completa.
25. Cálculo de chegada estimada ao destino.
26. Dados simulados e origem.
27. Cadastro não verificado.
28. Avisos e mensagens do contrato.
29. TimeProvider.
30. CancellationToken.
31. Ausência de consulta ao provedor quando não há linha direta.

Use relógio controlado.
Não codifique a data de novembro de 2026 nas regras de negócio.
20. TESTES UNITÁRIOS — RESUMO
Teste:

1. Parada válida.
2. Parada inexistente.
3. Parada inativa.
4. Resumo completo.
5. Provedor indisponível.
6. Resumo parcial.
7. Previsões ausentes mas provedor disponível.
8. Previsão parcial sem transformar todo o resumo em Parcial.
9. Parada sem linhas.
10. Últimas passagens preservadas.
11. Passagens antigas.
12. Registros futuros excluídos.
13. Mesmo consultadoEm nas três partes.
14. Uma única leitura de TimeProvider.
15. Avisos gerais versus avisos internos.
16. Campos null.
17. Dados simulados.
18. Cadastro administrativo.
19. CancellationToken.
20. Falha real de banco não mascarada como sucesso parcial.
21. Nenhuma operação simultânea indevida no mesmo DbContext.
22. Nenhuma duplicação de chamadas ao provedor.

Preserve o comportamento dos services anteriores.
21. TESTES DE INTEGRAÇÃO HTTP E SQLITE
Utilize WebApplicationFactory e SQLite real isolado.
Implemente testes HTTP para os dois endpoints.
Alternativas

* 200 Comparadas.
* 200 SemComparacao.
* 200 SemLinhaDireta.
* 400 destinoId ausente.
* 400 destinoId inválido.
* 404 parada inexistente.
* 404 destino inexistente.
* Ranking e desempates.
* Campos nulos.
* Ordenação.
* Avisos.
* 200 com provedor indisponível.
* Compatibilidade com exemplos JSON.
* Sem autenticação administrativa.

Resumo

* 200 Completo.
* 200 Parcial.
* 404 parada inexistente.
* Parada sem linhas.
* Instante único.
* Previsões e passagens corretas.
* Provedor indisponível sem perda das outras partes.
* JSON idêntico ao contrato, ressalvados IDs legitimamente dinâmicos.
* Sem acesso administrativo obrigatório.
* Ausência de escrita no banco.
* Cancelamento.

Não altere o banco de desenvolvimento para esses testes.
Não use IDs internos fixos.
22. COMPARAÇÃO OBRIGATÓRIA COM O CONTRATO
Compare campo a campo as respostas HTTP com:

* `alternativas.json`.
* `alternativas-sem-comparacao.json`.
* `resumo.json`.
* `resumo-provedor-indisponivel.json`.

Use o relógio e o banco de testes adequados para reproduzir as situações.
O exemplo de resumo possui um registro manual e observações históricas.
Prepare o cenário de teste reproduzindo esses fatos de maneira legítima, sem alterar o domínio nem codificar IDs internos fixos.
Normalize somente campos realmente dinâmicos, documentando cada normalização.
Não altere os exemplos apenas para obter testes aprovados.
Se houver divergência legítima entre a fixture atual e os exemplos históricos, explique e resolva o cenário de teste sem mudar silenciosamente o contrato.
23. OPENAPI E SCALAR
Acrescente os endpoints:

* `GET /api/v1/paradas/{id}/alternativas`.
* `GET /api/v1/paradas/{id}/resumo`.

Documente:

* Query destinoId obrigatório.
* DTOs.
* Enums.
* Códigos HTTP.
* ProblemDetails.
* Ordenação das alternativas.
* Situações Comparadas, SemComparacao e SemLinhaDireta.
* Situações Completo e Parcial.
* Comportamento diante da indisponibilidade.
* Avisos estruturados.
* Natureza simulada dos percursos.

Após B9, devem existir:

* 8 endpoints públicos de negócio.
* 5 endpoints administrativos.
* 1 endpoint de health.

Total: 14 operações implementadas.
Exportar novamente:
`pwsh ./scripts/exportar-openapi.ps1`
Confirme que o documento exportado corresponde exatamente à API real.
Preserve Scalar e a segurança aplicada somente às operações administrativas.
24. DOCUMENTAÇÃO E EVIDÊNCIAS
Atualize o necessário:

* README.md da raiz.
* backend/README.md.
* READMEs das camadas envolvidas.
* contracts/api-v1.md.
* contracts/README.md.
* contracts/openapi/pontovoz-v1.json.
* docs/dados.md, apenas se necessário.
* prompts.md.

Atualize a tabela de rotas para marcar alternativas e resumo como implementados.
Não altere regras já aprovadas.
Crie:
`backend/docs/evidencias/B9-alternativas-resumo.md`
Registre evidências reais:

* Ranking com SIM-L004 na frente da SIM-L002.
* Linha sem previsão sem posição.
* Comparação incompleta.
* Sem comparação.
* Sem linha direta.
* Resumo completo.
* Resumo parcial.
* Provedor indisponível com HTTP 200 nas duas novas rotas.
* Endpoint individual de previsões ainda retornando 503.
* Testes de uma única referência temporal.
* Integração real com SQLite.
* OpenAPI e Scalar.
* Total de testes e build.

Não invente capturas ou resultados.
Registre o prompt no prompts.md com as atribuições reais das ferramentas.
Não oculte o uso do ChatGPT ou Claude Code.
25. VALIDAÇÃO FINAL OBRIGATÓRIA
Execute:

* dotnet restore.
* dotnet build --no-incremental.
* dotnet test.
* dotnet ef migrations list.
* dotnet ef database update.
* dotnet ef migrations has-pending-model-changes.

Execute também:

* Verificador do contrato.
* Testes unitários.
* Testes de integração SQLite.
* Testes HTTP.
* Testes do provedor.
* Teste de igualdade do OpenAPI exportado.
* Teste real da API.
* Verificação de Scalar.

Confirme:

* Todos os testes antigos preservados.
* Testes B9 aprovados.
* Zero erros e zero avisos no build.
* 14 operações da API.
* Nenhum endpoint pendente do contrato.
* Nenhum segredo exposto.
* Nenhuma chamada externa indevida.
* Nenhum dado fictício apresentado como oficial.
* Nenhum percurso inventado.
* Nenhuma gravação desnecessária no banco.
* Nenhum problema de concorrência do DbContext.
* Nenhuma alteração indevida no banco de desenvolvimento.

Se um processo antigo do PontoVoz estiver bloqueando a compilação, você está autorizado a verificar e encerrar exclusivamente esse processo, desde que confirme sua origem e não afete outras aplicações.
Não considere válidos testes executados apenas sobre binários antigos.
26. RUBRICA E ORGANIZAÇÃO
Faça uma conferência técnica dos requisitos da FIAP:

* POO.
* Web API.
* EF Core.
* LINQ.
* Async/Await.
* Interfaces.
* Injeção de dependência.
* Testes unitários.
* Qualidade do código.
* Documentação.
* Evidências.

Não afirme garantia de nota.
O B10 ainda ficará responsável pela revisão final da entrega, documentação acadêmica e testes globais.
27. CONTROLE DO GIT
Não faça commit.
Não faça push.
Não utilize force push.
Não reescreva histórico.
Não descarte alterações existentes.
Não implemente funcionalidades fora do B9.
Deixe o projeto localmente concluído para auditoria independente.
28. RELATÓRIO FINAL OBRIGATÓRIO
Apresente:
A. Resumo simples do B9.
B. Arquivos criados e modificados.
C. Endpoint de alternativas.
D. Regras do ranking.
E. Tratamento de linhas sem previsão.
F. Estados Comparadas, SemComparacao e SemLinhaDireta.
G. Endpoint de resumo.
H. Instante único do TimeProvider.
I. Estratégia de Task.WhenAll e segurança do DbContext.
J. Resumo completo e parcial.
K. Reutilização dos serviços e do provedor B8.
L. Testes por grupo.
M. Total real de testes aprovados, falhos e ignorados.
N. Build e EF Core.
O. Resultados do verificador do contrato.
P. Comparação com os quatro exemplos JSON.
Q. OpenAPI e Scalar.
R. Evidências.
S. Confirmação da ausência de segredos, rotas inventadas e chamadas externas indevidas.
T. Requisitos FIAP implementados e pendentes.
U. Estado do Git.
V. Problemas encontrados e soluções.
W. Pendências para B10.
Inclua também uma explicação didática breve para apresentação:

* Por que o primeiro ônibus a chegar nem sempre é o mais rápido até o destino.
* Como funciona o ranking.
* O que significa comparação incompleta.
* Por que a resposta de alternativas não falha completamente quando o provedor está indisponível.
* Como o resumo reúne as partes e identifica informações indisponíveis.

Execute exclusivamente B9. Não faça commit, não faça push e não inicie B10.
Ao concluir, aguarde a auditoria independente.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** a ferramenta implementou
`GET /api/v1/paradas/{id}/alternativas?destinoId=` e `GET /api/v1/paradas/{id}/resumo`:

- **Alternativas:** validação 400 (`destinoId` obrigatório) → 404 (parada, destino) → percursos
  diretos cadastrados → provedor de previsões. O ranking usa o tempo total estimado (espera da
  próxima chegada + percurso), com desempate pela espera e pelo código da linha; linhas sem previsão
  aparecem por último, sem posição, e a comparação fica incompleta. Sem percurso direto, a resposta é
  `SemLinhaDireta`, sem consultar o provedor; com o provedor indisponível, a resposta é 200
  `SemComparacao`, não 503.
- **Resumo:** parada, previsões e últimas passagens num único instante (o relógio é lido uma vez),
  reaproveitando a montagem das rotas individuais (métodos `Montar` extraídos sem mudar o
  comportamento). O provedor roda em paralelo com a consulta de passagens (`Task.WhenAll`), sem duas
  operações no mesmo `DbContext`. Com o provedor indisponível, o resumo é 200 `Parcial`, com
  `partesIndisponiveis: ["previsoes"]`.
- **Testes:** 51 novos (service e HTTP); as respostas ficaram iguais aos quatro exemplos do
  contrato. No total, 805 testes passaram.
- **Correção:** o transformador do OpenAPI do B7 retirava `null` também de enums das respostas
  (`statusPrevisao`, `origemPrevisao`); passou a valer só para os corpos de entrada.
- **Documentação:** OpenAPI com 14 operações, contrato (4.7, 4.8 e tabela de rotas), READMEs,
  `docs/dados.md` e `backend/docs/evidencias/B9-alternativas-resumo.md`, com respostas reais em
  banco isolado e a imagem do Scalar. O adaptador Olho Vivo continua desligado e não foi testado
  contra a API real.
- **Git:** não houve commit.

<a id="prompt-31"></a>

### Prompt 31 — Bloco B9.1: pacote ZIP para auditoria independente do código

**Data:** 08/11/2026 · **Tipo:** Preparação para auditoria · **Commit:** —

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B9.1 — PREPARAÇÃO DO ZIP PARA AUDITORIA INDEPENDENTE
O relatório final do B9 foi recebido.
Segundo o relatório, as rotas de alternativas e resumo foram concluídas, os 805 testes passaram, o OpenAPI contém 14 operações e o B9 ainda não foi publicado.
Antes do commit e push, precisamos auditar o código real.
Sua tarefa é preparar um ZIP completo do projeto no estado atual, sem modificar sua implementação.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Diretório:
`C:\Users\paulo\Projetos\GS-CitizenTech-PontoVoz`
Commit-base esperado:
`f35d1a2cee1823f380587de3eec761b26a7bddbe`
1. CONFIRMAÇÃO INICIAL
Verifique:

* Diretório correto.
* Branch main.
* HEAD e origin/main no commit esperado.
* Estado do git status.
* Arquivos modificados e ainda não versionados.
* Presença integral das alterações locais do B9.

O relatório anterior informou 19 arquivos modificados e 7 novos.
Não descarte nenhuma alteração.
Não faça commit nem push.
Não implemente B10.
2. GERAÇÃO DO ZIP
Crie o arquivo:
`PontoVoz-B9-Auditoria.zip`
Salve fora do repositório, preferencialmente em:
`C:\Users\paulo\Projetos\`
Inclua o projeto completo no estado atual, incluindo os arquivos ainda não commitados.
Inclua obrigatoriamente:

* README.md da raiz.
* prompts.md.
* contracts/, com todos os exemplos JSON, OpenAPI e verificador.
* docs/, incluindo docs/dados.md.
* backend/src/, com todas as camadas.
* backend/tests/, incluindo todos os testes do B9.
* backend/docs/evidencias/, incluindo as evidências e imagens.
* Scripts, configurações e arquivos de solução .NET.
* READMEs das camadas.

Confirme especialmente:

* AlternativasService.cs.
* ResumoParadaService.cs.
* PrevisoesService.cs.
* UltimasPassagensService.cs.
* AlternativasServiceTests.cs.
* ResumoParadaServiceTests.cs.
* AlternativasResumoHttpTests.cs.
* ParadasController.cs.
* CatalogoAvisos.
* DocumentacaoOpenApi.
* B9-alternativas-resumo.md.
* B9-scalar-alternativas.jpg.

Use os caminhos reais dos arquivos no projeto.
Não utilize somente git archive HEAD, pois excluiria as alterações locais do B9.
3. EXCLUSÕES DE SEGURANÇA
Não inclua:

* .git/
* bin/
* obj/
* .vs/
* .idea/
* TestResults/
* Bancos SQLite.
* Arquivos -shm e -wal.
* User-secrets.
* Chaves e tokens reais.
* Senhas reais.
* Arquivos temporários.
* Logs sensíveis.
* Backups e executáveis gerados.

Preserve os dados fictícios dos testes e as evidências acadêmicas apropriadas.
4. AUDITORIA DE INTEGRIDADE DO ZIP
Depois de gerar o arquivo:

1. Confirme que o ZIP abre corretamente.
2. Conte os arquivos incluídos.
3. Confirme que todos os arquivos novos e alterados do B9 estão presentes.
4. Compare byte a byte cada arquivo do ZIP com o correspondente na pasta de trabalho.
5. Confirme que o OpenAPI contém 14 operações.
6. Confirme a presença dos quatro exemplos JSON utilizados no B9.
7. Confirme que os testes de alternativas e resumo estão presentes.
8. Confirme a ausência de bancos, binários e segredos.
9. Calcule o SHA-256 do ZIP.
10. Informe o caminho completo do arquivo.

Caso encontre dados sensíveis, não os inclua nem os divulgue; relate o problema.
5. PONTOS QUE SERÃO AUDITADOS
Não altere o código nesta etapa.
A revisão independente verificará:

* Validação de destinoId.
* Consultas de percursos diretos no SQLite.
* Ranking por espera + percurso.
* Ordenação e desempates.
* Linhas sem previsão e campos nulos.
* Comparação incompleta.
* Estados Comparadas, SemComparacao e SemLinhaDireta.
* Resumo completo e parcial.
* Leitura única do TimeProvider.
* Segurança das operações concorrentes do EF Core.
* Reutilização dos services anteriores.
* Comportamento diante da indisponibilidade do provedor.
* Compatibilidade com os quatro exemplos JSON.
* Regressões nos endpoints B5 a B8.
* OpenAPI, Scalar e evidências.
* Exigências oficiais da FIAP.

6. RELATÓRIO FINAL
Apresente:
A. Caminho completo do ZIP.
B. Tamanho do ZIP.
C. Quantidade de arquivos.
D. SHA-256.
E. Confirmação de que todas as alterações do B9 estão incluídas.
F. Confirmação da presença dos testes e evidências.
G. Resultado da verificação de segurança.
H. Estado do Git.
I. Confirmação de ausência de commit e push.
Não implemente B10. Não faça commit ou push. Apenas gere o ZIP e aguarde a auditoria independente.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code gerou
`PontoVoz-B9-Auditoria.zip` na pasta acima do repositório, a partir da árvore de trabalho
(arquivos versionados e os novos do B9 ainda não commitados), sem `.git`, binários, bancos SQLite,
logs nem segredos. Conferiu a integridade, os 204 arquivos, a igualdade byte a byte com a pasta de
trabalho, os 26 arquivos alterados ou novos do B9, as 14 operações nos controllers e no OpenAPI
exportado, os quatro exemplos JSON e os testes novos, e informou o SHA-256. Ao adaptar o script do
B8.1, uma substituição falhou e o script antigo sobrescreveu por engano o
`PontoVoz-B8-Auditoria.zip` com o estado atual; a ferramenta o reconstruiu a partir do commit
publicado do B8 (197 arquivos, com outro SHA-256) e relatou o erro. Nenhum código foi alterado e
não houve commit.

<a id="prompt-32"></a>

### Prompt 32 — Bloco B9.2: correções documentais, validação final e publicação do B9

**Data:** 08/11/2026 · **Tipo:** Correção, validação e publicação · **Commit:** `feat: implement travel alternatives and stop summary`

**Instrução original (texto integral, como foi enviada):**

````text
PONTOVOZ — GLOBAL SOLUTION FIAP 2026.2
BLOCO B9.2 — CORREÇÕES DOCUMENTAIS, VALIDAÇÃO FINAL E PUBLICAÇÃO
O arquivo PontoVoz-B9-Auditoria.zip foi auditado independentemente.
A auditoria confirmou:

* 204 arquivos e integridade do ZIP.
* SHA-256: 2e770e93582fe3541925f46061ed5c9121b6a833d061888398e4085d50a99145.
* 19 arquivos modificados e 7 novos em relação ao B8.
* Implementação coerente dos serviços de alternativas e resumo.
* Ranking correto no código.
* Linhas sem previsão sem posição.
* Resumo com instante único.
* Paralelismo sem operações simultâneas no mesmo DbContext.
* Comportamento de indisponibilidade coerente.
* 14 operações presentes no OpenAPI.
* Testes e exemplos contratuais presentes.

Não foram identificados defeitos funcionais impeditivos na revisão estática.
O B9 está aprovado para publicação, condicionado às correções documentais abaixo e à validação automática final.
Repositório:
https://github.com/PONTOVOZ-GLOBAL-SOLUTION/pontovoz-csharp
Commit-base esperado:
`f35d1a2cee1823f380587de3eec761b26a7bddbe`
1. Confirme o estado inicial
Verifique:

* Diretório correto.
* Branch main.
* HEAD e origin/main no commit esperado.
* Alterações B9 preservadas.
* Árvore contendo os mesmos arquivos auditados.

Não descarte trabalho.
Não recomece B9.
Não implemente B10.
2. Correção obrigatória — descrição do OpenAPI
Arquivo:
`backend/src/PontoVoz.Api/Documentacao/DocumentacaoOpenApi.cs`
O método `DescreverDocumento` ainda informa:
"Alternativas e resumo estão planejados em contracts/api-v1.md."
Essa informação está desatualizada.
Atualize a descrição para refletir que:

* B6 implementou consultas públicas.
* B7 implementou operações administrativas.
* B8 implementou previsões simuladas.
* B9 implementou alternativas e resumo.
* A API tem 13 operações de negócio e o endpoint /health.

Preserve a explicação sobre dados simulados e sobre a integração Olho Vivo não validada contra a API real.
Não altere a estrutura ou a segurança dos endpoints.
Após a correção, regenere o OpenAPI exportado.
3. Correção obrigatória — contrato 4.7
Arquivo:
`contracts/api-v1.md`
Na seção 4.7, a tabela de dependências afirma que as consultas ao provedor e ao banco acontecem em paralelo com `Task.WhenAll`.
Entretanto, a implementação B9 consulta primeiro os percursos diretos e depois o provedor, porque precisa identificar as linhas aplicáveis.
A própria seção 4.7 explica corretamente esse comportamento mais adiante.
Corrija somente a informação contraditória da tabela, mantendo:

* A dependência do provedor.
* A dependência do repositório de percursos.
* A ordem de validação.
* A regra de não consultar o provedor com `SemLinhaDireta`.

O uso de `Task.WhenAll` no resumo continua correto e deve ser preservado.
Não mude a implementação para introduzir paralelismo artificial.
4. Correção obrigatória — textos históricos desatualizados
No mesmo `api-v1.md`:

* Na seção 7, atualize a afirmação de que as rotas B7 a B9 ainda não estão implementadas.
* Na seção 11, atualize a introdução que ainda identifica o estado como B8, embora a nota final já descreva B9.

Preserve o histórico verdadeiro das decisões anteriores.
Não reescreva os registros históricos para parecer que funcionalidades existiam antes de sua implementação.
Caso encontre outra descrição de estado atual desatualizada, corrija apenas a inconsistência comprovada.
Não amplie o escopo.
5. Validação final completa
Depois dessas correções, execute a partir de backend/:

* dotnet restore
* dotnet build --no-incremental
* dotnet test
* dotnet ef migrations list
* dotnet ef database update
* dotnet ef migrations has-pending-model-changes

Execute também o verificador do contrato.
O último relatório informou:

* 805 testes aprovados.
* 0 falhas.
* 0 ignorados.
* 0 avisos e 0 erros no build.

Confirme os resultados atuais em binários recém-compilados.
Não utilize resultados antigos como validação final.
6. Verificações técnicas do B9
Confirme novamente:

* O ranking utiliza espera + percurso.
* A SIM-L004 ocupa a primeira posição no exemplo contratado.
* Linhas sem previsão têm posição null.
* A comparação incompleta produz os avisos corretos.
* SemLinhaDireta não consulta o provedor.
* O resumo utiliza um instante único.
* O resumo não executa duas operações simultâneas no mesmo DbContext.
* Os services de B5 e B8 continuam funcionando.
* Os quatro exemplos JSON do B9 continuam compatíveis.
* O provedor indisponível gera 503 em /previsoes.
* O provedor indisponível gera 200 em /alternativas.
* O provedor indisponível gera 200 Parcial em /resumo.
* As consultas não gravam dados.
* Nenhum dado fictício é apresentado como oficial.
* A Olho Vivo permanece desligada por padrão.

7. OpenAPI e Scalar
Execute novamente:
`pwsh ./scripts/exportar-openapi.ps1`
Confirme:

* 14 operações implementadas.
* Oito rotas públicas de negócio.
* Cinco rotas administrativas.
* GET /health.
* Chave administrativa exigida somente nas cinco rotas administrativas.
* destinoId obrigatório.
* Enums e campos anuláveis corretos.
* Descrição do documento atualizada.
* OpenAPI exportado idêntico ao documento da API real.
* Scalar funcionando.

8. Documentação e prompts.md
Atualize a evidência B9 somente se as verificações reais tiverem mudado.
Registre este prompt no prompts.md, mantendo a organização cronológica e o histórico fiel.
Preserve a identificação correta da participação do ChatGPT e do Claude Code.
Não invente trabalho manual humano.
Não modifique contratos JSON ou regras de negócio sem necessidade.
9. Auditoria de segurança
Revise o diff e os arquivos staged.
Não publique:

* Bancos SQLite.
* Arquivos de segredos.
* Tokens.
* Binários.
* Logs.
* ZIPs de auditoria.
* Arquivos temporários.

Não inclua caminhos pessoais desnecessários.
10. Commit autorizado
Se todas as verificações passarem, faça o commit:
`feat: implement travel alternatives and stop summary`
Confirme:

* Mensagem gravada exatamente como autorizada.
* Autoria legítima com e-mail noreply.
* Nenhum trailer automático de ferramenta.
* Nenhuma reescrita de histórico.
* Nenhum arquivo indevido no stage.

11. Push autorizado
Execute push normal para origin/main.
Não utilize force push.
Confirme:

* main e origin/main no mesmo commit.
* Árvore de trabalho limpa.
* Alterações publicadas corretamente.

Se encontrar uma falha importante que não consiga corrigir com segurança, interrompa a publicação e apresente o impedimento.
12. Relatório final
Apresente:
A. Resumo da publicação.
B. Quatro ajustes documentais realizados.
C. Quantidade total de testes aprovados, falhos e ignorados.
D. Resultado do build e EF Core.
E. Resultado do verificador do contrato.
F. Confirmação dos quatro exemplos JSON do B9.
G. Funcionamento do ranking e resumo parcial.
H. Confirmação das 14 operações e Scalar.
I. Confirmação da ausência de segredos.
J. Hash completo do commit.
K. Mensagem gravada.
L. Resultado do push.
M. Estado final do Git.
N. Pendências específicas para B10.
Não implemente B10. Corrija, valide e publique somente o B9. Depois pare para a próxima auditoria.
````

**Resultado (resumo escrito pela ferramenta; não é transcrição):** o Claude Code fez as
correções documentais pedidas: a descrição do documento OpenAPI (`DescreverDocumento`) passou a
listar os blocos B6 a B9, as 13 operações de negócio e o `/health`, e a informar que o adaptador
Olho Vivo está desligado e não validado; a tabela da seção 4.7 do contrato deixou de citar
`Task.WhenAll` e descreve a ordem real (percursos e depois provedor); a seção 7 diz que as rotas
B7 a B9 respondiam `RotaNaoEncontrada` no B6 e que hoje todas existem; a seção 11 e o título do
quadro de estado deixaram de indicar o B8 e um planejamento já concluído. O OpenAPI foi exportado de
novo (só a descrição mudou). Um processo da API PontoVoz deste repositório ocupava a porta 5080 e
bloqueava o build; foi encerrado com base na autorização já dada para esses bloqueios. Com os
binários recompilados do zero: build com 0 avisos e 0 erros, 805 testes aprovados, comandos
`dotnet ef` e verificador do contrato sem falhas, banco de desenvolvimento inalterado. Na API real,
sobre um banco isolado, conferiu o ranking, as linhas sem previsão, `SemLinhaDireta`, o resumo
completo e parcial, a indisponibilidade (503, 200, 200 `Parcial`), a ausência de gravação, nenhuma
chamada à SPTrans, o Scalar e o OpenAPI igual ao exportado. Publicou o B9 com o commit
`feat: implement travel alternatives and stop summary`, sem trailer, e push normal.
