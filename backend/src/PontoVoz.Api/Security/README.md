# Security

Proteção das rotas administrativas (`/api/v1/admin/...`) por chave em cabeçalho,
com comportamento diferente em Development e nos demais ambientes.

A chave nunca é gravada em arquivos versionados: usar `dotnet user-secrets`
ou variável de ambiente.

Estado: **vazio**. O filtro de chave administrativa será criado no bloco B7.
