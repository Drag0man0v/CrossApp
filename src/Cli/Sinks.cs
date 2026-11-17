using Core.Notifications;

sealed class ConsoleSink : INotificationSink
{
    public void Notify(string message) => Console.WriteLine("[sink:console] " + message);
}

sealed class FileSink(string path) : INotificationSink
{
    public void Notify(string message) =>
        File.AppendAllText(path, message + Environment.NewLine);
}