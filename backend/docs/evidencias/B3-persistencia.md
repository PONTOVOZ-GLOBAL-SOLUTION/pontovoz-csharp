# Evidências — Bloco B3 (persistência com EF Core e SQLite)

Data da execução: 07/11/2026, no Windows 11, com o .NET SDK 10.0.403 e o `dotnet-ef` 10.0.14
(ferramenta local), a partir da pasta `backend/`. Os resultados abaixo foram copiados das
saídas reais dos comandos. Os logs do EF Core foram omitidos onde indicado.

## Migration criada

```text
> dotnet ef migrations add InitialCreate --project src/PontoVoz.Api --output-dir Infrastructure/Persistence/Migrations --namespace PontoVoz.Api.Infrastructure.Persistence.Migrations
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

Arquivos gerados: `20261107211456_InitialCreate.cs`, `20261107211456_InitialCreate.Designer.cs` e
`PontoVozDbContextModelSnapshot.cs`. A migration foi revisada antes de ser aplicada.

**Problema encontrado:** com o parâmetro `--namespace`, o `dotnet-ef` gravou o snapshot em
`src/PontoVoz.Api/PontoVoz/Api/Infrastructure/Persistence/Migrations/`, uma pasta derivada do
namespace. **Solução:** o arquivo foi movido para `Infrastructure/Persistence/Migrations/`, junto
da migration (o namespace já estava correto), e as pastas vazias foram removidas. Depois disso:

```text
> dotnet ef migrations has-pending-model-changes --project src/PontoVoz.Api --no-build
No changes have been made to the model since the last migration.
```

## Migration aplicada

Antes do comando, não existia nenhum arquivo `.db` no projeto.

```text
> dotnet ef database update --project src/PontoVoz.Api
(logs dos comandos SQL omitidos: CREATE TABLE, CREATE INDEX e os INSERT do seed)
Done.

> dotnet ef migrations list --project src/PontoVoz.Api --no-build
20261107211456_InitialCreate
```

O banco foi criado em `src/PontoVoz.Api/pontovoz.db`, arquivo ignorado pelo Git (`*.db` no
`.gitignore`).

## Esquema e dados do primeiro provisionamento

Lidos do arquivo `pontovoz.db` com o módulo `sqlite3` do Python, em modo somente leitura
(`sqlite_master`, `PRAGMA table_info`, `PRAGMA foreign_key_list`, `PRAGMA index_list` e
`PRAGMA foreign_key_check`):

```text
Migrations: ['20261107211456_InitialCreate']
Tabelas: ['Destinos', 'Linhas', 'Paradas', 'ParadasLinhas', 'PercursosEstimados', 'RegistrosPassagem', '__EFMigrationsHistory', '__EFMigrationsLock']

[Destinos] 3 registros
  colunas: Id INTEGER PK, Nome TEXT, Tipo TEXT, Ativo INTEGER, Origem TEXT, DadosSimulados INTEGER, CriadoEmUtc INTEGER
  FKs: -
  índices: IX_Destinos_Nome UNIQUE

[Linhas] 4 registros
  colunas: Id INTEGER PK, Codigo TEXT, Letreiro TEXT, TerminalPrincipal TEXT, TerminalSecundario TEXT, Sentido TEXT, Ativa INTEGER, Origem TEXT, DadosSimulados INTEGER, CriadoEmUtc INTEGER
  FKs: -
  índices: IX_Linhas_Codigo UNIQUE

[Paradas] 2 registros
  colunas: Id INTEGER PK, CodigoExterno TEXT, Nome TEXT, Endereco TEXT NULL, Ativa INTEGER, Origem TEXT, DadosSimulados INTEGER, Latitude REAL NULL, Longitude REAL NULL, CriadoEmUtc INTEGER
  FKs: -
  índices: IX_Paradas_CodigoExterno UNIQUE

[ParadasLinhas] 5 registros
  colunas: ParadaId INTEGER PK, LinhaId INTEGER PK, AssociadaEmUtc INTEGER
  FKs: LinhaId->Linhas.Id (RESTRICT); ParadaId->Paradas.Id (RESTRICT)
  índices: IX_ParadasLinhas_LinhaId, sqlite_autoindex_ParadasLinhas_1 UNIQUE

[PercursosEstimados] 5 registros
  colunas: ParadaId INTEGER PK, LinhaId INTEGER PK, DestinoId INTEGER PK, MinutosEstimados INTEGER, Origem TEXT, DadosSimulados INTEGER
  FKs: DestinoId->Destinos.Id (RESTRICT); LinhaId->Linhas.Id (RESTRICT); LinhaId->ParadasLinhas.LinhaId (RESTRICT); ParadaId->Paradas.Id (RESTRICT); ParadaId->ParadasLinhas.ParadaId (RESTRICT)
  índices: IX_PercursosEstimados_LinhaId, IX_PercursosEstimados_DestinoId, sqlite_autoindex_PercursosEstimados_1 UNIQUE

[RegistrosPassagem] 2 registros
  colunas: Id INTEGER PK, ParadaId INTEGER, LinhaId INTEGER, ObservadaEmUtc INTEGER, RegistradaEmUtc INTEGER, PrefixoVeiculo TEXT NULL, TipoPassagem TEXT, DadosSimulados INTEGER NULL, OrigemRegistro TEXT NULL, ChaveSeed TEXT NULL, CriadoEmUtc INTEGER
  FKs: LinhaId->Linhas.Id (RESTRICT); LinhaId->ParadasLinhas.LinhaId (RESTRICT); ParadaId->Paradas.Id (RESTRICT); ParadaId->ParadasLinhas.ParadaId (RESTRICT)
  índices: IX_RegistrosPassagem_ParadaId_LinhaId_ObservadaEmUtc, IX_RegistrosPassagem_LinhaId, IX_RegistrosPassagem_ChaveSeed UNIQUE PARCIAL

TipoPassagem: [('Simulada', 'SIM-OBS-001', None, 639296820274932739, 639296829274932739), ('Simulada', 'SIM-OBS-002', None, 639296814274932739, 639296829274932739)]
Enums gravados: [('Simulado', 'PrincipalParaSecundario')] [('Terminal',)]
Destinos: [(1, 'Terminal Demonstração Centro'), (2, 'Terminal Demonstração Sul'), (3, 'Terminal Demonstração Norte')]
foreign_keys check: []
```

Leitura:

- **Seed:** criou 21 registros: 2 paradas, 4 linhas, 5 associações, 3 destinos, 5 percursos e
  2 passagens simuladas. O destino "Terminal Demonstração Norte" ficou com o id 3, como nos
  exemplos do contrato.
- **Chaves estrangeiras:** o `PRAGMA foreign_key_list` lista cada coluna em uma linha. As FKs
  para `ParadasLinhas` são compostas (`ParadaId`, `LinhaId`) e garantem que percursos e
  passagens só existam para uma linha que atende a parada.
- **Integridade:** `foreign_key_check` vazio, ou seja, nenhuma violação de integridade.
- **Instantes:** são gravados como ticks UTC (`INTEGER`). Convertidos, os valores acima são:

  | Ticks | Instante UTC | Significado |
  |---|---|---|
  | 639296829274932739 | 07/11/2026 21:15:27.4932739 | Provisionamento (`RegistradaEmUtc`) |
  | 639296820274932739 | 07/11/2026 21:00:27.4932739 | `SIM-OBS-001`, 15 minutos antes |
  | 639296814274932739 | 07/11/2026 20:50:27.4932739 | `SIM-OBS-002`, 25 minutos antes |

- **Campos de natureza:** `DadosSimulados` e `OrigemRegistro` ficam nulos nas passagens
  simuladas. São colunas do registro manual na tabela única (TPH); nas simuladas, o valor vem do
  tipo.

## Segundo provisionamento (idempotência)

```text
> dotnet ef database update --project src/PontoVoz.Api --no-build
No migrations were applied. The database is already up to date.
Done.
```

A inspeção do banco depois desse comando foi comparada com a anterior e é **idêntica**: mesma
quantidade de registros e mesmos horários das passagens. O seed reconheceu o banco já
provisionado e não criou, renovou nem apagou nenhum registro.

## Testes

```text
> dotnet test
Resumo da execução de teste: Aprovado!
  total: 313
  falhou: 0
  concluído com êxito: 313
  ignorados: 0
```

Código de saída: 0. Build: 0 avisos, 0 erros.

| Classe | Testes | Banco usado |
|---|---|---|
| `EsquemaMigrationTests` | 4 | SQLite em memória, esquema criado pela migration |
| `PersistenciaCadastrosTests` | 13 | SQLite em memória |
| `PersistenciaPassagensTests` | 10 | SQLite em memória |
| `ProvisionamentoDemonstracaoTests` | 8 | SQLite em memória e um arquivo temporário apagado ao final |
| Testes de domínio (B2 e B2.1) | 278 | Sem banco |

Mutações temporárias, depois restauradas, confirmaram que os testes detectam as regressões:

| Mutação | Testes que falham |
|---|---|
| Sem o conversor de instantes para ticks UTC | 35 |
| Seed assíncrono sem verificar se o banco já foi provisionado | 4 |

**Problema encontrado:** nas execuções com mutação, a migração falhava dentro do utilitário de
teste e a conexão ficava aberta. Com isso, dois bancos temporários (`pontovoz-teste-*.db`)
ficaram na pasta temporária do sistema. **Solução:** o utilitário `BancoSqliteTeste` passou a
fechar a conexão quando a migração falha. Os dois arquivos, criados pelos próprios testes,
foram apagados. Uma nova execução completa (313 aprovados) não deixou nenhum arquivo temporário.

## API

Com o banco já criado, a API foi iniciada (`dotnet run --project src/PontoVoz.Api --launch-profile http`):

```text
GET /health          → {"status":"Healthy","escopo":"processo","verificadoEm":"2026-11-07T21:22:17.2830327+00:00","verificacoes":[]}
GET /openapi/v1.json → 200, paths: ['/health']
GET /scalar          → 302 (redireciona para /scalar/)
GET /scalar/         → 200
```

Nessa execução do B3, o `/health` ainda verificava apenas o processo. A verificação do banco
entrou no B3.1 (abaixo). Nenhum endpoint de negócio foi criado.

---

## B3.1 — revalidação e verificação do banco no `/health`

Execução de 07/11/2026, depois das correções do B3.1, a partir de `backend/`.

### Comandos

```text
> dotnet restore
  Todos os projetos estão atualizados para restauração.          (código 0)

> dotnet build
    0 Aviso(s)
    0 Erro(s)                                                   (código 0)

> dotnet test
Resumo da execução de teste: Aprovado!
  total: 320
  falhou: 0
  concluído com êxito: 320
  ignorados: 0                                                  (código 0)

> dotnet ef migrations list --project src/PontoVoz.Api --no-build
20261107211456_InitialCreate

> dotnet ef database update --project src/PontoVoz.Api --no-build
No migrations were applied. The database is already up to date.
Done.

> dotnet ef migrations has-pending-model-changes --project src/PontoVoz.Api --no-build
No changes have been made to the model since the last migration.
```

A inspeção do `pontovoz.db` depois desses comandos foi comparada com a do primeiro
provisionamento e é **idêntica**: as mesmas 6 tabelas, chaves, índices e 21 registros, e os
mesmos horários das passagens simuladas. Nenhuma passagem foi reinserida nem renovada. Nenhuma
classe do domínio foi alterada no B3 nem no B3.1.

| Grupo de testes | Testes |
|---|---|
| Domínio (B2 e B2.1) | 278 |
| Integração com SQLite (`PontoVoz.Tests.Infrastructure`) | 39 (35 do B3 e 4 de `AuditoriaPersistenciaTests`) |
| `/health` pela API em execução (`PontoVoz.Tests.Api`) | 3 |
| Casos J1 a J5 e T1 a T7 (`*CasosDoContrato`) | 17, todos aprovados |

### `/health` com a API real

Banco de desenvolvimento disponível:

```text
GET /health → HTTP 200
{"status":"Healthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T21:47:05.9017706+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Healthy"}]}
GET /openapi/v1.json → 200, paths: ['/health']
GET /scalar → 302 · GET /scalar/ → 200
```

Banco indisponível. A API foi iniciada com a variável de ambiente `ConnectionStrings__PontoVoz`
apontando para um arquivo inexistente na pasta temporária, sem tocar no banco de
desenvolvimento:

```text
GET /health → HTTP 503
{"status":"Unhealthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T21:48:03.2666865+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Unhealthy"}]}
```

Depois da chamada, o arquivo continuou inexistente: o health check não cria o banco. A resposta
não traz caminho, connection string nem mensagem interna.

Os testes `HealthEndpointTests` repetem os dois cenários pela API real em memória
(`WebApplicationFactory`), com um arquivo SQLite temporário por teste:

| Cenário | Resultado verificado |
|---|---|
| Banco com a migration aplicada | 200, `Healthy`, verificação `banco-de-dados` `Healthy` |
| Arquivo inexistente | 503, `Unhealthy`, arquivo não criado, corpo sem o nome do arquivo e sem `Data Source` |
| Arquivo vazio, sem migrations | 503, `Unhealthy`, nenhuma tabela criada |

### Auditoria de `DadosSimulados` das passagens

Teste `AuditoriaPersistenciaTests`, com SQLite real:

| Verificação | Resultado |
|---|---|
| Colunas gravadas (`TipoPassagem:DadosSimulados`) | `Simulada:NULL`, `RegistroManual:1`, `RegistroManual:0` |
| Objetos lidos do banco | A `PassagemSimulada` informa `DadosSimulados = true`; os manuais, `true` e `false`, conforme a parada e a linha |
| `Where(p => p.DadosSimulados)` | Não é traduzido: o EF Core lança `InvalidOperationException`, em vez de filtrar errado por causa do `NULL` |
| Filtro pelo discriminador e pela coluna do registro manual | Traduzido para SQL (`"TipoPassagem" = 'Simulada'`); devolve as 2 passagens fictícias |

Conclusão: o `NULL` é uma decisão de armazenamento válida. A passagem simulada é identificada
como fictícia pelo discriminador, e nenhuma coluna redundante foi criada.

### Busca sem acentos

No mesmo teste, com os dados do seed, o `LIKE` do SQLite trouxe:

| Termo | Paradas encontradas |
|---|---|
| `%demonstracao%` | 0 |
| `%demonstração%` | 2 |
| `%DEMONSTRAÇÃO%` | 0 |

O banco atual não atende sozinho ao R7. O plano para o B4 (colunas de busca normalizadas em uma
nova migration) está no `backend/README.md`, seção "Orientações para o B4".
