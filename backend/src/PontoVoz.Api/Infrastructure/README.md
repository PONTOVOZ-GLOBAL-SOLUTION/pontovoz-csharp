# Infrastructure

Implementações técnicas do backend. O domínio (`Domain/`) não depende desta pasta.

| Pasta | Conteúdo |
|---|---|
| `Persistence/` | `PontoVozDbContext`, mapeamentos Fluent API, conversor de instantes UTC, coluna de busca normalizada, migrations, seed de dados fictícios e a aplicação das migrations ao iniciar em Development (`PersistenciaExtensions.AplicarMigracoesEmDesenvolvimentoAsync`) |
| `Repositories/` | Implementações das interfaces de `Application/Interfaces`, com EF Core e LINQ. As mesmas classes implementam as interfaces de escrita (`ICadastro*Repository`), e `UnidadeTrabalhoEf` grava as mudanças, convertendo só a violação de chave única ou primária do SQLite em `ViolacaoUnicidadeException` |
| `Providers/` | Provedores de previsão de chegada: `ProvedorPrevisaoSimulado` (padrão, calculado no instante da consulta), `ProvedorOlhoVivo` (adaptador HTTP da SPTrans, desligado e testado só com respostas simuladas), `SeletorProvedorPrevisao` (escolha por configuração; dados `SIM-` sempre no simulado), `OpcoesPrevisoes` e o registro na injeção de dependência |

Tabelas, seed e consultas LINQ: [`../../../README.md`](../../../README.md#persistência-e-linq).
