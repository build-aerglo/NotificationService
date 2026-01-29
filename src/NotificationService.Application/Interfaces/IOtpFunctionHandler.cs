namespace NotificationService.Application.Interfaces;

public interface IOtpFunctionHandler
{
    Task<bool> ExecuteAsync(string functionName, string id);
    bool IsValidFunction(string functionName);
}
