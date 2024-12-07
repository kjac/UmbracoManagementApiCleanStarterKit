using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class DictionaryItemsBuilder : BuilderBase
{
    private readonly ILogger<DictionaryItemsBuilder> _logger;

    public DictionaryItemsBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<DictionaryItemsBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting dictionary items");

        var apiClient = GetApiClient();

        await AddDictionaryItemAsync(apiClient, "Article.By", "by");
        await AddDictionaryItemAsync(apiClient, "Article.On", "on");
        await AddDictionaryItemAsync(apiClient, "Article.Posted", "Posted");
        await AddDictionaryItemAsync(apiClient, "ArticleList.ViewAll", "View all posts");
        await AddDictionaryItemAsync(apiClient, "Author.ReadMore", "Read More");
        await AddDictionaryItemAsync(apiClient, "ContactForm.Email", "Email Address");
        await AddDictionaryItemAsync(apiClient, "ContactForm.Message", "Message");
        await AddDictionaryItemAsync(apiClient, "ContactForm.Name", "Name");
        await AddDictionaryItemAsync(apiClient, "ContactForm.Send", "Send");
        await AddDictionaryItemAsync(apiClient, "Footer.CopyrightStatement", "Clean Starter Kit");
        await AddDictionaryItemAsync(apiClient, "Footer.CopyrightTitle", "Copyright");
        await AddDictionaryItemAsync(apiClient, "Navigation.MenuTitle", "Menu");
        await AddDictionaryItemAsync(apiClient, "Navigation.SiteName", "Clean Starter Kit");
        await AddDictionaryItemAsync(apiClient, "Paging.Next", "Next");
        await AddDictionaryItemAsync(apiClient, "Paging.Of", "of");
        await AddDictionaryItemAsync(apiClient, "Paging.Page", "Page");
        await AddDictionaryItemAsync(apiClient, "Paging.Previous", "Prev");
        await AddDictionaryItemAsync(apiClient, "Search.Placeholder", "Search...");
        await AddDictionaryItemAsync(apiClient, "Search.Results", "<p>We found <strong>{0}</strong> results when searching for <strong>{1}</strong></p>");
        await AddDictionaryItemAsync(apiClient, "Search.SearchButton", "Search");
    }

    private async Task AddDictionaryItemAsync(ApiClient apiClient, string name, string value)
        => await apiClient.PostDictionaryAsync(
            new ()
            {
                Name = name,
                Translations =
                [
                    new ()
                    {
                        Translation = value,
                        IsoCode = "en-US"
                    }
                ]
            }
        );
}