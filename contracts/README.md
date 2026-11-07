# Contratos de integração do PontoVoz

Esta pasta reúne os contratos que permitem que a interface do totem e o componente
de voz e IA consumam o backend C# sem depender de detalhes internos dele.

> **Estado atual (Bloco B0):** os endpoints de transporte público **ainda não existem**.
> Por isso não há contrato OpenAPI de negócio publicado aqui. O rascunho dos endpoints
> será adicionado no bloco B1, e o arquivo OpenAPI gerado pela própria API será
> exportado para `contracts/openapi/` a partir do bloco B6.

## Arquitetura de integração

```
Interface do totem ─┐
                    ├── HTTP + JSON ──►  Backend C# (ASP.NET Core)  ──►  SQLite
Voz / IA ───────────┘                     │
                                          └──► provedor de previsões (simulado ou Olho Vivo)
```

- O backend entrega **dados estruturados e avisos**. Montar frases faladas, conduzir o
  diálogo e desenhar a tela ficam com os componentes de voz/IA e da interface.
- Os componentes se comunicam apenas por HTTP, sem compartilhar banco de dados nem código.

## Endereço local

| Perfil | URL base |
|---|---|
| `http` (recomendado para a equipe) | `http://localhost:5080` |
| `https` | `https://localhost:7080` (exige certificado de desenvolvimento confiável) |

## Endpoints disponíveis hoje

| Verbo | Rota | Finalidade |
|---|---|---|
| GET | `/health` | Indica se o processo da API está ativo. Ainda não verifica banco de dados. |
| GET | `/openapi/v1.json` | Documento OpenAPI gerado pela API (somente em Development) |
| GET | `/scalar` | Interface interativa da documentação (somente em Development) |

## Convenções planejadas para os endpoints de negócio

- Prefixo versionado `/api/v1`. Depois do congelamento do contrato, só mudanças aditivas.
- Rotas públicas apenas com GET. Rotas administrativas em `/api/v1/admin`, protegidas por chave.
- JSON com nomes de campos em camelCase, em português. Enums como texto.
- Datas em ISO 8601 com deslocamento de fuso.
- Erros no formato `ProblemDetails` (RFC 9457).
- Respostas com dados simulados trazem `dadosSimulados: true` e um campo de aviso.

## Como a equipe vai receber o contrato

1. **B1:** rascunho dos endpoints e exemplos de JSON neste diretório, para revisão da equipe.
2. **B6 em diante:** o arquivo `openapi/pontovoz-v1.json` é exportado da API em execução
   e atualizado a cada mudança de endpoint.
3. Mudanças no contrato serão registradas na seção abaixo.

## Histórico de mudanças do contrato

| Data | Mudança |
|---|---|
| 06/11/2026 | Estrutura inicial. Apenas `/health` implementado. |
