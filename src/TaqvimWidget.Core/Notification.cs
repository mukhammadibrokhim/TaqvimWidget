namespace TaqvimWidget.Core;

public sealed class Notification
{
    public Notification(string title, string message)
    {
        Title = title;
        Message = message;
    }

    public string Title { get; }
    public string Message { get; }
}
