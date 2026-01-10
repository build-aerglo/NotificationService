using NUnit.Framework;
using NotificationService.Infrastructure.Services;

namespace NotificationService.Infrastructure.Tests.Services;

[TestFixture]
public class TemplateEngineTests
{
    private string _testTemplatesPath = null!;
    private TemplateEngine _templateEngine = null!;

    [SetUp]
    public void Setup()
    {
        // Create a temporary directory for test templates
        _testTemplatesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testTemplatesPath);

        // Create email and sms subdirectories
        Directory.CreateDirectory(Path.Combine(_testTemplatesPath, "email"));
        Directory.CreateDirectory(Path.Combine(_testTemplatesPath, "sms"));

        _templateEngine = new TemplateEngine(_testTemplatesPath);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test templates
        if (Directory.Exists(_testTemplatesPath))
        {
            Directory.Delete(_testTemplatesPath, true);
        }
    }

    [Test]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TemplateEngine(null!));
    }

    [Test]
    public void Render_WithNullTemplate_ThrowsArgumentNullException()
    {
        var variables = new Dictionary<string, string>();
        Assert.Throws<ArgumentNullException>(() => _templateEngine.Render(null!, variables));
    }

    [Test]
    public void Render_WithNullVariables_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _templateEngine.Render("template", null!));
    }

    [Test]
    public void Render_WithSimpleVariable_ReplacesCorrectly()
    {
        var template = "Hello {{name}}!";
        var variables = new Dictionary<string, string>
        {
            ["name"] = "John"
        };

        var result = _templateEngine.Render(template, variables);

        Assert.That(result, Is.EqualTo("Hello John!"));
    }

    [Test]
    public void Render_WithMultipleVariables_ReplacesAllCorrectly()
    {
        var template = "Hello {{name}}, your code is {{code}}";
        var variables = new Dictionary<string, string>
        {
            ["name"] = "Jane",
            ["code"] = "123456"
        };

        var result = _templateEngine.Render(template, variables);

        Assert.That(result, Is.EqualTo("Hello Jane, your code is 123456"));
    }

    [Test]
    public void Render_WithCaseInsensitiveVariables_ReplacesCorrectly()
    {
        var template = "Code: {{CODE}}";
        var variables = new Dictionary<string, string>
        {
            ["code"] = "789"
        };

        var result = _templateEngine.Render(template, variables);

        Assert.That(result, Is.EqualTo("Code: 789"));
    }

    [Test]
    public async Task LoadTemplateAsync_WithNullTemplateName_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _templateEngine.LoadTemplateAsync(null!, "email"));
    }

    [Test]
    public async Task LoadTemplateAsync_WithNullTemplateType_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _templateEngine.LoadTemplateAsync("test", null!));
    }

    [Test]
    public async Task LoadTemplateAsync_WithNonExistentTemplate_ThrowsFileNotFoundException()
    {
        Assert.ThrowsAsync<FileNotFoundException>(
            async () => await _templateEngine.LoadTemplateAsync("nonexistent", "email"));
    }

    [Test]
    public async Task LoadTemplateAsync_WithEmailTemplate_LoadsCorrectly()
    {
        var templateContent = "<html>Test email template</html>";
        var templatePath = Path.Combine(_testTemplatesPath, "email", "test.html");
        await File.WriteAllTextAsync(templatePath, templateContent);

        var result = await _templateEngine.LoadTemplateAsync("test", "email");

        Assert.That(result, Is.EqualTo(templateContent));
    }

    [Test]
    public async Task LoadTemplateAsync_WithSmsTemplate_LoadsCorrectly()
    {
        var templateContent = "Test SMS template";
        var templatePath = Path.Combine(_testTemplatesPath, "sms", "test.txt");
        await File.WriteAllTextAsync(templatePath, templateContent);

        var result = await _templateEngine.LoadTemplateAsync("test", "sms");

        Assert.That(result, Is.EqualTo(templateContent));
    }

    [Test]
    public async Task RenderEmailTemplateAsync_WithLayoutAndContent_RendersCorrectly()
    {
        // Create layout template
        var layoutTemplate = "<html><body><header>Header</header>{{content}}<footer>Footer</footer></body></html>";
        var layoutPath = Path.Combine(_testTemplatesPath, "email", "layout.html");
        await File.WriteAllTextAsync(layoutPath, layoutTemplate);

        // Create content template
        var contentTemplate = "<p>Your code is {{code}}</p>";
        var contentPath = Path.Combine(_testTemplatesPath, "email", "test_email.html");
        await File.WriteAllTextAsync(contentPath, contentTemplate);

        var variables = new Dictionary<string, string>
        {
            ["code"] = "123456"
        };

        var result = await _templateEngine.RenderEmailTemplateAsync("test_email", variables);

        Assert.That(result, Does.Contain("<header>Header</header>"));
        Assert.That(result, Does.Contain("<p>Your code is 123456</p>"));
        Assert.That(result, Does.Contain("<footer>Footer</footer>"));
    }

    [Test]
    public async Task RenderSmsTemplateAsync_RendersCorrectly()
    {
        var templateContent = "Your verification code is {{code}}";
        var templatePath = Path.Combine(_testTemplatesPath, "sms", "test_sms.txt");
        await File.WriteAllTextAsync(templatePath, templateContent);

        var variables = new Dictionary<string, string>
        {
            ["code"] = "789012"
        };

        var result = await _templateEngine.RenderSmsTemplateAsync("test_sms", variables);

        Assert.That(result, Is.EqualTo("Your verification code is 789012"));
    }
}
