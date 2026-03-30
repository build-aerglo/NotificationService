namespace NotificationService.Application.Configuration;

/// <summary>
/// Templates listed here are pushed directly to Azure Queue without being saved to the notifications database.
/// </summary>
public static class SkipDbSaveTemplates
{
    public static readonly HashSet<string> Templates =
    [
        "otp"
    ];
}
