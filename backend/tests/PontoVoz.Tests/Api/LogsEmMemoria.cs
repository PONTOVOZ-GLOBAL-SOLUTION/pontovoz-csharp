using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace PontoVoz.Tests.Api;

/// <summary>
/// Guarda os logs da API em memória para o teste conferir o que foi registrado (inclusive o texto
/// das exceções), por exemplo que um segredo nunca aparece no log.
/// </summary>
internal sealed class LogsEmMemoria : ILoggerProvider
{
    public ConcurrentQueue<(string Categoria, LogLevel Nivel, string Mensagem)> Entradas { get; } = new();

    /// <summary>Todo o texto registrado, mensagens e exceções, para buscas de vazamento.</summary>
    public string TextoCompleto => string.Join(Environment.NewLine, Entradas.Select(entrada => entrada.Mensagem));

    public ILogger CreateLogger(string categoryName) => new Registrador(categoryName, Entradas);

    public void Dispose()
    {
    }

    private sealed class Registrador(string categoria, ConcurrentQueue<(string, LogLevel, string)> entradas) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            entradas.Enqueue((categoria, logLevel, exception is null ? formatter(state, exception) : $"{formatter(state, exception)}{Environment.NewLine}{exception}"));
    }
}
