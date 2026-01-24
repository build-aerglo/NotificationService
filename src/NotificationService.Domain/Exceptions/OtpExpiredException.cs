namespace NotificationService.Domain.Exceptions;

public class OtpExpiredException : Exception
{
    public OtpExpiredException()
        : base("OTP has expired.") { }

    public OtpExpiredException(string message)
        : base(message) { }

    public OtpExpiredException(string message, Exception? innerException)
        : base(message, innerException) { }
}
