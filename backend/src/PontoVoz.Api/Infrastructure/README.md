# Infrastructure

Implementações técnicas das interfaces definidas em `Application`:

- `Data/`: `DbContext` do EF Core, configurações das tabelas e seed de dados simulados.
- `Repositories/`: consultas LINQ sobre o SQLite.
- `Providers/`: provedores de previsão de chegada (simulado e adaptador HTTP do Olho Vivo).

Estado: **vazio**. Os pacotes do EF Core 10 (SQLite e Design) já estão instalados,
mas ainda não existem `DbContext`, migrations nem banco de dados (blocos B3 em diante).
