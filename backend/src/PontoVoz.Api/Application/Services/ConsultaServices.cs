using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Mapeamentos;
using PontoVoz.Api.Application.Validacao;
using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Application.Services;

/// <summary>Busca e detalhe de paradas. Valida antes de consultar; inativas contam como inexistentes.</summary>
public sealed class ParadaService(IParadaRepository paradas, ILinhaRepository linhas) : IParadaService
{
    public async Task<PaginaDto<ParadaDto>> BuscarAsync(ConsultaParadas consulta, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        var validacao = new ValidacaoParametros();
        string? termo = validacao.Termo(consulta.Termo);
        string? codigoExterno = validacao.CodigoExterno(consulta.CodigoExterno);
        Paginacao? paginacao = validacao.Paginacao(consulta.Pagina, consulta.TamanhoPagina);
        validacao.Confirmar();

        Pagina<Parada> pagina = await paradas.BuscarAtivasAsync(termo, codigoExterno, paginacao!, cancellationToken);
        return pagina.ParaDto(MapeamentoDtos.ParaDto);
    }

    public async Task<ParadaDetalheDto> ObterDetalheAsync(int id, CancellationToken cancellationToken)
    {
        // Um id menor que 1 não chega a corresponder à rota (contrato, seção 2): é tratado como inexistente.
        Parada parada = (id >= 1 ? await paradas.ObterAtivaAsync(id, cancellationToken) : null)
            ?? throw ErroAplicacaoException.ParadaNaoEncontrada(id);

        IReadOnlyList<Linha> linhasAtivas = await linhas.ListarAtivasDaParadaAsync(parada.Id, cancellationToken);
        return parada.ParaDetalheDto(linhasAtivas);
    }
}

/// <summary>Busca de linhas ativas, por código, letreiro ou terminais.</summary>
public sealed class LinhaService(ILinhaRepository linhas) : ILinhaService
{
    public async Task<PaginaDto<LinhaDto>> BuscarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        var validacao = new ValidacaoParametros();
        string? termo = validacao.Termo(consulta.Termo);
        Paginacao? paginacao = validacao.Paginacao(consulta.Pagina, consulta.TamanhoPagina);
        validacao.Confirmar();

        Pagina<Linha> pagina = await linhas.BuscarAtivasAsync(termo, paginacao!, cancellationToken);
        return pagina.ParaDto(MapeamentoDtos.ParaDto);
    }
}

/// <summary>Busca de destinos ativos pelo nome, sem diferenciar acentos.</summary>
public sealed class DestinoService(IDestinoRepository destinos) : IDestinoService
{
    public async Task<PaginaDto<DestinoDto>> BuscarAsync(ConsultaPaginada consulta, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        var validacao = new ValidacaoParametros();
        string? termo = validacao.Termo(consulta.Termo);
        Paginacao? paginacao = validacao.Paginacao(consulta.Pagina, consulta.TamanhoPagina);
        validacao.Confirmar();

        Pagina<Destino> pagina = await destinos.BuscarAtivosAsync(termo, paginacao!, cancellationToken);
        return pagina.ParaDto(MapeamentoDtos.ParaDto);
    }
}
