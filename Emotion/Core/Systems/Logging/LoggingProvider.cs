#nullable enable

using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.Logging;

/// <summary>
/// Provides logging of actions.
/// </summary>
[DontSerialize]
public abstract class LoggingProvider : IDisposable
{
    public bool ExcludeExtraData;

    private HashSet<string> _oncePrint = new HashSet<string>();

    /// <summary>
    /// Create a new logging provider.
    /// </summary>
    protected LoggingProvider()
    {
        // Attach to default std err.
        var redirectErr = new StringWriterExt(true);
        Console.SetError(redirectErr);
        redirectErr.OnUpdate += () => { Error(redirectErr.ToString(), MessageSource.StdErr); };
    }

    #region Error

    /// <summary>
    /// Log an error.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="source">The message source.</param>
    public void Error(string message, string source)
    {
        Log(MessageType.Error, source, message);
    }

    /// <summary>
    /// Log an error.
    /// </summary>
    /// <param name="ex">The error's exception.</param>
    public void Error(Exception ex)
    {
        Log(MessageType.Error, MessageSource.StdExp, ex.ToString());
    }

    #endregion

    /// <summary>
    /// Log a warning. These are usually workarounds or destabilization of code.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="source">The message source.</param>
    /// <param name="once">Print only once.</param>
    public void Warning(string message, string source, bool once = false)
    {
        if (once)
            lock (_oncePrint)
            {
                if (_oncePrint.Contains(message)) return;
                _oncePrint.Add(message);
            }

        Log(MessageType.Warning, source, message);
    }

    /// <summary>
    /// Log an info message. These are general spam.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="source">The message source.</param>
    public void Info(string message, string source)
    {
        Log(MessageType.Info, source, message);
    }

    /// <summary>
    /// Log a trace. These are extreme spam used for debugging.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="source">The message source.</param>
    [Conditional("DEBUG")]
    public void Trace(string message, string source)
    {
        // todo until filtering is introduced.
        if (source == MessageSource.ShaderSource || source == MessageSource.Input) return;

        Log(MessageType.Trace, source, message);
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="type">The type of message to log.</param>
    /// <param name="source">The source of the message.</param>
    /// <param name="message">The message itself.</param>
    public abstract void Log(MessageType type, string source, string message);

    /// <summary>
    /// Stop logging and cleanup.
    /// </summary>
    public abstract void Dispose();

    #region New API

    public void ONE_Info(string source, [InterpolatedStringHandlerArgument("", "source")] ref InfoLogHandler handler)
    {
        // handler manages the logging via Dispose
    }

    public void ONE_Warning(string source, [InterpolatedStringHandlerArgument("", "source")] ref WarningLogHandler handler)
    {
        // handler manages the logging via Dispose
    }

    [Conditional("DEBUG")]
    public void ONE_Trace(string source, [InterpolatedStringHandlerArgument("", "source")] ref TraceLogHandler handler)
    {
        // handler manages the logging via Dispose
    }

    public void ONE_Error(string source, [InterpolatedStringHandlerArgument("", "source")] ref ErrorLogHandler handler)
    {
        // handler manages the logging via Dispose
    }

    public void Log(MessageType type, string source, [InterpolatedStringHandlerArgument("", "type", "source")] ref LogInterpolatedStringHandler handler)
    {
        // handler manages the logging via Dispose
    }

    public void ONE_Info(string source, in ReadOnlySpan<char> msg)
    {
        LogSpan(MessageType.Info, source, in msg);
    }

    public void ONE_Warning(string source, in ReadOnlySpan<char> msg)
    {
        LogSpan(MessageType.Warning, source, in msg);
    }

    [Conditional("DEBUG")]
    public void ONE_Trace(string source, in ReadOnlySpan<char> msg)
    {
        LogSpan(MessageType.Trace, source, in msg);
    }

    public void ONE_Error(string source, in ReadOnlySpan<char> msg)
    {
        LogSpan(MessageType.Error, source, in msg);
    }

    public void LogSpan(MessageType type, string source, in ReadOnlySpan<char> message)
    {
        using LogInterpolatedStringHandler handler = new(message.Length, 0, this, type, source);
        handler.AppendSpan(message);
    }

    #endregion

    #region Log Handlers

    [InterpolatedStringHandler]
    public ref struct TraceLogHandler(int literalLength, int formattedCount, LoggingProvider logger, string source)
    {
        private LogInterpolatedStringHandler _inner = new LogInterpolatedStringHandler(literalLength, formattedCount, logger, MessageType.Trace, source);

        public void AppendLiteral(string s) => _inner.AppendLiteral(s);
        public void AppendFormatted<T>(T val) => _inner.AppendFormatted(val);
        public void Dispose() => _inner.Dispose();
    }

    [InterpolatedStringHandler]
    public ref struct WarningLogHandler(int literalLength, int formattedCount, LoggingProvider logger, string source)
    {
        private LogInterpolatedStringHandler _inner = new LogInterpolatedStringHandler(literalLength, formattedCount, logger, MessageType.Warning, source);

        public void AppendLiteral(string s) => _inner.AppendLiteral(s);
        public void AppendFormatted<T>(T val) => _inner.AppendFormatted(val);
        public void Dispose() => _inner.Dispose();
    }

    [InterpolatedStringHandler]
    public ref struct InfoLogHandler(int literalLength, int formattedCount, LoggingProvider logger, string source)
    {
        private LogInterpolatedStringHandler _inner = new LogInterpolatedStringHandler(literalLength, formattedCount, logger, MessageType.Info, source);

        public void AppendLiteral(string s) => _inner.AppendLiteral(s);
        public void AppendFormatted<T>(T val) => _inner.AppendFormatted(val);
        public void Dispose() => _inner.Dispose();
    }

    [InterpolatedStringHandler]
    public ref struct ErrorLogHandler(int literalLength, int formattedCount, LoggingProvider logger, string source)
    {
        private LogInterpolatedStringHandler _inner = new LogInterpolatedStringHandler(literalLength, formattedCount, logger, MessageType.Error, source);

        public void AppendLiteral(string s) => _inner.AppendLiteral(s);
        public void AppendFormatted<T>(T val) => _inner.AppendFormatted(val);
        public void Dispose() => _inner.Dispose();
    }

    [InterpolatedStringHandler]
    public ref struct LogInterpolatedStringHandler
    {
        private bool _filtered;
        private char[]? _buffer;
        private int _charsWritten;
        private readonly LoggingProvider? _logger;
        private readonly MessageType _type;

        public LogInterpolatedStringHandler(int literalLength, int formattedCount, LoggingProvider logger, MessageType type, string source)
        {
            _filtered = logger.ShouldFilterLine(source);
            if (type == MessageType.Trace)
            {
                // todo until filtering is introduced.
                if (source == MessageSource.ShaderSource || source == MessageSource.Input)
                    _filtered = true;
            }
            if (_filtered) return;

            _logger = logger;
            _type = type;

            string threadName = Thread.CurrentThread.Name ?? (Thread.CurrentThread.IsThreadPoolThread ? "Worker" : "Thread");
            int threadId = Environment.CurrentManagedThreadId;
            float time = Engine.TotalTime;

            // Rent a buffer and write everything into it
            int estimatedSize = literalLength + (formattedCount * 32) + source.Length + threadName.Length + 64;
            char[] buffer = ArrayPool<char>.Shared.Rent(estimatedSize);

            _charsWritten = _logger.WriteLogFormatted(buffer, type, source, threadName, threadId, time);
            _buffer = buffer;
        }

        public void AppendLiteral(string s)
        {
            if (_filtered) return;

            AssertNotNull(_buffer);
            if (_charsWritten >= _buffer.Length) return;

            Span<char> bufferSlice = _buffer.AsSpan(_charsWritten);
            ValueStringWriter writer = new ValueStringWriter(bufferSlice);
            writer.WriteString(s);
            _charsWritten += writer.CharsWritten;
        }

        public void AppendFormatted<T>(T val)
        {
            if (_filtered) return;

            AssertNotNull(_buffer);
            if (_charsWritten >= _buffer.Length) return;

            if (val is not ISpanFormattable formattable)
            {
                AppendLiteral(val?.ToString() ?? "null");
                return;
            }

            Span<char> bufferSlice = _buffer.AsSpan(_charsWritten);
            formattable.TryFormat(bufferSlice, out int bytesWritten, default, default);
            _charsWritten += bytesWritten;
        }

        public void AppendSpan(ReadOnlySpan<char> span)
        {
            if (_filtered) return;

            AssertNotNull(_buffer);
            if (_charsWritten >= _buffer.Length) return;

            Span<char> bufferSlice = _buffer.AsSpan(_charsWritten);
            ValueStringWriter writer = new ValueStringWriter(bufferSlice);
            writer.WriteString(span);
            _charsWritten += writer.CharsWritten;
        }

        public readonly void Dispose()
        {
            if (_filtered) return;

            AssertNotNull(_logger);
            AssertNotNull(_buffer);
            _logger.SubmitLogDataForWriting(_type, _buffer, _charsWritten);
        }
    }

    protected virtual void SubmitLogDataForWriting(MessageType type, char[] buffer, int length)
    {
        ArrayPool<char>.Shared.Return(buffer);
    }

    private const int SOURCE_PADDING_MAX = 8;
    private int WriteLogFormatted(Span<char> dest, MessageType type, ReadOnlySpan<char> source, string threadName, int threadId, float time)
    {
        // Time [Source] [ThreadId:Name] Msg

        ValueStringWriter writer = new ValueStringWriter(dest);

        if (!ExcludeExtraData)
        {
            writer.WriteNumber(time, "00000");
        }

        writer.WriteString(" [");
        writer.WriteString(source);
        writer.WriteString("]");

        int paddingCount = source.Length < SOURCE_PADDING_MAX ? SOURCE_PADDING_MAX - source.Length : 0;
        writer.WriteChar(' ', paddingCount);

        if (!ExcludeExtraData)
        {
            writer.WriteString(" [");
            writer.WriteNumber(threadId, "D2");
            writer.WriteChar(':');
            writer.WriteString(threadName);
            writer.WriteString("] ");
        }

        return writer.CharsWritten;
    }

    #endregion

    #region Filtering

    private readonly List<int> _includeSources = new();

    public bool ShouldFilterLine(ReadOnlySpan<char> source)
    {
        if (_includeSources.Count == 0) return false;
        return !_includeSources.Contains(source.GetStableHashCode());
    }

    public void FilterAddSourceToShow(string source)
    {
        int hash = source.GetStableHashCode();
        if (_includeSources.Contains(hash)) return;
        _includeSources.Add(hash);
    }

    #endregion
}