using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

public class EmailTemplateService
{
    private readonly IWebHostEnvironment _env;

    public EmailTemplateService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(_env.WebRootPath, "EmailTemplates", $"{templateName}.html");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file {templateName} not found at {templatePath}");
        }

        return await File.ReadAllTextAsync(templatePath);
    }

    public string PopulateTemplate(string template, IDictionary<string, string> placeholders)
    {
        foreach (var placeholder in placeholders)
        {
            template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }

        return template;
    }
}
