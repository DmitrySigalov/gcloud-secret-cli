namespace Google.Cloud.SecretManager.Client.Common;

public static class ConsoleHelper
{
    public static void WriteInfo(string text) 
        => Info(() => Console.Write(text));
    
    public static void WriteLineInfo(string text) 
        => Info(() => Console.WriteLine(text));
    
    public static void WriteError(string text) 
        => Error(() => Console.Write(text));
    
    public static void WriteLineError(string text) 
        => Error(() => Console.WriteLine(text));
    
    public static void WriteWarn(string text) 
        => Warn(() => Console.Write(text));
    
    public static void WriteLineWarn(string text) 
        => Warn(() => Console.WriteLine(text));
    
    public static void WriteNotification(string text)
        => Notification(() => Console.Write(text));
    
    public static void WriteLineNotification(string text)
        => Notification(() => Console.WriteLine(text));
    
    public static void Info(Action action)
        => Handle(action, ConsoleColor.Green);
    
    public static void Warn(Action action)
        => Handle(action, ConsoleColor.Blue);
    
    public static void Error(Action action)
        => Handle(action, ConsoleColor.Red);
    
    public static void Notification(Action action)
        => Handle(action, ConsoleColor.DarkYellow);
    
    private static void Handle(Action action, ConsoleColor foregroundColor)
    {
        Console.ForegroundColor = foregroundColor;
        action();
        Console.ResetColor();
    }
}