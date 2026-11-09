# Contratos de integração do PontoVoz

Esta pasta reúne o contrato REST do backend C# do PontoVoz: o que um cliente da API (por
exemplo, a interface de um totem ou um assistente de voz) precisa saber para consultá-la sem
depender de detalhes internos. As 13 rotas de negócio e o `GET /health` estão implementados. As
previsões vêm de um provedor simulado; o adaptador Olho Vivo está desligado e não foi testado
contra a API real.

## Conteúdo

| Arquivo | Para que serve |
|---|---|
| [`api-v1.md`](api-v1.md) | Contrato completo: convenções, endpoints, campos, erros, avisos e divergências |
| [`exemplos/`](exemplos/) | Corpos JSON de requisição e resposta, coerentes entre si |
| [`verificador/`](verificador/) | Verificador da sintaxe e da coerência dos exemplos (seção [Verificação dos exemplos](#verificação-dos-exemplos)) |
| [`openapi/pontovoz-v1.json`](openapi/pontovoz-v1.json) | Documento OpenAPI exportado da API em execução, só com os endpoints implementados. Para atualizar, use `backend/scripts/exportar-openapi.ps1` ([README do backend](../backend/README.md)). |

## Arquitetura de integração

```
Cliente da API (tela, voz) ─┐
                            ├── HTTP + JSON ──►  Backend C# (ASP.NET Core)  ──►  SQLite
Operador (admin) ───────────┘                     │
                                                  └──► provedor de previsões (simulado ou Olho Vivo)
```

- Um cliente se comunica com o backend só por HTTP, sem compartilhar banco de dados nem código.
- O backend entrega **dados estruturados e avisos**. A fala, o diálogo e a tela ficam com o
  cliente; nenhum desses clientes faz parte deste repositório.

## Endereço local

| Perfil | URL base |
|---|---|
| `http` (recomendado) | `http://localhost:5080` |
| `https` | `https://localhost:7080` (exige certificado de desenvolvimento confiável) |

Para iniciar a API, siga [`../backend/README.md`](../backend/README.md).

## Endpoints

| # | Método | Rota | Estado |
|---|---|---|---|
| — | GET | `/health` | **Implementado** |
| — | GET | `/openapi/v1.json` e `/scalar` | **Implementados** (só em Development) |
| 1 | GET | `/api/v1/paradas` | **Implementado** |
| 2 | GET | `/api/v1/paradas/{id}` | **Implementado** |
| 3 | GET | `/api/v1/linhas` | **Implementado** |
| 4 | GET | `/api/v1/destinos` | **Implementado** |
| 5 | GET | `/api/v1/paradas/{id}/previsoes` | **Implementado**, provedor simulado |
| 6 | GET | `/api/v1/paradas/{id}/ultimas-passagens` | **Implementado** |
| 7 | GET | `/api/v1/paradas/{id}/alternativas?destinoId=` | **Implementado** |
| 8 | GET | `/api/v1/paradas/{id}/resumo` | **Implementado** |
| 9 | POST | `/api/v1/admin/paradas` | **Implementado**, com chave |
| 10 | PUT | `/api/v1/admin/paradas/{id}` | **Implementado**, com chave |
| 11 | POST | `/api/v1/admin/linhas` | **Implementado**, com chave |
| 12 | PUT | `/api/v1/admin/paradas/{id}/linhas/{linhaId}` | **Implementado**, com chave |
| 13 | POST | `/api/v1/admin/observacoes-passagem` | **Implementado**, com chave |

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

O cliente guarda o **código** da parada, porque o `id` interno pode mudar se o banco for
recriado:

1. O cliente conhece o código externo, por exemplo `SIM-P001`.
2. Consulta a busca pelo código:

   ```bash
   curl "http://localhost:5080/api/v1/paradas?codigoExterno=SIM-P001"
   ```

3. Lê o `id` em `itens[0].id` ([exemplo](exemplos/paradas-busca-codigo.json)). Como o código é
   único, vem no máximo um item. Com `totalItens: 0`, a parada não está cadastrada.
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

Quem usa o **resumo** não recebe 503: lá, a mesma situação aparece como
`previsoes.status: "ProvedorIndisponivel"` e `situacao: "Parcial"`.

### Localizar um destino

O cliente envia um termo de busca, por exemplo o que a pessoa disse ou digitou:

```bash
curl "http://localhost:5080/api/v1/destinos?termo=norte"
```

A busca não diferencia maiúsculas nem acentos ([exemplo](exemplos/destinos-busca.json)).
Se vier mais de um destino, o cliente pede à pessoa que escolha. Se não vier nenhum, o destino não
está cadastrado.

### Obter alternativas

```bash
curl "http://localhost:5080/api/v1/paradas/1/alternativas?destinoId=3"
```

As alternativas são ordenadas pelo **tempo total estimado** (espera mais percurso), e não por
quem chega primeiro ([exemplo](exemplos/alternativas.json)). Linhas sem previsão vêm no fim, com
`posicao: null`. Quando `comparacaoCompleta` é `false`, a primeira posição é a mais rápida
**entre as linhas com previsão**, e o cliente deve deixar isso claro.

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

- `dadosSimulados: true` significa que a informação é de **demonstração**. O cliente deve exibir
  ou falar essa marcação.
- Cada aviso tem `codigo` e `mensagem`. Use o código para decidir e a mensagem para exibir.
  A lista completa está na seção 8 de [`api-v1.md`](api-v1.md).
- `nivelConfirmacao` diz o quanto uma passagem foi confirmada. No MVP, **nenhuma** passagem é
  oficial: `verificada` e `oficialSptrans` são sempre `false`.
- `desatualizada: true` indica uma passagem observada há mais de 60 minutos. Ela continua
  aparecendo, mas o cliente deve informar que o registro é antigo. Uma linha sem nenhum registro
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

> *Uma pessoa está na Parada Demonstração Centro (`SIM-P001`) e quer saber qual ônibus pegar para o
> Terminal Norte.* Os passos mostram como um cliente usaria a API; o backend não gera frases nem
> interpreta voz.

| Passo | Chamada | O que o cliente obtém |
|---|---|---|
| 1 | `GET /api/v1/paradas?codigoExterno=SIM-P001` | O `id` da parada (`1`) |
| 2 | `GET /api/v1/paradas/1/resumo` | Parada, linhas, previsões e últimas passagens, para a tela inicial |
| 3 | `GET /api/v1/destinos?termo=norte` | O destino `id: 3`, Terminal Demonstração Norte |
| 4 | `GET /api/v1/paradas/1/alternativas?destinoId=3` | No exemplo do contrato, a SIM-104 chega em 8 minutos e leva 20 até o destino (28 no total); a SIM-102 chega antes, em 1 minuto, mas leva 40 (41 no total); a SIM-101 está sem previsão. Inclui os avisos. |

Com essa resposta, um cliente informaria que, entre as linhas com previsão, a mais rápida é a
SIM-104, e que os dados são de demonstração.

## Rotas administrativas

Exigem o cabeçalho `X-PontoVoz-Admin-Key`. Nos exemplos, o valor aparece como o marcador
`<CHAVE_ADMINISTRATIVA_LOCAL>`, nunca como uma chave real. As regras por ambiente estão na
seção 5 de [`api-v1.md`](api-v1.md). Os clientes de consulta não usam essas rotas.

Ao cadastrar uma parada ou uma linha, o operador informa `dadosSimulados`, que é obrigatório.
Um cadastro de teste, como `TESTE-P003`, vai com `true` e continua marcado como simulado nas
respostas públicas. `origem: "CadastroAdministrativo"` diz só como o dado entrou no sistema;
nenhum cadastro do MVP é verificado.

| Rota | Sucesso | Erros |
|---|---|---|
| `POST /api/v1/admin/paradas` | 201, com `Location: /api/v1/paradas/{id}` | 400, 401, 403, 409 |
| `PUT /api/v1/admin/paradas/{id}` | 200 | 400, 401, 403, 404 |
| `POST /api/v1/admin/linhas` | 201, sem `Location` | 400, 401, 403, 409 |
| `PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}` | 204, idempotente | 401, 403, 404, 422 |
| `POST /api/v1/admin/observacoes-passagem` | 201, sem `Location` | 400, 401, 403, 404, 422 |

A chave fica em `Admin:Chave` (user-secrets ou variável de ambiente `Admin__Chave`), nunca em
arquivo versionado. Como configurar e testar no Scalar: [README do backend](../backend/README.md#segurança).
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
