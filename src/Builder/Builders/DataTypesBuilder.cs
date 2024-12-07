using Builder.Models.DataType;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class DataTypesBuilder : BuilderBase
{
    private readonly ILogger<DataTypesBuilder> _logger;
    
    public DataTypesBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<DataTypesBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting data types");

        var apiClient = GetApiClient();

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.BlockListIconList,
                EditorAlias = "Umbraco.BlockList",
                EditorUiAlias = "Umb.PropertyEditorUi.BlockList"
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.BlockListMainContent,
                EditorAlias = "Umbraco.BlockList",
                EditorUiAlias = "Umb.PropertyEditorUi.BlockList"
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.DropdownSpacing,
                EditorAlias = "Umbraco.DropDown.Flexible",
                EditorUiAlias = "Umb.PropertyEditorUi.Dropdown",
                Values =
                [
                    new() { Alias = "multiple", Value = false },
                    new() { Alias = "items", Value = new [] { "Unset", "1", "2", "3", "4", "5" } }
                ]
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.MediaPickerSvg,
                EditorAlias = "Umbraco.MediaPicker3",
                EditorUiAlias = "Umb.PropertyEditorUi.MediaPicker",
                Values =
                [
                    new() { Alias = "multiple", Value = false },
                    new() { Alias = "filter", Value = await GetMediaTypeIdAsync(apiClient, MediaTypeNames.VectorGraphics) }
                ]
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.ContentPickerAuthors,
                EditorAlias = "Umbraco.MultiNodeTreePicker",
                EditorUiAlias = "Umb.PropertyEditorUi.ContentPicker"
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.ContentPickerCategories,
                EditorAlias = "Umbraco.MultiNodeTreePicker",
                EditorUiAlias = "Umb.PropertyEditorUi.ContentPicker"
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.UrlPickerSingle,
                EditorAlias = "Umbraco.MultiUrlPicker",
                EditorUiAlias = "Umb.PropertyEditorUi.MultiUrlPicker",
                Values =
                [
                    new() { Alias = "minNumber", Value = 0 },
                    new() { Alias = "maxNumber", Value = 1 }
                ]
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.SliderSpacing,
                EditorAlias = "Umbraco.Slider",
                EditorUiAlias = "Umb.PropertyEditorUi.Slider",
                Values =
                [
                    new() { Alias = "minVal", Value = -1 },
                    new() { Alias = "maxVal", Value = 0 },
                    new() { Alias = "initVal1", Value = -1 },
                    new() { Alias = "initVal2", Value = 5 },
                    new() { Alias = "step", Value = 1 }
                ]
            }
        );

        await apiClient.PostDataTypeAsync(
            new()
            {
                Name = DataTypeNames.ToggleDefaultTrue,
                EditorAlias = "Umbraco.TrueFalse",
                EditorUiAlias = "Umb.PropertyEditorUi.Toggle",
                Values =
                [
                    new() { Alias = "default", Value = true },
                    new() { Alias = "showLabels", Value = false }
                ]
            }
        );
    }

    public async Task UpdateDocumentTypesAsync()
    {
        _logger.LogInformation("Updating data types (configuration updates for document types)");

        var apiClient = GetApiClient();

        var contentElementTypes = await GetDocumentTypeIdsAsync(apiClient, "Elements", "Content Models");
        var settingsElementTypes = await GetDocumentTypeIdsAsync(apiClient, "Elements", "Setting Models");

        Guid ContentElementTypeId(string name)
            => contentElementTypes.TryGetValue(name, out Guid id)
                ? id
                : throw new InvalidOperationException($"Could not find content element type: {name}");

        Guid SettingsElementTypeId(string name)
            => settingsElementTypes.TryGetValue(name, out Guid id)
                ? id
                : throw new InvalidOperationException($"Could not find settings element type: {name}");

        var dataTypeId = await GetDataTypeIdAsync(apiClient, DataTypeNames.BlockListIconList);
        await apiClient.PutDataTypeByIdAsync(
            dataTypeId,
            new()
            {
                Name = DataTypeNames.BlockListIconList,
                EditorAlias = "Umbraco.BlockList",
                EditorUiAlias = "Umb.PropertyEditorUi.BlockList",
                Values =
                [
                    new()
                    {
                        Alias = "blocks",
                        Value = new BlockConfiguration []
                        {
                            new ()
                            {
                                ContentElementTypeKey = ContentElementTypeId(DocumentTypeNames.ContentElements.IconLinkRow),
                                SettingsElementTypeKey = SettingsElementTypeId(DocumentTypeNames.SettingsElements.IconLinkRow)
                            }
                        }
                    }
                ]
            }
        );

        dataTypeId = await GetDataTypeIdAsync(apiClient, DataTypeNames.BlockListMainContent);
        await apiClient.PutDataTypeByIdAsync(
            dataTypeId,
            new()
            {
                Name = DataTypeNames.BlockListMainContent,
                EditorAlias = "Umbraco.BlockList",
                EditorUiAlias = "Umb.PropertyEditorUi.BlockList",
                Values =
                [
                    new()
                    {
                        Alias = "blocks",
                        Value = new BlockConfiguration []
                        {
                            new ()
                            {
                                ContentElementTypeKey = ContentElementTypeId(DocumentTypeNames.ContentElements.RichTextRow),
                                SettingsElementTypeKey = SettingsElementTypeId(DocumentTypeNames.SettingsElements.RichTextRow)
                            },
                            new ()
                            {
                                ContentElementTypeKey = ContentElementTypeId(DocumentTypeNames.ContentElements.ImageRow),
                                SettingsElementTypeKey = SettingsElementTypeId(DocumentTypeNames.SettingsElements.ImageRow)
                            },
                            new ()
                            {
                                ContentElementTypeKey = ContentElementTypeId(DocumentTypeNames.ContentElements.VideoRow),
                                SettingsElementTypeKey = SettingsElementTypeId(DocumentTypeNames.SettingsElements.VideoRow)
                            },
                            new ()
                            {
                                ContentElementTypeKey = ContentElementTypeId(DocumentTypeNames.ContentElements.CodeSnippetRow),
                                SettingsElementTypeKey = SettingsElementTypeId(DocumentTypeNames.SettingsElements.CodeSnippetRow)
                            },
                            new ()
                            {
                                ContentElementTypeKey = ContentElementTypeId(DocumentTypeNames.ContentElements.ImageCarouselRow),
                                SettingsElementTypeKey = SettingsElementTypeId(DocumentTypeNames.SettingsElements.ImageCarouselRow)
                            },
                            new ()
                            {
                                ContentElementTypeKey = ContentElementTypeId(DocumentTypeNames.ContentElements.LatestArticlesRow),
                                SettingsElementTypeKey = SettingsElementTypeId(DocumentTypeNames.SettingsElements.LatestArticlesRow)
                            }
                        }
                    }
                ]
            }
        );
    }

    public async Task UpdateDocumentsAsync()
    {
        _logger.LogInformation("Updating data types (configuration updates for documents)");

        var apiClient = GetApiClient();
        var home = (await apiClient.GetTreeDocumentRootAsync(0, 1, null))?.Items.FirstOrDefault()
                   ?? throw new InvalidOperationException("Could not find the home document root.");

        var documentTypes = await GetDocumentTypeIdsAsync(apiClient, "Pages");
        if(documentTypes.TryGetValue(DocumentTypeNames.AuthorList, out Guid authorListDocumentTypeId) is false
            || documentTypes.TryGetValue(DocumentTypeNames.CategoryList, out Guid categoryListDocumentTypeId) is false)
        {
            throw new InvalidOperationException("Could not find the article list or category list document types.");
        }

        var contentPickerAuthorsId = await GetDataTypeIdAsync(apiClient, DataTypeNames.ContentPickerAuthors);
        var contentPickerCategoriesId = await GetDataTypeIdAsync(apiClient, DataTypeNames.ContentPickerCategories);

        await UpdateContentPickerConfigurationAsync(
            apiClient,
            contentPickerAuthorsId, 
            DataTypeNames.ContentPickerAuthors,
            1,
            home.Id,
            authorListDocumentTypeId);

        await UpdateContentPickerConfigurationAsync(
            apiClient,
            contentPickerCategoriesId, 
            DataTypeNames.ContentPickerCategories,
            0,
            home.Id,
            categoryListDocumentTypeId);
    }

    // both content pickers (MNTPs) have a similarly configured dynamic root:
    // - The origin is the Home document
    // - They have a single step, which is "NearestDescendantOrSelf" of a specific document type
    private async Task UpdateContentPickerConfigurationAsync(ApiClient apiClient, Guid contentPickerId, string name, int maxNumberOfItems, Guid homeId, Guid nearestDescendantDocumentTypeId)
        => await apiClient.PutDataTypeByIdAsync(
            contentPickerId,
            new()
            {
                Name = name,
                EditorAlias = "Umbraco.MultiNodeTreePicker",
                EditorUiAlias = "Umb.PropertyEditorUi.ContentPicker",
                Values =
                [
                    new()
                    {
                        Alias = "startNode",
                        Value = new ContentPickerConfiguration
                        {
                            DynamicRoot = new DynamicRoot
                            {
                                OriginAlias = "ByKey",
                                OriginKey = homeId,
                                QuerySteps =
                                [
                                    new()
                                    {
                                        Alias = "NearestDescendantOrSelf",
                                        AnyOfDocTypeKeys = [nearestDescendantDocumentTypeId]
                                    }
                                ]
                            }
                        }
                    },
                    new()
                    {
                        Alias = "minNumber",
                        Value = 0
                    },
                    new()
                    {
                        Alias = "maxNumber",
                        Value = maxNumberOfItems
                    }
                ]
            }
        );
}