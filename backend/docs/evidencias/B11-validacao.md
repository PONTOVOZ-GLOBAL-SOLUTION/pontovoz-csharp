# Evidências — Bloco B11 (execução sem erros e documentação da entrega)

Execução em 08/11/2026, no Windows 11, com o .NET SDK 10.0.403 (o .NET SDK 8.0.421 também está
instalado na máquina). Os resultados foram copiados das saídas reais.

O B11 não acrescentou rotas: as 14 operações continuam as mesmas. As mudanças de execução foram a
solução `PontoVoz.sln` na raiz (com `global.json` e `.config/dotnet-tools.json` movidos para a
raiz), a criação do banco ao iniciar só em Development e os atalhos `/` e `/swagger` para o Scalar,
também só em Development.

## Cópia limpa

A validação foi feita numa pasta nova, com os arquivos versionados e os novos do B11 (sem os
ignorados pelo `.gitignore`), como num clone do próximo commit: sem banco, sem `bin/` e sem `obj/`.

```text
## cópia limpa: 209 arquivos | bancos: 0 | bin/obj: 0
## SDKs instalados:
8.0.421 [C:\Program Files\dotnet\sdk]
10.0.403 [C:\Program Files\dotnet\sdk]
```

## Testes

### Na raiz do repositório

```text
## dotnet tool restore (raiz)
A restauração foi bem-sucedida.
exit 0
## dotnet build (raiz)
    0 Aviso(s)
    0 Erro(s)
exit 0
## dotnet test (raiz, com relatório TRX)
  total: 816
  falhou: 0
  concluído com êxito: 816
  ignorados: 0
exit 0
## dotnet ef migrations list (raiz)
20261107211456_InitialCreate (Pending)
20261107222533_AddSearchColumns (Pending)
exit 0
## dotnet ef migrations has-pending-model-changes (raiz)
No changes have been made to the model since the last migration.
exit 0
```

### Em `backend/`

```text
## dotnet tool restore (backend)
A restauração foi bem-sucedida.
exit 0
## dotnet build --no-incremental (backend)
    0 Aviso(s)
    0 Erro(s)
exit 0
## dotnet test (backend, com relatório TRX)
  total: 816
  falhou: 0
  concluído com êxito: 816
  ignorados: 0
exit 0
## dotnet ef migrations list (backend)
20261107211456_InitialCreate (Pending)
20261107222533_AddSearchColumns (Pending)
exit 0
```

As duas execuções de testes geraram relatórios TRX (`--report-xunit-trx`), com os contadores:

```text
raiz.trx    total="816" executed="816" passed="816" failed="0" error="0" timeout="0" aborted="0" notExecuted="0"
backend.trx total="816" executed="816" passed="816" failed="0" error="0" timeout="0" aborted="0" notExecuted="0"
```

São 816 testes: os 805 do B10, sem mudança no que verificam, e 11 novos
(`MigracaoAutomaticaTests`, com 6 casos, e os atalhos da documentação em
`InfraestruturaHttpTests`, com 5). Por grupo: 278 de domínio, 144 de aplicação, 172 de
infraestrutura e 222 de API. Os testes que conferem o `/health` com o banco ausente ou sem
migrations (`HealthEndpointTests`) foram mantidos e desligam a migração automática explicitamente.

O `global.json` da raiz vale também dentro de `backend/`: o `dotnet --info` executado em
`backend/` aponta para o arquivo da raiz, e o `dotnet test` usa a Microsoft.Testing.Platform nos
dois lugares. `dotnet ef migrations list` mostra as migrations como `Pending` porque a cópia ainda
não tinha banco nesse momento.

### Falha intermitente observada

Numa execução anterior desta mesma validação, `dotnet test` em `backend/` (logo depois de
`dotnet build --no-incremental`) terminou com **1 falha em 816** (código de saída 2). Essa
execução não guardou o nome do teste, a mensagem nem o stack trace, porque a saída foi filtrada só
para os totais. **A causa não foi identificada.**

Depois dela, na mesma cópia limpa, a suíte inteira foi executada mais 28 vezes, todas com
0 falhas: 1 repetição imediata, 6 seguidas, 4 depois de `dotnet build --no-incremental`, 15 em
sequência e as 2 desta validação, com TRX.

Fatores de risco encontrados na leitura dos testes (hipóteses, sem confirmação):

- `ResumoParadaServiceTests.ProvedorEPassagensEmParalelo_SemDuasOperacoesDeBancoAoMesmoTempo`
  depende de esperas reais (`Task.Delay` de 5 ms e de 30 ms): o provedor falso verifica, até 20
  vezes, se a consulta de passagens está em andamento. Com a máquina sobrecarregada e as classes de
  teste rodando em paralelo, essa verificação pode perder a janela.
- Três testes de cancelamento (`InfraestruturaHttpTests`, `PrevisoesHttpTests` e
  `AlternativasResumoHttpTests`) esperam uma mensagem de log por até 10 segundos de relógio real.
- `SqliteConnection.ClearAllPools()` é chamado ao descartar seis classes de teste, enquanto outras
  classes podem estar rodando em paralelo.

Nenhum teste foi desativado, ignorado ou teve o tempo de espera aumentado. Se a falha voltar a
ocorrer, o comando abaixo registra a classe, o caso e o stack trace num arquivo TRX:

```bash
dotnet test --report-xunit-trx --results-directory TestResults
```

## Banco criado na inicialização

API iniciada pela raiz com o perfil `http` do README, **sem** `dotnet ef database update`:

```text
## banco antes de iniciar: não existe
## banco depois de iniciar: existe | {'__EFMigrationsHistory': 2, 'Paradas': 2, 'Linhas': 4, 'ParadasLinhas': 5, 'Destinos': 3, 'PercursosEstimados': 5, 'RegistrosPassagem': 2}
GET /health -> 200
GET / -> 302 Location: http://localhost:5080/scalar
GET /swagger -> 302 Location: http://localhost:5080/scalar
GET /swagger/index.html -> 302 Location: http://localhost:5080/scalar
GET /scalar/ -> 200
## operações públicas
GET /api/v1/paradas?codigoExterno=SIM-P001 -> 200
GET /api/v1/paradas/1 -> 200
GET /api/v1/linhas -> 200
GET /api/v1/destinos?termo=norte -> 200
GET /api/v1/paradas/1/previsoes -> 200
GET /api/v1/paradas/1/ultimas-passagens -> 200
GET /api/v1/paradas/1/alternativas?destinoId=3 -> 200
GET /api/v1/paradas/1/resumo -> 200
## operações administrativas (Development sem Admin:Chave: permitido, com aviso no log)
POST /api/v1/admin/paradas -> 201
PUT /api/v1/admin/paradas/3 -> 200
POST /api/v1/admin/linhas -> 201
PUT /api/v1/admin/paradas/3/linhas/5 -> 204
POST /api/v1/admin/observacoes-passagem -> 201
## contagens após o fluxo: {'__EFMigrationsHistory': 2, 'Paradas': 3, 'Linhas': 5, 'ParadasLinhas': 6, 'Destinos': 3, 'PercursosEstimados': 5, 'RegistrosPassagem': 3}
```

## Reinício

```text
GET /health -> 200
## conteúdo antes/depois do reinício: a6c61693b9bf65ca / a6c61693b9bf65ca -> igual | {'__EFMigrationsHistory': 2, 'Paradas': 3, 'Linhas': 5, 'ParadasLinhas': 6, 'Destinos': 3, 'PercursosEstimados': 5, 'RegistrosPassagem': 3}
```

## Production

```text
GET /health -> 503 | GET / -> 404 | GET /scalar/ -> 404 | banco criado: não
## Production com Banco__AplicarMigracoesAoIniciar=true
GET /health -> 503 | banco criado: não | aviso no log: 1
```

Em Production o banco não é criado, mesmo com `Banco__AplicarMigracoesAoIniciar=true` (a opção é
ignorada, com um aviso no log), e os atalhos e o Scalar não existem.

## Banco de desenvolvimento

Uma cópia byte a byte do `pontovoz.db` de desenvolvimento foi aberta pela API em Development, com a
migração automática ligada; o conteúdo de todas as tabelas ficou igual (resumo SHA-256 das linhas):

```text
GET /health -> 200
## conteúdo da cópia antes/depois: 108f1d43d14bbea0 / 108f1d43d14bbea0 -> igual
```

O arquivo original não foi usado pela API; a inspeção somente leitura dele antes e depois da
validação deu o mesmo resultado (seção 8).

## Provedor indisponível

```text
GET /previsoes -> 503 Retry-After: 30
GET /alternativas -> 200 situacao: SemComparacao
GET /resumo -> 200 situacao: Parcial ['previsoes']
GET /paradas/1 -> 200 | GET /linhas -> 200 | GET /ultimas-passagens -> 200
```

## OpenAPI, verificador e segurança

```text
OpenAPI da API em execução == exportado: True | 14 operações | / ou /swagger no documento: []
## verificador: Nenhuma falha nas regras verificadas.
## chamadas à SPTrans nos logs: 0
## banco de desenvolvimento: inalterado (igual à linha de base B6)
```

## Tempo de resposta local

50 requisições seguidas por rota pública, com `curl`, na API da cópia limpa (Development, SQLite
de demonstração, mesma máquina). Mede só o backend numa máquina de desenvolvimento; não é um
indicador de impacto social nem da experiência no ponto de ônibus.

```text
GET /api/v1/paradas?codigoExterno=SIM-P001: média 6.7 ms | p95 17.7 ms | máx 29.7 ms
GET /api/v1/paradas/1: média 6.2 ms | p95 21.9 ms | máx 31.7 ms
GET /api/v1/linhas: média 7.0 ms | p95 24.3 ms | máx 31.0 ms
GET /api/v1/destinos?termo=norte: média 6.0 ms | p95 20.1 ms | máx 29.7 ms
GET /api/v1/paradas/1/previsoes: média 6.8 ms | p95 16.8 ms | máx 27.8 ms
GET /api/v1/paradas/1/ultimas-passagens: média 5.2 ms | p95 14.4 ms | máx 26.1 ms
GET /api/v1/paradas/1/alternativas?destinoId=3: média 5.1 ms | p95 19.4 ms | máx 27.5 ms
GET /api/v1/paradas/1/resumo: média 6.4 ms | p95 15.5 ms | máx 27.0 ms
```

## O que não foi verificado

- O Visual Studio 2026 (F5 com o perfil `http`): a IDE não está instalada nesta máquina. Foi
  verificada a linha de comando.
- Linux e macOS: os testes só rodaram no Windows 11.
- Uma máquina só com o .NET 8: o build falha nesse caso, como indicado no README; não foi simulado.
- GitHub Actions: não foi configurado, porque um workflow só pode ser validado depois do push.
