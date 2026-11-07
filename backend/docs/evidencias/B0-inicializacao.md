# Evidências — Bloco B0 (inicialização)

Data da execução: 06/11/2026 (horário local de São Paulo), no Windows 11, a partir da pasta `backend/`.
Os resultados abaixo foram copiados das saídas reais dos comandos.

## SDK

```text
> dotnet --version
10.0.403
```

`global.json`: `version 10.0.100`, `rollForward latestFeature`. O SDK 10.0.403 instalado foi selecionado.

## Ferramenta local do EF Core

```text
> dotnet tool restore
A ferramenta 'dotnet-ef' (versão '10.0.14') foi restaurada. Comandos disponíveis: dotnet-ef
A restauração foi bem-sucedida.

> dotnet ef --version
Entity Framework Core .NET Command-line Tools
10.0.14
```

## Restore e build

```text
> dotnet restore PontoVoz.slnx
Todos os projetos estão atualizados para restauração.   (código de saída 0)

> dotnet build PontoVoz.slnx --no-restore
PontoVoz.Api -> ...\src\PontoVoz.Api\bin\Debug\net10.0\PontoVoz.Api.dll
PontoVoz.Tests -> ...\tests\PontoVoz.Tests\bin\Debug\net10.0\PontoVoz.Tests.dll
Compilação com êxito.
    0 Aviso(s)
    0 Erro(s)
```

## Testes

```text
> dotnet test PontoVoz.slnx --no-build
1 arquivos de teste no total corresponderam ao padrão especificado.
Não há nenhum teste disponível em ...\PontoVoz.Tests.dll.   (código de saída 0)
```

**Interpretação:** o projeto de testes compila e o executor funciona, mas **0 testes foram
encontrados e 0 executados**. Isso não significa testes aprovados: ainda não existem regras
de negócio para testar. O teste vazio criado pelo template foi removido de propósito.

## Execução da API

```text
> dotnet run --project src/PontoVoz.Api --launch-profile http --no-build
Now listening on: http://localhost:5080
Application started. Press Ctrl+C to shut down.
Hosting environment: Development
```

Endereços em escuta na porta 5080: `127.0.0.1` e `::1` (somente loopback).

### GET /health

```text
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"status":"Healthy","escopo":"processo","verificadoEm":"2026-11-07T01:39:15.6929158+00:00","verificacoes":[]}
```

O `escopo: "processo"` e a lista `verificacoes` vazia indicam que **apenas o processo** foi
verificado. Ainda não há banco de dados.

### GET /openapi/v1.json

```text
HTTP 200 | application/json; charset=utf-8
openapi=3.1.1 | title=PontoVoz.Api | v1 | version=1.0.0
paths: /health
schemas: SaudeResponse, VerificacaoSaudeResponse
```

### GET /scalar

```text
GET /scalar   -> HTTP 302, redireciona para http://localhost:5080/scalar/
GET /scalar/  -> HTML com <title>Scalar API Reference</title>, carregando openapi/v1.json
```

A página foi aberta no navegador e renderizou a documentação com a operação `GET /health`:
[B0-scalar.jpg](B0-scalar.jpg).

Depois das verificações, o processo da API foi encerrado e a porta 5080 ficou livre.

---

## B0.1 — Auditoria e revalidação (06/11/2026)

### Ajustes feitos na auditoria

- `xunit` 2.9.3 estava marcado como obsoleto ("Legacy") no NuGet. Foi trocado por
  `xunit.v3` 4.0.2, com `xunit.runner.visualstudio` 4.0.1 e `OutputType Exe`.
- O .NET 10 SDK recusou o `dotnet test` no modo VSTest com o xUnit v3
  ("Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK").
  A Microsoft.Testing.Platform foi declarada em `global.json`.
- `coverlet.collector` foi removido: é um coletor do VSTest e não funciona na nova plataforma.
- `.gitignore`: incluídos `*.sqlite-*` e `*.sqlite3-*` (arquivos WAL, SHM e journal).

### Validação final (a partir de `backend/`)

```text
> dotnet --version                 10.0.403
> dotnet restore PontoVoz.slnx     Todos os projetos estão atualizados para restauração. (código 0)
> dotnet build PontoVoz.slnx       Compilação com êxito. 0 Aviso(s), 0 Erro(s)        (código 0)
> dotnet list package --deprecated nenhum pacote preterido nos dois projetos
> dotnet list package --vulnerable nenhum pacote vulnerável nos dois projetos
> dotnet tool restore              dotnet-ef 10.0.14 restaurada                         (código 0)
> dotnet ef --version              Entity Framework Core .NET Command-line Tools 10.0.14
> dotnet test
  Zero testes executados
  total: 0 | falhou: 0 | concluído com êxito: 0 | ignorados: 0
  Código de saída: 8
```

**Interpretação:** **0 testes encontrados e 0 executados.** O código 8 da
Microsoft.Testing.Platform significa "nenhum teste executado", não um teste com falha.
Não há testes aprovados porque ainda não existem testes.

### API (perfil `http`)

```text
Now listening on: http://localhost:5080   (escuta somente em 127.0.0.1 e ::1)
GET /health          HTTP 200  {"status":"Healthy","escopo":"processo","verificadoEm":"2026-11-07T01:49:21.9158024+00:00","verificacoes":[]}
GET /openapi/v1.json HTTP 200  openapi=3.1.1  paths=/health
GET /scalar/         HTTP 200
```

O processo foi encerrado após a verificação, e a porta 5080 ficou livre.
