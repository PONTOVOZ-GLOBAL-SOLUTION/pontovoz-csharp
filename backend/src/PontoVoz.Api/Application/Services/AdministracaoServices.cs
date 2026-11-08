using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Mapeamentos;
using PontoVoz.Api.Application.Validacao;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Rules;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Application.Services;

// Services das rotas administrativas (contracts/api-v1.md, seção 5). Em todos, a ordem é a do
// contrato (seção 2): corpo (400), recursos (404), estado (422) e só então a gravação. As regras
// de cadastro ficam no domínio; aqui elas são chamadas, não reescritas.

/// <summary>Cadastro, atualização e associação de linhas a paradas (seções 5.2, 5.3 e 5.5).</summary>
public sealed class AdministracaoParadasService(
    ICadastroParadaRepository paradas,
    ICadastroLinhaRepository linhas,
    IUnidadeTrabalho unidadeTrabalho,
    TimeProvider relogio) : IAdministracaoParadasService
{
    public async Task<ParadaAdministrativaDto> CadastrarAsync(CadastroParadaDto requisicao, CancellationToken cancellationToken)
    {
        CadastroParadaDto corpo = ValidacaoCorpo.Exigir(requisicao);
        var validacao = new ValidacaoCorpo();
        string? codigo = validacao.Texto(corpo.CodigoExterno, "codigoExterno",
            valor => RegrasCadastro.Codigo(valor, "codigoExterno", OrigemDado.CadastroAdministrativo));
        string? nome = validacao.Texto(corpo.Nome, "nome", valor => RegrasCadastro.Texto(valor, "nome", 3, 120));
        validacao.Regra(() => RegrasCadastro.TextoOpcional(corpo.Endereco, "endereco", 200));
        Coordenada? coordenada = validacao.Coordenada(corpo.Coordenada);
        bool? dadosSimulados = validacao.Obrigatorio(corpo.DadosSimulados, "dadosSimulados");
        validacao.Confirmar();

        // Verificação prévia para uma resposta clara; o índice único do banco cobre a corrida.
        if (await paradas.CodigoExternoExisteAsync(codigo!, cancellationToken))
        {
            throw ErroAplicacaoException.CodigoJaCadastrado("codigoExterno", codigo!);
        }

        // O domínio ainda confere a regra que depende de mais de um campo: parada fictícia sem endereço nem coordenada.
        Parada parada = Parada.CadastrarAdministrativa(codigo!, nome!, corpo.Endereco, coordenada, dadosSimulados!.Value, relogio.GetUtcNow());
        paradas.Adicionar(parada);
        await SalvarCadastroAsync(unidadeTrabalho, "codigoExterno", codigo!, cancellationToken);
        return parada.ParaAdministrativaDto();
    }

    public async Task<ParadaAdministrativaDto> AtualizarAsync(int id, AtualizacaoParadaDto requisicao, CancellationToken cancellationToken)
    {
        AtualizacaoParadaDto corpo = ValidacaoCorpo.Exigir(requisicao);
        var validacao = new ValidacaoCorpo();
        string? nome = validacao.Texto(corpo.Nome, "nome", valor => RegrasCadastro.Texto(valor, "nome", 3, 120));
        validacao.Regra(() => RegrasCadastro.TextoOpcional(corpo.Endereco, "endereco", 200));
        Coordenada? coordenada = validacao.Coordenada(corpo.Coordenada);
        bool? ativa = validacao.Obrigatorio(corpo.Ativa, "ativa");
        validacao.Confirmar();

        // Inativas também podem ser atualizadas (seção 5.3): só um id inexistente dá 404.
        Parada parada = await paradas.ObterParaAlteracaoAsync(id, cancellationToken)
            ?? throw ErroAplicacaoException.ParadaInexistente(id);

        // Código, origem e dadosSimulados não mudam; associações e passagens continuam no banco.
        parada.AtualizarDados(nome!, corpo.Endereco, coordenada);
        if (ativa!.Value)
        {
            parada.Ativar();
        }
        else
        {
            parada.Desativar();
        }

        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return parada.ParaAdministrativaDto();
    }

    public async Task AssociarLinhaAsync(int paradaId, int linhaId, CancellationToken cancellationToken)
    {
        Parada parada = await paradas.ObterParaAlteracaoAsync(paradaId, cancellationToken)
            ?? throw ErroAplicacaoException.ParadaInexistente(paradaId);
        Linha linha = await linhas.ObterParaAlteracaoAsync(linhaId, cancellationToken)
            ?? throw ErroAplicacaoException.LinhaInexistente(linhaId);

        // O domínio recusa parada ou linha inativa (422) e devolve a associação existente, se houver.
        parada.AssociarLinha(linha, relogio.GetUtcNow());
        try
        {
            await unidadeTrabalho.SalvarAsync(cancellationToken);
        }
        catch (ViolacaoUnicidadeException)
        {
            // Outra requisição gravou a mesma associação entre a leitura e a gravação. A chave
            // composta (ParadaId, LinhaId) impediu a duplicata, e o resultado pedido já existe:
            // a operação continua idempotente (204).
        }
    }

    /// <summary>Grava um cadastro; uma corrida com o mesmo código vira 409, não 500.</summary>
    internal static async Task SalvarCadastroAsync(IUnidadeTrabalho unidadeTrabalho, string campo, string codigo, CancellationToken cancellationToken)
    {
        try
        {
            await unidadeTrabalho.SalvarAsync(cancellationToken);
        }
        catch (ViolacaoUnicidadeException)
        {
            throw ErroAplicacaoException.CodigoJaCadastrado(campo, codigo);
        }
    }
}

/// <summary>Cadastro de linhas (seção 5.4). Cada sentido é um cadastro próprio; não há atualização no MVP.</summary>
public sealed class AdministracaoLinhasService(
    ICadastroLinhaRepository linhas,
    IUnidadeTrabalho unidadeTrabalho,
    TimeProvider relogio) : IAdministracaoLinhasService
{
    public async Task<LinhaAdministrativaDto> CadastrarAsync(CadastroLinhaDto requisicao, CancellationToken cancellationToken)
    {
        CadastroLinhaDto corpo = ValidacaoCorpo.Exigir(requisicao);
        var validacao = new ValidacaoCorpo();
        string? codigo = validacao.Texto(corpo.Codigo, "codigo",
            valor => RegrasCadastro.Codigo(valor, "codigo", OrigemDado.CadastroAdministrativo));
        validacao.Texto(corpo.Letreiro, "letreiro", valor =>
        {
            string letreiro = RegrasCadastro.Texto(valor, "letreiro", 1, 30);
            RegrasCadastro.PrefixoReservado(letreiro, "letreiro", OrigemDado.CadastroAdministrativo);
            return letreiro;
        });
        validacao.Texto(corpo.TerminalPrincipal, "terminalPrincipal", valor => RegrasCadastro.Texto(valor, "terminalPrincipal", 3, 120));
        validacao.Texto(corpo.TerminalSecundario, "terminalSecundario", valor => RegrasCadastro.Texto(valor, "terminalSecundario", 3, 120));
        SentidoLinha? sentido = validacao.Obrigatorio(corpo.Sentido, "sentido");
        if (sentido is SentidoLinha valorSentido)
        {
            validacao.Regra(() => RegrasCadastro.Definido(valorSentido, "sentido"));
        }

        bool? dadosSimulados = validacao.Obrigatorio(corpo.DadosSimulados, "dadosSimulados");
        validacao.Confirmar();

        if (await linhas.CodigoExisteAsync(codigo!, cancellationToken))
        {
            throw ErroAplicacaoException.CodigoJaCadastrado("codigo", codigo!);
        }

        Linha linha = Linha.CadastrarAdministrativa(codigo!, corpo.Letreiro!, corpo.TerminalPrincipal!, corpo.TerminalSecundario!,
            sentido!.Value, dadosSimulados!.Value, relogio.GetUtcNow());
        linhas.Adicionar(linha);
        await AdministracaoParadasService.SalvarCadastroAsync(unidadeTrabalho, "codigo", codigo!, cancellationToken);
        return linha.ParaAdministrativaDto();
    }
}

/// <summary>
/// Registro manual de passagens (seção 5.6). O instante do servidor é lido uma única vez do
/// <see cref="TimeProvider"/> e vale para a janela de 24 horas e 2 minutos, para
/// <c>registradaEm</c> e para os minutos da resposta.
/// </summary>
public sealed class ObservacoesPassagemService(
    ICadastroParadaRepository paradas,
    ICadastroLinhaRepository linhas,
    ICadastroPassagemRepository passagens,
    IUnidadeTrabalho unidadeTrabalho,
    TimeProvider relogio) : IObservacoesPassagemService
{
    public async Task<PassagemRegistradaDto> RegistrarAsync(ObservacaoPassagemDto requisicao, CancellationToken cancellationToken)
    {
        ObservacaoPassagemDto corpo = ValidacaoCorpo.Exigir(requisicao);
        DateTimeOffset agora = relogio.GetUtcNow();

        var validacao = new ValidacaoCorpo();
        int? paradaId = validacao.Id(corpo.ParadaId, "paradaId");
        int? linhaId = validacao.Id(corpo.LinhaId, "linhaId");
        DateTimeOffset? observadaEm = validacao.Obrigatorio(corpo.ObservadaEm, "observadaEm");
        if (observadaEm is DateTimeOffset instante && !RegraTemporalPassagem.ObservacaoAceitaNoRegistro(instante, agora))
        {
            validacao.Erro("observadaEm", PassagemRegistroManual.MensagemForaDaJanela);
        }

        validacao.Regra(() => RegrasCadastro.PrefixoVeiculo(corpo.PrefixoVeiculo));
        string? origemRegistro = validacao.Texto(corpo.OrigemRegistro, "origemRegistro", RegrasCadastro.OrigemRegistro);
        validacao.Confirmar();

        Parada parada = await paradas.ObterParaAlteracaoAsync(paradaId!.Value, cancellationToken)
            ?? throw ErroAplicacaoException.ParadaInexistente(paradaId.Value);
        Linha linha = await linhas.ObterParaAlteracaoAsync(linhaId!.Value, cancellationToken)
            ?? throw ErroAplicacaoException.LinhaInexistente(linhaId.Value);

        // O domínio confere o prefixo pela natureza do registro (400), parada e linha ativas e a
        // associação (422), e fixa RegistroManual, verificada = false e oficialSptrans = false.
        PassagemRegistroManual registro = PassagemRegistroManual.Registrar(parada, linha, observadaEm!.Value, corpo.PrefixoVeiculo, origemRegistro!, agora);
        passagens.Adicionar(registro);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return registro.ParaRegistradaDto(agora);
    }
}
