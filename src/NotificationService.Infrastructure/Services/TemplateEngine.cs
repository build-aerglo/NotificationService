using NotificationService.Application.Interfaces;
using System.Text.RegularExpressions;

namespace NotificationService.Infrastructure.Services;

public class TemplateEngine : ITemplateEngine
{
    private readonly string _templatesBasePath;
    private const string EmailLayoutTemplate = "layout";

    public TemplateEngine(string templatesBasePath)
    {
        _templatesBasePath = templatesBasePath ?? throw new ArgumentNullException(nameof(templatesBasePath));
    }

    public string Render(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template))
            throw new ArgumentNullException(nameof(template));

        if (variables == null)
            throw new ArgumentNullException(nameof(variables));

        var result = template;

        // Replace all {{variable}} placeholders with their values
        foreach (var variable in variables)
        {
            var pattern = $@"{{{{{variable.Key}}}}}";
            result = Regex.Replace(result, pattern, variable.Value, RegexOptions.IgnoreCase);
        }

        return result;
    }

    public async Task<string> LoadTemplateAsync(string templateName, string templateType)
    {
        if (string.IsNullOrEmpty(templateName))
            throw new ArgumentNullException(nameof(templateName));

        if (string.IsNullOrEmpty(templateType))
            throw new ArgumentNullException(nameof(templateType));

        var extension = templateType.ToLower() == "email" ? ".html" : ".txt";
        var templatePath = Path.Combine(_templatesBasePath, templateType, $"{templateName}{extension}");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template not found: {templatePath}");

        return await File.ReadAllTextAsync(templatePath);
    }

    public async Task<string> RenderEmailTemplateAsync(string templateName, Dictionary<string, string> variables)
    {
        // Load the email layout (header/footer)
        var layoutTemplate = await LoadTemplateAsync(EmailLayoutTemplate, "email");

        // Load the specific email template
        var contentTemplate = await LoadTemplateAsync(templateName, "email");

        // Render the content template with variables
        var renderedContent = Render(contentTemplate, variables);

        // Inject the rendered content into the layout
        var layoutVariables = new Dictionary<string, string>(variables)
        {
            ["content"] = renderedContent
        };

        // Render the final email with layout
        return Render(layoutTemplate, layoutVariables);
    }

    public async Task<string> RenderSmsTemplateAsync(string templateName, Dictionary<string, string> variables)
    {
        // Load the SMS template
        var template = await LoadTemplateAsync(templateName, "sms");

        // Render the template with variables
        return Render(template, variables);
    }
}
