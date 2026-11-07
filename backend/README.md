# PontoVoz — Backend C# (ASP.NET Core / .NET 10)

Backend da Global Solution FIAP 2026.2 (CitizenTech) para a disciplina
**C# Software Development**. Visão geral do projeto: [../README.md](../README.md).

## Estágio atual

**Bloco B0 — inicialização concluída.** Hoje a API:

- inicia localmente;
- responde em `GET /health`, que verifica **apenas se o processo está ativo**
  (ainda não há banco de dados para verificar);
- publica o documento OpenAPI e a interface Scalar em Development.

Ainda **não** existem entidades, `DbContext`, migrations, banco de dados, services,
endpoints de transporte público nem testes de regras de negócio. Os pacotes do
EF Core já estão instalados para os próximos blocos.

## Requisitos de ambiente

| Item | Versão |
|---|---|
| .NET SDK | 10.0.100 ou superior na linha 10.0 (testado com **10.0.403**) |
| Runtime ASP.NET Core | 10.0.x |
| Sistema | Testado no Windows 11 |
| Editor | Qualquer um (VS Code com C# Dev Kit, Visual Studio 2022+ ou Rider) |

O arquivo [global.json](global.json) exige SDK `10.0.100` com `rollForward: latestFeature`.
Qualquer SDK 10.0.x com feature band igual ou superior é aceito, sem exigir o mesmo patch de todos.

Não é necessário instalar Docker, banco de dados nem ferramentas globais.

## Tecnologias e pacotes

| Pacote | Versão | Uso |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.14 | Documento OpenAPI nativo |
| `Scalar.AspNetCore` | 2.18.7 | Interface de documentação em `/scalar` |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.14 | EF Core com SQLite (uso a partir do B3) |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.14 | Suporte às migrations (uso a partir do B3) |
| `dotnet-ef` (ferramenta **local**) | 10.0.14 | Comandos de migrations, declarado em `.config/dotnet-tools.json` |
| `xunit.v3` | 4.0.2 | Testes automatizados (o projeto de testes é um executável, como exige o xUnit v3) |
| `xunit.runner.visualstudio` e `Microsoft.NET.Test.Sdk` | 4.0.1 e 17.14.1 | Compatibilidade com o Test Explorer das IDEs |

## Como executar

Todos os comandos devem ser executados **dentro da pasta `backend/`**.

```bash
dotnet tool restore
```

```bash
dotnet restore
```

```bash
dotnet build
```

```bash
dotnet test
```

```bash
dotnet run --project src/PontoVoz.Api --launch-profile http
```

A API fica disponível em `http://localhost:5080` (somente localhost).

### Endereços em Development

| Recurso | Endereço |
|---|---|
| Saúde do processo | http://localhost:5080/health |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Interface Scalar | http://localhost:5080/scalar |

Resposta real de `GET /health`, registrada nas evidências do B0:

```json
{"status":"Healthy","escopo":"processo","verificadoEm":"2026-11-07T01:39:15.6929158+00:00","verificacoes":[]}
```

`escopo: "processo"` e `verificacoes` vazia indicam que só o processo da API foi
verificado. A verificação do banco de dados entra quando o banco existir (bloco B3).

### Testes

O projeto usa xUnit v3 sobre a Microsoft.Testing.Platform, declarada em
`global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`), como o
.NET 10 SDK exige para essa plataforma.

Hoje o projeto de testes **não contém testes**. Por isso `dotnet test` mostra
`Zero testes executados` (total 0) e termina com **código de saída 8**, que nessa
plataforma significa "nenhum teste executado", não uma falha de teste. Esse código
não é ocultado de propósito: os primeiros testes chegam no bloco B2.

### HTTPS (opcional)

O perfil `https` usa `https://localhost:7080` e `http://localhost:5080`:

```bash
dotnet run --project src/PontoVoz.Api --launch-profile https
```

Para o navegador não mostrar aviso de segurança, o certificado de desenvolvimento
precisa ser marcado como confiável. Esse comando altera o repositório de certificados
do Windows, então execute-o só se quiser:

```bash
dotnet dev-certs https --trust
```

Em Development a API não redireciona HTTP para HTTPS, para que a equipe possa
usar a porta 5080 mesmo sem o certificado confiável. Fora de Development o
redirecionamento fica ativo.

### Ferramenta do EF Core

```bash
dotnet ef --version
```

Deve mostrar `10.0.14`. Ainda não há migrations para aplicar.

## Estrutura

```
backend/
├─ PontoVoz.slnx                 solução (formato .slnx do .NET 10)
├─ global.json                   versão mínima do SDK e plataforma de testes
├─ .config/dotnet-tools.json     ferramenta local dotnet-ef
├─ .editorconfig                 padrão de estilo do código
├─ docs/evidencias/              evidências de funcionamento por bloco
├─ src/PontoVoz.Api/
│  ├─ Program.cs                 composição da aplicação e injeção de dependência
│  ├─ Controllers/               controllers REST (hoje só HealthController)
│  ├─ Domain/                    entidades e regras de domínio (B2)
│  ├─ Application/               services, interfaces e DTOs (B5)
│  ├─ Infrastructure/            EF Core, repositórios e provedores (B3, B4, B8)
│  ├─ Security/                  proteção das rotas administrativas (B7)
│  └─ Middleware/                tratamento global de erros (B6)
└─ tests/PontoVoz.Tests/         testes xUnit (referencia o projeto da API)
```

## Próximas etapas

| Bloco | Conteúdo |
|---|---|
| B1 | Rascunho do contrato v1 em `../contracts/` para a equipe |
| B2 | Entidades e regras de domínio, com testes |
| B3 | `DbContext`, migration inicial e seed de dados simulados |
| B4 | Repositórios com consultas LINQ, com testes |
| B5 | Services, DTOs e injeção de dependência, com testes |
| B6 | Endpoints públicos, tratamento de erros e exportação do contrato OpenAPI |
| B7 | Endpoints administrativos com chave de acesso |
| B8 | Provedores de previsão (simulado e adaptador HTTP Olho Vivo) |
| B9 | Alternativas de deslocamento e resumo da parada |
| B10 | Documentação final, evidências e entrega |
