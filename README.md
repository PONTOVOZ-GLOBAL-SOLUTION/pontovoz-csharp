# PontoVoz — Totem Inteligente de Mobilidade Urbana

**Global Solution FIAP 2026.2** · Engenharia de Software · Tema **CitizenTech**

> **Status real:** projeto em fase inicial. Só existe a estrutura do backend C#
> (Bloco B0): a API inicia, responde em `GET /health` e publica a documentação
> OpenAPI. Nenhuma funcionalidade de transporte público está implementada ainda.

---

## Ficha CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da solução | PontoVoz — Totem Inteligente de Mobilidade Urbana |
| Cidade | São Paulo, SP |
| Bairro / ponto de aplicação | **Pendente** — será definido quando a equipe escolher um local real |
| Cidadãos beneficiados | Passageiros de ônibus, com atenção a idosos, pessoas com deficiência visual, pessoas sem smartphone ou sem internet móvel e quem não conhece as linhas da região |
| Problema identificado | No ponto de ônibus nem sempre há informação acessível e contextualizada sobre a chegada das linhas e as alternativas de deslocamento. Quem não tem smartphone fica sem acesso a essas informações. |
| Funcionamento da solução | Um totem digital no ponto de ônibus permite consultar, por voz ou pela tela, a previsão de chegada das linhas, a última passagem registrada e alternativas de deslocamento até um destino |
| Contribuição do backend C# | API REST que organiza paradas, linhas, previsões, registros de passagem e alternativas, sempre informando a origem dos dados e avisos sobre dados simulados (detalhes abaixo) |
| Integrantes | **Pendente** (ver tabela de integrantes) |

## O problema

Em São Paulo, quem espera o ônibus muitas vezes não sabe quanto tempo falta para a
linha chegar nem se existe uma opção melhor para o seu destino. Os aplicativos de
mobilidade resolvem parte disso, mas exigem smartphone, internet móvel e
familiaridade com tecnologia. Também nem sempre são acessíveis a pessoas com
deficiência visual.

> A equipe ainda não realizou pesquisa de campo nem entrevistas. Esta descrição é
> a hipótese de trabalho do projeto, não um resultado de pesquisa.

## A proposta: o totem PontoVoz

Um totem instalado no ponto de ônibus, que o passageiro usa por voz ou por toque:

1. **Previsão de chegada:** quantos minutos faltam para cada linha.
2. **Última passagem registrada:** quando houver registro, sempre com o nível de
   confirmação e sem apresentar registros não verificados como dados oficiais.
3. **Alternativas de deslocamento:** linhas diretas da parada até um destino,
   comparadas pelo tempo total estimado (espera + percurso).
4. **Interação por voz e IA conversacional:** o passageiro pergunta em linguagem
   natural e ouve a resposta.

## Componentes previstos

| Componente | Responsabilidade | Situação |
|---|---|---|
| Backend C# / ASP.NET Core (`backend/`) | API REST, regras de negócio, persistência com EF Core e SQLite | Em desenvolvimento (B0 concluído) |
| Interface do totem | Tela de consulta acessível | Responsabilidade de outro integrante — pendente |
| Voz e IA conversacional | Reconhecimento de fala, diálogo e síntese de voz, consumindo a API | Responsabilidade de outro integrante — pendente |
| Contratos de integração (`contracts/`) | Documentação dos endpoints para os demais componentes | Estrutura inicial criada |

## Contribuição específica do backend C#

> **Escopo planejado.** Nenhum dos itens abaixo está implementado ainda; hoje só
> existem a estrutura da API e o endpoint `GET /health`. O andamento de cada item
> será atualizado aqui conforme os blocos forem concluídos.

- Cadastro e consulta de paradas, linhas e da associação entre elas.
- Previsões de chegada por meio de um provedor trocável: um provedor simulado
  identificado como tal e, opcionalmente, um adaptador para a API Olho Vivo da SPTrans.
- Registros de passagem com nível de confirmação explícito. No MVP nenhum
  registro é apresentado como oficial da SPTrans.
- Alternativas de deslocamento por linhas diretas, ordenadas pelo tempo total estimado.
- Separação entre consultas públicas e operações administrativas protegidas.
- Documentação OpenAPI/Scalar para integração com o totem e com a IA.

Detalhes técnicos e instruções de execução: [backend/README.md](backend/README.md).

## Objetivos de Desenvolvimento Sustentável (ODS)

| ODS | Meta relacionada | Justificativa |
|---|---|---|
| **ODS 10 — Redução das desigualdades** | 10.2: promover a inclusão social de todos, independentemente de idade, deficiência ou condição econômica | A informação de transporte deixa de depender de smartphone, plano de dados ou leitura de tela pequena. A interação por voz amplia o acesso de pessoas idosas e com deficiência visual. |
| **ODS 11 — Cidades e comunidades sustentáveis** | 11.2: proporcionar acesso a sistemas de transporte seguros, acessíveis e sustentáveis, com atenção às pessoas em situação de vulnerabilidade | Informação acessível no próprio ponto torna o transporte público mais previsível e fácil de usar, o que favorece seu uso. |

> A redação final das justificativas ainda depende de aprovação da equipe.

## Indicadores de impacto

Nenhum indicador de impacto foi medido até agora. A lista separa o que poderá ser
verificado no protótipo do que só poderia ser medido em um piloto real.

| Indicador | Tipo | Situação |
|---|---|---|
| Tempo de resposta da API nas consultas do totem | Técnico, verificável no MVP | Pendente (medido a partir dos blocos de API) |
| Percentual de consultas de previsão respondidas com dados disponíveis | Técnico, verificável no MVP com dados simulados | Pendente |
| Testes automatizados aprovados | Qualidade, verificável no MVP | Pendente |
| Número de consultas feitas por pessoas sem smartphone | Impacto, exige piloto real | **Meta para piloto — sem dados coletados** |
| Redução do tempo de espera percebido no ponto | Impacto, exige pesquisa com usuários | **Meta para piloto — sem dados coletados** |
| Avaliação de acessibilidade por pessoas com deficiência visual | Impacto, exige pesquisa com usuários | **Meta para piloto — sem dados coletados** |

## Dados utilizados

O projeto separa explicitamente dados reais, públicos e simulados. Nenhum dado
pessoal é coletado. Veja [docs/dados.md](docs/dados.md).

## Uso de inteligência artificial

O uso de ferramentas de IA no desenvolvimento é registrado em [prompts.md](prompts.md).

## Integrantes

| Nome | RM | Disciplina / componente |
|---|---|---|
| Pendente | Pendente | C# Software Development — backend |
| Pendente | Pendente | Pendente |
| Pendente | Pendente | Pendente |

## Estrutura do repositório

```
GS-CitizenTech-PontoVoz/
├─ README.md          ← este arquivo (visão geral e ficha CitizenTech)
├─ prompts.md         ← registro do uso de IA
├─ docs/dados.md      ← dados reais, públicos e simulados
├─ contracts/         ← contratos de integração entre os componentes
└─ backend/           ← solução .NET 10 (executável de forma isolada)
```
