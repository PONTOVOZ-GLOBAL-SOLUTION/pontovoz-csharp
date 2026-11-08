# Dados do PontoVoz: reais, públicos e simulados

Este documento registra a origem de cada dado usado pelo PontoVoz. A regra é
simples: **nenhum dado é apresentado como real ou oficial sem que a fonte tenha
sido verificada.**

> **Estado atual (08/11/2026):** os dados da demonstração são **fictícios**: o seed do SQLite
> (B3), as previsões de chegada do provedor simulado (B8) e os percursos estimados usados nas
> alternativas (B9). Os registros do seed têm o prefixo `SIM-` e são identificados como simulados.
> As rotas administrativas (B7) aceitam cadastros enviados pelo operador, que declara em
> `dadosSimulados` se o cadastro é fictício; esses cadastros não são verificados. O adaptador para
> a API Olho Vivo da SPTrans existe no código, mas está **desligado** e **nunca foi testado contra a
> API real**. Portanto, o ambiente demonstrado **não tem nenhum dado oficial de chegada** de ônibus.

## Categorias

| Categoria | Definição | Situação no projeto |
|---|---|---|
| **Dados reais** | Obtidos de uma fonte verificável sobre a operação real do transporte | Nenhum |
| **Dados públicos** | Publicados oficialmente por órgãos ou empresas, com acesso permitido | Nenhum integrado (ver Olho Vivo abaixo) |
| **Dados simulados** | Criados só para demonstração | Em uso: seed do SQLite (B3), previsões do provedor simulado (B8) e percursos estimados (B9) |
| **Cadastros administrativos** | Enviados pelo operador, com a natureza declarada em `dadosSimulados` | Possíveis; nunca verificados |
| **Dados pessoais** | Informações que identificam pessoas | **Não são solicitados** pela API; ver [Dados pessoais](#dados-pessoais) |

## Dados simulados

- **Seed do SQLite (B3):** paradas, linhas, destinos, percursos e duas observações de passagem
  de demonstração. Todos têm código com o prefixo `SIM-` e nomes como "Parada Demonstração
  Centro", com `dadosSimulados: true`. As observações simuladas são criadas uma vez, no
  provisionamento, e nunca são renovadas.
- **Cadastros administrativos (B7):** o operador declara em `dadosSimulados` se o cadastro é
  fictício. Nenhum cadastro administrativo é verificado (aviso `CadastroNaoVerificado`).
- **Previsões de chegada (B8):** calculadas pelo provedor simulado no instante de cada consulta,
  **sem gravação no banco**. Saem com `origem: "Simulado"`, `dadosSimulados: true`, aviso
  `DadosSimulados` e `nivelConfirmacao: "Previsao"`.
- **Alternativas de deslocamento (B9):** usam só os percursos estimados cadastrados no seed
  (`origemPercurso: "Simulado"`, aviso `PercursoEstimado`), sem trânsito em tempo real, baldeação
  ou rota inventada. Nenhuma alternativa é uma rota oficial da SPTrans.
- Nenhum nome, código, endereço ou horário real da SPTrans é copiado ou inventado como se fosse real.

### Como o provedor simulado calcula uma chegada

Cada linha de demonstração tem uma configuração fixa no código
(`ProvedorPrevisaoSimulado.LinhasDemonstracao`), identificada pelo código estável da linha:

| Linha | Intervalo | Defasagem | Chegadas por consulta | Veículos (fictícios) |
|---|---|---|---|---|
| `SIM-L002` | 18 min | 9 min | 2 | `SIM-V2001`, `SIM-V2002`, `SIM-V2003` |
| `SIM-L003` | 20 min | 7 min | 2 | `SIM-V3001` |
| `SIM-L004` | 15 min | 10 min | 1 | `SIM-V4001`, `SIM-V4002` |
| `SIM-L001` | — | — | — | sem configuração: aparece sem previsão |

A viagem número *k* de uma linha chega em `1970-01-01T00:00Z + defasagem + k × intervalo`
(minutos corridos desde uma âncora fixa) e é feita pelo veículo `veículos[k mod quantidade]`. O
provedor devolve as próximas viagens a partir do instante da consulta, que vem do
`TimeProvider`. Por isso:

- nenhuma data fica gravada no algoritmo: a mesma regra vale em qualquer dia, na virada do dia,
  do mês e do ano, e depois de reiniciar a API;
- nenhuma previsão vencida é devolvida; uma chegada exatamente no instante da consulta tem
  espera 0;
- os valores reproduzem o exemplo do contrato (10/11/2026, 14h32: `SIM-L002` às 14h33 e 14h51,
  `SIM-L004` às 14h40), mas esse exemplo é só uma referência de formato;
- linhas sem configuração, inclusive as cadastradas pelo operador, ficam **sem previsão**: o
  provedor não inventa horários para elas.

A indisponibilidade do provedor pode ser simulada pela configuração
`Previsoes:SimularIndisponibilidade` (requisito R6), para demonstrar o 503. Ela vem desligada.

## Dados públicos: API Olho Vivo da SPTrans

| Item | Situação |
|---|---|
| Token de acesso | **Não solicitado.** A equipe não tem credenciais. |
| Termos de uso | **Não verificados** |
| Adaptador no código | Implementado no B8, **desligado por padrão** (`Previsoes:OlhoVivo:Habilitado = false`) |
| Testes do adaptador | Só com respostas HTTP **simuladas** (`HttpMessageHandler` falso) |
| Teste real da integração | **Não realizado**. Nenhuma chamada foi feita à SPTrans. |
| Correspondência entre códigos do PontoVoz e da SPTrans | **Nenhuma declarada** |

### O que foi conferido na documentação oficial

Fonte: [Guia de referência da API Olho Vivo](https://www.sptrans.com.br/desenvolvedores/api-do-olho-vivo-guia-de-referencia/documentacao-api),
consultado em 08/11/2026.

- Endereço base da versão 2.1: `api.olhovivo.sptrans.com.br/v2.1`. A página informa que o acesso
  por HTTP seria desativado em 02/01/2024 e passaria a ser só por HTTPS; o adaptador usa só HTTPS.
- Autenticação: `POST /Login/Autenticar?token={token}`, que responde `true` ou `false`.
- Previsão: `GET /Previsao/Parada?codigoParada=` (todas as linhas da parada) e
  `GET /Previsao?codigoParada=&codigoLinha=`. Na resposta, `p.l[]` traz as linhas (`cl` é o código
  da linha) e `vs[]` os veículos, com `p` (prefixo) e `t` (horário previsto, `HH:mm`, sem data nem fuso).

Pontos **não confirmados** pela página oficial e tratados com cuidado no adaptador:

- A sessão depois do login usa um cookie, segundo fontes de terceiros; a página oficial não fala
  disso. O adaptador guarda cookies e autentica a cada consulta.
- O prefixo `p` é descrito como inteiro, mas aparece como texto nos exemplos; o adaptador aceita
  os dois e descarta um prefixo fora do formato.
- Os horários `HH:mm` são interpretados no fuso de São Paulo, no dia mais próximo do instante da
  consulta (uma previsão "00:05" consultada às 23h58 é do dia seguinte).

### Regras do adaptador

- Só faz chamadas com `Habilitado = true` **e** um token configurado (`Previsoes:OlhoVivo:Token`,
  por user-secrets ou variável de ambiente; nunca em arquivo versionado).
- Só consulta uma parada e linhas com **código oficial declarado** pela equipe
  (`Previsoes:OlhoVivo:Paradas` e `Previsoes:OlhoVivo:Linhas`). Um cadastro administrativo não é
  tratado como código real da SPTrans sem essa declaração.
- **Nunca** envia dados `SIM-` à SPTrans: as paradas e linhas de demonstração usam sempre o
  provedor simulado (requisito R8).
- Tempo limite de 5 segundos por chamada (`Previsoes:OlhoVivo:TempoLimiteSegundos`).
- Falha de autenticação, prazo esgotado, falha de conexão, HTTP sem sucesso ou JSON fora do
  formato viram indisponibilidade (503 no endpoint de previsões). Não há troca silenciosa para
  previsões simuladas.
- O token não aparece em logs, erros nem respostas: o registro automático de requisições do
  `HttpClient` foi desligado para esse cliente, porque gravaria a URL de login com o token.

Para declarar a integração funcionando, falta: obter um token, ler os termos de uso, declarar a
correspondência de pelo menos uma parada e uma linha reais e testar contra a API real,
registrando aqui a data e o resultado.

## Registros de passagem e previsões: níveis de confirmação

| Nível | Significado | Pode ser apresentado como oficial? |
|---|---|---|
| `Previsao` | Estimativa de um provedor de previsões; não é uma passagem e nunca aparece em `ultimas-passagens` | Não |
| `ObservacaoSimulada` | Criado pelo seed de demonstração | Não |
| `RegistroManual` | Informado por um operador, sem verificação independente | Não |
| `VerificadaFonteConfiavel` | Confirmado por fonte confiável; reservado, não emitido no MVP | Só com fonte oficial verificada |

## Dados pessoais

O PontoVoz não precisa identificar passageiros: nenhuma rota do backend pede nomes, documentos,
e-mails, telefones, imagens ou gravações de voz, e a demonstração não deve receber dados pessoais
reais. Os registros manuais devem usar apenas um rótulo técnico e impessoal do operador (por
exemplo, `operador-local`), nunca um nome, telefone ou e-mail. O formato de `origemRegistro`
recusa nomes com espaço ou maiúscula, e-mails e telefones formatados, mas essa validação não
garante, sozinha, a exclusão de toda informação pessoal (uma sequência só de dígitos passa); cabe
ao operador usar apenas identificadores técnicos. Os campos de texto dos cadastros
administrativos (nome e endereço da parada, por exemplo) também não devem receber dados pessoais.

Estas regras reduzem a exposição de dados pessoais na demonstração, mas não constituem, sozinhas,
uma avaliação de conformidade com a LGPD.
