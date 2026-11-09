# Middleware

Configuração da camada HTTP e tratamento global de erros. Os controllers não tratam exceções: os
erros chegam aqui e saem em ProblemDetails (RFC 9457), no formato da seção 7 de
[`contracts/api-v1.md`](../../../../contracts/api-v1.md).

| Arquivo | Papel |
|---|---|
| `WebApiExtensions.cs` | Registra controllers com o `JsonContrato`, ProblemDetails (com `instance` e o `codigo` dos erros gerados pelo roteamento), o tratador de exceções, a resposta dos erros de model binding (400 `RequisicaoInvalida`) e a política de CORS lida de `Cors:OrigensPermitidas`. |
| `TratadorExcecoes.cs` | `IExceptionHandler`: converte a exceção em ProblemDetails e escreve a resposta. Exceção inesperada vira 500 `ErroInterno` genérico e é registrada no log. |
| `ProblemasHttp.cs` | Tabela única de conversão: `ErroAplicacaoException` (400, 404, 409, 422), exceções de domínio (400 e 422 `ParadaInativa`, `LinhaInativa`, `LinhaNaoAtendeParada`, com título próprio), o 500 genérico e o `codigo` dos erros do roteamento (404 `RotaNaoEncontrada`, 405 `MetodoNaoPermitido`). |

- **Cancelamento:** uma requisição abortada pelo cliente não chega ao tratador. O
  `ExceptionHandlerMiddleware` do ASP.NET Core reconhece o cancelamento, registra em Debug e não
  escreve resposta, então ele nunca vira 500 (teste `Cancelamento_ChegaAoServiceENaoViraErroInterno`).
- **Corpos JSON:** um corpo malformado, ausente ou com `Content-Type` que não é JSON vira 400
  `RequisicaoInvalida` em `errors.corpo`, e os erros de leitura de um campo (`$.dadosSimulados`)
  saem com o nome do campo do contrato. Os 401 e 403 das rotas administrativas são montados em
  `Security/ChaveAdministrativaFilter`.
- **Previsões:** a exceção de domínio `ProvedorPrevisoesIndisponivelException` vira 503
  `ProvedorPrevisoesIndisponivel`, e o `TratadorExcecoes` acrescenta `Retry-After: 30`. Só ela
  produz 503: um erro inesperado do banco continua sendo 500 `ErroInterno`.
