# Infrastructure

Implementações técnicas do backend. O domínio (`Domain/`) não depende desta pasta.

| Pasta | Conteúdo | Estado |
|---|---|---|
| `Persistence/` | `PontoVozDbContext`, mapeamentos Fluent API, conversor de instantes UTC, coluna de busca normalizada, migrations e seed de dados fictícios | Implementado no B3 e no B4 |
| `Repositories/` | Implementações das interfaces de `Application/Interfaces`, com EF Core e LINQ | Implementado no B4 |
| `Providers/` | Provedores de previsão de chegada (simulado e adaptador HTTP do Olho Vivo) | Planejado (B8) |

Detalhes da persistência, das tabelas e do seed estão em
[`../../../README.md`](../../../README.md#persistência-b3), e as consultas, em
[`../../../README.md`](../../../README.md#consultas-linq-b4).
