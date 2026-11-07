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

### Prompt 07 — Etapa B0.2: conexão e publicação no GitHub (06/11/2026)

**Texto original:**

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

**Resumo da resposta (não é transcrição):** a IA conferiu o repositório local e o remoto, que
estava vazio e já era público, e auditou os arquivos antes de publicar. Encontrou o e-mail pessoal
do integrante nos metadados do commit e perguntou como prosseguir. O integrante escolheu o e-mail
*noreply* do GitHub. A IA configurou esse e-mail só no repositório local e refez o autor do commit,
que ainda não tinha sido publicado, sem alterar nenhum arquivo. Depois conectou o `origin`,
executou `git push -u origin main` sem force push e conferiu no GitHub que os 25 arquivos
publicados eram iguais aos locais. Commit publicado: `f7a8ccf`.

### Prompt 08 — Bloco B1: contratos REST e diretriz permanente (07/11/2026)

O prompt foi enviado em uma única mensagem, com duas partes.

**Texto original — parte 1:**

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

**Texto original — parte 2:**

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

**Resumo da resposta (não é transcrição):** a IA escreveu o rascunho do contrato v1 em
`contracts/api-v1.md` e o guia da equipe em `contracts/README.md`. Os documentos trazem as
convenções, a política de respostas, os 13 endpoints planejados e o `/health` implementado, os
catálogos de erros e avisos, a conferência das rotas e as divergências em relação à Etapa 01.1.
Gerou 12 exemplos JSON fictícios e coerentes entre si em `contracts/exemplos/` e os validou com
um script. Nenhum código-fonte da API foi alterado, e nenhum commit foi feito.

### Prompt 09 — Bloco B1.1: revisão técnica final dos contratos REST (07/11/2026)

**Texto original:**

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

**Resumo da resposta (não é transcrição):** a IA revisou os arquivos de `contracts/`. Mudou o
endpoint de previsões para devolver 503 quando o provedor está indisponível, mantendo 200 com
status explícito no resumo e nas alternativas. Renomeou o status para `SemPrevisoes` e definiu
regras separadas para validar, guardar, consultar e marcar como desatualizada uma passagem.
Informou que o limite de 7 dias do planejamento tinha sido trocado por 24 horas no B1 sem
registro e documentou a decisão. Acrescentou ao resumo `situacao` e `partesIndisponiveis`,
detalhou a identificação da parada pelo código externo e registrou os requisitos R1 a R8 para
os próximos blocos. Consolidou a matriz de divergências D1 a D15, gerou 15 exemplos e criou um
verificador em C# (`contracts/verificador/VerificarContrato.cs`), testado também com defeitos
plantados de propósito. Nenhum código da API foi alterado, e nenhum commit foi feito.

### Prompt 10 — Bloco B1.2: consolidação final dos contratos (07/11/2026)

**Texto original:**

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

**Resumo da resposta (não é transcrição):** a IA revisou as regras R3 a R5. Removeu a opção
que renovava as observações simuladas a cada inicialização e definiu um seed idempotente: as
observações simuladas são criadas uma única vez, mantêm o horário original e ficam marcadas
como desatualizadas com o tempo. Reiniciar a API não cria nem altera passagens. A passagem
recente da demonstração passou a vir do registro manual administrativo. Acrescentou a seção
4.6.2 ao `api-v1.md`, registrou a mudança como divergência D16 e atualizou o README. Ajustou
os exemplos de passagem, resumo e operação administrativa. Ampliou o verificador com um grupo
de regras para dados simulados e o testou com defeitos plantados. Nenhum código da API foi
alterado, e nenhum commit foi feito.

### Prompt 11 — Bloco B1.3: correções finais e publicação do contrato (07/11/2026)

**Texto original:**

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

**Resumo da resposta (não é transcrição):** a IA separou, no contrato, o provisionamento
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

### Prompt 12 — Bloco B2: implementação do domínio e Programação Orientada a Objetos (07/11/2026)

**Texto original:**

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

**Resumo da resposta (não é transcrição):** a IA implementou o domínio em
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

### Prompt 13 — Bloco B2.1: correções de domínio e testes de regressão (07/11/2026)

**Texto original:**

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

**Resumo da resposta (não é transcrição):** a IA corrigiu as três falhas.
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

### Prompt 14 — Bloco B2.2: auditoria final e publicação do domínio C# (07/11/2026)

**Texto original:**

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

**Resumo da resposta (não é transcrição):** a IA conferiu o estado do Git e o escopo do diff,
que continha apenas domínio, testes, documentação do backend e este registro. Confirmou que
`contracts/` e a configuração da API não tinham mudado e que não restavam alterações
temporárias dos testes de mutação. Executou `dotnet restore`, `dotnet build` (0 avisos e 0 erros)
e `dotnet test` (278 testes aprovados, incluindo os 17 casos J1 a J5 e T1 a T7). Fez a auditoria
de segredos, dados pessoais, binários e marcas de ferramenta e publicou o commit
`feat: implement PontoVoz domain model with unit tests`, sem trailer, com push normal para
`origin/main`. O B3 não foi iniciado.
