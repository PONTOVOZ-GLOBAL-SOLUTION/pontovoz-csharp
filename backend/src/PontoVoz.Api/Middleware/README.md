# Middleware

Configuração da camada HTTP e tratamento global de erros (B6). Os controllers não tratam
exceções: os erros chegam aqui e saem em ProblemDetails (RFC 9457), no formato da seção 7 de
[`contracts/api-v1.md`](../../../../contracts/api-v1.md).

| Arquivo | Papel |
|---|---|
| `WebApiExtensions.cs` | Registra controllers com o `JsonContrato`, ProblemDetails (com `instance` e o `codigo` dos erros gerados pelo roteamento), o tratador de exceções, a resposta dos erros de model binding (400 `RequisicaoInvalida`) e a política de CORS lida de `Cors:OrigensPermitidas`. |
| `TratadorExcecoes.cs` | `IExceptionHandler`: converte a exceção em ProblemDetails e escreve a resposta. Exceção inesperada vira 500 `ErroInterno` genérico e é registrada no log. |
| `ProblemasHttp.cs` | Tabela única de conversão: `ErroAplicacaoException` (400, 404, 422), exceções de domínio (400, 422), o 500 genérico e o `codigo` dos erros do roteamento (404 `RotaNaoEncontrada`, 405 `MetodoNaoPermitido`). |

Cancelamento: uma requisição abortada pelo cliente não chega ao tratador. O
`ExceptionHandlerMiddleware` do ASP.NET Core reconhece o cancelamento, registra em Debug e não
escreve resposta, então ele nunca vira 500 (teste `Cancelamento_ChegaAoServiceENaoViraErroInterno`).
