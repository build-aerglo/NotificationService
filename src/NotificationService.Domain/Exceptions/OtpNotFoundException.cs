namespace NotificationService.Domain.Exceptions;

public class OtpNotFoundException : Exception
{
    public OtpNotFoundException()
        : base("OTP not found.") { }

    public OtpNotFoundException(string message)
        : base(message) { }

    public OtpNotFoundException(string message, Exception? innerException)
        : base(message, innerException) { }
}
