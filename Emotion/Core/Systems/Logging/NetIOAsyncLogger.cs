#nullable enable

#region Using

using Emotion.Core.Platform.Implementation.Win32.Native.Kernel32;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

#endregion

namespace Emotion.Core.Systems.Logging;

/// <summary>
/// The default desktop-platform based logger.
/// </summary>
public class NetIOAsyncLogger : LoggingProvider
{
    private const int MAX_LOG_FILES = 10;
    private const int MAX_LOG_SIZE = 10000000;

    private bool _stdOut;
    protected string _logFolder;

    private readonly Channel<LogEntry> _logChannel;
    private readonly Task _loggingThread = Task.CompletedTask;

    /// <summary>
    /// Create a default logger.
    /// </summary>
    /// <param name="stdOut">Whether to redirect logs to STDOUT as well.</param>
    /// <param name="logFolder">The folder to log to. "Logs" by default/</param>
    public NetIOAsyncLogger(bool stdOut, string? logFolder = null)
    {
        _stdOut = stdOut;
        if (_stdOut) TryEnableVirtualTerminalProcessing();

        logFolder ??= Path.Join(".", "Logs");
        _logFolder = logFolder;

        _logChannel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        try
        {
            // Keep only the last 10 logs.
            Directory.CreateDirectory(_logFolder);
            string[] fileCount = Directory.GetFiles(_logFolder);
            if (fileCount.Length > MAX_LOG_FILES)
            {
                FileInfo[] filesWithDates = fileCount.Select(x => new FileInfo(x)).OrderBy(x => x.LastWriteTime.ToFileTime()).ToArray();

                // Delete oldest.
                try
                {
                    for (var i = 0; i < filesWithDates.Length - MAX_LOG_FILES; i++)
                    {
                        filesWithDates[i].Delete();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _loggingThread = Task.Factory.StartNew(ConsumerLoopAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        catch (Exception)
        {
            // ignored - no where to log it
        }
    }

    protected virtual string GenerateLogName()
    {
        return $"{_logFolder}{Path.DirectorySeparatorChar}{DateTime.Now:MM-dd-yyyy_HH-mm-ss}";
    }

    /// <inheritdoc />
    public override void Log(MessageType type, string source, string message)
    {
        LogSpan(type, source, message);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _logChannel.Writer.Complete();
        _loggingThread.Wait(TimeSpan.FromSeconds(30));
    }

    #region Log Queue Management

    protected override void SubmitLogDataForWriting(MessageType type, char[] buffer, int length)
    {
        if (!_logChannel.Writer.TryWrite(new LogEntry(type, buffer, length)))
            ArrayPool<char>.Shared.Return(buffer);
    }

    private readonly struct LogEntry
    {
        public readonly MessageType Type;
        public readonly char[] Buffer;
        public readonly int Length;

        public LogEntry(MessageType type, char[] buffer, int length)
        {
            Type = type;
            Buffer = buffer;
            Length = length;
        }
    }

    private async Task ConsumerLoopAsync()
    {
        string logFileNameBase = GenerateLogName();
        int logCount = 0;
        int currentFileBytes = 0;

        StreamWriter fileWriter = new StreamWriter($"{logFileNameBase}.log", false, Encoding.UTF8);
        TextWriter? consoleWriter = _stdOut ? Console.Out : null;

        await foreach (LogEntry entry in _logChannel.Reader.ReadAllAsync())
        {
            // 1. File Write (Plain)
            fileWriter.Write(GetPlainTag(entry.Type));
            fileWriter.Write(entry.Buffer, 0, entry.Length);
            fileWriter.WriteLine();

            // 2. Console Write (Colored)
            if (consoleWriter != null)
            {
                consoleWriter.Write(GetColorTag(entry.Type));
                consoleWriter.Write(entry.Buffer, 0, entry.Length);
                consoleWriter.WriteLine("\x1b[0m"); // Reset color
            }

            currentFileBytes += entry.Length + 10;
            ArrayPool<char>.Shared.Return(entry.Buffer);

            if (currentFileBytes > MAX_LOG_SIZE)
            {
                fileWriter.Dispose();
                logCount++;
                fileWriter = new StreamWriter($"{logFileNameBase}_{logCount}.log", false, Encoding.UTF8);
                currentFileBytes = 0;
            }
        }

        fileWriter.Dispose();
    }

    #endregion

    #region Metadata

    private static string GetColorTag(MessageType type) => type switch
    {
        MessageType.Error => "\x1b[38;5;0045m[ERR]\x1b[38;5;0015m ",
        MessageType.Info => "\x1b[38;5;0007m[INF]\x1b[38;5;0015m ",
        MessageType.Trace => "\x1b[38;5;0008m[DBG]\x1b[38;5;0015m ",
        MessageType.Warning => "\x1b[38;5;0011m[WRN]\x1b[38;5;0015m ",
        _ => "[???] "
    };

    private static string GetPlainTag(MessageType type) => type switch
    {
        MessageType.Error => "[ERR] ",
        MessageType.Info => "[INF] ",
        MessageType.Trace => "[DBG] ",
        MessageType.Warning => "[WRN] ",
        _ => "[???] "
    };

    #endregion

    #region Win32 ANSI Terminal Enable

    private static void TryEnableVirtualTerminalProcessing()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        IntPtr handle = Kernel32.GetStdHandle(StdHandle.STD_OUTPUT_HANDLE);
        if (handle == IntPtr.Zero || handle == new IntPtr(-1)) return;
        if (!Kernel32.GetConsoleMode(handle, out uint mode)) return;

        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) != 0) return; // Already set
        Kernel32.SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }

    #endregion
}

public class NetUIAsyncLoggerSingleFile : NetIOAsyncLogger
{
    private string _fileName;

    public NetUIAsyncLoggerSingleFile(string folder, string fileName) : base(false, folder)
    {
        _fileName = fileName;
    }

    protected override string GenerateLogName()
    {
        return $"{_logFolder}{Path.DirectorySeparatorChar}{_fileName}";
    }
}