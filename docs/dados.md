# Dados do PontoVoz: reais, públicos e simulados

Este documento registra a origem de cada dado usado pelo PontoVoz. A regra é
simples: **nenhum dado é apresentado como real ou oficial sem que a fonte tenha
sido verificada.**

> **Estado atual (Bloco B0):** o projeto ainda não armazena nem consome nenhum dado.
> As seções abaixo definem as regras que serão aplicadas a partir dos próximos blocos.

## Categorias

| Categoria | Definição | Situação no projeto |
|---|---|---|
| **Dados reais** | Obtidos de uma fonte verificável sobre a operação real do transporte | Nenhum até o momento |
| **Dados públicos** | Publicados oficialmente por órgãos ou empresas, com acesso permitido | Nenhum integrado até o momento (ver Olho Vivo abaixo) |
| **Dados simulados** | Criados pela equipe só para demonstração | Serão criados no bloco B3 |
| **Dados pessoais** | Informações que identificam pessoas | **Não serão coletados** |

## Dados simulados (planejado)

- Paradas, linhas, destinos, percursos e registros de passagem de demonstração.
- Todos fictícios: códigos com prefixo `SIM-` e nomes como "Parada Demonstração Centro".
- Nenhum nome, código, endereço ou horário real da SPTrans será copiado ou inventado como se fosse real.
- Toda resposta da API com dados simulados terá `dadosSimulados: true` e um aviso textual.

## Dados públicos: API Olho Vivo da SPTrans (a investigar)

- É uma possível fonte de previsões de chegada.
- **A equipe não possui credenciais de acesso** e ainda não leu os termos de uso.
- O backend terá um adaptador desligado por padrão. Ele só será declarado
  funcionando depois de testado contra a API real, com data e resultado registrados aqui.

| Item | Situação |
|---|---|
| Token de acesso | Não solicitado |
| Termos de uso | Não verificados |
| Teste real da integração | Não realizado |

## Registros de passagem: níveis de confirmação (planejado)

| Nível | Significado | Pode ser apresentado como oficial? |
|---|---|---|
| `Previsao` | Estimativa de um provedor de previsões; não é uma passagem | Não |
| `ObservacaoSimulada` | Criado pelo seed de demonstração | Não |
| `RegistroManual` | Informado por um operador, sem verificação independente | Não |
| `VerificadaFonteConfiavel` | Confirmado por fonte confiável; reservado, não emitido no MVP | Só com fonte oficial verificada |

## Dados pessoais

O PontoVoz não precisa identificar passageiros e não coletará nomes, documentos,
e-mails, telefones, imagens ou gravações de voz no backend. Registros manuais usam
apenas um rótulo técnico do operador (por exemplo, `operador-local`), nunca um nome.
