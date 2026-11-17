
# PontoVoz — Prompts de estudo e debate de C#

**Disciplina:** C# Software Development — FIAP Global Solution 2026.2  
**Projeto:** PontoVoz — Totem Inteligente de Mobilidade Urbana

Este documento reúne **exemplos de perguntas para aprendizado, revisão e discussão técnica**, relacionados ao código e às decisões do PontoVoz.

## 1. Programação Orientada a Objetos

**Prompt 01 — Entidades e objetos de valor**  
"Explique a diferença entre uma entidade e um objeto de valor em C#. No PontoVoz, por que `Parada` é identificada por um ID, enquanto `Coordenada` pode ser comparada pelos seus valores? Mostre as vantagens e os cuidados de cada abordagem."

**Prompt 02 — Encapsulamento**  
"Ao analisar uma entidade que usa `private set`, como podemos garantir que seu estado só seja alterado por operações válidas? Use as ações de cadastrar, atualizar e desativar uma parada do PontoVoz para discutir essa decisão."

**Prompt 03 — Abstração, herança e polimorfismo**  
"Por que representar registros de passagem com uma classe abstrata e subclasses para passagens simuladas e manuais? Explique onde há polimorfismo, como o comportamento varia e quando seria preferível usar composição."

## 2. Interfaces e injeção de dependência

**Prompt 04 — Programação orientada a interfaces**  
"Por que um serviço de aplicação deve depender de `IParadaRepository`, em vez de conhecer diretamente a implementação com Entity Framework Core? Relacione a resposta com baixo acoplamento e testes."

**Prompt 05 — Injeção de dependência**  
"Explique como o contêiner de injeção de dependência do ASP.NET Core monta controllers, services, repositórios e provedores de previsão. Quando usar os tempos de vida `Scoped`, `Singleton` e `Transient`?"

**Prompt 06 — Provedores intercambiáveis**  
"Como uma interface para previsão de chegada permite alternar um provedor simulado e outro provedor sem mudar a lógica dos serviços? Discuta os limites dessa flexibilidade quando uma integração real ainda não foi validada."

## 3. Organização do código e responsabilidades

**Prompt 07 — Arquitetura em camadas**  
"Compare as responsabilidades de Domain, Application, Infrastructure e API no PontoVoz. Onde devem ficar validações de negócio, acesso ao banco, tratamento HTTP e regras de comparação de alternativas?"

**Prompt 08 — Controllers enxutos**  
"Quais problemas podem surgir quando um controller contém consultas LINQ, regras de negócio e lógica de persistência? Explique por que delegar essas tarefas a serviços e repositórios torna o código mais fácil de testar."

**Prompt 09 — Escolha arquitetural**  
"Quais são as vantagens e limitações de organizar camadas em pastas dentro de um único projeto .NET, em comparação com separá-las em vários projetos? Em qual situação cada opção faz sentido?"

## 4. APIs REST e contratos

**Prompt 10 — Modelagem das rotas**  
"Como definir rotas REST coerentes para consultar paradas, linhas, previsões, últimas passagens e alternativas? Por que faz sentido versionar a API com `/api/v1`?"

**Prompt 11 — Idempotência e exclusão lógica**  
"O que significa uma operação HTTP ser idempotente? Discuta por que uma associação de linha a parada pode usar PUT e por que desativar uma parada pode ser mais adequado do que excluí-la fisicamente."

**Prompt 12 — Códigos de status**  
"Explique as diferenças entre HTTP 400, 401, 403, 404, 409, 422 e 503. Dê exemplos de quando cada resposta é adequada no contexto do PontoVoz."

**Prompt 13 — Indisponibilidade de informações**  
"Se o provedor de previsões estiver indisponível, quando a API deve retornar 503 e quando ainda é útil responder 200 com dados parciais? Debata o efeito dessa decisão sobre a experiência do passageiro."

## 5. Entity Framework Core e persistência

**Prompt 14 — Entidades e relacionamentos**  
"Como configurar, com EF Core Fluent API, uma relação entre paradas e linhas e uma tabela de percursos estimados? Quais chaves, índices e restrições ajudam a preservar a consistência dos dados?"

**Prompt 15 — Mapeamento de herança**  
"O que é o mapeamento Table-per-Hierarchy (TPH) do Entity Framework Core? Quais as vantagens e limitações de armazenar diferentes tipos de registro de passagem em uma única tabela?"

**Prompt 16 — Banco de demonstração**  
"Como manter dados simulados no SQLite sem misturá-los com informações oficiais? Que cuidados o modelo de dados deve adotar para informar origem, simulação e limitações da solução?"

## 6. Migrations e dados iniciais

**Prompt 17 — Evolução do banco**  
"Qual a função das migrations do EF Core, e como validar uma atualização do esquema sem comprometer dados já existentes? Explique os cuidados necessários antes de aplicar migrations fora de Development."

**Prompt 18 — Seed idempotente**  
"Como construir um seed que possa rodar mais de uma vez sem duplicar paradas e linhas de demonstração? Qual é a diferença entre dados iniciais e dados observados de verdade?"

## 7. LINQ e desempenho

**Prompt 19 — Consultas no banco**  
"Por que uma consulta LINQ com filtros, ordenação e paginação deve ser traduzida para SQL em vez de carregar todos os registros em memória? Relacione isso com desempenho e escalabilidade."

**Prompt 20 — Última passagem de cada linha**  
"Como usar GroupBy e ordenação para selecionar a última passagem registrada de cada linha? Que cuidados são importantes com empates, índices e datas?"

**Prompt 21 — Busca textual**  
"Como desenvolver uma busca por nomes de paradas que não diferencie acentos e maiúsculas? Quais limitações do SQLite devem ser consideradas nessa implementação?"

## 8. Programação assíncrona

**Prompt 22 — Async/await**  
"Quando async/await melhora uma Web API em ASP.NET Core? Explique a diferença entre aguardar operações de entrada e saída e executar trabalho de CPU em paralelo."

**Prompt 23 — Task.WhenAll**  
"Quando faz sentido consultar fontes independentes com Task.WhenAll? Quais problemas podem acontecer se duas operações concorrentes utilizarem a mesma instância de DbContext?"

**Prompt 24 — CancellationToken**  
"Como o CancellationToken da requisição chega do controller até a consulta ao banco? Por que o cancelamento pelo cliente não deve ser tratado automaticamente como um erro HTTP 500?"

## 9. Validações, erros e segurança

**Prompt 25 — Regras de domínio**  
"Como diferenciar uma entrada malformada de uma violação de regra de negócio? Discuta como representar essas situações de maneira consistente em uma API REST."

**Prompt 26 — ProblemDetails**  
"Por que usar respostas ProblemDetails padronizadas? Quais informações são úteis ao cliente e quais detalhes internos não devem ser expostos em erros 500?"

**Prompt 27 — Acesso administrativo**  
"Quais cuidados devem proteger operações administrativas em uma API de demonstração? Discuta a diferença entre Development e Production, armazenamento de chaves e comparação segura de segredos."

## 10. Testes e garantia de qualidade

**Prompt 28 — Testes de domínio**  
"Quais regras do PontoVoz precisam de testes unitários, como validade de datas, estado de uma parada ou cálculo do tempo total de viagem? Como manter esses testes independentes do banco e da rede?"

**Prompt 29 — Relógio controlado**  
"Como testar regras que dependem do tempo, como considerar uma passagem desatualizada depois de 60 minutos? Explique o papel de um relógio controlável nos testes e os riscos de depender do horário real."

**Prompt 30 — Testes de integração**  
"Quando devemos testar com SQLite real em vez do provedor InMemory do EF Core? Como validar migrations, consultas LINQ e respostas HTTP em um ambiente isolado?"

**Prompt 31 — Testes de contrato**  
"Como conferir se os exemplos JSON, o contrato REST e a especificação OpenAPI correspondem às respostas reais da API? Por que essa verificação é importante para evitar documentação desatualizada?"

## 11. Documentação e manutenção

**Prompt 32 — README reproduzível**  
"Que informações um README precisa fornecer para que alguém clone um projeto .NET, instale dependências, aplique migrations, execute a API e rode os testes sem ajuda dos desenvolvedores?"

**Prompt 33 — Scalar e OpenAPI**  
"Como o OpenAPI e uma interface como o Scalar ajudam a entender e testar uma API? Qual é a diferença entre o contrato técnico publicado e a documentação que explica as decisões de negócio?"

**Prompt 34 — Investigação de falhas intermitentes**  
"Como investigar um teste que falha de forma intermitente? Quais evidências coletar antes de atribuir o problema a paralelismo, dados compartilhados ou dependência do relógio?"

## 12. Debate sobre a proposta CitizenTech

**Prompt 35 — Decisão de viagem**  
"Por que a melhor alternativa de ônibus não é necessariamente a linha que chega primeiro ao ponto? Explique como combinar tempo de espera e tempo de percurso de forma compreensível para o passageiro."

**Prompt 36 — Resiliência do serviço**  
"Quando a previsão de chegada fica indisponível, que outras informações o PontoVoz ainda pode oferecer para ajudar o passageiro? Como deixar claras as partes indisponíveis sem induzir a erro?"

**Prompt 37 — Dados simulados e transparência**  
"Que riscos existem em mostrar previsões, últimas passagens ou posições de ônibus simuladas sem avisos claros? Como apresentar os limites do protótipo de forma responsável?"

**Prompt 38 — Escopo e evolução futura**  
"Quais componentes são necessários para transformar um backend demonstrativo em um totem físico com voz, dados reais e mapa de ônibus? Que etapas de integração, segurança, testes e acessibilidade precisariam ser verificadas antes de afirmar que tudo funciona em produção?"
