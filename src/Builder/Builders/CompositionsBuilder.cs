using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class CompositionsBuilder : DocumentTypeBuilderBase
{
    private readonly ILogger<CompositionsBuilder> _logger;

    public CompositionsBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<CompositionsBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting compositions");

        var apiClient = GetApiClient();

        _logger.LogInformation("- adding folders");

        var compositionsFolderId = await CreateFolderAsync(apiClient, "Compositions", null);
        var contentBlocksFolderId = await CreateFolderAsync(apiClient, "Content Blocks", compositionsFolderId);
        var settingsModelsFolderId = await CreateFolderAsync(apiClient, "Setting Models", contentBlocksFolderId);

        _logger.LogInformation("- adding element type settings compositions");

        await CreateHidePropertyCompositionAsync(apiClient, settingsModelsFolderId);
        await CreateSpacingPropertiesCompositionAsync(apiClient, settingsModelsFolderId);

        _logger.LogInformation("- adding document type compositions");

        await CreateArticleControlsCompositionAsync(apiClient, compositionsFolderId);
        await CreateContactFormControlsCompositionAsync(apiClient, compositionsFolderId);
        await CreateContentControlsCompositionAsync(apiClient, compositionsFolderId);
        await CreateFooterControlsCompositionAsync(apiClient, compositionsFolderId);
        await CreateHeaderControlsCompositionAsync(apiClient, compositionsFolderId);
        await CreateMainImageControlsCompositionAsync(apiClient, compositionsFolderId);
        await CreateSeoControlsCompositionAsync(apiClient, compositionsFolderId);
        await CreateVisibilityControlsCompositionAsync(apiClient, compositionsFolderId);
    }

    private async Task CreateHidePropertyCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.HideProperty,
                Alias = "hideProperty",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-defrag color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Settings",
                        Type = "Tab",
                        SortOrder = 100
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Hide",
                        Alias = "hide",
                        Description = "Set this to true if you want to hide this row from the front end of the site",
                        SortOrder = 10,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.Toggle) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );       
    }

    private async Task CreateSpacingPropertiesCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.SpacingProperties,
                Alias = "spacingProperties",
                Parent = new() { Id = folderId },
                IsElement = true,
                Icon = "icon-defrag color-blue",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Padding",
                        Type = "Tab",
                        SortOrder = 120
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Padding Top",
                        Alias = "paddingTop",
                        SortOrder = 5,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Padding Bottom",
                        Alias = "paddingBottom",
                        SortOrder = 10,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Padding Left",
                        Alias = "paddingLeft",
                        SortOrder = 15,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Padding Right",
                        Alias = "paddingRight",
                        SortOrder = 20,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Margin Top",
                        Alias = "marginTop",
                        SortOrder = 25,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Margin Bottom",
                        Alias = "marginBottom",
                        SortOrder = 30,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Margin Left",
                        Alias = "marginLeft",
                        SortOrder = 35,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Margin Right",
                        Alias = "marginRight",
                        SortOrder = 40,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DropdownSpacing) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );       
    }

    private async Task CreateArticleControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.ArticleControls,
                Alias = "articleControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 10
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Article Date",
                        Alias = "articleDate",
                        Description = "Enter the date for the article",
                        SortOrder = 20,
                        Validation = new() { Mandatory = true },
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.DatePickerWithTime) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Author",
                        Alias = "author",
                        SortOrder = 25,
                        Validation = new() { Mandatory = true },
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.ContentPickerAuthors) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Categories",
                        Alias = "categories",
                        SortOrder = 30,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.ContentPickerCategories) },
                        Container = new() { Id = containerId }
                    },
                ]
            }
        );        
    }

    private async Task CreateContactFormControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        var containerId2 = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.ContactFormControls,
                Alias = "contactFormControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 10
                    },
                    new()
                    {
                        Id = containerId2,
                        Name = "Result Messages",
                        Type = "Tab",
                        SortOrder = 20
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Instruction Message",
                        Alias = "instructionMessage",
                        Description = "Enter the message to tell the user what to do",
                        SortOrder = 20,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.RichTextEditor) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Success Message",
                        Alias = "successMessage",
                        Description = "Enter the message to show on success",
                        SortOrder = 5,
                        Validation = new() { Mandatory = true },
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.RichTextEditor) },
                        Container = new() { Id = containerId2 }
                    },
                    new()
                    {
                        Name = "Error Message",
                        Alias = "errorMessage",
                        Description = "Enter the message to show on error",
                        SortOrder = 10,
                        Validation = new() { Mandatory = true },
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.RichTextEditor) },
                        Container = new() { Id = containerId2 }
                    },
                ]
            }
        );       
    }

    private async Task CreateContentControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.ContentControls,
                Alias = "contentControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 10
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Content Rows",
                        Alias = "contentRows",
                        Description = "Add the rows of content for the page",
                        SortOrder = 16,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.BlockListMainContent) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );       
    }

    private async Task CreateFooterControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.FooterControls,
                Alias = "footerControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Footer",
                        Type = "Tab",
                        SortOrder = 20
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Social Icon Links",
                        Alias = "socialIconLinks",
                        Description = "Add any social links using the SVG icons",
                        SortOrder = 5,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.BlockListIconList) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );
        
    }

    private async Task CreateHeaderControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.HeaderControls,
                Alias = "headerControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 10
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Title",
                        Alias = "title",
                        Description =
                            "Enter the title for the page. If this is empty the name of the page will be used.",
                        SortOrder = 5,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextString) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Subtitle",
                        Alias = "subtitle",
                        Description = "Enter a subtitle for this page",
                        SortOrder = 10,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextString) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );        
    }

    private async Task CreateMainImageControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.MainImageControls,
                Alias = "mainImageControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Content",
                        Type = "Tab",
                        SortOrder = 10
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Main Image",
                        Alias = "mainImage",
                        Description = "Choose the main image for this page",
                        SortOrder = 15,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.MediaPickerImage) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );        
    }

    private async Task CreateSeoControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.SeoControls,
                Alias = "sEOControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "SEO",
                        Type = "Tab",
                        SortOrder = 25
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Meta Name",
                        Alias = "metaName",
                        Description = "Enter the meta name for this page",
                        SortOrder = 5,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextString) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Meta Description",
                        Alias = "metaDescription",
                        Description = "Enter the meta description for this page",
                        SortOrder = 10,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.TextArea) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Meta Keywords",
                        Alias = "metaKeywords",
                        Description = "Enter the keywords for this page",
                        SortOrder = 15,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.Tags) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Is Indexable",
                        Alias = "isIndexable",
                        Description = "Set this to true if you want this page to be indexable by robots",
                        SortOrder = 20,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.ToggleDefaultTrue) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Is Followable",
                        Alias = "isFollowable",
                        Description = "Set this to true if you want the page to be followable by robots",
                        SortOrder = 25,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.ToggleDefaultTrue) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );        
    }

    private async Task CreateVisibilityControlsCompositionAsync(ApiClient apiClient, Guid folderId)
    {
        var containerId = Guid.NewGuid();
        await apiClient.PostDocumentTypeAsync(
            new()
            {
                Name = DocumentTypeNames.Compositions.VisibilityControls,
                Alias = "visibilityControls",
                Parent = new() { Id = folderId },
                Icon = "icon-settings color-red",
                Containers =
                [
                    new()
                    {
                        Id = containerId,
                        Name = "Visibility",
                        Type = "Tab",
                        SortOrder = 30
                    }
                ],
                Properties =
                [
                    new()
                    {
                        Name = "Hide From Top Navigation",
                        Alias = "hideFromTopNavigation",
                        SortOrder = 5,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.Toggle) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Hide From Search",
                        Alias = "umbracoNaviHide",
                        Description =
                            "Tick this box if you want to hide this page from the navigation and from search results",
                        SortOrder = 10,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.Toggle) },
                        Container = new() { Id = containerId }
                    },
                    new()
                    {
                        Name = "Hide From XML Sitemap",
                        Alias = "hideFromXMLSitemap",
                        Description = "Tick this if you want to hide this page from the XML sitemap",
                        SortOrder = 15,
                        DataType = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.Toggle) },
                        Container = new() { Id = containerId }
                    }
                ]
            }
        );        
    }
}