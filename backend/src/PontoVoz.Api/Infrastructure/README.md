# Infrastructure

Implementações técnicas do backend. O domínio (`Domain/`) não depende desta pasta.

| Pasta | Conteúdo | Estado |
|---|---|---|
| `Persistence/` | `PontoVozDbContext`, mapeamentos Fluent API, conversor de instantes UTC, coluna de busca normalizada, migrations e seed de dados fictícios | Implementado no B3 e no B4 |
| `Repositories/` | Implementações das interfaces de `Application/Interfaces`, com EF Core e LINQ. No B7, as mesmas classes implementam as interfaces de escrita (`ICadastro*Repository`), e `UnidadeTrabalhoEf` grava as mudanças, convertendo só a violação de chave única ou primária do SQLite em `ViolacaoUnicidadeException` | Implementado no B4 e no B7 |
| `Providers/` | Provedores de previsão de chegada: `ProvedorPrevisaoSimulado` (padrão, calculado no instante da consulta), `ProvedorOlhoVivo` (adaptador HTTP da SPTrans, desligado e testado só com respostas simuladas), `SeletorProvedorPrevisao` (escolha por configuração; dados `SIM-` sempre no simulado), `OpcoesPrevisoes` e o registro na injeção de dependência | Implementado no B8 |

Detalhes da persistência, das tabelas e do seed estão em
[`../../../README.md`](../../../README.md#persistência-b3), e as consultas, em
[`../../../README.md`](../../../README.md#consultas-linq-b4).
