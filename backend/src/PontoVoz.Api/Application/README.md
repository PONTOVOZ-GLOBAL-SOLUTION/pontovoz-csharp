# Application

Casos de uso da API: interfaces de services e repositórios, services com as regras
de aplicação, DTOs de entrada e saída e mapeamentos entre entidades e DTOs.

- Depende de `Domain`.
- Não conhece detalhes de EF Core nem de HTTP externo (usa apenas interfaces).

| Pasta | Conteúdo | Estado |
|---|---|---|
| `Consultas/` | `Paginacao` e `Pagina<T>` | Implementado no B4 |
| `Interfaces/` | `IParadaRepository`, `ILinhaRepository`, `IDestinoRepository` e `IRegistroPassagemRepository` | Implementado no B4 |
| `Services/`, `Dtos/` | Services e DTOs | Planejado (B5) |
