# Infrastructure

Implementações técnicas do backend. O domínio (`Domain/`) não depende desta pasta.

| Pasta | Conteúdo | Estado |
|---|---|---|
| `Persistence/` | `PontoVozDbContext`, mapeamentos Fluent API, conversor de instantes UTC, migrations e seed de dados fictícios | Implementado no B3 |
| `Repositories/` | Consultas LINQ sobre o SQLite | Planejado (B4) |
| `Providers/` | Provedores de previsão de chegada (simulado e adaptador HTTP do Olho Vivo) | Planejado (B8) |

Detalhes da persistência, das tabelas e do seed estão em
[`../../../README.md`](../../../README.md#persistência-b3).
