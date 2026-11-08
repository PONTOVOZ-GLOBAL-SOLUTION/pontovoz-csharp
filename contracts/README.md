# Contratos de integração do PontoVoz

Esta pasta reúne os contratos que permitem à interface do totem e ao componente de voz e IA
consumir o backend C# sem depender de detalhes internos dele.

> **Estado atual (Bloco B7):** o contrato v1 está **consolidado** e serve de base para a implementação.
> Estão implementados o `GET /health`, as cinco consultas públicas do B6 (paradas, detalhe da
> parada, linhas, destinos e últimas passagens) e as cinco rotas administrativas do B7.
> Previsões, alternativas e resumo continuam **planejados** (B8 e B9). Os JSON de [`exemplos/`](exemplos/) mostram o
> formato combinado; os das rotas implementadas são comparados com as respostas reais nos testes
> do backend.

## Conteúdo

| Arquivo | Para que serve |
|---|---|
| [`api-v1.md`](api-v1.md) | Contrato completo: convenções, endpoints, campos, erros, avisos e divergências |
| [`exemplos/`](exemplos/) | Corpos JSON de requisição e resposta, coerentes entre si |
| [`verificador/`](verificador/) | Verificador da sintaxe e da coerência dos exemplos (seção [Verificação dos exemplos](#verificação-dos-exemplos)) |
| [`openapi/pontovoz-v1.json`](openapi/pontovoz-v1.json) | Documento OpenAPI exportado da API em execução, só com os endpoints implementados. Para atualizar, use `backend/scripts/exportar-openapi.ps1` ([README do backend](../backend/README.md)). |

## Arquitetura de integração

```
Interface do totem ─┐
                    ├── HTTP + JSON ──►  Backend C# (ASP.NET Core)  ──►  SQLite
Voz / IA ───────────┘                     │
Operador (admin) ───┘                     └──► provedor de previsões (simulado ou Olho Vivo)
```

- Os componentes se comunicam só por HTTP, sem compartilhar banco de dados nem código.
- O backend entrega **dados estruturados e avisos**. A fala, o diálogo e a tela ficam com os
  outros componentes.

## Endereço local

| Perfil | URL base |
|---|---|
| `http` (recomendado para a equipe) | `http://localhost:5080` |
| `https` | `https://localhost:7080` (exige certificado de desenvolvimento confiável) |

Para iniciar a API, siga [`../backend/README.md`](../backend/README.md).

## Endpoints

| # | Método | Rota | Estado |
|---|---|---|---|
| — | GET | `/health` | **Implementado** |
| — | GET | `/openapi/v1.json` e `/scalar` | **Implementados** (só em Development) |
| 1 | GET | `/api/v1/paradas` | **Implementado (B6)** |
| 2 | GET | `/api/v1/paradas/{id}` | **Implementado (B6)** |
| 3 | GET | `/api/v1/linhas` | **Implementado (B6)** |
| 4 | GET | `/api/v1/destinos` | **Implementado (B6)** |
| 5 | GET | `/api/v1/paradas/{id}/previsoes` | Planejado (B8) |
| 6 | GET | `/api/v1/paradas/{id}/ultimas-passagens` | **Implementado (B6)** |
| 7 | GET | `/api/v1/paradas/{id}/alternativas?destinoId=` | Planejado (B9) |
| 8 | GET | `/api/v1/paradas/{id}/resumo` | Planejado (B9) |
| 9 | POST | `/api/v1/admin/paradas` | **Implementado (B7)**, com chave |
| 10 | PUT | `/api/v1/admin/paradas/{id}` | **Implementado (B7)**, com chave |
| 11 | POST | `/api/v1/admin/linhas` | **Implementado (B7)**, com chave |
| 12 | PUT | `/api/v1/admin/paradas/{id}/linhas/{linhaId}` | **Implementado (B7)**, com chave |
| 13 | POST | `/api/v1/admin/observacoes-passagem` | **Implementado (B7)**, com chave |

## Como consumir

### Padrões de resposta

- JSON em camelCase, com nomes em português e sem acentos. Enums vêm como texto (`"Disponivel"`).
- Horários em ISO 8601 com fuso de São Paulo, como `2026-11-10T14:32:00-03:00`.
- Campos sem valor vêm como `null`, e listas vazias como `[]`. Nenhum campo documentado é omitido.
- Erros seguem o padrão ProblemDetails, com o campo `codigo` para decidir o que fazer.
- Com o provedor de previsões indisponível, `GET /previsoes` devolve **503**. O resumo e as
  alternativas continuam respondendo **200**, com status e avisos explícitos.
- Os horários de 10/11/2026 nos exemplos são fixos e só ilustrativos. A API calcula os horários
  a partir do relógio do servidor.

Detalhes nas seções 1 e 2 de [`api-v1.md`](api-v1.md).

### Consultar uma parada

O totem guarda o **código** da parada onde está instalado, porque o `id` interno pode mudar se o
banco for recriado:

1. O totem conhece o código externo, por exemplo `SIM-P001`.
2. Consulta a busca pelo código:

   ```bash
   curl "http://localhost:5080/api/v1/paradas?codigoExterno=SIM-P001"
   ```

3. Lê o `id` em `itens[0].id` ([exemplo](exemplos/paradas-busca-codigo.json)). Como o código é
   único, vem no máximo um item. Com `totalItens: 0`, o totem mostra um erro de configuração.
4. Usa o `id` nas consultas da parada: resumo, previsões, últimas passagens ou alternativas.
   Se alguma delas devolver 404, repete o passo 2 uma vez.

O detalhe da parada, com as linhas, está em `GET /api/v1/paradas/1`
([exemplo](exemplos/parada-detalhe.json)). A sequência completa está na seção 4.1.1 de
[`api-v1.md`](api-v1.md).

### Consultar previsões

```bash
curl "http://localhost:5080/api/v1/paradas/1/previsoes"
```

Leia primeiro o código HTTP e, se for 200, o campo `status`:

| Resposta | O que fazer |
|---|---|
| 200, `status: "Disponivel"` ([exemplo](exemplos/previsoes.json)) | Mostrar `previsoes`. Se `linhasSemPrevisao` não estiver vazia, informar que essas linhas estão sem previsão. |
| 200, `status: "SemPrevisoes"` | Informar que não há previsão no momento |
| 503, `codigo: "ProvedorPrevisoesIndisponivel"` ([exemplo](exemplos/erro-provedor-previsoes-indisponivel.json)) | Informar que as previsões estão fora do ar, mostrar as últimas passagens e tentar de novo depois do tempo indicado em `Retry-After` |

O totem que usa o **resumo** não recebe 503: lá, a mesma situação aparece como
`previsoes.status: "ProvedorIndisponivel"` e `situacao: "Parcial"`.

### Localizar um destino

A IA transforma a frase da pessoa em um termo de busca:

```bash
curl "http://localhost:5080/api/v1/destinos?termo=norte"
```

A busca não diferencia maiúsculas nem acentos ([exemplo](exemplos/destinos-busca.json)).
Se vier mais de um destino, a IA pede à pessoa que escolha. Se não vier nenhum, informa que o
destino não está cadastrado.

### Obter alternativas

```bash
curl "http://localhost:5080/api/v1/paradas/1/alternativas?destinoId=3"
```

As alternativas são ordenadas pelo **tempo total estimado** (espera mais percurso), e não por
quem chega primeiro ([exemplo](exemplos/alternativas.json)). Linhas sem previsão vêm no fim, com
`posicao: null`. Quando `comparacaoCompleta` é `false`, a primeira posição é a mais rápida
**entre as linhas com previsão**, e a fala deve deixar isso claro.

Com `situacao: "SemComparacao"`, nenhuma linha tem previsão: a resposta lista as linhas diretas
e os percursos, mas não indica a mais rápida
([exemplo](exemplos/alternativas-sem-comparacao.json)). Com `situacao: "SemLinhaDireta"`, não
há linha direta cadastrada até o destino.

### Usar o resumo

```bash
curl "http://localhost:5080/api/v1/paradas/1/resumo"
```

Uma chamada traz a parada, as linhas, as previsões e as últimas passagens
([exemplo](exemplos/resumo.json)). Cada parte tem o mesmo formato do endpoint individual. O
resumo continua respondendo com o provedor de previsões fora do ar
([exemplo](exemplos/resumo-provedor-indisponivel.json)).

### Interpretar avisos e dados simulados

- `dadosSimulados: true` significa que a informação é de **demonstração**. A tela deve exibir
  uma marcação, e a fala deve mencionar isso.
- Cada aviso tem `codigo` e `mensagem`. Use o código para decidir e a mensagem para exibir.
  A lista completa está na seção 8 de [`api-v1.md`](api-v1.md).
- `nivelConfirmacao` diz o quanto uma passagem foi confirmada. No MVP, **nenhuma** passagem é
  oficial: `verificada` e `oficialSptrans` são sempre `false`.
- `desatualizada: true` indica uma passagem observada há mais de 60 minutos. Ela continua
  aparecendo, mas a fala deve dizer que o registro é antigo. Uma linha sem nenhum registro
  aparece em `linhasSemRegistro`.
- `minutosDesdeObservacao` nunca é negativo. Um registro manual com horário até 2 minutos no
  futuro (tolerância de relógio) só aparece em `ultimas-passagens` quando esse horário chegar.
- As observações simuladas são criadas uma vez, quando o banco vazio é provisionado, e
  **mantêm o horário original**. Reiniciar a API não cria passagens nem as torna recentes,
  então com o tempo elas aparecem desatualizadas. Para
  mostrar uma passagem recente na demonstração, registre uma observação manual pela rota
  administrativa (seção 5.6 de [`api-v1.md`](api-v1.md)). Ela aparece como `RegistroManual`,
  nunca como passagem verificada. As previsões simuladas são sempre atuais, porque são
  calculadas na hora da consulta.
- No resumo, `situacao: "Parcial"` e `partesIndisponiveis` mostram o que faltou. O restante
  continua válido.

## Jornada de exemplo

> *A pessoa está na Parada Demonstração Centro (`SIM-P001`) e pergunta ao totem:
> "Qual ônibus eu pego para o Terminal Norte?"*

| Passo | Quem | O que acontece |
|---|---|---|
| 1 | Interface | Ao iniciar, resolve `SIM-P001` em `id: 1` com `GET /api/v1/paradas?codigoExterno=SIM-P001` |
| 2 | Interface | Mostra a tela inicial com `GET /api/v1/paradas/1/resumo` e atualiza periodicamente |
| 3 | Voz / IA | Converte a fala em texto e identifica a intenção "ir para um destino" e o termo "norte" |
| 4 | Voz / IA | Chama `GET /api/v1/destinos?termo=norte` e recebe o destino `id: 3`, Terminal Demonstração Norte |
| 5 | Voz / IA | Chama `GET /api/v1/paradas/1/alternativas?destinoId=3` |
| 6 | Backend | Responde que a SIM-104 chega em 8 minutos e leva 20 até o destino (28 no total). A SIM-102 chega antes, em 1 minuto, mas leva 40 (41 no total). A SIM-101 está sem previsão. Inclui os avisos. |
| 7 | Voz / IA | Monta a resposta falada, por exemplo: *"Entre as linhas com previsão, a mais rápida é a SIM-104. Ela chega em cerca de 8 minutos e deve levar 20 minutos até o Terminal Demonstração Norte. A SIM-102 chega em 1 minuto, mas demora mais no caminho. Estes são dados de demonstração."* |
| 8 | Interface | Mostra a mesma comparação na tela, com a marcação de dados simulados |

O texto falado do passo 7 é só uma sugestão para o componente de IA. O backend não gera frases.

## O que é de cada componente

| Responsabilidade | Backend C# | Voz / IA | Interface do totem |
|---|---|---|---|
| Paradas, linhas e destinos | Armazena e consulta | Usa a busca | Exibe |
| Previsões de chegada | Consulta o provedor e informa o estado | Fala | Exibe |
| Última passagem | Armazena, consulta e indica o nível de confirmação | Fala junto com o aviso | Exibe junto com o aviso |
| Alternativas | Calcula e ordena só as linhas diretas | Entende o destino pedido e explica o resultado | Exibe a comparação |
| Avisos | Gera `codigo` e `mensagem` | Decide como falar | Decide como exibir |
| Reconhecimento e síntese de voz, diálogo | — | Implementa | Captura o áudio, se fizer parte da tela |
| Cadastro e observações manuais | Rotas administrativas | — | — |

O backend **não** implementa IA conversacional.

## Rotas administrativas

Exigem o cabeçalho `X-PontoVoz-Admin-Key`. Nos exemplos, o valor aparece como o marcador
`<CHAVE_ADMINISTRATIVA_LOCAL>`, nunca como uma chave real. As regras por ambiente estão na
seção 5 de [`api-v1.md`](api-v1.md). O totem e a IA não usam essas rotas.

Ao cadastrar uma parada ou uma linha, o operador informa `dadosSimulados`, que é obrigatório.
Um cadastro de teste, como `TESTE-P003`, vai com `true` e continua marcado como simulado nas
respostas públicas. `origem: "CadastroAdministrativo"` diz só como o dado entrou no sistema;
nenhum cadastro do MVP é verificado.

Implementadas no B7:

| Rota | Sucesso | Erros |
|---|---|---|
| `POST /api/v1/admin/paradas` | 201, com `Location: /api/v1/paradas/{id}` | 400, 401, 403, 409 |
| `PUT /api/v1/admin/paradas/{id}` | 200 | 400, 401, 403, 404 |
| `POST /api/v1/admin/linhas` | 201, sem `Location` | 400, 401, 403, 409 |
| `PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}` | 204, idempotente | 401, 403, 404, 422 |
| `POST /api/v1/admin/observacoes-passagem` | 201, sem `Location` | 400, 401, 403, 404, 422 |

A chave fica em `Admin:Chave` (user-secrets ou variável de ambiente `Admin__Chave`), nunca em
arquivo versionado. Como configurar e testar no Scalar: [README do backend](../backend/README.md#rotas-administrativas-b7).
`origemRegistro` deve ser só um rótulo técnico impessoal; o padrão do campo não impede, sozinho,
toda informação pessoal (seção 5.6 de [`api-v1.md`](api-v1.md)).

## Exemplos

| Arquivo | Requisição correspondente |
|---|---|
| [`paradas-listagem.json`](exemplos/paradas-listagem.json) | `GET /api/v1/paradas?termo=demonstracao` |
| [`paradas-busca-codigo.json`](exemplos/paradas-busca-codigo.json) | `GET /api/v1/paradas?codigoExterno=SIM-P001` |
| [`parada-detalhe.json`](exemplos/parada-detalhe.json) | `GET /api/v1/paradas/1` |
| [`destinos-busca.json`](exemplos/destinos-busca.json) | `GET /api/v1/destinos?termo=norte` |
| [`previsoes.json`](exemplos/previsoes.json) | `GET /api/v1/paradas/1/previsoes` |
| [`ultimas-passagens.json`](exemplos/ultimas-passagens.json) | `GET /api/v1/paradas/1/ultimas-passagens` |
| [`alternativas.json`](exemplos/alternativas.json) | `GET /api/v1/paradas/1/alternativas?destinoId=3` |
| [`alternativas-sem-comparacao.json`](exemplos/alternativas-sem-comparacao.json) | O mesmo, com o provedor de previsões indisponível |
| [`resumo.json`](exemplos/resumo.json) | `GET /api/v1/paradas/1/resumo` |
| [`resumo-provedor-indisponivel.json`](exemplos/resumo-provedor-indisponivel.json) | O mesmo, com o provedor de previsões fora do ar |
| [`admin-observacao-passagem-requisicao.json`](exemplos/admin-observacao-passagem-requisicao.json) | Corpo de `POST /api/v1/admin/observacoes-passagem` |
| [`admin-observacao-passagem-resposta.json`](exemplos/admin-observacao-passagem-resposta.json) | Resposta 201 da requisição acima |
| [`erro-parada-nao-encontrada.json`](exemplos/erro-parada-nao-encontrada.json) | `GET /api/v1/paradas/99/previsoes` (404) |
| [`erro-validacao.json`](exemplos/erro-validacao.json) | `GET /api/v1/paradas?termo=a&tamanhoPagina=100` (400) |
| [`erro-provedor-previsoes-indisponivel.json`](exemplos/erro-provedor-previsoes-indisponivel.json) | `GET /api/v1/paradas/1/previsoes` com o provedor indisponível (503) |

Todos os exemplos usam o mesmo cenário fictício: consulta às 14h32 de 10/11/2026 na parada
`SIM-P001`, atendida pelas linhas `SIM-L001`, `SIM-L002` e `SIM-L004`. A data é fixa só nos
exemplos.

## Verificação dos exemplos

O verificador [`verificador/VerificarContrato.cs`](verificador/VerificarContrato.cs) confere a
sintaxe e a coerência dos exemplos e dos documentos. Ele usa só o .NET 10 SDK, que o projeto já
exige. A partir da raiz do repositório:

```bash
dotnet run contracts/verificador/VerificarContrato.cs
```

O código de saída é 0 quando nenhuma verificação falha e 1 quando alguma falha. As falhas são
listadas na saída. O verificador cobre as regras listadas no próprio arquivo, e não substitui a
revisão humana nem os testes da API.

## Como a equipe vai receber o contrato

1. **B1 a B1.3 (concluído):** contrato consolidado e publicado no repositório.
2. **B6 (12/11/2026):** o contrato é congelado. Depois disso, só mudanças aditivas.
3. **B6 em diante:** `openapi/pontovoz-v1.json` é exportado da API em execução e atualizado a
   cada mudança de endpoint. Um teste do backend falha se o arquivo ficar diferente do documento
   gerado pela API.

## Histórico de mudanças do contrato

| Data | Mudança |
|---|---|
| 06/11/2026 | Estrutura inicial. Apenas `/health` implementado. |
| 07/11/2026 | Rascunho do contrato v1: 13 endpoints planejados, convenções, catálogos de erros e avisos, e exemplos. Nenhum endpoint novo implementado. |
| 07/11/2026 | Revisão B1.1: 503 em `/previsoes` com o provedor indisponível; `SemPrevisoes`; regras de passagem com `desatualizada`; resumo com `situacao` e `partesIndisponiveis`; `SemComparacao` nas alternativas; sequência de identificação do totem; requisitos R1 a R8; verificador reproduzível. Divergências D1 a D15 em `api-v1.md`. |
| 07/11/2026 | Consolidação B1.2: seed idempotente, sem renovar observações simuladas ao iniciar a API; passagem recente da demonstração pelo registro manual; exemplos de passagem ajustados. Divergência D16 e requisitos R3 a R5 revisados. |
| 07/11/2026 | Correções B1.3: provisionamento inicial separado da inicialização posterior (R3 e R4); `dadosSimulados` obrigatório nos cadastros administrativos e representação de `CadastroNaoVerificado`; regra temporal para observações no futuro, com casos de teste. Divergências D17 a D19. |
| 07/11/2026 | B3.1: o `/health` passa a incluir a verificação `banco-de-dados`, com 503 quando o banco está indisponível. O formato do corpo foi mantido (seção 6 de `api-v1.md`). |
| 07/11/2026 | B6: cinco consultas públicas implementadas (paradas, detalhe, linhas, destinos e últimas passagens), com ProblemDetails, CORS por `Cors:OrigensPermitidas` e `openapi/pontovoz-v1.json` exportado. Conferências registradas em `api-v1.md`: valores de `type`, 400 de model binding, 404 sem `codigo` para rota fora do padrão e 500 genérico. O resumo fica para o B9, sem parte antecipada no B6. Nenhum campo ou regra dos exemplos mudou. |
| 07/11/2026 | B6.1: todo erro passa a ter `codigo`. Novos códigos no catálogo (seção 7 de `api-v1.md`): `RotaNaoEncontrada` (404, caminho sem rota correspondente, inclusive `id` fora do padrão) e `MetodoNaoPermitido` (405). Mudança aditiva; nenhum código ou campo existente mudou. |
| 08/11/2026 | B7: as cinco rotas administrativas implementadas, com a chave da seção 5.1. Conferências registradas em `api-v1.md`: valores de `type` de 401, 403 e 409, erros de corpo (campos obrigatórios, JSON inválido, número como texto, data sem fuso) e 409 também em gravação simultânea. A frase sobre `origemRegistro` foi corrigida: o padrão do campo não impede toda informação pessoal (um número só de dígitos passa); a regra não mudou. |
