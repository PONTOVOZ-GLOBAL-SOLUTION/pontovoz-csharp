# Evidências — Bloco B10 (validação final da entrega)

Execução em 08/11/2026, no Windows 11, com o .NET SDK 10.0.403. Todos os resultados abaixo foram
copiados das saídas reais. Valores que dependem da máquina foram substituídos por marcadores: a
pasta temporária do clone virou `<pasta-vazia>`, e a chave administrativa (aleatória, gerada só
para o teste) nunca foi impressa. Os cabeçalhos `Date` e `Server` foram omitidos.

O B10 não acrescentou funcionalidades nem mudou código ou configuração: só documentação.

## Reprodução num clone novo

Numa pasta vazia, sem banco, binários nem configuração anteriores, seguindo apenas o README
(`backend/README.md`, "Executar a partir do GitHub"):

```text
## git clone
exit 0
096f0512345819024bdb1d500a67196041e6c129 feat: implement travel alternatives and stop summary
README.md
backend
contracts
docs
prompts.md
## arquivos gerados presentes no clone:
## dotnet --version
10.0.403
## dotnet tool restore

A restauração foi bem-sucedida.
exit 0
## dotnet restore
  <pasta-vazia>\pontovoz-csharp\backend\tests\PontoVoz.Tests\PontoVoz.Tests.csproj restaurado (em 2,05 seg).
exit 0
## dotnet build
    0 Aviso(s)
    0 Erro(s)
exit 0
## dotnet ef database update
Done.
exit 0
arquivo criado: src/PontoVoz.Api/pontovoz.db (98304 bytes)
## dotnet test
Resumo da execução de teste: Aprovado!
  total: 805
  falhou: 0
  concluído com êxito: 805
  ignorados: 0
  duração: 23s 491ms
exit 0
```

- O clone não trouxe nenhum `*.db`, `bin/` ou `obj/` (a lista de arquivos gerados veio vazia).
- O banco do clone foi criado pelo `dotnet ef database update`, com as duas migrations e o seed:

```text
banco do clone, recém-criado pelo README: {'Paradas': 2, 'Linhas': 4, 'ParadasLinhas': 5, 'Destinos': 3, 'PercursosEstimados': 5, 'RegistrosPassagem': 2}
```

## As 14 operações

A API do clone foi iniciada como o README indica (`dotnet run --project src/PontoVoz.Api
--launch-profile http`, em `http://localhost:5080`), com `Admin:Chave` definida por variável de
ambiente com um valor aleatório.

```text
horário do servidor (UTC): 2026-11-08T21:20:18Z
```

```text
GET /scalar/ -> 200 | GET /openapi/v1.json -> 200
```

### Rotas públicas e erros

| Requisição | Status | Código | Cabeçalhos |
|---|---|---|---|
| `GET /health` | 200 OK |  |  |
| `GET /api/v1/paradas?codigoExterno=SIM-P001` | 200 OK |  |  |
| `GET /api/v1/paradas?termo=DEMONSTRACAO` | 200 OK |  |  |
| `GET /api/v1/paradas/1` | 200 OK |  |  |
| `GET /api/v1/linhas?termo=104` | 200 OK |  |  |
| `GET /api/v1/destinos?termo=norte` | 200 OK |  |  |
| `GET /api/v1/paradas/1/previsoes` | 200 OK |  |  |
| `GET /api/v1/paradas/1/ultimas-passagens` | 200 OK |  |  |
| `GET /api/v1/paradas/1/alternativas?destinoId=3` | 200 OK |  |  |
| `GET /api/v1/paradas/1/resumo` | 200 OK |  |  |
| `GET /api/v1/paradas?termo=a&tamanhoPagina=100` | 400 Bad Request | `RequisicaoInvalida` |  |
| `GET /api/v1/paradas/99` | 404 Not Found | `ParadaNaoEncontrada` |  |
| `GET /api/v1/paradas/1/alternativas` | 400 Bad Request | `RequisicaoInvalida` |  |
| `GET /api/v1/rota-inexistente` | 404 Not Found | `RotaNaoEncontrada` |  |
| `POST /api/v1/paradas (método não aceito)` | 405 Method Not Allowed | `MetodoNaoPermitido` | Allow: GET |

Nenhuma rota pública precisou da chave (ela estava configurada). Depois das consultas públicas, o
banco continuava igual:

```text
contagens depois das consultas públicas: {'Paradas': 2, 'Linhas': 4, 'ParadasLinhas': 5, 'Destinos': 3, 'PercursosEstimados': 5, 'RegistrosPassagem': 2}
```

### Fluxo administrativo

| Requisição | Status | Código | Cabeçalhos |
|---|---|---|---|
| `POST /api/v1/admin/paradas (sem chave)` | 401 Unauthorized | `ChaveAdministrativaInvalida` | WWW-Authenticate: ApiKey header="X-PontoVoz-Admin-Key" |
| `POST /api/v1/admin/paradas (chave errada)` | 401 Unauthorized | `ChaveAdministrativaInvalida` | WWW-Authenticate: ApiKey header="X-PontoVoz-Admin-Key" |
| `POST /api/v1/admin/paradas (chave correta)` | 201 Created |  | Location: /api/v1/paradas/3 |
| `POST /api/v1/admin/paradas (repetida)` | 409 Conflict | `CodigoJaCadastrado` |  |
| `POST /api/v1/admin/paradas (inválida)` | 400 Bad Request | `RequisicaoInvalida` |  |
| `PUT /api/v1/admin/paradas/3 (chave correta)` | 200 OK |  |  |
| `POST /api/v1/admin/linhas (chave correta)` | 201 Created |  |  |
| `PUT /api/v1/admin/paradas/3/linhas/5 (chave correta)` | 204 No Content |  |  |
| `PUT /api/v1/admin/paradas/3/linhas/5 (repetida, idempotente)` | 204 No Content |  |  |
| `POST /api/v1/admin/observacoes-passagem (chave correta)` | 201 Created |  |  |
| `GET /api/v1/paradas?termo=renomeada` | 200 OK |  |  |
| `GET /api/v1/paradas/3/ultimas-passagens` | 200 OK |  |  |
| `GET /api/v1/paradas/3/resumo` | 200 OK |  |  |
| `POST /api/v1/admin/paradas (Production, sem Admin:Chave)` | 403 Forbidden | `OperacoesAdministrativasDesabilitadas` |  |

```text
contagens depois do fluxo administrativo: {'Paradas': 3, 'Linhas': 5, 'ParadasLinhas': 6, 'Destinos': 3, 'PercursosEstimados': 5, 'RegistrosPassagem': 3}
```

O cadastro feito pelo operador aparece logo nas consultas públicas, como cadastro fictício e não
verificado:

```text
### GET /api/v1/paradas/3/ultimas-passagens
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "paradaId": 3,
  "codigoExternoParada": "TESTE-P010",
  "nomeParada": "Parada Cadastro de Teste B10 Renomeada",
  "dadosSimulados": true,
  "consultadoEm": "2026-11-08T18:20:26.2382873-03:00",
  "limiteDesatualizacaoMinutos": 60,
  "passagens": [
    {
      "id": 3,
      "linhaId": 5,
      "codigoLinha": "TESTE-L010",
      "letreiro": "TESTE-110",
      "observadaEm": "2026-11-08T18:15:25-03:00",
      "registradaEm": "2026-11-08T18:20:25.9320391-03:00",
      "minutosDesdeObservacao": 5,
      "desatualizada": false,
      "prefixoVeiculo": "SIM-V9001",
      "nivelConfirmacao": "RegistroManual",
      "verificada": false,
      "oficialSptrans": false,
      "origemRegistro": "operador-local",
      "dadosSimulados": true,
      "aviso": {
        "codigo": "RegistroManualNaoVerificado",
        "mensagem": "Registro manual sem verificação independente. Não é um dado oficial da SPTrans."
      }
    }
  ],
  "linhasSemRegistro": [],
  "avisos": [
    {
      "codigo": "DadosSimulados",
      "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
    },
    {
      "codigo": "CadastroNaoVerificado",
      "mensagem": "Cadastro feito por uma rota administrativa, sem conferência com fonte oficial."
    }
  ]
}
```

O último item da tabela usou outra instância, em Production (`http://localhost:5081`) e sem
`Admin:Chave`: as rotas administrativas ficam desabilitadas (403), e o OpenAPI e o Scalar não são
publicados fora de Development:

```text
GET /scalar/ em Production -> 404 | GET /openapi/v1.json -> 404
```

## Jornada do cidadão

A mesma sequência que o totem faria. Os ids vieram das respostas da própria API.

1. **Parada pelo código do totem**, com as linhas que a atendem:

```text
### GET /api/v1/paradas/1
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "id": 1,
  "codigoExterno": "SIM-P001",
  "nome": "Parada Demonstração Centro",
  "endereco": null,
  "coordenada": null,
  "origem": "Simulado",
  "dadosSimulados": true,
  "linhas": [
    {
      "id": 1,
      "codigo": "SIM-L001",
      "letreiro": "SIM-101",
      "terminalPrincipal": "Terminal Demonstração Centro",
      "terminalSecundario": "Terminal Demonstração Norte",
      "sentido": "PrincipalParaSecundario",
      "origem": "Simulado",
      "dadosSimulados": true
    },
    {
      "id": 2,
      "codigo": "SIM-L002",
      "letreiro": "SIM-102",
      "terminalPrincipal": "Terminal Demonstração Centro",
      "terminalSecundario": "Terminal Demonstração Norte",
      "sentido": "PrincipalParaSecundario",
      "origem": "Simulado",
      "dadosSimulados": true
    },
    {
      "id": 4,
      "codigo": "SIM-L004",
      "letreiro": "SIM-104",
      "terminalPrincipal": "Terminal Demonstração Centro",
      "terminalSecundario": "Terminal Demonstração Norte",
      "sentido": "PrincipalParaSecundario",
      "origem": "Simulado",
      "dadosSimulados": true
    }
  ]
}
```

2. **Próximas chegadas simuladas** (a SIM-L001 não tem previsão: aviso `PrevisaoParcial`):

```text
### GET /api/v1/paradas/1/previsoes
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "paradaId": 1,
  "codigoExternoParada": "SIM-P001",
  "nomeParada": "Parada Demonstração Centro",
  "status": "Disponivel",
  "origem": "Simulado",
  "dadosSimulados": true,
  "consultadoEm": "2026-11-08T18:20:21.2419417-03:00",
  "previsoes": [
    {
      "linhaId": 4,
      "codigoLinha": "SIM-L004",
      "letreiro": "SIM-104",
      "previstaPara": "2026-11-08T18:25:00-03:00",
      "esperaMinutos": 4,
      "prefixoVeiculo": "SIM-V4002",
      "nivelConfirmacao": "Previsao"
    },
    {
      "linhaId": 2,
      "codigoLinha": "SIM-L002",
      "letreiro": "SIM-102",
      "previstaPara": "2026-11-08T18:27:00-03:00",
      "esperaMinutos": 6,
      "prefixoVeiculo": "SIM-V2001",
      "nivelConfirmacao": "Previsao"
    },
    {
      "linhaId": 2,
      "codigoLinha": "SIM-L002",
      "letreiro": "SIM-102",
      "previstaPara": "2026-11-08T18:45:00-03:00",
      "esperaMinutos": 24,
      "prefixoVeiculo": "SIM-V2002",
      "nivelConfirmacao": "Previsao"
    }
  ],
  "linhasSemPrevisao": [
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
      "codigo": "PrevisaoParcial",
      "mensagem": "Não há previsão de chegada para algumas linhas desta parada."
    }
  ]
}
```

3. **Últimas passagens registradas** (observação simulada do seed, nunca verificada nem oficial):

```text
### GET /api/v1/paradas/1/ultimas-passagens
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "paradaId": 1,
  "codigoExternoParada": "SIM-P001",
  "nomeParada": "Parada Demonstração Centro",
  "dadosSimulados": true,
  "consultadoEm": "2026-11-08T18:20:21.3891326-03:00",
  "limiteDesatualizacaoMinutos": 60,
  "passagens": [
    {
      "id": 1,
      "linhaId": 2,
      "codigoLinha": "SIM-L002",
      "letreiro": "SIM-102",
      "observadaEm": "2026-11-08T18:03:42.2214716-03:00",
      "registradaEm": "2026-11-08T18:18:42.2214716-03:00",
      "minutosDesdeObservacao": 16,
      "desatualizada": false,
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
    },
    {
      "linhaId": 4,
      "codigoLinha": "SIM-L004",
      "letreiro": "SIM-104"
    }
  ],
  "avisos": [
    {
      "codigo": "DadosSimulados",
      "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
    },
    {
      "codigo": "PassagemNaoRegistrada",
      "mensagem": "Não há passagem registrada para algumas linhas desta parada."
    }
  ]
}
```

4. **Destino escolhido** e **alternativas**: a SIM-L004 tem o menor tempo total; a SIM-L001 fica
   sem posição e a comparação fica incompleta (aviso `ComparacaoIncompleta`):

```text
### GET /api/v1/paradas/1/alternativas?destinoId=3
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "paradaId": 1,
  "codigoExternoParada": "SIM-P001",
  "nomeParada": "Parada Demonstração Centro",
  "destino": {
    "id": 3,
    "nome": "Terminal Demonstração Norte",
    "tipo": "Terminal"
  },
  "criterio": "MenorTempoTotalEstimado",
  "situacao": "Comparadas",
  "comparacaoCompleta": false,
  "statusPrevisao": "Disponivel",
  "origemPrevisao": "Simulado",
  "dadosSimulados": true,
  "consultadoEm": "2026-11-08T18:20:22.0145946-03:00",
  "alternativas": [
    {
      "posicao": 1,
      "linhaId": 4,
      "codigoLinha": "SIM-L004",
      "letreiro": "SIM-104",
      "situacao": "Comparavel",
      "previstaPara": "2026-11-08T18:25:00-03:00",
      "esperaMinutos": 4,
      "percursoEstimadoMinutos": 20,
      "tempoTotalEstimadoMinutos": 24,
      "chegadaEstimadaAoDestino": "2026-11-08T18:45:00-03:00",
      "baldeacoes": 0,
      "origemPercurso": "Simulado"
    },
    {
      "posicao": 2,
      "linhaId": 2,
      "codigoLinha": "SIM-L002",
      "letreiro": "SIM-102",
      "situacao": "Comparavel",
      "previstaPara": "2026-11-08T18:27:00-03:00",
      "esperaMinutos": 6,
      "percursoEstimadoMinutos": 40,
      "tempoTotalEstimadoMinutos": 46,
      "chegadaEstimadaAoDestino": "2026-11-08T19:07:00-03:00",
      "baldeacoes": 0,
      "origemPercurso": "Simulado"
    },
    {
      "posicao": null,
      "linhaId": 1,
      "codigoLinha": "SIM-L001",
      "letreiro": "SIM-101",
      "situacao": "SemPrevisao",
      "previstaPara": null,
      "esperaMinutos": null,
      "percursoEstimadoMinutos": 30,
      "tempoTotalEstimadoMinutos": null,
      "chegadaEstimadaAoDestino": null,
      "baldeacoes": 0,
      "origemPercurso": "Simulado"
    }
  ],
  "avisos": [
    {
      "codigo": "DadosSimulados",
      "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
    },
    {
      "codigo": "LimitacaoLinhasDiretas",
      "mensagem": "Considera apenas linhas diretas desta parada, sem baldeação nem outros meios de transporte."
    },
    {
      "codigo": "PercursoEstimado",
      "mensagem": "Os tempos de percurso são estimativas simuladas para demonstração e não consideram o trânsito em tempo real."
    },
    {
      "codigo": "ComparacaoIncompleta",
      "mensagem": "Algumas linhas não têm previsão de chegada e ficaram fora da comparação."
    }
  ]
}
```

5. **Resumo da parada**, com um único instante nas três partes:

```text
### GET /api/v1/paradas/1/resumo
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "consultadoEm": "2026-11-08T18:20:22.1833631-03:00",
  "dadosSimulados": true,
  "situacao": "Completo",
  "partesIndisponiveis": [],
  "parada": {
    "id": 1,
    "codigoExterno": "SIM-P001",
    "nome": "Parada Demonstração Centro",
    "endereco": null,
    "coordenada": null,
    "origem": "Simulado",
    "dadosSimulados": true,
    "linhas": [
      {
        "id": 1,
        "codigo": "SIM-L001",
        "letreiro": "SIM-101",
        "terminalPrincipal": "Terminal Demonstração Centro",
        "terminalSecundario": "Terminal Demonstração Norte",
        "sentido": "PrincipalParaSecundario",
        "origem": "Simulado",
        "dadosSimulados": true
      },
      {
        "id": 2,
        "codigo": "SIM-L002",
        "letreiro": "SIM-102",
        "terminalPrincipal": "Terminal Demonstração Centro",
        "terminalSecundario": "Terminal Demonstração Norte",
        "sentido": "PrincipalParaSecundario",
        "origem": "Simulado",
        "dadosSimulados": true
      },
      {
        "id": 4,
        "codigo": "SIM-L004",
        "letreiro": "SIM-104",
        "terminalPrincipal": "Terminal Demonstração Centro",
        "terminalSecundario": "Terminal Demonstração Norte",
        "sentido": "PrincipalParaSecundario",
        "origem": "Simulado",
        "dadosSimulados": true
      }
    ]
  },
  "previsoes": {
    "paradaId": 1,
    "codigoExternoParada": "SIM-P001",
    "nomeParada": "Parada Demonstração Centro",
    "status": "Disponivel",
    "origem": "Simulado",
    "dadosSimulados": true,
    "consultadoEm": "2026-11-08T18:20:22.1833631-03:00",
    "previsoes": [
      {
        "linhaId": 4,
        "codigoLinha": "SIM-L004",
        "letreiro": "SIM-104",
        "previstaPara": "2026-11-08T18:25:00-03:00",
        "esperaMinutos": 4,
        "prefixoVeiculo": "SIM-V4002",
        "nivelConfirmacao": "Previsao"
      },
      {
        "linhaId": 2,
        "codigoLinha": "SIM-L002",
        "letreiro": "SIM-102",
        "previstaPara": "2026-11-08T18:27:00-03:00",
        "esperaMinutos": 6,
        "prefixoVeiculo": "SIM-V2001",
        "nivelConfirmacao": "Previsao"
      },
      {
        "linhaId": 2,
        "codigoLinha": "SIM-L002",
        "letreiro": "SIM-102",
        "previstaPara": "2026-11-08T18:45:00-03:00",
        "esperaMinutos": 24,
        "prefixoVeiculo": "SIM-V2002",
        "nivelConfirmacao": "Previsao"
      }
    ],
    "linhasSemPrevisao": [
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
        "codigo": "PrevisaoParcial",
        "mensagem": "Não há previsão de chegada para algumas linhas desta parada."
      }
    ]
  },
  "ultimasPassagens": {
    "paradaId": 1,
    "codigoExternoParada": "SIM-P001",
    "nomeParada": "Parada Demonstração Centro",
    "dadosSimulados": true,
    "consultadoEm": "2026-11-08T18:20:22.1833631-03:00",
    "limiteDesatualizacaoMinutos": 60,
    "passagens": [
      {
        "id": 1,
        "linhaId": 2,
        "codigoLinha": "SIM-L002",
        "letreiro": "SIM-102",
        "observadaEm": "2026-11-08T18:03:42.2214716-03:00",
        "registradaEm": "2026-11-08T18:18:42.2214716-03:00",
        "minutosDesdeObservacao": 16,
        "desatualizada": false,
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
      },
      {
        "linhaId": 4,
        "codigoLinha": "SIM-L004",
        "letreiro": "SIM-104"
      }
    ],
    "avisos": [
      {
        "codigo": "DadosSimulados",
        "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
      },
      {
        "codigo": "PassagemNaoRegistrada",
        "mensagem": "Não há passagem registrada para algumas linhas desta parada."
      }
    ]
  },
  "avisos": []
}
```

**Valor para o cidadão:** numa só parada, a pessoa sabe o que chega, quando cada linha passou pela
última vez e qual linha a leva mais rápido ao destino, levando em conta o percurso, e não só quem
chega primeiro. Quando falta informação, a resposta avisa em vez de esconder. Todos os dados desta
demonstração são simulados e aparecem como tal (`dadosSimulados: true`, `origem: "Simulado"`,
aviso `DadosSimulados`). A demonstração é só do backend: a interface do totem e a voz são outros
componentes.

## Falha controlada

A API do clone foi reiniciada com `Previsoes__SimularIndisponibilidade=true`, sem mudar código nem
banco:

| Requisição | Status | Código | Cabeçalhos |
|---|---|---|---|
| `GET /api/v1/paradas/1/previsoes` | 503 Service Unavailable | `ProvedorPrevisoesIndisponivel` | Retry-After: 30 |
| `GET /api/v1/paradas/1/alternativas?destinoId=3` | 200 OK |  |  |
| `GET /api/v1/paradas/1/resumo` | 200 OK |  |  |
| `GET /api/v1/paradas/1` | 200 OK |  |  |
| `GET /api/v1/linhas` | 200 OK |  |  |
| `GET /api/v1/paradas/1/ultimas-passagens` | 200 OK |  |  |
| `GET /health` | 200 OK |  |  |

```text
### GET /api/v1/paradas/1/alternativas?destinoId=3
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "paradaId": 1,
  "codigoExternoParada": "SIM-P001",
  "nomeParada": "Parada Demonstração Centro",
  "destino": {
    "id": 3,
    "nome": "Terminal Demonstração Norte",
    "tipo": "Terminal"
  },
  "criterio": "MenorTempoTotalEstimado",
  "situacao": "SemComparacao",
  "comparacaoCompleta": false,
  "statusPrevisao": "ProvedorIndisponivel",
  "origemPrevisao": "Simulado",
  "dadosSimulados": true,
  "consultadoEm": "2026-11-08T18:20:32.4447475-03:00",
  "alternativas": [
    {
      "posicao": null,
      "linhaId": 4,
      "codigoLinha": "SIM-L004",
      "letreiro": "SIM-104",
      "situacao": "SemPrevisao",
      "previstaPara": null,
      "esperaMinutos": null,
      "percursoEstimadoMinutos": 20,
      "tempoTotalEstimadoMinutos": null,
      "chegadaEstimadaAoDestino": null,
      "baldeacoes": 0,
      "origemPercurso": "Simulado"
    },
    {
      "posicao": null,
      "linhaId": 1,
      "codigoLinha": "SIM-L001",
      "letreiro": "SIM-101",
      "situacao": "SemPrevisao",
      "previstaPara": null,
      "esperaMinutos": null,
      "percursoEstimadoMinutos": 30,
      "tempoTotalEstimadoMinutos": null,
      "chegadaEstimadaAoDestino": null,
      "baldeacoes": 0,
      "origemPercurso": "Simulado"
    },
    {
      "posicao": null,
      "linhaId": 2,
      "codigoLinha": "SIM-L002",
      "letreiro": "SIM-102",
      "situacao": "SemPrevisao",
      "previstaPara": null,
      "esperaMinutos": null,
      "percursoEstimadoMinutos": 40,
      "tempoTotalEstimadoMinutos": null,
      "chegadaEstimadaAoDestino": null,
      "baldeacoes": 0,
      "origemPercurso": "Simulado"
    }
  ],
  "avisos": [
    {
      "codigo": "ProvedorIndisponivel",
      "mensagem": "O serviço de previsões não está disponível no momento. Tente novamente em instantes."
    },
    {
      "codigo": "SemComparacao",
      "mensagem": "Nenhuma linha direta tem previsão de chegada no momento. Não é possível indicar a alternativa mais rápida."
    },
    {
      "codigo": "DadosSimulados",
      "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
    },
    {
      "codigo": "LimitacaoLinhasDiretas",
      "mensagem": "Considera apenas linhas diretas desta parada, sem baldeação nem outros meios de transporte."
    },
    {
      "codigo": "PercursoEstimado",
      "mensagem": "Os tempos de percurso são estimativas simuladas para demonstração e não consideram o trânsito em tempo real."
    }
  ]
}
```

```text
### GET /api/v1/paradas/1/resumo
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{
  "consultadoEm": "2026-11-08T18:20:32.7035241-03:00",
  "dadosSimulados": true,
  "situacao": "Parcial",
  "partesIndisponiveis": [
    "previsoes"
  ],
  "parada": {
    "id": 1,
    "codigoExterno": "SIM-P001",
    "nome": "Parada Demonstração Centro",
    "endereco": null,
    "coordenada": null,
    "origem": "Simulado",
    "dadosSimulados": true,
    "linhas": [
      {
        "id": 1,
        "codigo": "SIM-L001",
        "letreiro": "SIM-101",
        "terminalPrincipal": "Terminal Demonstração Centro",
        "terminalSecundario": "Terminal Demonstração Norte",
        "sentido": "PrincipalParaSecundario",
        "origem": "Simulado",
        "dadosSimulados": true
      },
      {
        "id": 2,
        "codigo": "SIM-L002",
        "letreiro": "SIM-102",
        "terminalPrincipal": "Terminal Demonstração Centro",
        "terminalSecundario": "Terminal Demonstração Norte",
        "sentido": "PrincipalParaSecundario",
        "origem": "Simulado",
        "dadosSimulados": true
      },
      {
        "id": 4,
        "codigo": "SIM-L004",
        "letreiro": "SIM-104",
        "terminalPrincipal": "Terminal Demonstração Centro",
        "terminalSecundario": "Terminal Demonstração Norte",
        "sentido": "PrincipalParaSecundario",
        "origem": "Simulado",
        "dadosSimulados": true
      }
    ]
  },
  "previsoes": {
    "paradaId": 1,
    "codigoExternoParada": "SIM-P001",
    "nomeParada": "Parada Demonstração Centro",
    "status": "ProvedorIndisponivel",
    "origem": "Simulado",
    "dadosSimulados": true,
    "consultadoEm": "2026-11-08T18:20:32.7035241-03:00",
    "previsoes": [],
    "linhasSemPrevisao": [
      {
        "linhaId": 1,
        "codigoLinha": "SIM-L001",
        "letreiro": "SIM-101"
      },
      {
        "linhaId": 2,
        "codigoLinha": "SIM-L002",
        "letreiro": "SIM-102"
      },
      {
        "linhaId": 4,
        "codigoLinha": "SIM-L004",
        "letreiro": "SIM-104"
      }
    ],
    "avisos": [
      {
        "codigo": "ProvedorIndisponivel",
        "mensagem": "O serviço de previsões não está disponível no momento. Tente novamente em instantes."
      },
      {
        "codigo": "DadosSimulados",
        "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
      }
    ]
  },
  "ultimasPassagens": {
    "paradaId": 1,
    "codigoExternoParada": "SIM-P001",
    "nomeParada": "Parada Demonstração Centro",
    "dadosSimulados": true,
    "consultadoEm": "2026-11-08T18:20:32.7035241-03:00",
    "limiteDesatualizacaoMinutos": 60,
    "passagens": [
      {
        "id": 1,
        "linhaId": 2,
        "codigoLinha": "SIM-L002",
        "letreiro": "SIM-102",
        "observadaEm": "2026-11-08T18:03:42.2214716-03:00",
        "registradaEm": "2026-11-08T18:18:42.2214716-03:00",
        "minutosDesdeObservacao": 16,
        "desatualizada": false,
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
      },
      {
        "linhaId": 4,
        "codigoLinha": "SIM-L004",
        "letreiro": "SIM-104"
      }
    ],
    "avisos": [
      {
        "codigo": "DadosSimulados",
        "mensagem": "Dados simulados para demonstração. Não representam a operação real da SPTrans."
      },
      {
        "codigo": "PassagemNaoRegistrada",
        "mensagem": "Não há passagem registrada para algumas linhas desta parada."
      }
    ]
  },
  "avisos": [
    {
      "codigo": "ResumoParcial",
      "mensagem": "As previsões estão indisponíveis no momento. As demais informações da parada continuam válidas."
    }
  ]
}
```

```text
contagens com o provedor indisponível: {'Paradas': 3, 'Linhas': 5, 'ParadasLinhas': 6, 'Destinos': 3, 'PercursosEstimados': 5, 'RegistrosPassagem': 3}
```

Nenhum dado foi criado para esconder a indisponibilidade: as contagens são as mesmas do fim do
fluxo administrativo, e as previsões não foram trocadas por valores inventados.

## Testes

Na pasta de trabalho, com os binários recompilados do zero:

```text
## restore
  Todos os projetos estão atualizados para restauração.
exit 0
## build --no-incremental
    0 Aviso(s)
    0 Erro(s)
Tempo Decorrido 00:00:19.95
exit 0
## test

Resumo da execução de teste: Aprovado!
  total: 805
  falhou: 0
  concluído com êxito: 805
  ignorados: 0
  duração: 45s 017ms
exit 0
## ef migrations list
20261107211456_InitialCreate
20261107222533_AddSearchColumns
exit 0
## ef database update (banco de desenvolvimento, já atualizado)
Done.
exit 0
## ef has-pending-model-changes
No changes have been made to the model since the last migration.
exit 0
## verificador
  Sintaxe JSON                                          37 verificações, 0 falha(s)
  Convenções (campos, datas, enums, IDs, avisos)       844 verificações, 0 falha(s)
  Identificação da parada e paginação                   12 verificações, 0 falha(s)
  IDs e códigos das linhas                              32 verificações, 0 falha(s)
  Datas e cálculos de previsões                         15 verificações, 0 falha(s)
  Últimas passagens                                     17 verificações, 0 falha(s)
  Política de dados simulados de passagem               11 verificações, 0 falha(s)
  Alternativas                                          51 verificações, 0 falha(s)
  Resumo                                                 9 verificações, 0 falha(s)
  Operação administrativa                                7 verificações, 0 falha(s)
  Regra temporal das observações (4.6.3)                29 verificações, 0 falha(s)
  Natureza do dado e cadastro administrativo            26 verificações, 0 falha(s)
  Erros (ProblemDetails)                                23 verificações, 0 falha(s)
  Links, exemplos referenciados e rotas                 68 verificações, 0 falha(s)
  Segurança e dados pessoais                            38 verificações, 0 falha(s)
Nenhuma falha nas regras verificadas.
exit 0
```

Grupos pedidos na revisão final (mesma compilação):

| Grupo | Filtro | Resultado |
|---|---|---|
| Integração SQLite | `--filter-namespace PontoVoz.Tests.Infrastructure` | total: 172  falhou: 0  ignorados: 0 |
| HTTP (API em memória) | `--filter-namespace PontoVoz.Tests.Api` | total: 211  falhou: 0  ignorados: 0 |
| Domínio | `--filter-namespace PontoVoz.Tests.Domain` | total: 278  falhou: 0  ignorados: 0 |
| Aplicação | `--filter-namespace PontoVoz.Tests.Application` | total: 144  falhou: 0  ignorados: 0 |
| Segurança administrativa | `--filter-class *AdministracaoAutorizacaoTests` | total: 26  falhou: 0  ignorados: 0 |
| Provedor simulado | `--filter-class *ProvedorPrevisaoSimuladoTests` | total: 22  falhou: 0  ignorados: 0 |
| Ranking (alternativas) | `--filter-class *Alternativas*` | total: 39  falhou: 0  ignorados: 0 |
| Resumo | `--filter-class *ResumoParadaServiceTests` | total: 12  falhou: 0  ignorados: 0 |
| OpenAPI exportado = API | `--filter-method *OpenApiExportado_EstaIgualAoDocumentoGeradoPelaApi` | total: 1  falhou: 0  ignorados: 0 |
| Exemplos JSON do contrato | `--filter-method *Contrato*` | total: 43  falhou: 0  ignorados: 0 |

O filtro `*Contrato*` reúne os testes que comparam as respostas com os exemplos JSON do contrato
e os casos J1 a J5 e T1 a T7 da regra temporal. O documento OpenAPI servido pela API do clone
também foi comparado com `contracts/openapi/pontovoz-v1.json`: são iguais, com 14 operações.

O `dotnet ef database update` da pasta de trabalho rodou sobre o banco de desenvolvimento, que já
estava atualizado: a inspeção somente leitura antes e depois deu o mesmo resultado, igual à
linha de base registrada no B6.

## Segurança e higiene do repositório

```text
chave nos logs: 0 linha(s)
chamadas à SPTrans nos logs: 0 linha(s)
endereços em escuta (logs): Now listening on: http://localhost:5080 Now listening on: http://localhost:5081 
```

- **Chave administrativa:** comparada em tempo constante (`CryptographicOperations.FixedTimeEquals`);
  401 com `WWW-Authenticate` sem chave ou com chave errada; 403 fora de Development sem chave
  configurada; o valor não aparece em respostas nem em logs. Em Development sem chave, o acesso
  local é permitido com aviso no log, como prevê o contrato; isso não é adequado para uma API
  exposta.
- **Segredos:** nenhum arquivo versionado tem chave, token, senha ou chave privada. A única
  ocorrência de `Password=` é um valor fictício num teste que confere que a connection string não
  vaza (`InfraestruturaHttpTests`). O `appsettings.json` não tem `Admin:Chave` nem token, e o
  adaptador Olho Vivo vem com `Habilitado: false`.
- **Arquivos indevidos:** nenhum banco SQLite, `bin/`, `obj/`, log, ZIP, executável ou backup é
  versionado (204 arquivos, todos de código, documentação ou evidência).
- **Entrada e SQL:** as consultas usam LINQ parametrizado pelo EF Core, sem SQL bruto
  (`FromSql` ou `ExecuteSql`); parâmetros e corpos são validados antes de qualquer acesso ao banco.
- **Erros 500:** mensagem genérica, sem detalhes internos; o erro real fica só no log.
- **CORS:** lista de origens vazia por padrão; quando configurada, só `GET`, sem credenciais.
- **Rede:** a API escuta só em `localhost`.
- **Dados pessoais:** o seed só tem dados fictícios `SIM-`. O campo `origemRegistro` recusa nomes
  com espaço ou maiúscula, e-mails e telefones formatados, mas **não** impede toda informação
  pessoal (uma sequência só de dígitos passa). A limitação está documentada no contrato, no
  `docs/dados.md` e no `backend/README.md`; a mitigação é usar só rótulos técnicos, como
  `operador-local`.
- **Caminhos pessoais:** só aparecem dentro dos prompts originais registrados em `prompts.md`
  (a pasta local do projeto), que são mantidos no texto integral.
- **Olho Vivo:** desligado por padrão e **não validado** contra a API real (sem credenciais
  oficiais). Nenhuma requisição desta validação chamou a SPTrans.

## Problemas encontrados e soluções

| Problema | Solução |
|---|---|
| O `backend/README.md` ainda dizia que "só o `/health` está disponível", que o resumo "só será criado" e que o B9 aguardava auditoria | Frases corrigidas para o estado atual |
| Quadros "funciona e pendente" do B3, do B4 e do B6 podiam ser lidos como o estado atual | Mantidos como registro, com título e nota de "retrato histórico" |
| O README da raiz não tinha execução, tecnologias, arquitetura nem links para as evidências | README reorganizado como ponto de entrada, sem repetir os detalhes do `backend/README.md` |
| O indicador "testes automatizados aprovados" constava como pendente | Atualizado com o resultado medido (805 de 805) |
