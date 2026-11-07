// Verificador dos documentos e exemplos do contrato REST v1 do PontoVoz.
//
// Uso, a partir da raiz do repositório (exige apenas o .NET 10 SDK):
//     dotnet run contracts/verificador/VerificarContrato.cs
// Opcionalmente, informe a raiz do repositório:
//     dotnet run contracts/verificador/VerificarContrato.cs -- <caminho-da-raiz>
//
// Código de saída: 0 sem falhas, 1 com falhas, 2 quando os arquivos não são encontrados.
//
// Regras verificadas:
//   1. Sintaxe de todos os JSON de contracts/exemplos e dos blocos ```json dos documentos.
//   2. Convenções: nomes de campos em camelCase ASCII, datas ISO 8601 em -03:00, valores de enum
//      documentados, IDs inteiros positivos, avisos no formato { codigo, mensagem } e
//      presentes no catálogo da seção 8 do api-v1.md.
//   3. Coerência entre exemplos: partes do resumo, IDs e códigos das linhas, cálculos de minutos,
//      ordenação, valores nulos, regras de alternativas, desatualização de passagens, paginação,
//      requisição e resposta administrativas, e erros contra o catálogo da seção 7.
//   3a. Dados simulados (D16): registro manual dentro da janela de 24 h e 2 min, observação
//      simulada antiga mantida como desatualizada, passagem recente vinda do registro manual,
//      seed idempotente em 4.6.2 e R3, requisitos R1 a R8 e nenhuma menção a renovar
//      observações simuladas ao iniciar a API fora do histórico de divergências; provisionamento
//      inicial separado da inicialização posterior.
//   3b. Regra temporal (4.6.3): casos J e T recalculados com minutos nunca negativos,
//      desatualização pelo intervalo exato e registro futuro fora da consulta.
//   3c. Natureza do dado: dadosSimulados obrigatório nos cadastros administrativos, origem
//      Simulado sempre com dadosSimulados true e CadastroNaoVerificado nas respostas com avisos.
//   4. Documentos: links para arquivos existentes, exemplos referenciados no README e
//      13 rotas sem duplicidade, iguais no api-v1.md e no README.
//   5. Segurança: sem e-mails, sem caminhos locais e sem valor real de chave administrativa.
//
// Passar em todas as regras não prova que o contrato está correto. Ele só mostra que os
// exemplos seguem as regras acima.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

var raiz = args.Length > 0 ? Path.GetFullPath(args[0]) : LocalizarRaiz(Directory.GetCurrentDirectory());
if (raiz is null || !File.Exists(Path.Combine(raiz, "contracts", "api-v1.md")))
{
    Console.Error.WriteLine("Não encontrei contracts/api-v1.md. Execute a partir da raiz do repositório ou informe o caminho.");
    return 2;
}

var contratos = Path.Combine(raiz, "contracts");
var pastaExemplos = Path.Combine(contratos, "exemplos");
var falhas = new List<string>();
var totaisPorGrupo = new List<(string Grupo, int Verificacoes, int Falhas)>();
var grupoAtual = "";
var verificacoesGrupo = 0;
var falhasGrupo = 0;

void IniciarGrupo(string nome)
{
    FecharGrupo();
    grupoAtual = nome;
}

void FecharGrupo()
{
    if (grupoAtual.Length > 0)
    {
        totaisPorGrupo.Add((grupoAtual, verificacoesGrupo, falhasGrupo));
    }

    verificacoesGrupo = 0;
    falhasGrupo = 0;
}

void Verificar(bool condicao, string mensagem)
{
    verificacoesGrupo++;
    if (!condicao)
    {
        falhasGrupo++;
        falhas.Add($"[{grupoAtual}] {mensagem}");
    }
}

// 1. Sintaxe -------------------------------------------------------------------------------
IniciarGrupo("Sintaxe JSON");
var exemplos = new SortedDictionary<string, JsonNode>(StringComparer.Ordinal);
foreach (var arquivo in Directory.GetFiles(pastaExemplos, "*.json").OrderBy(f => f, StringComparer.Ordinal))
{
    var nome = Path.GetFileName(arquivo);
    try
    {
        exemplos[nome] = JsonNode.Parse(File.ReadAllText(arquivo))!;
        Verificar(true, nome);
    }
    catch (JsonException e)
    {
        Verificar(false, $"{nome}: {e.Message}");
    }
}

var apiMd = File.ReadAllText(Path.Combine(contratos, "api-v1.md"));
var readme = File.ReadAllText(Path.Combine(contratos, "README.md"));
var blocosJson = 0;
foreach (var (nomeMd, texto) in new[] { ("api-v1.md", apiMd), ("README.md", readme) })
{
    foreach (Match bloco in Regex.Matches(texto, @"```json\r?\n(.*?)```", RegexOptions.Singleline))
    {
        blocosJson++;
        try
        {
            JsonNode.Parse(bloco.Groups[1].Value);
            Verificar(true, nomeMd);
        }
        catch (JsonException e)
        {
            Verificar(false, $"{nomeMd}: bloco JSON inválido: {e.Message}");
        }
    }
}

string[] obrigatorios =
[
    "paradas-listagem.json", "paradas-busca-codigo.json", "parada-detalhe.json", "destinos-busca.json",
    "previsoes.json", "ultimas-passagens.json", "alternativas.json", "alternativas-sem-comparacao.json",
    "resumo.json", "resumo-provedor-indisponivel.json", "admin-observacao-passagem-requisicao.json",
    "admin-observacao-passagem-resposta.json", "erro-parada-nao-encontrada.json", "erro-validacao.json",
    "erro-provedor-previsoes-indisponivel.json",
];
foreach (var nome in obrigatorios)
{
    Verificar(exemplos.ContainsKey(nome), $"exemplo obrigatório ausente ou inválido: {nome}");
}

if (falhas.Count > 0)
{
    return Encerrar();
}

// 2. Convenções ----------------------------------------------------------------------------
IniciarGrupo("Convenções (campos, datas, enums, IDs, avisos)");
var secaoAvisos = Secao(apiMd, "## 8.", "## 9.");
var codigosAviso = Regex.Matches(secaoAvisos, @"^\| `([A-Za-z]+)` \|", RegexOptions.Multiline)
    .Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
var secaoErros = Secao(apiMd, "## 7.", "## 8.");
var catalogoErros = Regex.Matches(secaoErros, @"^\| `([A-Za-z]+)` \| (\d{3}) \|", RegexOptions.Multiline)
    .ToDictionary(m => m.Groups[1].Value, m => int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));

var campoCamel = new Regex("^[a-z][a-zA-Z0-9]*$");
var dataIso = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?([+-]\d{2}:\d{2}|Z)$");
string[] origens = ["Simulado", "CadastroAdministrativo", "OlhoVivo"];
string[] statusPrevisao = ["Disponivel", "SemPrevisoes", "ProvedorIndisponivel"];
var enums = new Dictionary<string, string[]>(StringComparer.Ordinal)
{
    ["origem"] = origens,
    ["origemPrevisao"] = origens,
    ["origemPercurso"] = origens,
    ["tipo"] = ["Terminal", "Estacao", "PontoReferencia"],
    ["sentido"] = ["PrincipalParaSecundario", "SecundarioParaPrincipal"],
    ["status"] = statusPrevisao,
    ["statusPrevisao"] = statusPrevisao,
    ["nivelConfirmacao"] = ["Previsao", "ObservacaoSimulada", "RegistroManual", "VerificadaFonteConfiavel"],
    ["criterio"] = ["MenorTempoTotalEstimado"],
    ["situacao"] = ["Comparadas", "SemComparacao", "SemLinhaDireta", "Comparavel", "SemPrevisao", "Completo", "Parcial"],
};

void VerificarAviso(JsonNode? aviso, string local)
{
    var objeto = aviso as JsonObject;
    Verificar(objeto is not null && objeto.Count == 2 && objeto["codigo"] is not null && objeto["mensagem"] is not null,
        $"{local}: aviso fora do formato {{ codigo, mensagem }}");
    if (objeto?["codigo"] is JsonNode codigo)
    {
        Verificar(codigosAviso.Contains(codigo.GetValue<string>()), $"{local}: aviso {codigo} não está no catálogo");
    }
}

void Percorrer(JsonNode? no, string arquivo, string caminho)
{
    if (no is JsonObject objeto)
    {
        foreach (var (chave, valor) in objeto)
        {
            var local = $"{arquivo} {caminho}.{chave}";
            Verificar(campoCamel.IsMatch(chave), $"{local}: nome de campo fora do padrão camelCase ASCII");
            if (valor is JsonValue valorSimples && valorSimples.TryGetValue<string>(out var texto))
            {
                if (EhCampoData(chave))
                {
                    Verificar(dataIso.IsMatch(texto), $"{local}: data fora do formato ISO 8601 com fuso");
                    Verificar(texto.EndsWith("-03:00", StringComparison.Ordinal), $"{local}: data de negócio fora de -03:00");
                }

                if (enums.TryGetValue(chave, out var valores))
                {
                    Verificar(valores.Contains(texto), $"{local}: valor de enum desconhecido \"{texto}\"");
                }
            }

            if (chave == "aviso")
            {
                VerificarAviso(valor, local);
            }

            if (chave == "avisos")
            {
                foreach (var aviso in valor!.AsArray())
                {
                    VerificarAviso(aviso, local);
                }
            }

            if (chave == "id" || (chave.EndsWith("Id", StringComparison.Ordinal) && chave != "traceId"))
            {
                Verificar(valor is JsonValue id && id.TryGetValue<int>(out var numero) && numero >= 1, $"{local}: ID deve ser inteiro ≥ 1");
            }

            Percorrer(valor, arquivo, $"{caminho}.{chave}");
        }
    }
    else if (no is JsonArray lista)
    {
        for (var i = 0; i < lista.Count; i++)
        {
            Percorrer(lista[i], arquivo, $"{caminho}[{i}]");
        }
    }
}

foreach (var (nome, conteudo) in exemplos)
{
    Percorrer(conteudo, nome, "$");
}

// 3. Coerência entre exemplos --------------------------------------------------------------
var detalhe = exemplos["parada-detalhe.json"];
var listagem = exemplos["paradas-listagem.json"];
var buscaCodigo = exemplos["paradas-busca-codigo.json"];
var destinos = exemplos["destinos-busca.json"];
var previsoes = exemplos["previsoes.json"];
var ultimas = exemplos["ultimas-passagens.json"];
var alternativas = exemplos["alternativas.json"];
var semComparacao = exemplos["alternativas-sem-comparacao.json"];
var resumo = exemplos["resumo.json"];
var resumoParcial = exemplos["resumo-provedor-indisponivel.json"];

IniciarGrupo("Identificação da parada e paginação");
var semLinhas = (JsonObject)detalhe.DeepClone();
semLinhas.Remove("linhas");
Verificar(listagem["itens"]!.AsArray().Any(p => JsonNode.DeepEquals(p, semLinhas)), "a listagem não contém a parada do detalhe");
Verificar(I(buscaCodigo, "totalItens") == 1 && buscaCodigo["itens"]!.AsArray().Count == 1, "a busca por código deve devolver exatamente um item");
Verificar(JsonNode.DeepEquals(buscaCodigo["itens"]![0], semLinhas), "a busca por código não devolve a parada SIM-P001 do detalhe");
foreach (var (nome, pagina) in new[] { ("paradas-listagem", listagem), ("paradas-busca-codigo", buscaCodigo), ("destinos-busca", destinos) })
{
    var itens = pagina["itens"]!.AsArray();
    var tamanho = I(pagina, "tamanhoPagina");
    Verificar(tamanho is >= 1 and <= 50 && itens.Count <= tamanho, $"{nome}: tamanho de página");
    Verificar(I(pagina, "totalPaginas") == (int)Math.Ceiling(I(pagina, "totalItens") / (double)tamanho), $"{nome}: totalPaginas");
    var nomes = itens.Select(i => S(i, "nome")).ToList();
    Verificar(nomes.SequenceEqual(nomes.Order(StringComparer.Ordinal)), $"{nome}: ordem por nome");
}

IniciarGrupo("IDs e códigos das linhas");
var linhas = detalhe["linhas"]!.AsArray().ToDictionary(l => I(l, "id"));
var codigosLinhas = linhas.Values.Select(l => S(l, "codigo")).ToList();
Verificar(codigosLinhas.SequenceEqual(codigosLinhas.Order(StringComparer.Ordinal)), "linhas do detalhe fora da ordem por código");

void VerificarReferencia(JsonNode? referencia, string local)
{
    var linhaId = I(referencia, "linhaId");
    Verificar(linhas.TryGetValue(linhaId, out var linha), $"{local}: a linha {linhaId} não atende a parada");
    if (linha is not null)
    {
        Verificar(S(referencia, "codigoLinha") == S(linha, "codigo") && S(referencia, "letreiro") == S(linha, "letreiro"),
            $"{local}: código ou letreiro divergente da linha {linhaId}");
    }
}

foreach (var (nome, resposta) in new[]
{
    ("previsoes", previsoes), ("ultimas-passagens", ultimas), ("alternativas", alternativas),
    ("alternativas-sem-comparacao", semComparacao), ("resumo-parcial.previsoes", resumoParcial["previsoes"]),
})
{
    Verificar(I(resposta, "paradaId") == I(detalhe, "id") && S(resposta, "codigoExternoParada") == S(detalhe, "codigoExterno")
        && S(resposta, "nomeParada") == S(detalhe, "nome"), $"{nome}: cabeçalho da parada divergente");
}

foreach (var item in previsoes["previsoes"]!.AsArray()) VerificarReferencia(item, "previsoes");
foreach (var item in previsoes["linhasSemPrevisao"]!.AsArray()) VerificarReferencia(item, "previsoes.linhasSemPrevisao");
foreach (var item in ultimas["passagens"]!.AsArray()) VerificarReferencia(item, "ultimas-passagens");
foreach (var item in ultimas["linhasSemRegistro"]!.AsArray()) VerificarReferencia(item, "ultimas-passagens.linhasSemRegistro");
foreach (var item in alternativas["alternativas"]!.AsArray()) VerificarReferencia(item, "alternativas");
foreach (var item in semComparacao["alternativas"]!.AsArray()) VerificarReferencia(item, "alternativas-sem-comparacao");

IniciarGrupo("Datas e cálculos de previsões");
var consulta = D(previsoes, "consultadoEm");
foreach (var resposta in new[] { ultimas, alternativas, semComparacao, resumo, resumoParcial })
{
    Verificar(D(resposta, "consultadoEm") == consulta, "consultadoEm divergente entre os exemplos");
}

Verificar(S(previsoes, "status") is "Disponivel" or "SemPrevisoes", "previsoes.json: o endpoint só devolve Disponivel ou SemPrevisoes com 200");
foreach (var item in previsoes["previsoes"]!.AsArray())
{
    var espera = MinutosEntre(consulta, D(item, "previstaPara"));
    Verificar(espera >= 0 && I(item, "esperaMinutos") == espera, $"previsoes: esperaMinutos incorreto na linha {I(item, "linhaId")}");
    Verificar(S(item, "nivelConfirmacao") == "Previsao", "previsoes: nivelConfirmacao deve ser Previsao");
}

var chavesPrevisao = previsoes["previsoes"]!.AsArray().Select(p => (S(p, "previstaPara"), S(p, "codigoLinha"))).ToList();
Verificar(chavesPrevisao.SequenceEqual(chavesPrevisao.OrderBy(c => D2(c.Item1)).ThenBy(c => c.Item2, StringComparer.Ordinal)), "previsoes fora de ordem");
var comPrevisao = previsoes["previsoes"]!.AsArray().Select(p => I(p, "linhaId")).ToHashSet();
var semPrevisao = previsoes["linhasSemPrevisao"]!.AsArray().Select(p => I(p, "linhaId")).ToHashSet();
Verificar(comPrevisao.Union(semPrevisao).ToHashSet().SetEquals(linhas.Keys) && !comPrevisao.Overlaps(semPrevisao),
    "previsoes: as linhas com e sem previsão não cobrem exatamente a parada");
Verificar(semPrevisao.Count == 0 || TemAviso(previsoes, "PrevisaoParcial"), "previsoes: falta o aviso PrevisaoParcial");

IniciarGrupo("Últimas passagens");
var limite = I(ultimas, "limiteDesatualizacaoMinutos");
var algumaDesatualizada = false;
foreach (var passagem in ultimas["passagens"]!.AsArray().OfType<JsonNode>())
{
    var observada = D(passagem, "observadaEm");
    Verificar(observada <= consulta, $"passagem {I(passagem, "id")}: observação ainda no futuro não pode aparecer na consulta");
    Verificar(I(passagem, "minutosDesdeObservacao") == MinutosPassagem(observada, consulta), $"passagem {I(passagem, "id")}: minutosDesdeObservacao incorreto");
    Verificar(B(passagem, "desatualizada") == Desatualizada(observada, consulta, limite), $"passagem {I(passagem, "id")}: desatualizada incoerente com o intervalo exato e o limite");
    algumaDesatualizada |= B(passagem, "desatualizada");
    Verificar(!B(passagem, "verificada") && !B(passagem, "oficialSptrans"), $"passagem {I(passagem, "id")}: marcada como verificada ou oficial");
    var nivel = S(passagem, "nivelConfirmacao");
    Verificar(nivel is "ObservacaoSimulada" or "RegistroManual", $"passagem {I(passagem, "id")}: nível {nivel} não é emitido no MVP");
    if (nivel == "ObservacaoSimulada")
    {
        Verificar(passagem["origemRegistro"] is null && S(passagem["aviso"], "codigo") == "ObservacaoSimulada", "observação simulada incoerente");
    }
    else
    {
        Verificar(Regex.IsMatch(S(passagem, "origemRegistro"), "^[a-z0-9-]{3,40}$") && S(passagem["aviso"], "codigo") == "RegistroManualNaoVerificado",
            "registro manual incoerente");
    }
}

var observacoes = ultimas["passagens"]!.AsArray().Select(p => D(p, "observadaEm")).ToList();
Verificar(observacoes.SequenceEqual(observacoes.OrderDescending()), "passagens fora da ordem do mais recente para o mais antigo");
var comRegistro = ultimas["passagens"]!.AsArray().Select(p => I(p, "linhaId")).ToList();
var semRegistro = ultimas["linhasSemRegistro"]!.AsArray().Select(p => I(p, "linhaId")).ToList();
Verificar(comRegistro.Distinct().Count() == comRegistro.Count, "mais de uma passagem para a mesma linha");
Verificar(comRegistro.Union(semRegistro).ToHashSet().SetEquals(linhas.Keys), "as linhas com e sem registro não cobrem a parada");
Verificar(!algumaDesatualizada || TemAviso(ultimas, "PassagemDesatualizada"), "falta o aviso PassagemDesatualizada");
Verificar(semRegistro.Count == 0 || TemAviso(ultimas, "PassagemNaoRegistrada"), "falta o aviso PassagemNaoRegistrada");

IniciarGrupo("Política de dados simulados de passagem");
foreach (var passagem in ultimas["passagens"]!.AsArray().OfType<JsonNode>())
{
    var observada = D(passagem, "observadaEm");
    var registro = D(passagem, "registradaEm");
    Verificar(registro <= consulta, $"passagem {I(passagem, "id")}: registradaEm depois da consulta");
    if (S(passagem, "nivelConfirmacao") == "RegistroManual")
    {
        Verificar(observada >= registro.AddHours(-24) && observada <= registro.AddMinutes(2),
            $"passagem {I(passagem, "id")}: registro manual fora da janela de 24 h antes e 2 min depois do registro");
    }
}

var passagensExemplo = ultimas["passagens"]!.AsArray().OfType<JsonNode>().ToList();
Verificar(passagensExemplo.Any(p => S(p, "nivelConfirmacao") == "ObservacaoSimulada" && B(p, "desatualizada")),
    "o exemplo não mostra uma observação simulada antiga mantida com desatualizada: true");
Verificar(passagensExemplo.Where(p => !B(p, "desatualizada")).All(p => S(p, "nivelConfirmacao") == "RegistroManual"),
    "no exemplo, a passagem recente deve vir do registro manual, não de uma observação simulada renovada");

var secaoDadosSimulados = Secao(apiMd, "#### 4.6.2", "### 4.7");
Verificar(secaoDadosSimulados.Contains("idempotente", StringComparison.Ordinal) && secaoDadosSimulados.Contains("nunca é atualizado", StringComparison.Ordinal),
    "api-v1.md 4.6.2: faltam as regras de seed idempotente e de horário não atualizado");
Verificar(new[] { "Provisionamento inicial", "Inicialização posterior", "Registro manual na demonstração" }.All(t => secaoDadosSimulados.Contains(t, StringComparison.Ordinal)),
    "api-v1.md 4.6.2: faltam as três situações (provisionamento, inicialização posterior e registro manual)");
var secaoRequisitos = Secao(apiMd, "## 11.", "\u0000");
var requisitos = Regex.Matches(secaoRequisitos, @"^\| (R\d+) \|", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToList();
Verificar(requisitos.SequenceEqual(Enumerable.Range(1, 8).Select(n => $"R{n}")), "api-v1.md 11: esperados os requisitos R1 a R8");
Verificar(Regex.IsMatch(secaoRequisitos, @"^\| R3 \|[^\n]*idempotente", RegexOptions.Multiline), "api-v1.md 11: R3 deve exigir seed idempotente");
foreach (var (nomeMd, texto) in new[] { ("api-v1.md (exceto a seção 10)", apiMd.Replace(Secao(apiMd, "## 10.", "## 11."), "", StringComparison.Ordinal)), ("README.md", readme) })
{
    Verificar(!Regex.IsMatch(texto, @"RenovarObservacoesSimuladas|substitui, a cada inicialização", RegexOptions.IgnoreCase),
        $"{nomeMd}: ainda descreve renovação de observações simuladas ao iniciar a API");
}

IniciarGrupo("Alternativas");
var proximaChegada = new Dictionary<int, JsonNode>();
foreach (var previsao in previsoes["previsoes"]!.AsArray())
{
    proximaChegada.TryAdd(I(previsao, "linhaId"), previsao!);
}

Verificar(I(alternativas["destino"], "id") == I(destinos["itens"]![0], "id") && S(alternativas["destino"], "nome") == S(destinos["itens"]![0], "nome"),
    "alternativas: destino diferente do encontrado na busca");
var comparaveis = new List<JsonNode>();
var viuSemPrevisao = false;
foreach (var alternativa in alternativas["alternativas"]!.AsArray())
{
    Verificar(I(alternativa, "percursoEstimadoMinutos") is >= 1 and <= 240 && I(alternativa, "baldeacoes") == 0, "alternativas: percurso ou baldeações fora da regra");
    Verificar(S(alternativa, "origemPercurso") == "Simulado", "alternativas: percurso do MVP deve ser identificado como simulado");
    if (S(alternativa, "situacao") == "Comparavel")
    {
        Verificar(!viuSemPrevisao, "alternativas: linha comparável depois de uma sem previsão");
        var previsao = proximaChegada.GetValueOrDefault(I(alternativa, "linhaId"));
        Verificar(previsao is not null && S(previsao, "previstaPara") == S(alternativa, "previstaPara") && I(previsao, "esperaMinutos") == I(alternativa, "esperaMinutos"),
            $"alternativas: a linha {I(alternativa, "linhaId")} não usa a próxima chegada prevista");
        Verificar(I(alternativa, "tempoTotalEstimadoMinutos") == I(alternativa, "esperaMinutos") + I(alternativa, "percursoEstimadoMinutos"), "alternativas: tempo total diferente de espera + percurso");
        Verificar(D(alternativa, "chegadaEstimadaAoDestino") == D(alternativa, "previstaPara").AddMinutes(I(alternativa, "percursoEstimadoMinutos")), "alternativas: chegada ao destino incorreta");
        comparaveis.Add(alternativa!);
    }
    else
    {
        viuSemPrevisao = true;
        Verificar(!proximaChegada.ContainsKey(I(alternativa, "linhaId")), "alternativas: linha com previsão marcada como sem previsão");
        VerificarNulosSemPrevisao(alternativa, "alternativas");
    }
}

Verificar(comparaveis.Select(a => I(a, "posicao")).SequenceEqual(Enumerable.Range(1, comparaveis.Count)), "alternativas: posições não sequenciais");
var chavesAlternativas = comparaveis.Select(a => (I(a, "tempoTotalEstimadoMinutos"), I(a, "esperaMinutos"), S(a, "codigoLinha"))).ToList();
Verificar(chavesAlternativas.SequenceEqual(chavesAlternativas.OrderBy(c => c.Item1).ThenBy(c => c.Item2).ThenBy(c => c.Item3, StringComparer.Ordinal)),
    "alternativas fora da ordem por tempo total, espera e código");
Verificar(comparaveis.Count > 1 && I(comparaveis[0], "esperaMinutos") > comparaveis.Min(a => I(a, "esperaMinutos")),
    "o exemplo não demonstra o caso 'chega primeiro, mas demora mais'");
var todasComparaveis = comparaveis.Count == alternativas["alternativas"]!.AsArray().Count;
Verificar(S(alternativas, "situacao") == "Comparadas", "alternativas.json: situação deveria ser Comparadas");
Verificar(B(alternativas, "comparacaoCompleta") == (S(alternativas, "situacao") == "Comparadas" && todasComparaveis), "alternativas.json: comparacaoCompleta incoerente");
Verificar(B(alternativas, "comparacaoCompleta") || TemAviso(alternativas, "ComparacaoIncompleta"), "alternativas.json: falta o aviso ComparacaoIncompleta");
Verificar(TemAviso(alternativas, "LimitacaoLinhasDiretas") && TemAviso(alternativas, "PercursoEstimado"), "alternativas.json: faltam avisos obrigatórios");

Verificar(S(semComparacao, "situacao") == "SemComparacao" && !B(semComparacao, "comparacaoCompleta"), "sem-comparacao: situação ou comparacaoCompleta incorretas");
Verificar(S(semComparacao, "statusPrevisao") == "ProvedorIndisponivel", "sem-comparacao: statusPrevisao deveria ser ProvedorIndisponivel");
Verificar(TemAviso(semComparacao, "SemComparacao") && TemAviso(semComparacao, "ProvedorIndisponivel") && TemAviso(semComparacao, "PercursoEstimado"),
    "sem-comparacao: faltam avisos");
foreach (var alternativa in semComparacao["alternativas"]!.AsArray())
{
    Verificar(S(alternativa, "situacao") == "SemPrevisao", "sem-comparacao: todas as linhas deveriam estar sem previsão");
    VerificarNulosSemPrevisao(alternativa, "sem-comparacao");
}

var chavesSemComparacao = semComparacao["alternativas"]!.AsArray().Select(a => (I(a, "percursoEstimadoMinutos"), S(a, "codigoLinha"))).ToList();
Verificar(chavesSemComparacao.SequenceEqual(chavesSemComparacao.OrderBy(c => c.Item1).ThenBy(c => c.Item2, StringComparer.Ordinal)), "sem-comparacao fora da ordem por percurso");
var percursos = alternativas["alternativas"]!.AsArray().ToDictionary(a => I(a, "linhaId"), a => I(a, "percursoEstimadoMinutos"));
var percursosSemComparacao = semComparacao["alternativas"]!.AsArray().ToDictionary(a => I(a, "linhaId"), a => I(a, "percursoEstimadoMinutos"));
Verificar(percursos.Count == percursosSemComparacao.Count && percursos.All(p => percursosSemComparacao.GetValueOrDefault(p.Key) == p.Value),
    "sem-comparacao: linhas ou percursos diferentes de alternativas.json");

IniciarGrupo("Resumo");
Verificar(JsonNode.DeepEquals(resumo["parada"], detalhe), "resumo.parada difere de parada-detalhe.json");
Verificar(JsonNode.DeepEquals(resumo["previsoes"], previsoes), "resumo.previsoes difere de previsoes.json");
Verificar(JsonNode.DeepEquals(resumo["ultimasPassagens"], ultimas), "resumo.ultimasPassagens difere de ultimas-passagens.json");
Verificar(S(resumo, "situacao") == "Completo" && resumo["partesIndisponiveis"]!.AsArray().Count == 0 && resumo["avisos"]!.AsArray().Count == 0,
    "resumo.json: deveria estar completo, sem partes indisponíveis nem avisos gerais");
Verificar(JsonNode.DeepEquals(resumoParcial["parada"], detalhe) && JsonNode.DeepEquals(resumoParcial["ultimasPassagens"], ultimas),
    "resumo parcial: parada ou passagens divergentes");
Verificar(S(resumoParcial, "situacao") == "Parcial" && resumoParcial["partesIndisponiveis"]!.ToJsonString() == "[\"previsoes\"]",
    "resumo parcial: situação ou partesIndisponiveis incorretas");
Verificar(TemAviso(resumoParcial, "ResumoParcial"), "resumo parcial: falta o aviso ResumoParcial");
var previsoesParcial = resumoParcial["previsoes"];
Verificar(S(previsoesParcial, "status") == "ProvedorIndisponivel" && previsoesParcial!["previsoes"]!.AsArray().Count == 0 && TemAviso(previsoesParcial, "ProvedorIndisponivel"),
    "resumo parcial: o bloco de previsões deveria indicar ProvedorIndisponivel");
Verificar(previsoesParcial!["linhasSemPrevisao"]!.AsArray().Select(l => I(l, "linhaId")).ToHashSet().SetEquals(linhas.Keys),
    "resumo parcial: linhasSemPrevisao deveria listar todas as linhas");

IniciarGrupo("Operação administrativa");
var requisicao = exemplos["admin-observacao-passagem-requisicao.json"].AsObject();
var respostaAdmin = exemplos["admin-observacao-passagem-resposta.json"];
Verificar(requisicao.Select(p => p.Key).Order(StringComparer.Ordinal)
    .SequenceEqual(new[] { "linhaId", "observadaEm", "origemRegistro", "paradaId", "prefixoVeiculo" }), "requisição administrativa com campos diferentes do contrato");
Verificar(I(respostaAdmin, "paradaId") == I(requisicao, "paradaId") && I(respostaAdmin, "linhaId") == I(requisicao, "linhaId")
    && S(respostaAdmin, "observadaEm") == S(requisicao, "observadaEm"), "a resposta administrativa difere da requisição");
var registrada = D(respostaAdmin, "registradaEm");
var observadaAdmin = D(requisicao, "observadaEm");
Verificar(observadaAdmin >= registrada.AddHours(-24) && observadaAdmin <= registrada.AddMinutes(2), "observadaEm fora da janela de 24 h antes e 2 min depois do registro");
Verificar(I(respostaAdmin, "minutosDesdeObservacao") == MinutosPassagem(observadaAdmin, registrada), "resposta administrativa: minutos até o registro");
Verificar(B(respostaAdmin, "desatualizada") == Desatualizada(observadaAdmin, registrada, 60), "resposta administrativa: desatualizada incoerente com o intervalo exato");
var passagemListada = ultimas["passagens"]!.AsArray().FirstOrDefault(p => I(p, "id") == I(respostaAdmin, "id"));
Verificar(passagemListada is not null, "o registro manual criado não aparece em ultimas-passagens.json");
if (passagemListada is not null)
{
    var esperado = (JsonObject)passagemListada.DeepClone();
    var obtido = (JsonObject)respostaAdmin.DeepClone();
    foreach (var campo in new[] { "minutosDesdeObservacao", "desatualizada" })
    {
        esperado.Remove(campo);
        obtido.Remove(campo);
    }

    obtido.Remove("paradaId");
    Verificar(JsonNode.DeepEquals(esperado, obtido), "a resposta administrativa difere da passagem listada");
}

IniciarGrupo("Regra temporal das observações (4.6.3)");
var secaoTemporal = Secao(apiMd, "#### 4.6.3", "### 4.7");
var casosJanela = Regex.Matches(secaoTemporal, @"^\| J\d+ \| (-?\d+) \| (\d{3}) \|", RegexOptions.Multiline);
var casosCalculo = Regex.Matches(secaoTemporal, @"^\| T\d+ \| (-?\d+) \| (\d+) \| (true|false) \| (sim|não) \|", RegexOptions.Multiline);
Verificar(casosJanela.Count >= 5 && casosCalculo.Count >= 7, "api-v1.md 4.6.3: faltam os casos de teste J e T");
var limiteFuturo = -120;
var limitePassado = 24 * 60 * 60;
foreach (Match caso in casosJanela)
{
    var segundos = int.Parse(caso.Groups[1].Value, CultureInfo.InvariantCulture);
    var esperado = segundos >= limiteFuturo && segundos <= limitePassado ? "201" : "400";
    Verificar(caso.Groups[2].Value == esperado, $"4.6.3: caso de janela com {segundos} s deveria resultar em {esperado}");
}

Verificar(casosJanela.Select(c => int.Parse(c.Groups[1].Value, CultureInfo.InvariantCulture)).ToHashSet().IsSupersetOf([limiteFuturo - 1, limiteFuturo, limitePassado, limitePassado + 1]),
    "4.6.3: os casos de janela não cobrem os dois limites e os valores logo fora deles");
var referenciaTeste = new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.FromHours(-3));
foreach (Match caso in casosCalculo)
{
    var segundos = int.Parse(caso.Groups[1].Value, CultureInfo.InvariantCulture);
    var observada = referenciaTeste.AddSeconds(-segundos);
    Verificar(int.Parse(caso.Groups[2].Value, CultureInfo.InvariantCulture) == MinutosPassagem(observada, referenciaTeste),
        $"4.6.3: minutos incorretos no caso de {segundos} s");
    Verificar((caso.Groups[3].Value == "true") == Desatualizada(observada, referenciaTeste, 60), $"4.6.3: desatualizada incorreta no caso de {segundos} s");
    Verificar((caso.Groups[4].Value == "sim") == (segundos >= 0), $"4.6.3: presença na consulta incorreta no caso de {segundos} s");
}

Verificar(casosCalculo.Select(c => int.Parse(c.Groups[1].Value, CultureInfo.InvariantCulture)).ToHashSet().IsSupersetOf([-1, 0, 3600, 3601, 3660]),
    "4.6.3: os casos de cálculo não cobrem 0, 60 min, logo acima de 60 min, 61 min e a tolerância futura");

IniciarGrupo("Natureza do dado e cadastro administrativo");
var blocosSecao5 = new Dictionary<string, List<JsonNode>>();
foreach (var (subsecao, fim) in new[] { ("### 5.2", "### 5.3"), ("### 5.4", "### 5.5") })
{
    blocosSecao5[subsecao] = Regex.Matches(Secao(apiMd, subsecao, fim), @"```json\r?\n(.*?)```", RegexOptions.Singleline)
        .Select(m => JsonNode.Parse(m.Groups[1].Value)!).ToList();
}

var corposCadastro = new[] { blocosSecao5["### 5.2"].FirstOrDefault(), blocosSecao5["### 5.4"].FirstOrDefault() };
foreach (var corpo in corposCadastro)
{
    Verificar(corpo?["dadosSimulados"] is JsonValue valor && valor.TryGetValue<bool>(out _), "corpo de cadastro administrativo sem dadosSimulados booleano");
}

var respostaCadastro = blocosSecao5["### 5.2"].ElementAtOrDefault(1);
Verificar(respostaCadastro is not null && S(respostaCadastro, "origem") == "CadastroAdministrativo"
    && S(respostaCadastro, "codigoExterno") == S(corposCadastro[0], "codigoExterno")
    && BoolOpcional(respostaCadastro, "dadosSimulados") is bool naturezaResposta && naturezaResposta == BoolOpcional(corposCadastro[0], "dadosSimulados"),
    "5.2: a resposta não preserva origem, código ou dadosSimulados da requisição");
foreach (var (corpo, campoCodigo) in new[] { (corposCadastro[0], "codigoExterno"), (corposCadastro[1], "codigo") })
{
    Verificar(!S(corpo, campoCodigo).StartsWith("TESTE-", StringComparison.Ordinal) || BoolOpcional(corpo, "dadosSimulados") == true,
        $"cadastro de teste {S(corpo, campoCodigo)} deveria ter dadosSimulados: true");
}

void VerificarNatureza(JsonNode? no, string arquivo, List<string> origensAdministrativas)
{
    if (no is JsonObject objeto)
    {
        if (objeto["origem"] is JsonValue origem && objeto["dadosSimulados"] is JsonValue)
        {
            Verificar(origem.GetValue<string>() != "Simulado" || B(objeto, "dadosSimulados"), $"{arquivo}: item com origem Simulado e dadosSimulados false");
            if (origem.GetValue<string>() == "CadastroAdministrativo")
            {
                origensAdministrativas.Add(arquivo);
            }
        }

        foreach (var (_, valor) in objeto)
        {
            VerificarNatureza(valor, arquivo, origensAdministrativas);
        }
    }
    else if (no is JsonArray lista)
    {
        foreach (var item in lista)
        {
            VerificarNatureza(item, arquivo, origensAdministrativas);
        }
    }
}

foreach (var (nome, conteudo) in exemplos)
{
    var administrativas = new List<string>();
    VerificarNatureza(conteudo, nome, administrativas);
    if (administrativas.Count > 0 && conteudo["avisos"] is JsonArray)
    {
        Verificar(TemAviso(conteudo, "CadastroNaoVerificado"), $"{nome}: envolve cadastro administrativo e não traz CadastroNaoVerificado");
    }
}

var linhaAvisoCadastro = Regex.Match(secaoAvisos, @"^\| `CadastroNaoVerificado` \|[^\n]*", RegexOptions.Multiline).Value;
Verificar(linhaAvisoCadastro.Contains("avisos", StringComparison.Ordinal) && linhaAvisoCadastro.Contains("não trazem o aviso", StringComparison.Ordinal),
    "seção 8: CadastroNaoVerificado deve dizer em quais listas avisos aparece e onde não aparece");
Verificar(!apiMd.Contains("A resposta pública dela traz o aviso", StringComparison.Ordinal),
    "api-v1.md ainda diz que a resposta da parada traz um aviso que o DTO Parada não tem");

IniciarGrupo("Erros (ProblemDetails)");
foreach (var nome in new[] { "erro-parada-nao-encontrada.json", "erro-validacao.json", "erro-provedor-previsoes-indisponivel.json" })
{
    var erro = exemplos[nome].AsObject();
    foreach (var campo in new[] { "type", "title", "status", "instance", "codigo", "traceId" })
    {
        Verificar(erro.ContainsKey(campo), $"{nome}: falta o campo {campo}");
    }

    var codigo = S(erro, "codigo");
    Verificar(catalogoErros.TryGetValue(codigo, out var status) && status == I(erro, "status"), $"{nome}: código {codigo} e status fora do catálogo");
}

Verificar(S(exemplos["erro-provedor-previsoes-indisponivel.json"], "instance").EndsWith("/previsoes", StringComparison.Ordinal),
    "o 503 de previsões deve se referir ao endpoint de previsões");
Verificar(exemplos["erro-validacao.json"]["errors"] is JsonObject, "erro-validacao.json: falta o objeto errors");

// 4. Documentos ----------------------------------------------------------------------------
IniciarGrupo("Links, exemplos referenciados e rotas");
foreach (var (nomeMd, texto) in new[] { ("api-v1.md", apiMd), ("README.md", readme) })
{
    foreach (Match link in Regex.Matches(texto, @"\]\(((?:exemplos/|verificador/|\.\./)[^)#]+)\)"))
    {
        var alvo = Path.GetFullPath(Path.Combine(contratos, link.Groups[1].Value));
        Verificar(File.Exists(alvo) || Directory.Exists(alvo), $"{nomeMd}: link quebrado {link.Groups[1].Value}");
    }
}

foreach (var nome in exemplos.Keys)
{
    Verificar(readme.Contains($"exemplos/{nome}", StringComparison.Ordinal), $"README.md não referencia {nome}");
}

var padraoRota = new Regex(@"^\| (\d+) \| (GET|POST|PUT) \| `([^`?]+)[^`]*` \|", RegexOptions.Multiline);
var rotasApi = padraoRota.Matches(Secao(apiMd, "## 9.", "## 10.")).Select(m => (Numero: int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), Metodo: m.Groups[2].Value, Rota: m.Groups[3].Value)).ToList();
var rotasReadme = padraoRota.Matches(readme).Select(m => (Numero: int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), Metodo: m.Groups[2].Value, Rota: m.Groups[3].Value)).ToList();
Verificar(rotasApi.Select(r => r.Numero).SequenceEqual(Enumerable.Range(1, 13)), $"api-v1.md: esperadas as rotas 1 a 13, encontradas {rotasApi.Count}");
Verificar(rotasApi.SequenceEqual(rotasReadme), "as rotas do README.md diferem das do api-v1.md");
var rotasNormalizadas = rotasApi.Select(r => $"{r.Metodo} {Regex.Replace(r.Rota, @"\{[^}]+\}", "{}")}").ToList();
Verificar(rotasNormalizadas.Distinct().Count() == rotasNormalizadas.Count, "há duas rotas com o mesmo método e o mesmo formato de caminho");
Verificar(rotasApi.Where(r => r.Rota.StartsWith("/api/v1/admin/", StringComparison.Ordinal)).All(r => r.Metodo != "GET")
    && rotasApi.Where(r => !r.Rota.StartsWith("/api/v1/admin/", StringComparison.Ordinal)).All(r => r.Metodo == "GET"),
    "rotas públicas devem ser GET e rotas administrativas não podem ser GET");

// 5. Segurança -----------------------------------------------------------------------------
IniciarGrupo("Segurança e dados pessoais");
var textos = Directory.GetFiles(contratos, "*.*", SearchOption.AllDirectories)
    .Where(f => f.EndsWith(".md", StringComparison.Ordinal) || f.EndsWith(".json", StringComparison.Ordinal))
    .Select(f => (Arquivo: Path.GetRelativePath(contratos, f), Texto: File.ReadAllText(f)))
    .ToList();
foreach (var (arquivo, texto) in textos)
{
    Verificar(!Regex.IsMatch(texto, @"[\w.+-]+@[\w-]+\.[a-zA-Z]{2,}"), $"{arquivo}: contém endereço de e-mail");
    Verificar(!texto.Contains(@"C:\Users", StringComparison.OrdinalIgnoreCase), $"{arquivo}: contém caminho local");
    foreach (Match chave in Regex.Matches(texto, @"X-PontoVoz-Admin-Key:\s*(\S+)"))
    {
        Verificar(chave.Groups[1].Value.Trim('"', '`', '.', ',') == "<CHAVE_ADMINISTRATIVA_LOCAL>", $"{arquivo}: valor de chave administrativa que não é o marcador");
    }
}

return Encerrar();

int Encerrar()
{
    FecharGrupo();
    grupoAtual = "";
    Console.WriteLine($"Verificador do contrato v1 — {raiz}");
    Console.WriteLine($"Exemplos JSON: {exemplos.Count} | blocos JSON nos documentos: {blocosJson}");
    foreach (var (grupo, total, comFalha) in totaisPorGrupo)
    {
        Console.WriteLine($"  {grupo,-50} {total,5} verificações, {comFalha} falha(s)");
    }

    if (falhas.Count == 0)
    {
        Console.WriteLine("Nenhuma falha nas regras verificadas.");
        return 0;
    }

    Console.WriteLine($"{falhas.Count} falha(s):");
    foreach (var falha in falhas)
    {
        Console.WriteLine($"  - {falha}");
    }

    return 1;
}

void VerificarNulosSemPrevisao(JsonNode? alternativa, string local)
{
    foreach (var campo in new[] { "posicao", "previstaPara", "esperaMinutos", "tempoTotalEstimadoMinutos", "chegadaEstimadaAoDestino" })
    {
        Verificar(alternativa!.AsObject().ContainsKey(campo) && alternativa[campo] is null, $"{local}: {campo} deveria ser null na linha {I(alternativa, "linhaId")}");
    }
}

static string? LocalizarRaiz(string inicio)
{
    for (var pasta = new DirectoryInfo(inicio); pasta is not null; pasta = pasta.Parent)
    {
        if (File.Exists(Path.Combine(pasta.FullName, "contracts", "api-v1.md")))
        {
            return pasta.FullName;
        }
    }

    return null;
}

static string Secao(string texto, string inicio, string fim)
{
    var de = texto.IndexOf(inicio, StringComparison.Ordinal);
    var ate = texto.IndexOf(fim, de + 1, StringComparison.Ordinal);
    return de < 0 ? "" : texto[de..(ate < 0 ? texto.Length : ate)];
}

static bool EhCampoData(string chave) =>
    chave.EndsWith("Em", StringComparison.Ordinal) || chave is "previstaPara" or "chegadaEstimadaAoDestino";

static string S(JsonNode? no, string chave) => no![chave]!.GetValue<string>();

static int I(JsonNode? no, string chave) => no![chave]!.GetValue<int>();

static bool B(JsonNode? no, string chave) => no![chave]!.GetValue<bool>();

static bool? BoolOpcional(JsonNode? no, string chave) =>
    no?[chave] is JsonValue valor && valor.TryGetValue<bool>(out var resultado) ? resultado : null;

static DateTimeOffset D(JsonNode? no, string chave) => D2(S(no, chave));

static DateTimeOffset D2(string valor) => DateTimeOffset.Parse(valor, CultureInfo.InvariantCulture);

static int MinutosEntre(DateTimeOffset inicio, DateTimeOffset fim) => (int)Math.Floor((fim - inicio).TotalMinutes);

// Regra 4.6.3: minutos nunca negativos e desatualização pelo intervalo exato.
static int MinutosPassagem(DateTimeOffset observada, DateTimeOffset referencia) => Math.Max(0, MinutosEntre(observada, referencia));

static bool Desatualizada(DateTimeOffset observada, DateTimeOffset referencia, int limiteMinutos) =>
    referencia - observada > TimeSpan.FromMinutes(limiteMinutos);

static bool TemAviso(JsonNode? resposta, string codigo) =>
    resposta!["avisos"]!.AsArray().Any(a => S(a, "codigo") == codigo);
