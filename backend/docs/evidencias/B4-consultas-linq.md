# Evidências — Bloco B4 (repositórios, interfaces e consultas LINQ)

Data da execução: 07/11/2026, no Windows 11, com o .NET SDK 10.0.403, o EF Core 10.0.14 e o
`dotnet-ef` 10.0.14, a partir da pasta `backend/`. Os resultados foram copiados das saídas reais.

## Migration `AddSearchColumns`

```text
> dotnet ef migrations add AddSearchColumns --project src/PontoVoz.Api --output-dir Infrastructure/Persistence/Migrations
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

A migration gerada só adiciona a coluna `TextoBusca` (`TEXT NOT NULL DEFAULT ''`) em `Paradas`,
`Linhas` e `Destinos`. No SQLite, `ADD COLUMN` não recria a tabela, então os dados existentes não
são copiados nem perdidos. A migration `InitialCreate` não foi alterada.

### Aplicação a uma cópia isolada do banco de desenvolvimento

O `pontovoz.db` foi copiado com a API de backup do SQLite para a pasta temporária, e a migration
foi aplicada à cópia (`ConnectionStrings__PontoVoz` apontando para ela):

```text
Applying migration '20261107222533_AddSearchColumns'.
Done.
```

Na cópia, os 21 registros do seed continuaram iguais, as 2 passagens mantiveram os horários e a
coluna `TextoBusca` foi preenchida em todas as paradas, linhas e destinos.

### Aplicação ao banco de desenvolvimento

```text
> dotnet ef database update --project src/PontoVoz.Api
Applying migration '20261107222533_AddSearchColumns'.
Done.

> dotnet ef database update --project src/PontoVoz.Api --no-build
No migrations were applied. The database is already up to date.
Done.

> dotnet ef migrations list --project src/PontoVoz.Api --no-build
20261107211456_InitialCreate
20261107222533_AddSearchColumns

> dotnet ef migrations has-pending-model-changes --project src/PontoVoz.Api --no-build
No changes have been made to the model since the last migration.
```

Inspeção do banco de desenvolvimento depois da migration (módulo `sqlite3` do Python, somente
leitura; os planos vêm de `EXPLAIN QUERY PLAN`):

```text
Migrations: ['20261107211456_InitialCreate', '20261107222533_AddSearchColumns']
Paradas 2
Linhas 4
ParadasLinhas 5
Destinos 3
PercursosEstimados 5
RegistrosPassagem 2
Passagens: [(1, 'Simulada', 'SIM-OBS-001', 639296820274932739, 639296829274932739), (2, 'Simulada', 'SIM-OBS-002', 639296814274932739, 639296829274932739)]
TextoBusca Paradas: ['parada demonstracao centro\nsim-p001', 'parada demonstracao avenida\nsim-p002']
TextoBusca Linhas: ['sim-l001\nsim-101\nterminal demonstracao centro\nterminal demonstracao norte', 'sim-l002\nsim-102\nterminal demonstracao centro\nterminal demonstracao norte', 'sim-l004\nsim-104\nterminal demonstracao centro\nterminal demonstracao norte', 'sim-l003\nsim-203\nterminal demonstracao centro\nterminal demonstracao sul']
TextoBusca Destinos: ['terminal demonstracao centro', 'terminal demonstracao sul', 'terminal demonstracao norte']
Vazios: 0
foreign_key_check: []
Plano busca por termo: ['SCAN Paradas', 'USE TEMP B-TREE FOR ORDER BY']
Plano codigoExterno: ['SEARCH Paradas USING INDEX IX_Paradas_CodigoExterno (CodigoExterno=?)']
Plano passagens da parada: ['SEARCH RegistrosPassagem USING INDEX IX_RegistrosPassagem_ParadaId_LinhaId_ObservadaEmUtc (ParadaId=? AND LinhaId=? AND ObservadaEmUtc<?)']
```

- **Registros:** as contagens e os horários das passagens (`ObservadaEmUtc`, `RegistradaEmUtc`)
  são os mesmos registrados nas evidências do B3. Nenhum registro foi renovado, apagado ou
  duplicado, e nenhuma coluna `TextoBusca` ficou vazia.
- **Busca por termo:** `SCAN Paradas`. Um `LIKE '%termo%'` não usa índice, por isso nenhum índice
  foi criado para `TextoBusca`. Com o volume do MVP (dados de demonstração), a varredura é
  adequada.
- **`codigoExterno`:** usa o índice único `IX_Paradas_CodigoExterno`.
- **Passagens por parada e linha:** usam o índice `IX_RegistrosPassagem_ParadaId_LinhaId_ObservadaEmUtc`.

No banco de desenvolvimento, provisionado no B3, as linhas `SIM-L003` e `SIM-L004` estão com os
ids 4 e 3, e o banco funciona normalmente. Os ids são gerados pelo banco e não são garantidos
(revisão do B4.1, abaixo).

## SQL gerado pelo EF Core

Capturado pelo log do EF Core (`CommandExecuted`, com os valores dos parâmetros) ao executar cada
método dos repositórios contra um SQLite em memória, criado pelas migrations e com o seed.
Filtros, ordenação, contagem, paginação e a escolha da última passagem acontecem no SQL; nenhuma
consulta traz a tabela para filtrar em memória.

### `ParadaRepository.BuscarAtivasAsync(termo: "DEMONSTRAÇÃO", pagina: 1, tamanhoPagina: 1)`

Resultado: totalItens=2, totalPaginas=2, itens=[Parada Demonstração Avenida]

```sql
      Executed DbCommand (0ms) [Parameters=[@padrao='%demonstracao%' (Size = 14)], CommandType='Text', CommandTimeout='30']
      SELECT COUNT(*)
      FROM "Paradas" AS "p"
      WHERE "p"."Ativa" AND "p"."TextoBusca" LIKE @padrao ESCAPE '\'

      Executed DbCommand (0ms) [Parameters=[@padrao='%demonstracao%' (Size = 14), @p3='1', @p='0'], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Ativa", "p"."CodigoExterno", "p"."CriadoEmUtc", "p"."DadosSimulados", "p"."Endereco", "p"."Nome", "p"."Origem", "p"."TextoBusca", "p"."Latitude", "p"."Longitude"
      FROM "Paradas" AS "p"
      WHERE "p"."Ativa" AND "p"."TextoBusca" LIKE @padrao ESCAPE '\'
      ORDER BY "p"."Nome", "p"."Id"
      LIMIT @p3 OFFSET @p
```

### `ParadaRepository.BuscarAtivasAsync(codigoExterno: "sim-p001")`

Resultado: itens=[1:SIM-P001]

```sql
      Executed DbCommand (0ms) [Parameters=[@codigo='SIM-P001' (Size = 8)], CommandType='Text', CommandTimeout='30']
      SELECT COUNT(*)
      FROM "Paradas" AS "p"
      WHERE "p"."Ativa" AND "p"."CodigoExterno" = @codigo

      Executed DbCommand (0ms) [Parameters=[@codigo='SIM-P001' (Size = 8), @p2='20', @p='0'], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Ativa", "p"."CodigoExterno", "p"."CriadoEmUtc", "p"."DadosSimulados", "p"."Endereco", "p"."Nome", "p"."Origem", "p"."TextoBusca", "p"."Latitude", "p"."Longitude"
      FROM "Paradas" AS "p"
      WHERE "p"."Ativa" AND "p"."CodigoExterno" = @codigo
      ORDER BY "p"."Nome", "p"."Id"
      LIMIT @p2 OFFSET @p
```

### `LinhaRepository.ListarAtivasDaParadaAsync(paradaId: 1)`

Resultado: 1:SIM-L001, 2:SIM-L002, 4:SIM-L004

```sql
      Executed DbCommand (0ms) [Parameters=[@paradaId='1'], CommandType='Text', CommandTimeout='30']
      SELECT "l"."Id", "l"."Ativa", "l"."Codigo", "l"."CriadoEmUtc", "l"."DadosSimulados", "l"."Letreiro", "l"."Origem", "l"."Sentido", "l"."TerminalPrincipal", "l"."TerminalSecundario", "l"."TextoBusca"
      FROM "ParadasLinhas" AS "p"
      INNER JOIN "Linhas" AS "l" ON "p"."LinhaId" = "l"."Id"
      WHERE "p"."ParadaId" = @paradaId AND "l"."Ativa"
      ORDER BY "l"."Codigo", "l"."Id"
```

### `LinhaRepository.AtendeParadaAsync(paradaId: 1, linhaId: 4)`

Resultado: True

```sql
      Executed DbCommand (0ms) [Parameters=[@paradaId='1', @linhaId='4'], CommandType='Text', CommandTimeout='30']
      SELECT EXISTS (
          SELECT 1
          FROM "ParadasLinhas" AS "p"
          WHERE "p"."ParadaId" = @paradaId AND "p"."LinhaId" = @linhaId)
```

### `DestinoRepository.BuscarAtivosAsync(termo: "norte")`

Resultado: 3:Terminal Demonstração Norte

```sql
      Executed DbCommand (0ms) [Parameters=[@padrao='%norte%' (Size = 7)], CommandType='Text', CommandTimeout='30']
      SELECT COUNT(*)
      FROM "Destinos" AS "d"
      WHERE "d"."Ativo" AND "d"."TextoBusca" LIKE @padrao ESCAPE '\'

      Executed DbCommand (0ms) [Parameters=[@padrao='%norte%' (Size = 7), @p3='20', @p='0'], CommandType='Text', CommandTimeout='30']
      SELECT "d"."Id", "d"."Ativo", "d"."CriadoEmUtc", "d"."DadosSimulados", "d"."Nome", "d"."Origem", "d"."TextoBusca", "d"."Tipo"
      FROM "Destinos" AS "d"
      WHERE "d"."Ativo" AND "d"."TextoBusca" LIKE @padrao ESCAPE '\'
      ORDER BY "d"."Nome", "d"."Id"
      LIMIT @p3 OFFSET @p
```

### `DestinoRepository.ListarPercursosDiretosAsync(paradaId: 1, destinoId: 3)`

Resultado: SIM-L004=20min, SIM-L001=30min, SIM-L002=40min

```sql
      Executed DbCommand (0ms) [Parameters=[@paradaId='1', @destinoId='3'], CommandType='Text', CommandTimeout='30']
      SELECT "p"."ParadaId", "p"."LinhaId", "p"."DestinoId", "p"."DadosSimulados", "p"."MinutosEstimados", "p"."Origem", "l"."Id", "l"."Ativa", "l"."Codigo", "l"."CriadoEmUtc", "l"."DadosSimulados", "l"."Letreiro", "l"."Origem", "l"."Sentido", "l"."TerminalPrincipal", "l"."TerminalSecundario", "l"."TextoBusca"
      FROM "PercursosEstimados" AS "p"
      INNER JOIN "Paradas" AS "p0" ON "p"."ParadaId" = "p0"."Id"
      INNER JOIN "Linhas" AS "l" ON "p"."LinhaId" = "l"."Id"
      INNER JOIN "Destinos" AS "d" ON "p"."DestinoId" = "d"."Id"
      WHERE "p"."ParadaId" = @paradaId AND "p"."DestinoId" = @destinoId AND "p0"."Ativa" AND "l"."Ativa" AND "d"."Ativo"
      ORDER BY "p"."MinutosEstimados", "l"."Codigo"
```

### `RegistroPassagemRepository.ListarUltimasPorLinhaAsync(paradaId: 1, referencia: instante do provisionamento)`

Resultado: PassagemSimulada SIM-L002 2026-11-08T11:45:00.0000000+00:00

```sql
      Executed DbCommand (1ms) [Parameters=[@paradaId='1', @referencia='639297360000000000'], CommandType='Text', CommandTimeout='30']
      SELECT "r"."Id", "r"."CriadoEmUtc", "r"."LinhaId", "r"."ObservadaEmUtc", "r"."ParadaId", "r"."PrefixoVeiculo", "r"."RegistradaEmUtc", "r"."TipoPassagem", "r"."DadosSimulados", "r"."OrigemRegistro", "r"."ChaveSeed", "l1"."Id", "l1"."Ativa", "l1"."Codigo", "l1"."CriadoEmUtc", "l1"."DadosSimulados", "l1"."Letreiro", "l1"."Origem", "l1"."Sentido", "l1"."TerminalPrincipal", "l1"."TerminalSecundario", "l1"."TextoBusca"
      FROM "RegistrosPassagem" AS "r"
      INNER JOIN "Linhas" AS "l1" ON "r"."LinhaId" = "l1"."Id"
      WHERE "r"."Id" IN (
          SELECT (
              SELECT "r1"."Id"
              FROM "RegistrosPassagem" AS "r1"
              INNER JOIN "Linhas" AS "l0" ON "r1"."LinhaId" = "l0"."Id"
              WHERE "r1"."ParadaId" = @paradaId AND "r1"."ObservadaEmUtc" <= @referencia AND "l0"."Ativa" AND "r0"."LinhaId" = "r1"."LinhaId"
              ORDER BY "r1"."ObservadaEmUtc" DESC, "r1"."RegistradaEmUtc" DESC, "r1"."Id" DESC
              LIMIT 1)
          FROM "RegistrosPassagem" AS "r0"
          INNER JOIN "Linhas" AS "l" ON "r0"."LinhaId" = "l"."Id"
          WHERE "r0"."ParadaId" = @paradaId AND "r0"."ObservadaEmUtc" <= @referencia AND "l"."Ativa"
          GROUP BY "r0"."LinhaId"
      )
      ORDER BY "r"."ObservadaEmUtc" DESC, "l1"."Codigo"
```

## Testes

```text
> dotnet test
Resumo da execução de teste: Aprovado!
  total: 387
  falhou: 0
  concluído com êxito: 387
  ignorados: 0
```

Código de saída: 0. Build: 0 avisos, 0 erros. Os 17 casos J1 a J5 e T1 a T7 continuam aprovados.

| Classe | Testes | Bloco |
|---|---|---|
| Domínio (10 classes) | 278 | B2 |
| `HealthEndpointTests` | 3 | B3.1 |
| `EsquemaMigrationTests`, `PersistenciaCadastrosTests`, `PersistenciaPassagensTests`, `AuditoriaPersistenciaTests` | 31 | B3 e B3.1 |
| `ProvisionamentoDemonstracaoTests` | 9 (8 do B3 e 1 novo do B4, substituído no B4.1) | B3 e B4 |
| `TextoBuscaTests` | 19 | B4 |
| `RepositoriosConsultaTests` | 34 | B4 |
| `UltimasPassagensTests` | 9 | B4 |
| `MigracaoBuscaTests` | 4 | B4 |

Dois testes do B3 foram atualizados porque a lista de migrations aplicadas passou a ter duas
entradas.

### Mutações

Mudanças temporárias no código, depois restauradas e conferidas com `cmp` (arquivo idêntico ao
original):

| Mutação | Testes que falham |
|---|---|
| Sem o filtro `ObservadaEmUtc <= referencia` | 2 |
| Termo da busca sem normalização | 6 |
| Sem o desempate por `RegistradaEmUtc` | 2 |
| `SaveChanges` sem atualizar `TextoBusca` | 80 |
| Linhas inativas na lista da parada | 1 |
| Sem o preenchimento dos registros antigos | 1 |
| Seed adicionando as paradas antes das linhas (teste removido no B4.1) | 1 |

## Ambiente limpo

Os arquivos versionáveis da árvore atual (sem `bin/`, `obj/` e sem banco) foram copiados para uma
pasta temporária, e o roteiro do README foi executado do zero:

```text
arquivos .db: 0, bin/obj: 0
dotnet tool restore  → A restauração foi bem-sucedida.
dotnet build         → 0 Aviso(s), 0 Erro(s)
dotnet ef database update → Applying migration '20261107211456_InitialCreate'.
                            Applying migration '20261107222533_AddSearchColumns'.
                            Done.
Linhas: [(1, 'SIM-L001'), (2, 'SIM-L002'), (3, 'SIM-L003'), (4, 'SIM-L004')]
Paradas 2 | associações 5 | passagens 2 | TextoBusca vazios 0 | foreign_key_check []
dotnet test          → total: 387, falhou: 0, ignorados: 0
GET /health          → HTTP 200 {"status":"Healthy","escopo":"processo-e-dependencias",...,"verificacoes":[{"nome":"banco-de-dados","status":"Healthy"}]}
GET /scalar/         → 200
```

A pasta temporária foi removida. Esta execução foi feita com o código final do B4. Uma execução
anterior, antes da correção da ordem do seed, teve 386 testes aprovados. No banco de
desenvolvimento, a API também foi verificada: `GET /openapi/v1.json` lista só `/health`, e
nenhum endpoint de negócio foi criado.

## Problemas encontrados e soluções

1. **Página vazia com `new Paginacao()`.** Os testes de paginação mostraram `LIMIT @p OFFSET @p`
   com `@p = 0`. A causa era a primeira versão de `Paginacao`, uma `record struct`: em C#,
   `new Paginacao()` numa struct não chama o construtor com parâmetros opcionais e zera os campos
   (tamanho 0). **Solução:** `Paginacao` passou a ser um `record` de classe, e `new Paginacao()`
   recebe página 1 e tamanho 20.
2. **`Include` depois do `GroupBy`.** O EF Core não aceita `Include` sobre o resultado de
   `GroupBy(...).Select(g => g...First())`. **Solução:** o agrupamento escolhe só o `Id` da
   última passagem de cada linha, e a consulta externa carrega a entidade e a linha, num único
   comando SQL (acima).
3. **Ids do seed diferentes dos exemplos.** O seed gerava `SIM-L004` com id 3, porque as linhas
   entravam no rastreamento pelas associações. No B4, o seed passou a adicionar as linhas antes
   das paradas, e um teste exigia os ids 1 a 4. **Revisto no B4.1:** ids internos não são
   garantidos, então esse teste foi substituído por outro que prova o contrário: com ids
   diferentes, tudo continua correto pelos códigos (veja abaixo).

---

## B4.1 — auditoria final

Execução de 07/11/2026, a partir de `backend/`.

### Ids internos

- **Revisão:** os ids numéricos são gerados pelo banco e não são garantidos (contrato, seção 1.2).
  Nenhum repositório, regra ou teste funcional depende de um id específico. Os testes localizam
  paradas e linhas pelo código, e destinos pelo nome.
- **Correções:** o teste do B4 que exigia `SIM-L001` a `SIM-L004` = 1 a 4 foi removido, e a
  busca do destino Norte por `Id == 3` passou a usar o nome. Os comentários do seed não prometem
  mais ids. A ordem de inserção do seed não foi alterada.
- **Novo teste:** `Provisionamento_ComIdsDiferentesDosExemplos_RelacionaTudoPeloCodigo`. Cadastros
  feitos antes do seed ocupam os primeiros ids (`SIM-L001` não fica com o id 1), e mesmo assim
  as linhas da parada, os percursos e a última passagem continuam corretos pelos códigos.
- **Banco de desenvolvimento:** está com `SIM-L003` = 4 e `SIM-L004` = 3, responde
  `/health` com 200 e ficou idêntico ao registrado no B4.

### Paginação

`PaginacaoTests` (15 testes) cobre os valores padrão (`new Paginacao()` = página 1 e tamanho 20,
nunca 0), as faixas aceitas e recusadas e o `totalPaginas`. Com `Paginacao` voltando
temporariamente a ser `record struct`, 21 testes falharam, entre eles
`SemArgumentos_UsaPagina1ETamanho20_NuncaTamanhoZero`. O código foi restaurado e conferido com
`cmp`. Nenhum repositório chama `ToList()` antes dos filtros.

### Comandos

```text
> dotnet restore        → Todos os projetos estão atualizados para restauração. (código 0)
> dotnet build          → 0 Aviso(s), 0 Erro(s) (código 0)
> dotnet test
Resumo da execução de teste: Aprovado!
  total: 402
  falhou: 0
  concluído com êxito: 402
  ignorados: 0                                   (código 0)
> dotnet ef migrations list --project src/PontoVoz.Api --no-build
20261107211456_InitialCreate
20261107222533_AddSearchColumns
> dotnet ef database update --project src/PontoVoz.Api --no-build
No migrations were applied. The database is already up to date.
> dotnet ef migrations has-pending-model-changes --project src/PontoVoz.Api --no-build
No changes have been made to the model since the last migration.
```

| Filtro | Testes |
|---|---|
| `--filter-method "*CasosDoContrato"` (J1 a J5, T1 a T7) | 17 aprovados |
| `--filter-namespace "PontoVoz.Tests.Infrastructure"` | 106 aprovados |
| `--filter-namespace "PontoVoz.Tests.Application"` | 15 aprovados |
| `--filter-namespace "PontoVoz.Tests.Api"` | 3 aprovados |

Total: 278 de domínio, 15 de paginação, 106 de integração e 3 do `/health`.

### API (banco de desenvolvimento)

```text
GET /health          → HTTP 200 {"status":"Healthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T23:00:53.2149811+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Healthy"}]}
GET /openapi/v1.json → 200, paths: ['/health']
GET /scalar          → 302 · GET /scalar/ → 200
```

### Ambiente limpo

Cópia dos arquivos versionáveis (0 arquivos `.db`, 0 pastas `bin`/`obj`):

```text
dotnet tool restore, restore e build → sucesso, 0 avisos, 0 erros
dotnet ef database update → Applying migration '20261107211456_InitialCreate'.
                            Applying migration '20261107222533_AddSearchColumns'.
Banco novo: 2 paradas, 4 linhas, 5 associações, 3 destinos, 5 percursos, 2 passagens,
            nenhum TextoBusca vazio, foreign_key_check vazio
dotnet test → total: 402, falhou: 0, ignorados: 0
GET /health → HTTP 200 · paths: ['/health'] · GET /scalar/ → 200
```

A pasta temporária foi removida.
