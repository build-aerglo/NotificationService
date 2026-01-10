namespace NotificationService.Application.Interfaces;

public interface ITemplateEngine
{
    /// <summary>
    /// Renders a template by replacing variables with their corresponding values
    /// </summary>
    /// <param name="template">The template content with {{variable}} placeholders</param>
    /// <param name="variables">Dictionary of variable names and their values</param>
    /// <returns>The rendered template with variables replaced</returns>
    string Render(string template, Dictionary<string, string> variables);

    /// <summary>
    /// Loads a template from a file
    /// </summary>
    /// <param name="templateName">The name of the template file (without extension)</param>
    /// <param name="templateType">The type of template (email or sms)</param>
    /// <returns>The template content</returns>
    Task<string> LoadTemplateAsync(string templateName, string templateType);

    /// <summary>
    /// Renders an email template with layout (header/footer wrapper)
    /// </summary>
    /// <param name="templateName">The name of the email template</param>
    /// <param name="variables">Dictionary of variable names and their values</param>
    /// <returns>The fully rendered email HTML with layout</returns>
    Task<string> RenderEmailTemplateAsync(string templateName, Dictionary<string, string> variables);

    /// <summary>
    /// Renders an SMS template
    /// </summary>
    /// <param name="templateName">The name of the SMS template</param>
    /// <param name="variables">Dictionary of variable names and their values</param>
    /// <returns>The rendered SMS content</returns>
    Task<string> RenderSmsTemplateAsync(string templateName, Dictionary<string, string> variables);
}
