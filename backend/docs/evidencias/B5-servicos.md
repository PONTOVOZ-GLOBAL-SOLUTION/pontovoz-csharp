# Evidências — Bloco B5 (serviços de aplicação, DTOs e validações)

Data da execução: 07/11/2026, no Windows 11, com o .NET SDK 10.0.403, a partir da pasta
`backend/`. Os resultados foram copiados das saídas reais.

## Resposta real de um service

`UltimasPassagensService.ObterAsync` executado contra um SQLite em memória, criado pelas
migrations e com o seed provisionado em 08/11/2026 às 09h00 de São Paulo. Foi acrescentado o
registro manual dos exemplos do contrato (SIM-L004 vista às 14h24), e o relógio controlado
(`FakeTimeProvider`) foi posto em 10/11/2026 às 14h32. O DTO foi serializado com `JsonContrato`:

```json
{
  "paradaId": 1,
  "codigoExternoParada": "SIM-P001",
  "nomeParada": "Parada Demonstração Centro",
  "dadosSimulados": true,
  "consultadoEm": "2026-11-10T14:32:00-03:00",
  "limiteDesatualizacaoMinutos": 60,
  "passagens": [
    {
      "id": 3,
      "linhaId": 4,
      "codigoLinha": "SIM-L004",
      "letreiro": "SIM-104",
      "observadaEm": "2026-11-10T14:24:00-03:00",
      "registradaEm": "2026-11-10T14:25:10-03:00",
      "minutosDesdeObservacao": 8,
      "desatualizada": false,
      "prefixoVeiculo": null,
      "nivelConfirmacao": "RegistroManual",
      "verificada": false,
      "oficialSptrans": false,
      "origemRegistro": "operador-local",
      "dadosSimulados": true,
      "aviso": {
        "codigo": "RegistroManualNaoVerificado",
        "mensagem": "Registro manual sem verificação independente. Não é um dado oficial da SPTrans."
      }
    },
    {
      "id": 1,
      "linhaId": 2,
      "codigoLinha": "SIM-L002",
      "letreiro": "SIM-102",
      "observadaEm": "2026-11-08T08:45:00-03:00",
      "registradaEm": "2026-11-08T09:00:00-03:00",
      "minutosDesdeObservacao": 3227,
      "desatualizada": true,
      "prefixoVeiculo": "SIM-V2003",
      "nivelConfirmacao": "ObservacaoSimulada",
      "verificada": false,
      "oficialSptrans": false,
      "origemRegistro": null,
      "dadosSimulados": true,
      "aviso": {
        "codigo": "ObservacaoSimulada",
        "mensagem": "Observação simulada criada com os dados de demonstração. Não é um registro oficial da SPTrans."
      }
    }
  ],
  "linhasSemRegistro": [
    {
      "linhaId": 1,
      "codigoLinha": "SIM-L001",
      "letreiro": "SIM-101"
    }
  ],
  "avisos": [
    {
      "codigo": "DadosSimulados",
      "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
    },
    {
      "codigo": "PassagemDesatualizada",
      "mensagem": "Algumas passagens foram observadas há mais de 60 minutos e podem não refletir a situação atual."
    },
    {
      "codigo": "PassagemNaoRegistrada",
      "mensagem": "Não há passagem registrada para algumas linhas desta parada."
    }
  ]
}
```

O mesmo resultado, sem os ids internos, é comparado campo a campo com
`contracts/exemplos/ultimas-passagens.json` no teste
`UltimasPassagens_DoBancoReal_SaoIguaisAoExemploDoContrato`.

## Erro de validação real

`ParadaService.BuscarAsync(new ConsultaParadas(Termo: "a", TamanhoPagina: 100))`, a mesma
requisição do exemplo `erro-validacao.json`. O service lançou `ErroAplicacaoException`, aqui
serializada:

```json
{
  "tipo": "Validacao",
  "codigo": "RequisicaoInvalida",
  "titulo": "A requisição contém parâmetros inválidos.",
  "erros": {
    "termo": [
      "O termo deve ter entre 2 e 60 caracteres."
    ],
    "tamanhoPagina": [
      "O tamanho da página deve estar entre 1 e 50."
    ]
  }
}
```

Os dois erros vêm juntos, e o repositório não foi consultado (teste
`Paradas_ComTermoCurtoETamanhoDePaginaGrande_DevolvemOsErrosDoExemploDoContrato_SemConsultar`).

## Comandos

```text
> dotnet restore → Todos os projetos estão atualizados para restauração. (código 0)
> dotnet build   → 0 Aviso(s), 0 Erro(s) (código 0)
> dotnet test
Resumo da execução de teste: Aprovado!
  total: 463
  falhou: 0
  concluído com êxito: 463
  ignorados: 0                                   (código 0)
> dotnet ef migrations list --project src/PontoVoz.Api --no-build
20261107211456_InitialCreate
20261107222533_AddSearchColumns
> dotnet ef migrations has-pending-model-changes --project src/PontoVoz.Api --no-build
No changes have been made to the model since the last migration.
```

| Filtro | Testes aprovados |
|---|---|
| `--filter-namespace "PontoVoz.Tests.Domain"` | 278 |
| `--filter-namespace "PontoVoz.Tests.Application"` | 70 (15 de paginação do B4.1 e 55 novos) |
| `--filter-namespace "PontoVoz.Tests.Infrastructure"` | 106 |
| `--filter-namespace "PontoVoz.Tests.Api"` | 9 (3 do `/health` e 6 novos de injeção de dependência) |
| `--filter-method "*CasosDoContrato"` (J1 a J5, T1 a T7) | 17 |

Os 402 testes anteriores continuam aprovados, e 61 são novos.

| Classe nova | Testes | Tipo |
|---|---|---|
| `ServicosConsultaTests` | 29 | Unitários, com dublês de repositório |
| `UltimasPassagensServiceTests` | 17 | Unitários, com dublês de repositório e relógio que conta leituras |
| `ServicosIntegracaoTests` | 9 | Integração com SQLite real e o seed |
| `InjecaoDependenciaTests` | 6 | Contêiner da API real (`WebApplicationFactory`) |

## Compatibilidade com o contrato

Respostas montadas a partir do banco real (seed e um registro manual) e comparadas campo a campo
com os exemplos, sem os ids internos. Todas as comparações passaram:

| Service | Exemplo |
|---|---|
| `ParadaService.BuscarAsync(termo: "demonstracao")` | `paradas-listagem.json` |
| `ParadaService.BuscarAsync(codigoExterno: "SIM-P001")` | `paradas-busca-codigo.json` |
| `ParadaService.ObterDetalheAsync` | `parada-detalhe.json` |
| `DestinoService.BuscarAsync(termo: "norte")` | `destinos-busca.json` |
| `LinhaService.BuscarAsync(termo: "104")` | Exemplo de `GET /api/v1/linhas?termo=104` em `api-v1.md` |
| `UltimasPassagensService.ObterAsync` | `ultimas-passagens.json` |
| `ParadaService.BuscarAsync(termo: "a", tamanhoPagina: 100)` | `errors` e `title` de `erro-validacao.json` |
| `ParadaService.ObterDetalheAsync(99)` | `codigo`, `title` e `detail` de `erro-parada-nao-encontrada.json` |

## Mutações

Mudanças temporárias no código, depois restauradas e conferidas com `cmp` (arquivos idênticos ao
original):

| Mutação | Testes que falham |
|---|---|
| Mensagem do aviso `PassagemDesatualizada` alterada | 1 |
| Horário sem conversão para São Paulo | 2 |
| Sem o aviso `PassagemNaoRegistrada` | 3 |
| Sem a verificação de que a linha atende a parada | 1 |
| Termo aceito até 61 caracteres | 3 |
| Passagem nunca marcada como desatualizada | 4 |

## Banco e API

- **Banco de desenvolvimento:** a inspeção do `pontovoz.db` (módulo `sqlite3` do Python, somente
  leitura) foi comparada com a registrada no B4 e é **idêntica**. O B5 não alterou o banco nem as
  migrations.
- **API:** com o banco de desenvolvimento, as rotas responderam assim:

```text
GET /health          → HTTP 200 {"status":"Healthy","escopo":"processo-e-dependencias","verificadoEm":"2026-11-07T23:31:04.0785933+00:00","verificacoes":[{"nome":"banco-de-dados","status":"Healthy"}]}
GET /openapi/v1.json → 200, paths: ['/health']
GET /scalar          → 302 · GET /scalar/ → 200
GET /api/v1/paradas  → 404 (nenhum endpoint de negócio implementado)
```

Os services existem e estão registrados, mas não são expostos por HTTP; os controllers entram
no B6.

## Problemas encontrados e soluções

1. **`JsonContrato` sem `TypeInfoResolver`.** Ao ser marcado como somente leitura, o
   `JsonSerializerOptions` exige um resolvedor de tipos. **Solução:** `DefaultJsonTypeInfoResolver`
   na criação das opções.
2. **Passagem simulada de teste com 3227 minutos.** O domínio só aceita passagens simuladas criadas
   até 24 horas antes do provisionamento, e o teste tentava criar uma com mais. **Solução:** o
   teste passou a provisionar a passagem no passado, como faz o seed; a regra do domínio não
   mudou.
