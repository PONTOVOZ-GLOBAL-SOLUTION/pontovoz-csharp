# Contrato REST v1 do PontoVoz

**Versão do documento:** B1.3, 07/11/2026. Ele será congelado no bloco B6 (12/11/2026).

> **O que já existe e o que é planejado**
>
> - **Implementado e testado:** apenas `GET /health`.
> - **Planejado:** todos os endpoints em `/api/v1`. Eles **ainda não existem** na API.
>   Os exemplos deste documento e de [`exemplos/`](exemplos/) mostram o formato combinado,
>   não resultados de execução.
> - O documento OpenAPI gerado pela API (`/openapi/v1.json`) descreve apenas o que já foi
>   implementado. Hoje ele contém só o `/health`.
> - Os horários de **10/11/2026** que aparecem nos exemplos são **fixos e só ilustrativos**.
>   A aplicação calcula todos os horários a partir do relógio do servidor (seção [11](#11-requisitos-para-os-próximos-blocos)).

## Sumário

1. [Convenções](#1-convenções)
2. [Política de respostas](#2-política-de-respostas)
3. [Tipos comuns](#3-tipos-comuns)
4. [Consultas públicas](#4-consultas-públicas)
5. [Operações administrativas](#5-operações-administrativas)
6. [Verificação de saúde](#6-verificação-de-saúde)
7. [Catálogo de erros](#7-catálogo-de-erros)
8. [Catálogo de avisos](#8-catálogo-de-avisos)
9. [Conferência das rotas](#9-conferência-das-rotas)
10. [Divergências em relação ao planejamento](#10-divergências-em-relação-ao-planejamento)
11. [Requisitos para os próximos blocos](#11-requisitos-para-os-próximos-blocos)

---

## 1. Convenções

### 1.1 Formato

- Requisições e respostas em JSON, UTF-8 (`application/json; charset=utf-8`).
- Erros em `application/problem+json` (seção [7](#7-catálogo-de-erros)).
- Nomes de campos em **camelCase**, em **português**, **sem acentos nem cedilha**
  (`observadaEm`, `situacao`, `codigoLinha`). Os **valores** de texto podem ter acentos
  (`"Parada Demonstração Centro"`).
- Os textos para pessoas (`mensagem`, `title`, `detail`) estão em português do Brasil.
- Consumidores devem **ignorar campos desconhecidos**. Campos novos podem ser acrescentados
  sem mudar a versão (seção [1.9](#19-versionamento)).

### 1.2 Identificadores

| Campo | Tipo | Uso |
|---|---|---|
| `id`, `paradaId`, `linhaId`, `destinoId` | `integer` (≥ 1) | Identificador interno gerado pelo banco. É usado nas rotas e nos parâmetros. |
| `codigoExterno` (parada) | `string` | Código estável da parada, como `SIM-P001`. É único. |
| `codigo` (linha) | `string` | Código estável da linha, como `SIM-L001`. É único. |
| `letreiro` (linha) | `string` | Texto exibido no ônibus, como `SIM-101`. Serve para exibir e falar, não para identificar. |

**Regra para a equipe:** os IDs internos podem mudar quando o banco é recriado. O totem guarda
o `codigoExterno` da parada e obtém o `id` com a sequência da seção [4.1.1](#411-identificação-da-parada-pelo-totem).

**Dados simulados** sempre usam o prefixo `SIM-` em códigos, letreiros e prefixos de veículo.
O prefixo é reservado ao seed de demonstração. Nenhum código da SPTrans é usado ou imitado.

### 1.3 Datas e fuso horário

- Todo instante é uma `string` no formato **ISO 8601 com deslocamento de fuso explícito**.
- **Saída:** os endpoints de negócio devolvem os instantes no horário de São Paulo
  (`America/Sao_Paulo`, hoje `-03:00`, sem horário de verão). Exemplo: `2026-11-10T14:32:00-03:00`.
- **Armazenamento:** o banco guarda os instantes em UTC. A conversão para São Paulo acontece
  só na saída.
- **Entrada:** só são aceitos instantes com deslocamento (`-03:00`, `+00:00` ou `Z`).
  Um valor sem fuso, como `2026-11-10T14:05:00`, é recusado com 400.
- Cada resposta usa **um único instante de referência** (`consultadoEm`), lido uma vez do
  relógio do servidor (`TimeProvider`). Todos os minutos calculados partem dele.
- **Minutos calculados** (`esperaMinutos`, `minutosDesdeObservacao`) são inteiros
  arredondados **para baixo** e **nunca negativos**. `esperaMinutos: 0` significa "chegando, em
  menos de 1 minuto". As regras de `minutosDesdeObservacao` e `desatualizada` estão em
  [4.6.3](#463-regra-temporal-das-observações).
- O consumidor deve sempre respeitar o deslocamento informado e não presumir o fuso.
  O `/health` já implementado responde em UTC (`+00:00`); veja a [divergência D1](#10-divergências-em-relação-ao-planejamento).

### 1.4 Valores nulos e listas

- Todos os campos documentados **sempre aparecem** na resposta. Quando o valor não existe ou
  não pode ser determinado, o campo vem como `null`, nunca omitido.
- Listas **nunca** são `null`: quando não há itens, vêm como `[]`.
- Na entrada, campos opcionais podem ser omitidos ou enviados como `null`, com o mesmo efeito.

### 1.5 Paginação

As listagens `GET /paradas`, `GET /linhas` e `GET /destinos` são paginadas.

| Parâmetro | Tipo | Padrão | Faixa válida |
|---|---|---|---|
| `pagina` | `integer` | `1` | ≥ 1 |
| `tamanhoPagina` | `integer` | `20` | 1 a 50 |

A resposta segue o envelope `Pagina<T>` (seção [3.1](#31-paginat)). Uma página além do total
devolve **200** com `itens: []`, não 404. Valores fora da faixa devolvem **400**.

### 1.6 Filtros e ordenação

- `termo`: busca por trecho do texto, **sem diferenciar maiúsculas, minúsculas e acentos**
  (`"demonstracao"` encontra `"Demonstração"`). Depois de remover espaços das pontas, deve
  ter de 2 a 60 caracteres. Vazio ou só com espaços conta como ausente.
- `codigoExterno`: comparação **exata**, sem diferenciar maiúsculas e minúsculas.
- Outros filtros são exatos (`linhaId`, `destinoId`). Filtros combinados se somam: o item
  precisa atender a todos.
- **A ordenação é fixa e documentada em cada endpoint.** O MVP não tem parâmetro de
  ordenação. Sempre há um critério final de desempate, para a ordem ser estável.

### 1.7 Enums

Enums são enviados e recebidos **como texto**, em PascalCase e sem acentos
(`"Disponivel"`, `"RegistroManual"`). Um valor desconhecido na entrada devolve 400.

| Enum | Valores |
|---|---|
| `OrigemDado` | `Simulado`: criado pelo seed ou pelo provedor simulado · `CadastroAdministrativo`: cadastrado por uma rota administrativa. Informa só o método de cadastro: não diz se o dado é fictício (veja `dadosSimulados`) nem que foi verificado · `OlhoVivo`: obtido da API Olho Vivo da SPTrans (só depois de integrada e testada) |
| `TipoDestino` | `Terminal`, `Estacao`, `PontoReferencia` |
| `SentidoLinha` | `PrincipalParaSecundario`, `SecundarioParaPrincipal` |
| `StatusPrevisao` | `Disponivel`: há previsão para ao menos uma linha · `SemPrevisoes`: o provedor respondeu sem previsões · `ProvedorIndisponivel`: o provedor não pôde ser consultado (aparece só no resumo e nas alternativas; no endpoint de previsões, a mesma situação devolve 503) |
| `NivelConfirmacao` | `Previsao`, `ObservacaoSimulada`, `RegistroManual`, `VerificadaFonteConfiavel` (reservado, nunca emitido no MVP) |
| `SituacaoAlternativas` | `Comparadas`, `SemComparacao`, `SemLinhaDireta` |
| `SituacaoAlternativa` | `Comparavel`, `SemPrevisao` |
| `CriterioAlternativas` | `MenorTempoTotalEstimado` |
| `SituacaoResumo` | `Completo`, `Parcial` |

### 1.8 Origem e confiabilidade dos dados

Toda resposta de negócio informa de onde vêm os dados:

| Campo | Significado |
|---|---|
| `origem`, `origemPrevisao`, `origemPercurso` | `OrigemDado` do registro, do provedor de previsões ou do percurso estimado |
| `dadosSimulados` | `true` quando o item, ou qualquer dado de que ele depende, é **fictício**: tem origem `Simulado` ou foi cadastrado pela rota administrativa com `dadosSimulados: true`. No nível da resposta, `true` se algum dado da resposta é fictício. |
| `nivelConfirmacao` | Em previsões e passagens: o quanto aquela informação foi confirmada |
| `verificada`, `oficialSptrans` | Em passagens: sempre `false` no MVP |
| `aviso` / `avisos` | Avisos com `codigo` estável e `mensagem` legível (seção [8](#8-catálogo-de-avisos)) |

Três informações diferentes, que não se deduzem uma da outra:

| Dimensão | Pergunta | Campo |
|---|---|---|
| Método de cadastro | Como a informação entrou no sistema? | `origem` (`Simulado`, `CadastroAdministrativo`, `OlhoVivo`) |
| Natureza do dado | É fictício, de demonstração? | `dadosSimulados` |
| Verificação | Foi confirmado por uma fonte confiável? | Passagens: `verificada`, `oficialSptrans` e `nivelConfirmacao`. Cadastros: no MVP **nenhum** é verificado; o aviso `CadastroNaoVerificado` (seção 8) sinaliza isso nas respostas que têm `avisos`. |

Um cadastro administrativo **não** é considerado real nem verificado. O operador declara a
natureza em `dadosSimulados` (5.2 e 5.4), e a verificação continua `false` em qualquer caso.

**Uma previsão, uma observação simulada ou um registro manual nunca é apresentado como passagem
oficialmente confirmada.**

### 1.9 Versionamento

- As rotas de negócio têm o prefixo de versão `/api/v1`. O `/health` é técnico e não tem versão.
- **Até o congelamento (B6, 12/11/2026):** o contrato pode mudar, e cada mudança é registrada
  no [histórico](README.md#histórico-de-mudanças-do-contrato).
- **Depois do congelamento:** só mudanças aditivas (campos, valores de enum documentados,
  parâmetros opcionais). Uma mudança incompatível exigiria `/api/v2`.
- O contrato oficial passa a ser o arquivo `contracts/openapi/pontovoz-v1.json`, exportado da
  API em execução a partir do B6.

### 1.10 Cancelamento e serviços externos

- Toda operação recebe o `CancellationToken` da requisição. Se o cliente desistir, o
  processamento é interrompido e **nenhuma resposta** é enviada. Isso não é tratado como erro.
- O provedor de previsões é o único serviço externo. Ele terá um **tempo limite** definido no B8.
  O provedor está **indisponível** quando falha, não responde no prazo ou está desligado.
- Consequência da indisponibilidade, pela regra da seção [2](#2-política-de-respostas):
  - `GET /previsoes` devolve **503**, porque a previsão é todo o conteúdo dele;
  - `GET /alternativas` e `GET /resumo` devolvem **200** com status explícito e avisos, porque
    continuam tendo informação útil.
- Erros inesperados da própria API, como falha de banco, devolvem **500** em ProblemDetails,
  sem detalhes internos. Desde o B3, o `/health` também indica quando o banco está
  indisponível (seção 6).

### 1.11 Acesso

- **Rotas públicas:** só `GET`, sem autenticação, para o totem e o componente de voz/IA.
- **Rotas administrativas:** prefixo `/api/v1/admin`, protegidas pelo cabeçalho
  `X-PontoVoz-Admin-Key` (seção [5](#5-operações-administrativas)).
- **CORS:** as origens do frontend serão configuradas em `Cors:OrigensPermitidas` (B6).
  A equipe precisa informar a origem que usará em desenvolvimento.

---

## 2. Política de respostas

**Regra geral:** um endpoint devolve **503** quando a dependência indisponível é o seu conteúdo
principal e ele não pode ser atendido. Um endpoint que combina várias fontes devolve **200**
quando ainda tem informação útil, com status explícito e avisos estruturados sobre o que faltou.

| Situação | Resposta |
|---|---|
| Busca ou listagem sem resultados | **200**, `itens: []` |
| Página além do total | **200**, `itens: []` |
| Recurso por `id` inexistente ou **inativo** (rotas públicas) | **404**, ProblemDetails |
| `id` não numérico ou menor que 1 na rota | **404**, a rota não corresponde |
| Parâmetro com tipo, formato ou faixa inválidos | **400**, ProblemDetails com `errors` |
| `linhaId` existe, mas não atende a parada | **422**, `LinhaNaoAtendeParada` |
| Parada sem linhas associadas | **200**, listas vazias e aviso `ParadaSemLinhas`. O provedor não é consultado. |
| **Previsões:** há previsão para ao menos uma linha | **200**, `status: "Disponivel"` (com `PrevisaoParcial` se faltar alguma linha) |
| **Previsões:** o provedor respondeu sem nenhuma previsão | **200**, `status: "SemPrevisoes"` |
| **Previsões:** o provedor está indisponível | **503**, `ProvedorPrevisoesIndisponivel`, com `Retry-After: 30` |
| **Alternativas:** o provedor está indisponível | **200**, `statusPrevisao: "ProvedorIndisponivel"`, `situacao: "SemComparacao"` |
| **Resumo:** o provedor está indisponível | **200**, `situacao: "Parcial"`, `partesIndisponiveis: ["previsoes"]` |
| Linha sem passagem registrada | **200**, a linha aparece em `linhasSemRegistro` |
| Última passagem com mais de 60 minutos | **200**, `desatualizada: true` e aviso `PassagemDesatualizada` |
| Nenhuma linha direta até o destino | **200**, `situacao: "SemLinhaDireta"`, `alternativas: []` |
| Erro inesperado da API | **500**, ProblemDetails genérico |
| Cliente cancelou a requisição | Nenhuma resposta |

Rotas públicas consideram apenas paradas, linhas e destinos **ativos**. Registros inativos
são tratados como inexistentes.

**Ordem das verificações** nos endpoints de uma parada: parâmetros (400), parada (404),
linha ou destino (404), linha atende a parada (422) e, por último, a consulta ao provedor.
Uma requisição inválida nunca chega a consultar o provedor.

---

## 3. Tipos comuns

### 3.1 `Pagina<T>`

| Campo | Tipo | Descrição |
|---|---|---|
| `itens` | `T[]` | Itens da página |
| `pagina` | `integer` | Página devolvida |
| `tamanhoPagina` | `integer` | Tamanho de página aplicado |
| `totalItens` | `integer` | Total de itens que atendem aos filtros |
| `totalPaginas` | `integer` | `ceil(totalItens / tamanhoPagina)`; `0` quando não há itens |

### 3.2 `Aviso`

| Campo | Tipo | Descrição |
|---|---|---|
| `codigo` | `string` | Código estável (seção [8](#8-catálogo-de-avisos)). É o campo que a IA e a interface devem usar para decidir o que fazer. |
| `mensagem` | `string` | Texto em português, pronto para exibir. A IA pode reformular. |

### 3.3 `Parada`

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | `integer` | sim | ID interno |
| `codigoExterno` | `string` | sim | Código estável |
| `nome` | `string` | sim | Nome da parada |
| `endereco` | `string \| null` | não | Endereço. É `null` nos dados simulados, para não associar um ponto fictício a um endereço real. |
| `coordenada` | `Coordenada \| null` | não | Posição geográfica. É `null` nos dados simulados, pelo mesmo motivo. |
| `origem` | `OrigemDado` | sim | Origem do cadastro |
| `dadosSimulados` | `boolean` | sim | `true` se o cadastro é fictício: origem `Simulado` ou cadastro administrativo declarado fictício (1.8) |

`Coordenada`: `{ "latitude": number (-90 a 90), "longitude": number (-180 a 180) }`.

`ParadaDetalhe`: `Parada` mais `linhas: Linha[]`, as linhas ativas que atendem a parada,
ordenadas por `codigo`.

### 3.4 `Linha`

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | `integer` | sim | ID interno |
| `codigo` | `string` | sim | Código estável |
| `letreiro` | `string` | sim | Texto exibido no ônibus |
| `terminalPrincipal` | `string` | sim | Terminal de um extremo da linha |
| `terminalSecundario` | `string` | sim | Terminal do outro extremo |
| `sentido` | `SentidoLinha` | sim | Sentido deste cadastro. Cada sentido é um cadastro de linha separado. |
| `origem` | `OrigemDado` | sim | Origem do cadastro |
| `dadosSimulados` | `boolean` | sim | `true` se o cadastro é fictício: origem `Simulado` ou cadastro administrativo declarado fictício (1.8) |

`ReferenciaLinha`: `{ "linhaId", "codigoLinha", "letreiro" }`. É a forma curta usada dentro
de previsões, passagens e alternativas.

### 3.5 `Destino`

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `integer` | ID interno |
| `nome` | `string` | Nome do destino |
| `tipo` | `TipoDestino` | Categoria |
| `origem` | `OrigemDado` | Origem do cadastro (só `Simulado` no MVP) |
| `dadosSimulados` | `boolean` | `true` se o cadastro é fictício (1.8). No MVP, sempre `true`. |

### 3.6 Cabeçalho das respostas de uma parada

As respostas de previsões, últimas passagens e alternativas começam com:
`paradaId` (`integer`), `codigoExternoParada` (`string`) e `nomeParada` (`string`).
O nome vem junto para a IA e a interface não precisarem de outra chamada.

---

## 4. Consultas públicas

Todas as consultas públicas usam `GET` e não exigem autenticação.

### 4.1 `GET /api/v1/paradas` — buscar paradas

| Item | Definição |
|---|---|
| Finalidade | Buscar paradas ativas por nome ou identificar a parada do totem pelo código |
| Consumidores | Interface do totem (identificação ao iniciar), componente de voz/IA, equipe |
| Query | `termo` (`string`, opcional; busca no nome e no `codigoExterno`) · `codigoExterno` (`string`, opcional; exato, sem diferenciar maiúsculas, `^[A-Za-z0-9-]{3,30}$`) · `pagina` · `tamanhoPagina` |
| Corpo | — |
| Sucesso | **200**, `Pagina<Parada>` |
| Ordenação | `nome`, depois `id` |
| Erros | **400** `RequisicaoInvalida` (`termo` fora de 2 a 60 caracteres, `codigoExterno` fora do formato, paginação fora da faixa) |
| Acesso | Público |
| Dependências | Repositório de paradas (EF Core + LINQ: filtro, projeção, contagem e paginação) |
| Bloco | B4 (consulta) e B6 (endpoint) |

Exemplos: [`exemplos/paradas-listagem.json`](exemplos/paradas-listagem.json), resposta de
`GET /api/v1/paradas?termo=demonstracao` (a ordem é pelo nome, por isso a parada 2 vem antes da 1),
e [`exemplos/paradas-busca-codigo.json`](exemplos/paradas-busca-codigo.json), resposta de
`GET /api/v1/paradas?codigoExterno=SIM-P001`.
Erro: [`exemplos/erro-validacao.json`](exemplos/erro-validacao.json), resposta de
`GET /api/v1/paradas?termo=a&tamanhoPagina=100`.

#### 4.1.1 Identificação da parada pelo totem

O `codigoExterno` é único, então o filtro `codigoExterno` devolve **no máximo um item**.

| Passo | Requisição | Resultado |
|---|---|---|
| 1 | — | O totem é configurado com o código da parada onde está instalado, por exemplo `SIM-P001`. Ele não guarda o `id` interno. |
| 2 | `GET /api/v1/paradas?codigoExterno=SIM-P001` | **200** com `totalItens: 1` ([exemplo](exemplos/paradas-busca-codigo.json)) |
| 3 | — | O totem lê `itens[0].id` (no exemplo, `1`) e o guarda em memória até reiniciar |
| 4 | `GET /api/v1/paradas/1/resumo`, `/previsoes`, `/ultimas-passagens` ou `/alternativas?destinoId=` | Consultas da parada pelo `id` |

Situações de falha:
- `totalItens: 0`: o código não existe ou a parada está inativa. O totem mostra uma mensagem de
  configuração, sem inventar dados.
- **400**: o código configurado está fora do formato.
- Uma consulta do passo 4 que devolver **404** indica que o banco foi recriado ou a parada foi
  desativada. O totem repete o passo 2 uma vez antes de mostrar o erro.

### 4.2 `GET /api/v1/paradas/{id}` — detalhe da parada

| Item | Definição |
|---|---|
| Finalidade | Dados da parada e as linhas que a atendem |
| Consumidores | Interface do totem, componente de voz/IA |
| Rota | `id` (`integer` ≥ 1) |
| Query | — |
| Sucesso | **200**, `ParadaDetalhe` |
| Erros | **404** `ParadaNaoEncontrada` (inexistente ou inativa) |
| Acesso | Público |
| Dependências | Repositório de paradas (consulta com as linhas associadas) |
| Bloco | B4 e B6 |

Exemplo: [`exemplos/parada-detalhe.json`](exemplos/parada-detalhe.json), resposta de `GET /api/v1/paradas/1`.

### 4.3 `GET /api/v1/linhas` — buscar linhas

| Item | Definição |
|---|---|
| Finalidade | Localizar uma linha pelo que a pessoa falou ou digitou ("a 104", "a que vai para o Norte") |
| Consumidores | Componente de voz/IA, interface do totem |
| Query | `termo` (`string`, opcional; busca em `codigo`, `letreiro`, `terminalPrincipal` e `terminalSecundario`) · `pagina` · `tamanhoPagina` |
| Sucesso | **200**, `Pagina<Linha>` |
| Ordenação | `codigo`, depois `id` |
| Erros | **400** `RequisicaoInvalida` |
| Acesso | Público |
| Dependências | Repositório de linhas |
| Bloco | B4 e B6 |

Exemplo de `GET /api/v1/linhas?termo=104`:

```json
{
  "itens": [
    {
      "id": 4,
      "codigo": "SIM-L004",
      "letreiro": "SIM-104",
      "terminalPrincipal": "Terminal Demonstração Centro",
      "terminalSecundario": "Terminal Demonstração Norte",
      "sentido": "PrincipalParaSecundario",
      "origem": "Simulado",
      "dadosSimulados": true
    }
  ],
  "pagina": 1,
  "tamanhoPagina": 20,
  "totalItens": 1,
  "totalPaginas": 1
}
```

### 4.4 `GET /api/v1/destinos` — localizar destinos

| Item | Definição |
|---|---|
| Finalidade | Transformar "quero ir para o Terminal Norte" em um `destinoId` |
| Consumidores | Componente de voz/IA (principal), interface do totem |
| Query | `termo` (`string`, opcional; busca no `nome`) · `pagina` · `tamanhoPagina` |
| Sucesso | **200**, `Pagina<Destino>` |
| Ordenação | `nome`, depois `id` |
| Erros | **400** `RequisicaoInvalida` |
| Acesso | Público |
| Dependências | Repositório de destinos. No MVP os destinos vêm só do seed simulado. |
| Bloco | B4 e B6 |

Exemplo: [`exemplos/destinos-busca.json`](exemplos/destinos-busca.json), resposta de
`GET /api/v1/destinos?termo=norte`. Se a busca devolver mais de um destino, a IA pede à
pessoa que escolha. Se não devolver nenhum, a IA informa que o destino não está cadastrado.

### 4.5 `GET /api/v1/paradas/{id}/previsoes` — previsões de chegada

| Item | Definição |
|---|---|
| Finalidade | Próximas chegadas previstas nas linhas da parada |
| Consumidores | Interface do totem, componente de voz/IA |
| Rota | `id` (`integer` ≥ 1) |
| Query | `linhaId` (`integer` ≥ 1, opcional): limita a uma linha |
| Sucesso | **200**, `PrevisoesParada` com `status` `Disponivel` ou `SemPrevisoes` |
| Ordenação | `previsoes` por `previstaPara`, depois `codigoLinha` · `linhasSemPrevisao` por `codigoLinha` |
| Erros | **400** `RequisicaoInvalida` · **404** `ParadaNaoEncontrada`, `LinhaNaoEncontrada` · **422** `LinhaNaoAtendeParada` · **503** `ProvedorPrevisoesIndisponivel` |
| Acesso | Público |
| Dependências | `IProvedorPrevisaoChegada` (simulado por padrão; Olho Vivo desligado) e repositório de paradas |
| Bloco | B8 |

**`PrevisoesParada`**

| Campo | Tipo | Descrição |
|---|---|---|
| `paradaId`, `codigoExternoParada`, `nomeParada` | | Cabeçalho (3.6) |
| `status` | `StatusPrevisao` | Neste endpoint: `Disponivel` ou `SemPrevisoes`. `ProvedorIndisponivel` só aparece quando o mesmo objeto vem dentro do resumo. |
| `origem` | `OrigemDado` | Provedor consultado: `Simulado` ou `OlhoVivo` |
| `dadosSimulados` | `boolean` | `true` se a previsão ou a parada são simuladas |
| `consultadoEm` | `string (date-time)` | Instante da consulta |
| `previsoes` | `Previsao[]` | Chegadas previstas, podendo haver mais de uma por linha. `[]` se não houver. |
| `linhasSemPrevisao` | `ReferenciaLinha[]` | Linhas consultadas sem previsão. Com `SemPrevisoes` ou `ProvedorIndisponivel`, traz todas as linhas consultadas. |
| `avisos` | `Aviso[]` | Avisos da consulta |

**`Previsao`**: `linhaId`, `codigoLinha`, `letreiro`, `previstaPara` (`date-time`),
`esperaMinutos` (`integer` ≥ 0), `prefixoVeiculo` (`string | null`) e
`nivelConfirmacao` (sempre `Previsao`).

| Situação | Status HTTP | Corpo |
|---|---|---|
| Previsão para todas as linhas | 200 | `status: "Disponivel"`, `linhasSemPrevisao: []` |
| Previsão para parte das linhas | 200 | `status: "Disponivel"`, `linhasSemPrevisao` preenchida, aviso `PrevisaoParcial` |
| O provedor respondeu sem previsões | 200 | `status: "SemPrevisoes"`, `previsoes: []`, aviso `SemPrevisoes` |
| A parada não tem linhas | 200 | `status: "SemPrevisoes"`, listas vazias, aviso `ParadaSemLinhas` |
| O provedor está indisponível | 503 | ProblemDetails `ProvedorPrevisoesIndisponivel`, cabeçalho `Retry-After: 30` |

Exemplos: [`exemplos/previsoes.json`](exemplos/previsoes.json) (200, previsão parcial) e
[`exemplos/erro-provedor-previsoes-indisponivel.json`](exemplos/erro-provedor-previsoes-indisponivel.json) (503).

Regras:
- Uma previsão é uma estimativa, **não** uma passagem. Ela nunca aparece em `ultimas-passagens`.
- Previsões já vencidas (`previstaPara` antes de `consultadoEm`) não são devolvidas.
- O MVP não copia nem inventa previsões da SPTrans. O provedor simulado gera horários fictícios,
  marcados como `Simulado` e calculados a partir do relógio do servidor (requisito R1, seção 11).
  O adaptador Olho Vivo continua desligado até ser testado com credenciais
  (veja [`../docs/dados.md`](../docs/dados.md)).

### 4.6 `GET /api/v1/paradas/{id}/ultimas-passagens` — última passagem por linha

| Item | Definição |
|---|---|
| Finalidade | O registro de passagem mais recente de cada linha da parada, com o nível de confirmação e a indicação de desatualização |
| Consumidores | Interface do totem, componente de voz/IA |
| Rota | `id` (`integer` ≥ 1) |
| Query | `linhaId` (`integer` ≥ 1, opcional) |
| Sucesso | **200**, `UltimasPassagensParada`, inclusive sem nenhum registro |
| Ordenação | `passagens` por `observadaEm` do mais recente para o mais antigo, depois `codigoLinha` · `linhasSemRegistro` por `codigoLinha` |
| Erros | **400** `RequisicaoInvalida` · **404** `ParadaNaoEncontrada`, `LinhaNaoEncontrada` · **422** `LinhaNaoAtendeParada` |
| Acesso | Público |
| Dependências | Repositório de passagens (LINQ com `GroupBy` por linha e o maior `observadaEm`) |
| Bloco | B4 e B6 |

**`UltimasPassagensParada`**: cabeçalho (3.6), `dadosSimulados`, `consultadoEm`,
`limiteDesatualizacaoMinutos` (`integer`, `60` no MVP), `passagens: Passagem[]`,
`linhasSemRegistro: ReferenciaLinha[]` e `avisos: Aviso[]`.

**`Passagem`**

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `integer` | ID do registro |
| `linhaId`, `codigoLinha`, `letreiro` | | Linha |
| `observadaEm` | `string (date-time)` | Quando o ônibus foi visto |
| `registradaEm` | `string (date-time)` | Quando o registro entrou no sistema |
| `minutosDesdeObservacao` | `integer` ≥ 0 | Minutos inteiros transcorridos entre `observadaEm` e `consultadoEm`, arredondados para baixo e nunca negativos (4.6.3) |
| `desatualizada` | `boolean` | `true` quando o intervalo **exato** transcorrido é maior que `limiteDesatualizacaoMinutos` (4.6.3) |
| `prefixoVeiculo` | `string \| null` | Identificação do veículo, quando informada |
| `nivelConfirmacao` | `NivelConfirmacao` | `ObservacaoSimulada` ou `RegistroManual` no MVP |
| `verificada` | `boolean` | Sempre `false` no MVP |
| `oficialSptrans` | `boolean` | Sempre `false` no MVP |
| `origemRegistro` | `string \| null` | Rótulo técnico de quem registrou (como `operador-local`). É `null` em observações simuladas. |
| `dadosSimulados` | `boolean` | `true` se o registro, a linha ou a parada são simulados |
| `aviso` | `Aviso` | Sempre presente. Explica o nível de confirmação. |

Exemplo: [`exemplos/ultimas-passagens.json`](exemplos/ultimas-passagens.json). Ele mostra
os três casos ao mesmo tempo:
- um registro manual recente (SIM-L004, há 8 minutos, `desatualizada: false`), criado pela
  operação administrativa da seção 5.6;
- uma observação simulada antiga (SIM-L002), criada pelo seed na primeira execução, em
  08/11/2026. O horário dela nunca é renovado, por isso aparece com `desatualizada: true`;
- uma linha sem registro (SIM-L001, em `linhasSemRegistro`).

Os horários são fictícios e fixos só nos exemplos.

#### 4.6.1 Regras de passagem

As cinco regras são independentes e cada uma pode ser testada isoladamente com um relógio
controlado (`FakeTimeProvider`).

| Regra | Onde vale | Definição |
|---|---|---|
| **Validação de um novo registro manual** | `POST /admin/observacoes-passagem` | `observadaEm` precisa estar entre **24 horas antes** e **2 minutos depois** do instante do servidor. Fora disso: 400. |
| **Persistência histórica** | Banco de dados | Todo registro aceito é guardado e **não é apagado nem alterado** no MVP. Não há expiração automática. Isso vale também para as observações simuladas do provisionamento: o horário delas não muda quando a API reinicia (4.6.2). |
| **Consulta da última passagem** | `GET /ultimas-passagens` e resumo | Para cada linha, o registro com o maior `observadaEm` **até `consultadoEm`**, sem limite para o passado. Um registro com `observadaEm` ainda no futuro não entra na consulta (4.6.3). Em empate, o maior `registradaEm` e, depois, o maior `id`. |
| **Indicação de desatualização** | Mesma consulta | `desatualizada = (consultadoEm − observadaEm) > 60 minutos`, com o intervalo exato (4.6.3). A resposta traz o aviso `PassagemDesatualizada` quando algum item estiver desatualizado. |
| **Ausência de registros** | Mesma consulta | A linha sem nenhum registro vai para `linhasSemRegistro`, com o aviso `PassagemNaoRegistrada`. Sem registro em nenhuma linha, a resposta é 200 com `passagens: []`. |

Por que essa combinação:
- A consulta não esconde registros antigos. Em vez disso, ela **marca** a desatualização. Assim a
  pessoa sabe que existe um registro, mas que ele é velho, e nada some sem explicação.
- O limite de 24 horas vale só para **criar** um registro manual. Ele impede o cadastro de
  observações que já nasceriam muito antigas. Um registro aceito continua no histórico depois
  desse prazo e aparece marcado como desatualizado.
- `VerificadaFonteConfiavel` existe no enum, mas **nenhum fluxo do MVP cria esse nível**.
  `verificada` e `oficialSptrans` são sempre `false`.

#### 4.6.2 Dados simulados de passagem

Há três situações diferentes, e só a primeira cria passagens automaticamente:

| Situação | Quando acontece | O que o seed faz com as passagens |
|---|---|---|
| **1. Provisionamento inicial** | O banco é criado vazio (por exemplo, na primeira execução ou depois que o desenvolvedor apaga o banco de propósito) | Cria, na mesma transação, os cadastros de demonstração e as **observações simuladas iniciais**, com horário relativo ao instante do provisionamento |
| **2. Inicialização posterior** | A API inicia com um banco já provisionado | **Nada.** Não cria passagens, não muda horários e não apaga registros. Os cadastros de referência que faltarem (por exemplo, um destino acrescentado ao seed) podem ser criados, mas nunca uma passagem. |
| **3. Registro manual na demonstração** | Um operador chama `POST /admin/observacoes-passagem` (5.6) | Não envolve o seed. É o **único** caminho para uma nova passagem depois do provisionamento. |

| Item | Regra |
|---|---|
| **Idempotência** | O seed é idempotente. Cada dado do seed tem uma chave estável: o `codigoExterno` nas paradas e linhas e uma chave interna fixa em cada observação simulada. Antes de criar, o seed confere a chave e não duplica nada. Executar o seed de novo, em qualquer momento, não muda o banco. |
| **Quando o banco conta como vazio** | Quando não existe nenhuma parada do seed. Só nesse caso as observações simuladas são criadas. Uma observação simulada que falte num banco já provisionado **não** é recriada. |
| **Horário das observações simuladas** | Calculado uma única vez, no provisionamento (por exemplo, 15 minutos antes dele). `registradaEm` é o instante do provisionamento. Depois disso, o horário é histórico e **nunca é atualizado**. |
| **Rastreabilidade** | Toda passagem guarda `registradaEm`, `nivelConfirmacao` e a chave interna (simulada) ou `origemRegistro` (manual). Dá para saber como e quando cada uma entrou no banco. |
| **Reinício da API** | Igual à situação 2. Nenhum evento fictício nasce só porque a aplicação foi iniciada. Apagar o banco é uma ação manual do desenvolvedor, nunca automática. |
| **Registros antigos** | Continuam visíveis e marcados como `desatualizada: true` pela regra de 60 minutos. |
| **Previsões simuladas** | Não são persistidas. O provedor simulado calcula cada previsão no momento da consulta, a partir do `TimeProvider` (R1). Por isso as previsões são sempre futuras, mesmo com um banco antigo. |
| **Passagem recente na demonstração** | Registrada pela operação administrativa `POST /admin/observacoes-passagem` (5.6). O registro sai com `nivelConfirmacao: "RegistroManual"`, `origemRegistro` preenchido e o aviso `RegistroManualNaoVerificado`. |
| **Nunca é oficial** | Nem a observação simulada nem o registro manual aparecem como passagem verificada: `verificada` e `oficialSptrans` são sempre `false`, e o nível `VerificadaFonteConfiavel` não é emitido. |

#### 4.6.3 Regra temporal das observações

`intervalo = referência − observadaEm`, calculado com os instantes completos, sem arredondar.
A referência é `consultadoEm` nas consultas e `registradaEm` na resposta do `POST` (5.6).

| Regra | Definição |
|---|---|
| **Janela de criação** | No `POST`, aceita `observadaEm` com `intervalo` entre **−2 minutos** (tolerância de relógio) e **+24 horas**, inclusive. Fora disso: 400. |
| **Minutos** | `minutosDesdeObservacao = max(0, piso(intervalo em minutos))`. Nunca é negativo. |
| **Desatualização** | `desatualizada = intervalo > 60 minutos`, comparando o **intervalo exato**, não os minutos arredondados. Com 60 min e 1 s, os minutos mostram `60` e `desatualizada` já é `true`. Com intervalo negativo, é `false`. |
| **Observação ainda no futuro** | Um registro aceito pela tolerância, com `observadaEm` depois da referência, **não entra** em `ultimas-passagens` nem no resumo até o horário dele chegar (no máximo 2 minutos). Até lá, a linha segue com o registro anterior ou em `linhasSemRegistro`. O horário gravado não é corrigido. |
| **Resposta do `POST`** | Com `observadaEm` no futuro, a resposta traz `minutosDesdeObservacao: 0` e `desatualizada: false`. É o recibo do cadastro, com `nivelConfirmacao: "RegistroManual"` e `verificada: false`, e não uma passagem confirmada. |

Casos de teste da janela de criação (`intervalo` em segundos, no instante do `POST`):

| Caso | Intervalo (s) | Resultado do POST |
|---|---|---|
| J1 | -121 | 400 |
| J2 | -120 | 201 |
| J3 | 0 | 201 |
| J4 | 86400 | 201 |
| J5 | 86401 | 400 |

Casos de teste do cálculo (`intervalo` em segundos, na referência):

| Caso | Intervalo (s) | minutosDesdeObservacao | desatualizada | Aparece em ultimas-passagens |
|---|---|---|---|---|
| T1 | -120 | 0 | false | não |
| T2 | -1 | 0 | false | não |
| T3 | 0 | 0 | false | sim |
| T4 | 59 | 0 | false | sim |
| T5 | 3600 | 60 | false | sim |
| T6 | 3601 | 60 | true | sim |
| T7 | 3660 | 61 | true | sim |

Esses casos viram testes de domínio e de serviço com `FakeTimeProvider` (B2 e B5).

### 4.7 `GET /api/v1/paradas/{id}/alternativas` — alternativas até um destino

| Item | Definição |
|---|---|
| Finalidade | Comparar as linhas **diretas** da parada até um destino cadastrado |
| Consumidores | Componente de voz/IA (principal), interface do totem |
| Rota | `id` (`integer` ≥ 1) |
| Query | `destinoId` (`integer` ≥ 1, **obrigatório**) |
| Sucesso | **200**, `AlternativasDeslocamento`, inclusive quando o provedor de previsões está indisponível |
| Erros | **400** `RequisicaoInvalida` (`destinoId` ausente ou inválido) · **404** `ParadaNaoEncontrada`, `DestinoNaoEncontrado` |
| Acesso | Público |
| Dependências | `IProvedorPrevisaoChegada`, repositório de percursos estimados (seed) e o serviço de alternativas. A consulta ao provedor e ao banco acontecem em paralelo com `Task.WhenAll`. |
| Bloco | B9 |

**Cálculo:**

```
tempoTotalEstimadoMinutos = esperaMinutos (próxima chegada da linha) + percursoEstimadoMinutos
chegadaEstimadaAoDestino  = previstaPara + percursoEstimadoMinutos
```

Uma linha entra na lista quando atende a parada **e** tem um percurso estimado cadastrado até o
destino. Se não houver percurso cadastrado, a linha não é considerada uma rota até o destino.
Só são considerados trajetos **diretos** cadastrados, sem baldeação.

**Ordenação e linhas sem previsão:**

1. Primeiro as linhas com `situacao: "Comparavel"`, em ordem crescente de
   `tempoTotalEstimadoMinutos`. Em empate, a menor `esperaMinutos` vem antes e, depois, o `codigoLinha`.
   Recebem `posicao` 1, 2, 3...
2. Depois as linhas com `situacao: "SemPrevisao"`, com `posicao: null`, `previstaPara: null`,
   `esperaMinutos: null`, `tempoTotalEstimadoMinutos: null` e `chegadaEstimadaAoDestino: null`.
   O `percursoEstimadoMinutos` continua informado, porque não depende do provedor. Entre elas, a
   ordem é pelo menor percurso e, depois, pelo `codigoLinha`.
3. **Uma linha sem previsão nunca recebe posição** e nunca é chamada de mais rápida.
4. `comparacaoCompleta` é `true` **somente** quando `situacao` é `Comparadas` e todas as linhas
   da lista são comparáveis. Em qualquer outro caso é `false`, inclusive com a lista vazia.
5. Com `comparacaoCompleta: false`, a posição 1 é apenas a mais rápida **entre as linhas com
   previsão**. A resposta traz o aviso `ComparacaoIncompleta`, e a fala não deve dizer que ela é
   a mais rápida em termos absolutos.

**Situação geral (`situacao`)**

| `situacao` | Quando | `alternativas` | Avisos principais |
|---|---|---|---|
| `Comparadas` | Ao menos uma linha comparável | Comparáveis com posição, depois as sem previsão | `ComparacaoIncompleta`, se houver linha sem previsão |
| `SemComparacao` | Há linhas diretas, mas nenhuma tem previsão (provedor sem previsões ou indisponível) | Todas com `posicao: null` | `SemComparacao` e, conforme o caso, `SemPrevisoes` ou `ProvedorIndisponivel` |
| `SemLinhaDireta` | Nenhuma linha direta cadastrada até o destino | `[]` | `SemLinhaDireta` |

**`AlternativasDeslocamento`**

| Campo | Tipo | Descrição |
|---|---|---|
| `paradaId`, `codigoExternoParada`, `nomeParada` | | Cabeçalho (3.6) |
| `destino` | `{ id, nome, tipo }` | Destino consultado |
| `criterio` | `CriterioAlternativas` | Sempre `MenorTempoTotalEstimado` no MVP |
| `situacao` | `SituacaoAlternativas` | Ver tabela acima |
| `comparacaoCompleta` | `boolean` | Ver regra 4 |
| `statusPrevisao` | `StatusPrevisao` | Resultado da consulta ao provedor, inclusive `ProvedorIndisponivel`. Com `SemLinhaDireta`, o provedor não é consultado e o valor é `null`. |
| `origemPrevisao` | `OrigemDado \| null` | Provedor consultado. `null` quando não foi consultado. |
| `dadosSimulados` | `boolean` | `true` se algum dado usado é simulado |
| `consultadoEm` | `string (date-time)` | Instante de referência do cálculo |
| `alternativas` | `Alternativa[]` | Ver ordenação |
| `avisos` | `Aviso[]` | Sempre inclui `LimitacaoLinhasDiretas`. Com percursos, inclui `PercursoEstimado`. |

**`Alternativa`**: `posicao` (`integer | null`), `linhaId`, `codigoLinha`, `letreiro`,
`situacao` (`SituacaoAlternativa`), `previstaPara` (`date-time | null`), `esperaMinutos`
(`integer | null`), `percursoEstimadoMinutos` (`integer`, de 1 a 240),
`tempoTotalEstimadoMinutos` (`integer | null`), `chegadaEstimadaAoDestino` (`date-time | null`),
`baldeacoes` (`integer`, sempre `0` no MVP) e `origemPercurso` (`OrigemDado`).

Os tempos de percurso do MVP vêm do seed, com `origemPercurso: "Simulado"` em cada item e o
aviso `PercursoEstimado` deixando claro que são simulados.

Exemplos:
- [`exemplos/alternativas.json`](exemplos/alternativas.json), resposta de
  `GET /api/v1/paradas/1/alternativas?destinoId=3`. Ele mostra a regra principal: a linha
  SIM-L002 chega em 1 minuto mas leva 40 até o destino, então fica **atrás** da SIM-L004, que
  chega em 8 e leva 20. A SIM-L001 não tem previsão e aparece por último, sem posição. Por isso
  `comparacaoCompleta` é `false`.
- [`exemplos/alternativas-sem-comparacao.json`](exemplos/alternativas-sem-comparacao.json): a
  mesma consulta com o provedor indisponível. As três linhas aparecem com o percurso, mas sem
  posição, e nenhuma é indicada como mais rápida.

Fora do MVP: baldeações, outros meios de transporte, trânsito em tempo real e rotas que não
estejam cadastradas. Nenhuma rota é inventada.

### 4.8 `GET /api/v1/paradas/{id}/resumo` — resumo para o totem

| Item | Definição |
|---|---|
| Finalidade | Tudo o que a tela inicial do totem precisa, em uma chamada |
| Consumidores | Interface do totem (principal), componente de voz/IA |
| Rota | `id` (`integer` ≥ 1) |
| Query | — |
| Sucesso | **200**, `ResumoParada`, **mesmo com o provedor de previsões indisponível** |
| Erros | **404** `ParadaNaoEncontrada` |
| Acesso | Público |
| Dependências | Os mesmos serviços de 4.2, 4.5 e 4.6. Provedor e banco são consultados em paralelo com `Task.WhenAll`; as consultas ao banco são feitas em sequência no mesmo `DbContext`. |
| Bloco | B6 (parcial) e B9 (completo) |

**`ResumoParada`**

| Campo | Tipo | Descrição |
|---|---|---|
| `consultadoEm` | `string (date-time)` | Instante único usado em todas as partes |
| `dadosSimulados` | `boolean` | `true` se alguma parte é simulada |
| `situacao` | `SituacaoResumo` | `Completo`: todas as partes foram obtidas · `Parcial`: alguma parte ficou indisponível |
| `partesIndisponiveis` | `string[]` | Nomes das partes indisponíveis. No MVP, só pode conter `"previsoes"`. `[]` quando completo. |
| `parada` | `ParadaDetalhe` | Igual à resposta de 4.2 |
| `previsoes` | `PrevisoesParada` | Mesmo formato da resposta de 4.5. Aqui, a indisponibilidade aparece como `status: "ProvedorIndisponivel"` em vez de 503. |
| `ultimasPassagens` | `UltimasPassagensParada` | Igual à resposta de 4.6 |
| `avisos` | `Aviso[]` | Avisos sobre o resumo como um todo. Os avisos de cada parte ficam dentro dela. |

As partes têm **o mesmo formato** das respostas individuais, então a interface pode usar o mesmo
código para ler as duas.

Exemplos: [`exemplos/resumo.json`](exemplos/resumo.json) (`situacao: "Completo"`) e
[`exemplos/resumo-provedor-indisponivel.json`](exemplos/resumo-provedor-indisponivel.json)
(`situacao: "Parcial"`, `partesIndisponiveis: ["previsoes"]` e aviso `ResumoParcial`). No
segundo, a parada, as linhas e as últimas passagens continuam disponíveis.

---

## 5. Operações administrativas

Usadas pelo operador da demonstração para cadastrar dados e registrar observações. **Não são
usadas pelo totem nem pela IA.**

### 5.1 Chave administrativa

Toda rota em `/api/v1/admin` passa pelo filtro `ChaveAdministrativaFilter` (B7):

| Ambiente | Chave configurada (`Admin:Chave`)? | Resultado |
|---|---|---|
| Development | não | Permitido, com aviso no log a cada inicialização |
| Development | sim | Exige o cabeçalho. Sem ele ou com valor errado: **401** |
| Outros | não | **403** `OperacoesAdministrativasDesabilitadas` |
| Outros | sim | Exige o cabeçalho. Sem ele ou com valor errado: **401** |

- Cabeçalho: `X-PontoVoz-Admin-Key: <CHAVE_ADMINISTRATIVA_LOCAL>`. O valor entre `< >` é um
  **marcador**, não uma chave real.
- A chave fica em user-secrets ou em variável de ambiente, **nunca** em arquivo versionado.
- A comparação usa tempo constante (`CryptographicOperations.FixedTimeEquals`).
- A resposta 401 inclui `WWW-Authenticate: ApiKey header="X-PontoVoz-Admin-Key"`.
- A verificação da chave acontece **antes** da validação do corpo. Uma requisição sem
  autorização recebe 401 ou 403, nunca 400.
- Limitações do MVP: chave única e compartilhada, sem identidade individual nem auditoria,
  troca manual e sem limite de requisições.

Erros comuns a todas as rotas administrativas: **401** `ChaveAdministrativaInvalida` e
**403** `OperacoesAdministrativasDesabilitadas`.

### 5.2 `POST /api/v1/admin/paradas` — cadastrar parada

| Item | Definição |
|---|---|
| Finalidade | Cadastrar uma parada |
| Consumidor | Operador |
| Corpo | `codigoExterno` (`string`, obrigatório, `^[A-Z0-9-]{3,30}$`) · `nome` (`string`, obrigatório, 3 a 120 caracteres) · `endereco` (`string \| null`, até 200) · `coordenada` (`Coordenada \| null`) · `dadosSimulados` (`boolean`, **obrigatório**, sem valor padrão) |
| Sucesso | **201**, `ParadaAdministrativa`, com `Location: /api/v1/paradas/{id}` |
| Erros | **400** `RequisicaoInvalida` · **401** · **403** · **409** `CodigoJaCadastrado` |
| Regras | A parada nasce ativa, com `origem: "CadastroAdministrativo"` e o `dadosSimulados` informado, que não muda depois. O operador precisa declarar se o cadastro é fictício; a API não presume nenhum dos dois. As respostas públicas que têm `avisos` passam a incluir `CadastroNaoVerificado` quando envolvem essa parada (seção 8). Um código com o prefixo reservado `SIM-` é recusado com 400. |
| Bloco | B7 |

`ParadaAdministrativa` = `Parada` mais `ativa` (`boolean`) e `criadoEm` (`date-time`).

O exemplo usa o prefixo `TESTE-`, porque `SIM-` é reservado para os dados do seed (`origem: "Simulado"`).
Como é um cadastro fictício, ele é enviado com `dadosSimulados: true`, e a resposta mantém
`origem: "CadastroAdministrativo"` (o método) separado de `dadosSimulados: true` (a natureza).

```json
{
  "codigoExterno": "TESTE-P003",
  "nome": "Parada Cadastro de Teste",
  "endereco": null,
  "coordenada": null,
  "dadosSimulados": true
}
```

Resposta **201**:

```json
{
  "id": 3,
  "codigoExterno": "TESTE-P003",
  "nome": "Parada Cadastro de Teste",
  "endereco": null,
  "coordenada": null,
  "origem": "CadastroAdministrativo",
  "dadosSimulados": true,
  "ativa": true,
  "criadoEm": "2026-11-10T14:10:00-03:00"
}
```

### 5.3 `PUT /api/v1/admin/paradas/{id}` — atualizar parada

| Item | Definição |
|---|---|
| Finalidade | Atualizar os dados editáveis e ativar ou desativar a parada |
| Consumidor | Operador |
| Rota | `id` (`integer` ≥ 1) |
| Corpo | Substituição completa: `nome` (obrigatório) · `endereco` (`string \| null`) · `coordenada` (`Coordenada \| null`) · `ativa` (`boolean`, obrigatório) |
| Sucesso | **200**, `ParadaAdministrativa` |
| Erros | **400** · **401** · **403** · **404** `ParadaNaoEncontrada` (só quando não existe; paradas inativas podem ser atualizadas) |
| Regras | O `codigoExterno` e o `dadosSimulados` não mudam. Para trocá-los, cadastre outra parada. Desativar substitui a exclusão: o histórico de passagens é preservado. |
| Bloco | B7 |

```json
{
  "nome": "Parada Cadastro de Teste",
  "endereco": null,
  "coordenada": null,
  "ativa": false
}
```

### 5.4 `POST /api/v1/admin/linhas` — cadastrar linha

| Item | Definição |
|---|---|
| Finalidade | Cadastrar uma linha em um sentido |
| Consumidor | Operador |
| Corpo | `codigo` (obrigatório, `^[A-Z0-9-]{3,30}$`) · `letreiro` (obrigatório, 1 a 30) · `terminalPrincipal` e `terminalSecundario` (obrigatórios, 3 a 120) · `sentido` (`SentidoLinha`, obrigatório) · `dadosSimulados` (`boolean`, **obrigatório**, sem valor padrão) |
| Sucesso | **201**, `Linha` mais `ativa` e `criadoEm`. Sem `Location`, porque o MVP não tem consulta pública de linha por `id`. |
| Erros | **400** · **401** · **403** · **409** `CodigoJaCadastrado` |
| Regras | A linha nasce ativa, com `origem: "CadastroAdministrativo"` e o `dadosSimulados` informado, como na parada (5.2). Códigos e letreiros com o prefixo reservado `SIM-` são recusados com 400. |
| Bloco | B7 |

```json
{
  "codigo": "TESTE-L005",
  "letreiro": "TESTE-105",
  "terminalPrincipal": "Terminal Demonstração Centro",
  "terminalSecundario": "Terminal Demonstração Sul",
  "sentido": "PrincipalParaSecundario",
  "dadosSimulados": true
}
```

### 5.5 `PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}` — associar linha à parada

| Item | Definição |
|---|---|
| Finalidade | Indicar que a linha atende a parada |
| Consumidor | Operador |
| Rota | `id` e `linhaId` (`integer` ≥ 1) |
| Corpo | — |
| Sucesso | **204**. É idempotente: repetir a chamada também devolve 204, sem duplicar. |
| Erros | **401** · **403** · **404** `ParadaNaoEncontrada`, `LinhaNaoEncontrada` · **422** `ParadaInativa`, `LinhaInativa` |
| Bloco | B7 |

### 5.6 `POST /api/v1/admin/observacoes-passagem` — registrar observação manual

| Item | Definição |
|---|---|
| Finalidade | Registrar que um ônibus de uma linha foi visto na parada |
| Consumidor | Operador |
| Corpo | `paradaId` (obrigatório) · `linhaId` (obrigatório) · `observadaEm` (`date-time`, obrigatório, com fuso) · `prefixoVeiculo` (`string \| null`, `^[A-Za-z0-9-]{1,20}$`) · `origemRegistro` (obrigatório, `^[a-z0-9-]{3,40}$`) |
| Sucesso | **201**, `Passagem` mais `paradaId`. Sem `Location`, porque cada observação não tem consulta própria; ela passa a aparecer em `ultimas-passagens`. |
| Erros | **400** · **401** · **403** · **404** `ParadaNaoEncontrada`, `LinhaNaoEncontrada` · **422** `LinhaNaoAtendeParada`, `ParadaInativa`, `LinhaInativa` |
| Bloco | B7 |

Regras de validação (veja também a seção [4.6.1](#461-regras-de-passagem)):
- `observadaEm` precisa estar entre **24 horas antes** e **2 minutos depois** do instante do
  servidor. A tolerância de 2 minutos cobre diferenças de relógio. Fora disso: 400. O cálculo
  de minutos e o tratamento de um horário ainda no futuro estão em [4.6.3](#463-regra-temporal-das-observações).
- `origemRegistro` é um **rótulo técnico**, como `operador-local`. O padrão aceita só letras
  minúsculas, dígitos e hífen, então nome, e-mail ou telefone de uma pessoa não passam na
  validação.
- O registro recebe `nivelConfirmacao: "RegistroManual"`, `verificada: false` e
  `oficialSptrans: false`. Não há como uma rota do MVP criar outro nível.

Requisição: [`exemplos/admin-observacao-passagem-requisicao.json`](exemplos/admin-observacao-passagem-requisicao.json).
Resposta: [`exemplos/admin-observacao-passagem-resposta.json`](exemplos/admin-observacao-passagem-resposta.json).
Na resposta, `minutosDesdeObservacao` é contado até o instante do registro (14h25min10s), por isso
vale 1. O mesmo registro aparece em `ultimas-passagens.json` com 8 minutos, contados até a
consulta das 14h32. Nos dois casos `desatualizada` é `false`.

Esta é a forma de mostrar uma passagem recente na demonstração (4.6.2): as observações
simuladas do seed mantêm o horário original e ficam desatualizadas com o tempo.

Exemplo com curl, válido a partir do B7:

```bash
curl -X POST http://localhost:5080/api/v1/admin/observacoes-passagem -H "Content-Type: application/json" -H "X-PontoVoz-Admin-Key: <CHAVE_ADMINISTRATIVA_LOCAL>" -d @contracts/exemplos/admin-observacao-passagem-requisicao.json
```

O corpo de exemplo tem horário fixo de 10/11/2026. Para executar de verdade, troque `observadaEm`
por um horário das últimas 24 horas.

---

## 6. Verificação de saúde

### `GET /health` — **implementado**

| Item | Definição |
|---|---|
| Finalidade | Indicar se a API está ativa |
| Consumidores | Equipe, scripts de demonstração, interface do totem (antes de começar) |
| Sucesso | **200** com `status: "Healthy"` ou `"Degraded"` |
| Erros | **503** com `status: "Unhealthy"` (no mesmo formato de corpo) |
| Acesso | Público |

Resposta real com o banco disponível, registrada nas evidências do B3:

```json
{"status":"Healthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T21:47:05.9017706+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Healthy"}]}
```

Resposta real com o banco indisponível (**503**):

```json
{"status":"Unhealthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T21:48:03.2666865+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Unhealthy"}]}
```

- **`escopo`:** é `processo` quando não há verificação registrada e `processo-e-dependencias`
  quando há. Desde o B3, a API sempre registra a verificação `banco-de-dados`.
- **`banco-de-dados`:** confere se o SQLite está acessível e se todas as migrations foram
  aplicadas. Ela só lê: não cria o arquivo do banco, não aplica migrations e não roda o seed. A
  resposta não traz caminhos, connection string nem mensagens internas.
- **Provedor de previsões:** o estado dele **não** entra no `/health`; é informado pelos
  próprios endpoints de negócio.

O formato do corpo não mudou desde o B0. Mudam apenas o valor de `escopo` e o conteúdo de
`verificacoes`.

---

## 7. Catálogo de erros

Formato **ProblemDetails** (RFC 9457), com dois campos extras:

| Campo | Descrição |
|---|---|
| `type` | Link padrão do ASP.NET Core para o status, apontando para a seção da RFC 9110. O valor exato será conferido no B6. **Não use para decidir lógica.** |
| `title` | Resumo legível |
| `status` | Código HTTP |
| `detail` | Explicação do caso, sem dados internos nem rastros de pilha |
| `instance` | Caminho da requisição |
| `codigo` | **Código estável do erro.** É o campo que a interface e a IA devem usar. |
| `errors` | Só em 400: objeto com o nome do campo, em camelCase, e a lista de mensagens |
| `traceId` | Identificador para localizar a requisição nos logs |

| `codigo` | Status | Quando |
|---|---|---|
| `RequisicaoInvalida` | 400 | Parâmetro ou corpo com tipo, formato ou faixa inválidos |
| `ChaveAdministrativaInvalida` | 401 | Cabeçalho de chave ausente ou errado, quando a chave é exigida |
| `OperacoesAdministrativasDesabilitadas` | 403 | Fora de Development, sem chave configurada |
| `ParadaNaoEncontrada` | 404 | Parada inexistente; nas rotas públicas, também inativa |
| `LinhaNaoEncontrada` | 404 | Linha inexistente; nas rotas públicas, também inativa |
| `DestinoNaoEncontrado` | 404 | Destino inexistente ou inativo |
| `CodigoJaCadastrado` | 409 | Código de parada ou de linha já usado |
| `LinhaNaoAtendeParada` | 422 | A linha existe, mas não atende a parada |
| `ParadaInativa` | 422 | Operação administrativa que exige parada ativa |
| `LinhaInativa` | 422 | Operação administrativa que exige linha ativa |
| `ErroInterno` | 500 | Erro inesperado da API |
| `ProvedorPrevisoesIndisponivel` | 503 | Só em `GET /previsoes`: o provedor falhou, não respondeu no prazo ou está desligado |

Exemplos: [`exemplos/erro-parada-nao-encontrada.json`](exemplos/erro-parada-nao-encontrada.json),
[`exemplos/erro-validacao.json`](exemplos/erro-validacao.json) e
[`exemplos/erro-provedor-previsoes-indisponivel.json`](exemplos/erro-provedor-previsoes-indisponivel.json).
O `traceId` dos exemplos é o valor ilustrativo da especificação W3C Trace Context.

Diferença entre **400** e **422**: 400 é um valor mal formado ou fora da faixa, verificado sem
consultar o banco. 422 é uma requisição bem formada que contradiz o estado dos dados.

---

## 8. Catálogo de avisos

| `codigo` | Onde aparece | Como interpretar |
|---|---|---|
| `DadosSimulados` | Qualquer resposta com dados simulados | A informação é de demonstração. A IA deve dizer isso à pessoa. |
| `CadastroNaoVerificado` | Lista `avisos` de previsões, últimas passagens e alternativas, e das partes do resumo, quando a parada ou alguma linha da resposta tem `origem: "CadastroAdministrativo"`. Aparece uma vez por resposta. As respostas sem `avisos` (paradas, linhas, destinos e rotas administrativas) não trazem o aviso; nelas, o mesmo fato aparece em `origem`. | Foi cadastrado por uma rota administrativa, sem conferência com fonte confiável. Não indica se o dado é fictício: isso é `dadosSimulados`. |
| `PrevisaoParcial` | Previsões | Algumas linhas estão em `linhasSemPrevisao` |
| `SemPrevisoes` | Previsões, alternativas | O provedor respondeu, mas não há previsão |
| `ProvedorIndisponivel` | Previsões dentro do resumo, alternativas | O serviço de previsões não pôde ser consultado. Vale tentar de novo depois. |
| `ParadaSemLinhas` | Previsões, passagens, alternativas, resumo | Nenhuma linha associada à parada |
| `PassagemNaoRegistrada` | Últimas passagens | Algumas linhas estão em `linhasSemRegistro` |
| `PassagemDesatualizada` | Últimas passagens | Algum item tem `desatualizada: true` |
| `ObservacaoSimulada` | Item de passagem | Observação criada uma vez pelo seed, com horário histórico que não é renovado. Não é registro oficial. |
| `RegistroManualNaoVerificado` | Item de passagem | Informado por um operador, sem verificação. Não é registro oficial. |
| `LimitacaoLinhasDiretas` | Alternativas | Só linhas diretas cadastradas, sem baldeação nem outros modais |
| `PercursoEstimado` | Alternativas | Percurso estimado e simulado, sem trânsito em tempo real |
| `ComparacaoIncompleta` | Alternativas | Há linhas sem previsão fora da comparação |
| `SemComparacao` | Alternativas | Nenhuma linha pôde ser comparada |
| `SemLinhaDireta` | Alternativas | Nenhuma linha direta até o destino |
| `ResumoParcial` | Resumo | Alguma parte do resumo está indisponível; as outras continuam válidas |

Novos códigos podem ser acrescentados. Um consumidor que receber um código desconhecido deve
mostrar a `mensagem` como está.

---

## 9. Conferência das rotas

| # | Método | Rota | Acesso | Estado |
|---|---|---|---|---|
| 1 | GET | `/api/v1/paradas` | Público | Planejado (B6) |
| 2 | GET | `/api/v1/paradas/{id}` | Público | Planejado (B6) |
| 3 | GET | `/api/v1/linhas` | Público | Planejado (B6) |
| 4 | GET | `/api/v1/destinos` | Público | Planejado (B6) |
| 5 | GET | `/api/v1/paradas/{id}/previsoes` | Público | Planejado (B8) |
| 6 | GET | `/api/v1/paradas/{id}/ultimas-passagens` | Público | Planejado (B6) |
| 7 | GET | `/api/v1/paradas/{id}/alternativas` | Público | Planejado (B9) |
| 8 | GET | `/api/v1/paradas/{id}/resumo` | Público | Planejado (B6 parcial, B9 completo) |
| 9 | POST | `/api/v1/admin/paradas` | Chave | Planejado (B7) |
| 10 | PUT | `/api/v1/admin/paradas/{id}` | Chave | Planejado (B7) |
| 11 | POST | `/api/v1/admin/linhas` | Chave | Planejado (B7) |
| 12 | PUT | `/api/v1/admin/paradas/{id}/linhas/{linhaId}` | Chave | Planejado (B7) |
| 13 | POST | `/api/v1/admin/observacoes-passagem` | Chave | Planejado (B7) |
| — | GET | `/health` | Público | **Implementado (B0; verificação do banco no B3)** |

Por que as rotas não entram em conflito:
- `{id}` e `{linhaId}` terão a restrição `int:min(1)`. Um texto nunca é confundido com um `id`.
- Os sufixos de `/paradas/{id}/...` (`previsoes`, `ultimas-passagens`, `alternativas`,
  `resumo`) são literais diferentes entre si.
- `/api/v1/admin/...` começa com o literal `admin`, que não é capturado por nenhuma rota pública.
- O mesmo caminho só se repete com métodos diferentes ou em prefixos separados:
  `GET /api/v1/paradas/{id}` (público) e `PUT /api/v1/admin/paradas/{id}` (administrativo).
- `/health`, `/openapi/v1.json` e `/scalar` ficam fora de `/api/v1`.

Todas as rotas atendem a uma das quatro funções do totem (previsão, última passagem,
alternativas e resumo), à identificação da parada ou ao cadastro mínimo necessário para a
demonstração. Nenhuma rota sai do tema de mobilidade urbana.

---

## 10. Divergências em relação ao planejamento

Mudanças em relação à Etapa 01.1 e ao rascunho B1. Nenhuma altera a lista de 13 endpoints.
A coluna **B1** mostra o que o rascunho anterior dizia. A coluna **B1.1** mostra a decisão
consolidada na revisão B1.1, aprovada com um ajuste. O ajuste da consolidação B1.2 está em D16,
e as correções da B1.3 estão em D17 a D19.

| # | Planejamento (Etapa 01.1) | B1 | B1.1 (consolidado) | Justificativa técnica | Arquivos | Impacto na implementação |
|---|---|---|---|---|---|---|
| D1 | Datas "com deslocamento de fuso" | Saída de negócio em `-03:00`; `/health` em UTC | **Mantido** | O totem e a IA usam horário local. O `/health` é técnico e já foi publicado; os dois formatos são ISO 8601 válidos. | `api-v1.md` 1.3, 6 | B5/B6: um conversor único de UTC para `America/Sao_Paulo` na saída dos DTOs. `/health` sem mudança. |
| D2 | Endpoint 5 com **503** quando o provedor falha | 200 com `status: "ProvedorIndisponivel"` | **Revertido para 503** em `/previsoes`. Resumo e alternativas continuam 200 com status explícito. | A previsão é todo o conteúdo do endpoint 5; sem ela, a requisição não foi atendida. Os endpoints compostos ainda têm informação útil (seção 2). | `api-v1.md` 1.7, 1.10, 2, 4.5, 4.7, 4.8, 7; `README.md`; `erro-provedor-previsoes-indisponivel.json` | B6: exceção de domínio `ProvedorPrevisoesIndisponivelException` convertida em 503 pelo tratador global. B8/B9: o service do resumo e o das alternativas capturam a indisponibilidade e a transformam em status. |
| D3 | Endpoint 6 só com **404** | Também 400 e 422, pelo `linhaId` opcional | **Mantido** | O mesmo parâmetro tem a mesma validação em todos os endpoints | `api-v1.md` 4.6 | B5: validação de "linha atende a parada" compartilhada entre previsões e passagens |
| D4 | `GET /destinos?termo` sem paginação | Paginado | **Mantido** | Um único formato de listagem para quem consome. Mudança aditiva. | `api-v1.md` 4.4; `destinos-busca.json` | B4: a mesma paginação genérica das outras listagens |
| D5 | `GET /paradas?termo&pagina&tamanhoPagina` | Mais o filtro `codigoExterno` | **Mantido e detalhado**: comparação exata sem diferenciar maiúsculas, no máximo um item e a sequência do totem (4.1.1) | O `id` interno pode mudar quando o banco é recriado; o código é estável e único | `api-v1.md` 1.6, 4.1, 4.1.1; `README.md`; `paradas-busca-codigo.json` | B3: índice único em `CodigoExterno`. B4: filtro exato. |
| D6 | `limitacoes` como lista de textos | Avisos `LimitacaoLinhasDiretas` e `PercursoEstimado` | **Mantido**. A mensagem de `PercursoEstimado` passou a dizer que os tempos são simulados. | Um único mecanismo de avisos, com códigos estáveis | `api-v1.md` 4.7, 8; `alternativas*.json` | B9: o service monta a lista de avisos |
| D7 | Passagem com campo `fonte` | `origemRegistro`, `nivelConfirmacao` e `aviso` | **Mantido** | `fonte` repetia o que esses campos já informam | `api-v1.md` 4.6 | B2: a hierarquia `RegistroPassagem` expõe `NivelConfirmacao` e `Aviso` polimórficos, sem `Fonte` |
| D8 | `calculadoEm` nas alternativas | `consultadoEm` em todas as respostas | **Mantido** | Um único nome para o instante de referência | `api-v1.md` 1.3 e respostas | B5: o instante é lido uma vez do `TimeProvider` por requisição e repassado aos cálculos |
| D9 | Avisos como texto | Objeto `{ codigo, mensagem }` | **Mantido** | A IA decide pelo código, e a interface exibe a mensagem | `api-v1.md` 3.2, 8 | B5: catálogo de avisos centralizado (códigos e mensagens) |
| D10 | Sem regra para registros inativos | Rotas públicas tratam inativos como inexistentes | **Mantido** | Evita que o totem mostre uma parada desativada | `api-v1.md` 2, 7 | B4: as consultas públicas filtram `Ativa`. As rotas administrativas não filtram. |
| D11 | Etapa 01: observação manual não pode ser **anterior a 7 dias** | Trocado para **24 horas** sem registro como divergência | **24 horas**, agora documentado e separado das outras regras de passagem (4.6.1) | O registro manual alimenta a "última passagem" do totem, que já fica desatualizada após 60 minutos. Aceitar até 7 dias só permitiria registros que nascem desatualizados. 24 horas ainda tolera um lançamento atrasado no mesmo dia de operação. | `api-v1.md` 4.6.1, 5.6 | B2: a regra fica na criação de `PassagemRegistroManual`, testada com `FakeTimeProvider` nos limites de 24 h e 2 min |
| D12 | — | Status `SemPrevisao`; alternativas com `SemPrevisao` também como situação geral | `StatusPrevisao.SemPrevisoes` e `SituacaoAlternativas.SemComparacao`; `SituacaoAlternativa.SemPrevisao` continua só no item | Nome pedido na revisão. A situação geral passa a ter um nome próprio e não se confunde com o item. | `api-v1.md` 1.7, 4.5, 4.7, 8 | B2: enums com esses valores |
| D13 | — | Passagem antiga só indicada por `minutosDesdeObservacao` | Campo `desatualizada`, `limiteDesatualizacaoMinutos: 60` e aviso `PassagemDesatualizada` | Regra simples e testável. A interface não precisa decidir o que é antigo. | `api-v1.md` 2, 4.6, 8; `ultimas-passagens.json` | B5: o limite fica em uma constante do domínio, coberta por teste |
| D14 | — | Resumo sem indicação geral de completude | `situacao` (`Completo`/`Parcial`) e `partesIndisponiveis` | Status explícito na resposta parcial, como pedido na revisão | `api-v1.md` 4.8; `resumo*.json` | B9: o service do resumo preenche os dois campos |
| D15 | — | `comparacaoCompleta` sem regra para lista vazia | `true` somente com `situacao: "Comparadas"` e todas as linhas comparáveis | Evita "comparação completa" quando nada foi comparado | `api-v1.md` 4.7 | B9: regra coberta por teste |
| D16 | — | Seed com horários relativos, sem regra de reinício | **B1.1:** em Development, uma opção do seed substituía as observações simuladas a cada inicialização. **B1.2 (consolidado):** opção removida. O seed é idempotente, as observações simuladas mantêm o horário original e ficam desatualizadas; a passagem recente da demonstração vem do registro manual administrativo. | Renovar a cada reinício criava eventos históricos fictícios e alterava registros já persistidos, o que contradiz a persistência histórica (4.6.1). As previsões continuam atuais porque são calculadas na consulta. | `api-v1.md` 4.6, 4.6.2, 5.6, 8, 11; `README.md`; `ultimas-passagens.json`, `resumo*.json`, `admin-observacao-passagem-*.json` | B3: seed com verificação de existência por chave estável e sem atualização de horários. B7: o registro manual é o caminho da demonstração. |
| D17 | — | — | **B1.2:** R3 dizia que o seed cria os dados que faltam, e R4 que iniciar a API não cria passagens. **B1.3:** separadas três situações: provisionamento inicial (cria as observações simuladas iniciais), inicialização posterior (nunca cria passagens) e registro manual. | Remove a contradição entre R3 e R4 sem mudar o comportamento aprovado | `api-v1.md` 4.6.1, 4.6.2, 11 | B3: o seed decide pela existência das paradas do seed; as observações só são criadas junto com o provisionamento |
| D18 | `dadosSimulados` derivado só da origem | Cadastro administrativo sempre com `dadosSimulados: false` | **B1.3:** `dadosSimulados` (`boolean`, obrigatório) no corpo de `POST /admin/paradas` e `POST /admin/linhas`, guardado e devolvido nas respostas públicas. `CadastroNaoVerificado` só nas listas `avisos` que existem. | O exemplo `TESTE-P003` era fictício e aparecia como não simulado. Método de cadastro, natureza e verificação passam a ser informações separadas (1.8). | `api-v1.md` 1.7, 1.8, 3.3 a 3.5, 5.2 a 5.4, 8 | B2: campo `DadosSimulados` em `Parada` e `Linha`, definido na criação. B7: campo obrigatório nos dois DTOs de entrada. B9: aviso `CadastroNaoVerificado`. |
| D19 | — | `minutosDesdeObservacao` e `desatualizada` sem regra para horário futuro | **B1.3:** minutos com `max(0, piso)`; `desatualizada` pelo intervalo exato; registro ainda no futuro fora da consulta até o horário chegar; casos de teste J1 a J5 e T1 a T7. | A tolerância de 2 minutos permitia minutos negativos e uma passagem "que ainda não aconteceu" | `api-v1.md` 1.3, 4.6, 4.6.1, 4.6.3, 5.6, 11 | B2: cálculo no domínio. B4/B5: filtro `observadaEm <= consultadoEm` na consulta. |

---

## 11. Requisitos para os próximos blocos

Regras que o contrato pressupõe e que **ainda precisam ser implementadas**. Nenhuma delas existe
no código hoje.

| # | Requisito | Bloco | Como testar |
|---|---|---|---|
| R1 | O provedor simulado calcula todos os horários a partir do `TimeProvider`. Cada linha simulada tem um intervalo e uma defasagem fixos; a próxima chegada é o próximo horário dessa sequência a partir de agora. Nunca devolve previsão vencida. | B8 | Com `FakeTimeProvider` em datas diferentes, as previsões são sempre futuras e repetem o mesmo padrão |
| R2 | Os horários fixos de 10/11/2026 existem **só nos exemplos de `contracts/`**. Nenhum horário fixo vai para o código, o seed ou os testes de regra. | B2 a B9 | Revisão de código e testes com datas variadas |
| R3 | O seed é **idempotente** e identifica cada dado por chave estável. No **provisionamento inicial** de um banco vazio, cria os cadastros de demonstração e as observações simuladas iniciais, com horário calculado uma única vez em relação ao instante do provisionamento, nunca com data fixa (4.6.2, situação 1). | B3 | Com `FakeTimeProvider`: provisionar um banco vazio cria as observações com horários relativos; executar o seed de novo, em outro instante, mantém a mesma quantidade de registros e os mesmos `observadaEm` |
| R4 | Numa **inicialização posterior**, com o banco já provisionado, o seed não cria, não altera e não apaga passagens (4.6.2, situação 2). Não existe opção para renovar observações simuladas. Depois do provisionamento, uma nova passagem só nasce pelo registro manual administrativo (5.6), com `RegistroManual` e `origemRegistro` identificados (situação 3). | B3 e B7 | Teste: reiniciar o host com o banco provisionado, mesmo sem uma das observações simuladas, mantém as passagens iguais |
| R5 | `minutosDesdeObservacao` e `desatualizada` seguem 4.6.3: minutos nunca negativos, desatualização pelo intervalo exato e registro no futuro fora da consulta. As observações simuladas antigas continuam visíveis com `desatualizada: true`. | B2 e B5 | Casos J1 a J5 e T1 a T7 de 4.6.3 com `FakeTimeProvider` |
| R6 | O provedor simulado terá uma opção para simular a indisponibilidade, usada nos testes e na demonstração do 503 e do resumo parcial. | B8 | Teste dos dois comportamentos |
| R7 | A busca por `termo` usa uma coluna normalizada (minúsculas e sem acentos), porque o SQLite não ignora acentos. | B3/B4 | Teste: `demonstracao` encontra `Demonstração` |
| R8 | O adaptador Olho Vivo só faz sentido para códigos reais da SPTrans. Para os dados `SIM-`, as previsões vêm sempre do provedor simulado. | B8 | Documentado em `docs/dados.md` quando o adaptador existir |
