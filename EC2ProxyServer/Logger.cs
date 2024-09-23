using System;
using System.IO;

class Logger
{
    private static readonly string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static readonly string logDirectory = Path.Combine(homeDirectory, "proxy_logs");
    private static readonly string logFilePath = Path.Combine(logDirectory, "proxy.log");

    static Logger()
    {
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public static void Log(string message)
    {
        string logMessage = $"{DateTime.Now}: {message}";
        Console.WriteLine(logMessage);

        try
        {
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Failed to log message to file: {ex.Message}");
        }
    }

    public static void LogError(string message, Exception ex)
    {
        string errorLog = $"{DateTime.Now}: ERROR - {message} | Exception: {ex.Message}";
        Console.WriteLine(errorLog);

        try
        {
            File.AppendAllText(logFilePath, errorLog + Environment.NewLine);
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"Failed to log error to file: {ioEx.Message}");
        }
    }
}