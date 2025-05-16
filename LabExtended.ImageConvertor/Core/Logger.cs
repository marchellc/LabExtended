using CommonLib;

namespace LabExtended.ImageConvertor;

/// <summary>
/// Used to log messages from the Common library into the console.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Starts a new thread that checks the log queue.
    /// </summary>
    public static void Run()
    {
        Task.Run(Update);
    }

    /// <summary>
    /// Logs an entry into the console.
    /// </summary>
    /// <param name="entry">The entry to log.</param>
    public static void Show(CommonLog.LogEntry entry)
    {
        switch (entry.Level)
        {
            case CommonLog.LogLevel.Debug:
                Output(ConsoleColor.Cyan, ConsoleColor.White, "DEBUG", entry.Source, entry.Message);
                break;

            case CommonLog.LogLevel.Info:
                Output(ConsoleColor.Green, ConsoleColor.White, "INFO", entry.Source, entry.Message);
                break;

            case CommonLog.LogLevel.Warn:
                Output(ConsoleColor.Yellow, ConsoleColor.White, "WARN", entry.Source, entry.Message);
                break;

            case CommonLog.LogLevel.Error:
                Output(ConsoleColor.Red, ConsoleColor.White, "ERROR", entry.Source, entry.Message);
                break;
            
            case CommonLog.LogLevel.Raw:
                LogRaw(entry.Message, ConsoleColor.Cyan);
                break;

            case CommonLog.LogLevel.None:
            {
                if (entry.Type is CommonLog.LogType.CommandOutput)
                {
                    if (entry.Message.StartsWith("&!"))
                    {
                        LogRaw($"{entry.Source} >>> {entry.Message.Substring(2, entry.Message.Length - 2)}", ConsoleColor.Red);
                    }
                    else
                    {
                        LogRaw($"{entry.Source} >>> {entry.Message}", ConsoleColor.Magenta);
                    }
                }
                else
                {
                    LogRaw($"[{entry.Type}] {entry.Source} >>> {entry.Message}", ConsoleColor.DarkBlue);
                }
                
                break;
            }
        }
    }

    private static void Update()
    {
        while (true)
        {
            try
            {
                while (CommonLog.Logs.TryDequeue(out var entry))
                {
                    Show(entry);
                }
            }
            catch 
            { 
                // ignored
            }
        }
    }
    
    public static void LogRaw(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    private static void Output(ConsoleColor tagColor, ConsoleColor textColor, string tag, string source, string message)
    {
        Console.ForegroundColor = tagColor;
        
        Console.Write("[");
        Console.Write(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        Console.Write("] ");
        
        Console.Write("[ ");
        Console.Write(tag);
        Console.Write("] ");
        
        Console.Write(" [ ");
        Console.Write(source.ToUpper());
        Console.Write("] ");
        
        Console.ForegroundColor = textColor;
        
        Console.Write(message);
        Console.WriteLine();
        
        Console.ResetColor();
    }
}