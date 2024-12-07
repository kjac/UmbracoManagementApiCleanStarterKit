using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class ElementTypesBuilder : DocumentTypeBuilderBase
{
    private readonly ILogger<ElementTypesBuilder> _logger;

    private Dictionary<string, Guid>? _settingsModelsCompositions;

    public ElementTypesBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<ElementTypesBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting element types");

        var apiClient = GetApiClient();

        _logger.LogInformation("- adding folders");

        var elementsFolderId = await CreateFolderAsync(apiClient, "Elements", null);
        var contentModelsFolderId = await CreateFolderAsync(apiClient, "Content Models", elementsFolderId);
        var settingsModelsFolderId = await CreateFolderAsync(apiClient, "Setting Models", elementsFolderId);

        _logger.LogInformation("- adding content element types");

        await CreateCodeSnippetRowElementAsync(apiClient, contentModelsFolderId);
        await CreateIconLinkRowElementAsync(apiClient, contentModelsFolderId);
        await CreateImageCarouselRowElementAsync(apiClient, contentModelsFolderId);
        await CreateImageRowElementAsync(apiClient, contentModelsFolderId);
        await CreateLatestArticlesRowElementAsync(apiClient, contentModelsFolderId);
        await CreateRichTextRowElementAsync(apiClient, contentModelsFolderId);
        await CreateVideoRowElementAsync(apiClient, contentModelsFolderId);

        _logger.LogInformation("- adding settings element types");

        await CreateCodeSnippetRowSettingsElementAsync(apiClient, settingsModelsFolderId);
        await CreateIconLinkRowSettingsElementAsync(apiClient, settingsModelsFolderId);
        await CreateImageCarouselRowSettingsElementAsync(apiClient, settingsModelsFolderId);
        await CreateImageRowSettingsElementAsync(apiClient, settingsModelsFolderId);
        await CreateLatestArticlesRowSettingsElementAsync(apiClient, settingsModelsFolderId);
        await CreateRichTextRowSettingsElementAsync(apiClient, settingsModelsFolderId);
        await CreateVideoRowSettingsElementAsync(apiClient, settingsModelsFolderId);
    }

    private async Task CreateCodeSnippetRowElementAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.ContentElements.CodeSnippetRow,
                Alias = "codeSnippetRow",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-code color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 0
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Title",
                        Alias = "title",
                        Description = "Enter a name for this code snippet",
                        SortOrder = 1,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextString) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Code",
                        Alias = "code",
                        SortOrder = 1,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextArea) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
    }

    private async Task CreateIconLinkRowElementAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.ContentElements.IconLinkRow,
                Alias = "iconLinkRow",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-link color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 0
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Icon",
                        Alias = "icon",
                        Description = "Choose the icon for this item. It must be an SVG",
                        SortOrder = 10,
                        Validation = new () { Mandatory = true, MandatoryMessage = "You must choose an icon"},
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.MediaPickerSvg) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Link",
                        Alias = "link",
                        Description = "Enter your link for this item",
                        SortOrder = 20,
                        Validation = new () { Mandatory = true, MandatoryMessage = "You must add a link"},
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.UrlPickerSingle) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
    }

    private async Task CreateImageCarouselRowElementAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.ContentElements.ImageCarouselRow,
                Alias = "imageCarouselRow",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-files color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 0
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Images",
                        Alias = "images",
                        Description = "Choose the images for the carousel row",
                        SortOrder = 1,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.MediaPickerMultipleImage) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
    }

    private async Task CreateImageRowElementAsync( ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.ContentElements.ImageRow,
                Alias = "imageRow",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-picture color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 0
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Image",
                        Alias = "image",
                        Description = "Add the image for this row",
                        SortOrder = 10,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.MediaPickerImage) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Caption",
                        Alias = "caption",
                        Description = "Enter a caption for the image",
                        SortOrder = 20,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextString) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
    }

    private async Task CreateLatestArticlesRowElementAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.ContentElements.LatestArticlesRow,
                Alias = "latestArticlesRow",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-bulleted-list color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 0
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Article List",
                        Alias = "articleList",
                        Description = "Choose the parent page where you want to display articles from",
                        SortOrder = 5,
                        Validation = new () { Mandatory = true, MandatoryMessage = "You need to choose an article list page"},
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DocumentPicker) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Page Size",
                        Alias = "pageSize",
                        Description = "Choose the amount of articles to display per page",
                        SortOrder = 10,
                        Validation = new () { Mandatory = true, MandatoryMessage = "You need to enter the page size"},
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.Numeric) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Show Pagination",
                        Alias = "showPagination",
                        Description = "Set this to true if you would like to show the pagination for these articles",
                        SortOrder = 15,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.Toggle) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
    }

    private async Task CreateRichTextRowElementAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.ContentElements.RichTextRow,
                Alias = "richTextRow",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-notepad color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 0
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Content",
                        Alias = "content",
                        Description = "Enter the content for this rich text item",
                        SortOrder = 10,
                        Appearance = new () { LabelOnTop = true },
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.RichTextEditor) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
    }

    private async Task CreateVideoRowElementAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.ContentElements.VideoRow,
                Alias = "videoRow",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-video color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 0
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Video Url",
                        Alias = "videoUrl",
                        Description = "Add the YouTube Url in here",
                        SortOrder = 10,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextString) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Caption",
                        Alias = "caption",
                        Description = "Add a caption to display under the video if you would like",
                        SortOrder = 20,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextString) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
    }

    private async Task CreateCodeSnippetRowSettingsElementAsync(ApiClient apiClient, Guid folderId)
        => await CreateSettingsElementAsync(apiClient, folderId, DocumentTypeNames.SettingsElements.CodeSnippetRow, "codeSnippetRowSettings",
            DocumentTypeNames.Compositions.HideProperty, DocumentTypeNames.Compositions.SpacingProperties);

    private async Task CreateIconLinkRowSettingsElementAsync(ApiClient apiClient, Guid folderId)
        => await CreateSettingsElementAsync(apiClient, folderId, DocumentTypeNames.SettingsElements.IconLinkRow, "iconLinkRowSettings", DocumentTypeNames.Compositions.HideProperty);

    private async Task CreateImageCarouselRowSettingsElementAsync(ApiClient apiClient, Guid folderId)
        => await CreateSettingsElementAsync(apiClient, folderId, DocumentTypeNames.SettingsElements.ImageCarouselRow, "imageCarouselRowSettings",
            DocumentTypeNames.Compositions.HideProperty, DocumentTypeNames.Compositions.SpacingProperties);

    private async Task CreateImageRowSettingsElementAsync(ApiClient apiClient, Guid folderId)
        => await CreateSettingsElementAsync(apiClient, folderId, DocumentTypeNames.SettingsElements.ImageRow, "imageRowSettings",
            DocumentTypeNames.Compositions.HideProperty, DocumentTypeNames.Compositions.SpacingProperties);

    private async Task CreateLatestArticlesRowSettingsElementAsync(ApiClient apiClient, Guid folderId)
        => await CreateSettingsElementAsync(apiClient, folderId, DocumentTypeNames.SettingsElements.LatestArticlesRow, "latestArticlesRowSettings",
            DocumentTypeNames.Compositions.HideProperty, DocumentTypeNames.Compositions.SpacingProperties);

    private async Task CreateRichTextRowSettingsElementAsync(ApiClient apiClient, Guid folderId)
        => await CreateSettingsElementAsync(apiClient, folderId, DocumentTypeNames.SettingsElements.RichTextRow, "richTextRowSettings",
            DocumentTypeNames.Compositions.HideProperty, DocumentTypeNames.Compositions.SpacingProperties);

    private async Task CreateVideoRowSettingsElementAsync(ApiClient apiClient, Guid folderId)
        => await CreateSettingsElementAsync(apiClient, folderId, DocumentTypeNames.SettingsElements.VideoRow, "videoRowSettings",
            DocumentTypeNames.Compositions.HideProperty, DocumentTypeNames.Compositions.SpacingProperties);

    private async Task CreateSettingsElementAsync(ApiClient apiClient, Guid folderId, string name, string alias, params string[] settingsModelCompositions)
    {
        var compositionIds = new List<Guid>();
        foreach (var settingsModelComposition in settingsModelCompositions)
        {
            compositionIds.Add(await GetSettingsModelsCompositionIdAsync(apiClient, settingsModelComposition));
        }

        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = name,
                Alias = alias,
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-settings color-light-blue",
                Compositions = compositionIds.Select(id => new DocumentTypeCompositionModel
                {
                    CompositionType = CompositionTypeModel.Composition,
                    DocumentType = new() { Id = id }
                }).ToArray()
            }
        );
    }

    private async Task<Guid> GetSettingsModelsCompositionIdAsync(ApiClient apiClient, string name)
    {
        if (_settingsModelsCompositions is null)
        {
            _settingsModelsCompositions = await GetDocumentTypeIdsAsync(apiClient, "Compositions", "Content Blocks", "Setting Models");
            if (_settingsModelsCompositions is null)
            {
                throw new InvalidOperationException("Could not fetch settings models compositions");
            }
        }

        if (_settingsModelsCompositions.TryGetValue(name, out var itemId) is false)
        {
            throw new InvalidOperationException($"The settings models composition did not exist: {name}");
        }

        return itemId;
    }
}