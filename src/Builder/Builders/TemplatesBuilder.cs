using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class TemplatesBuilder : BuilderBase
{
    private readonly ILogger<TemplatesBuilder> _logger;

    public TemplatesBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<TemplatesBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting templates");

        var apiClient = GetApiClient();

        await CreateTemplateAsync(apiClient, TemplateNames.Master, "master");
        await CreateTemplateAsync(apiClient, TemplateNames.Article, "article");
        await CreateTemplateAsync(apiClient, TemplateNames.ArticleList, "articleList");
        await CreateTemplateAsync(apiClient, TemplateNames.Author, "author");
        await CreateTemplateAsync(apiClient, TemplateNames.AuthorList, "authorList");
        await CreateTemplateAsync(apiClient, TemplateNames.Contact, "contact");
        await CreateTemplateAsync(apiClient, TemplateNames.Content, "content");
        await CreateTemplateAsync(apiClient, TemplateNames.Error, "error");
        await CreateTemplateAsync(apiClient, TemplateNames.Home, "home");
        await CreateTemplateAsync(apiClient, TemplateNames.Search, "search");
        await CreateTemplateAsync(apiClient, TemplateNames.XmlSitemap, "xMLSitemap");
    }

    private async Task CreateTemplateAsync(ApiClient apiClient, string name, string alias)
    {
        var file = new FileInfo($"Views\\{alias}.cshtml");
        if (file.Exists is false)
        {
            throw new ArgumentException($"Could not find the template view file on disk: {file.Name}");
        }

        var templateContent = await File.ReadAllTextAsync(file.FullName);

        // NOTE: the Management API will automatically assign the template structure based on the view file contents
        await apiClient.PostTemplateAsync(
            new()
            {
                Alias = alias,
                Name = name,
                Content = templateContent
            }
        );
    }
}