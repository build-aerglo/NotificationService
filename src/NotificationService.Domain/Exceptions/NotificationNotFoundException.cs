namespace NotificationService.Domain.Exceptions;

public class NotificationNotFoundException: Exception
{
    public NotificationNotFoundException()
        : base("Notification not found.") { }

    public NotificationNotFoundException(string message)
        : base(message) { }

    public NotificationNotFoundException(string message, Exception? innerException)
        : base(message, innerException) { }
}