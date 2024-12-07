using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class DocumentTypesBuilder : DocumentTypeBuilderBase
{
    private readonly ILogger<DocumentTypesBuilder> _logger;

    private Dictionary<string, Guid>? _compositions;
    private Dictionary<string, Guid>? _templates;

    public DocumentTypesBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<DocumentTypesBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting document types");

        var apiClient = GetApiClient();

        _compositions = await GetDocumentTypeIdsAsync(apiClient, "Compositions");
        _templates = await GetTemplateIdsAsync(apiClient);

        _logger.LogInformation("- adding folders");

        var pagesFolderId = await CreateFolderAsync(apiClient, "Pages", null);
        
        _logger.LogInformation("- adding document types");
        
        var allowedHomeDocumentTypeIds = new List<Guid>();

        var documentTypeId = await CreateArticleDocumentTypeAsync(apiClient, pagesFolderId);
        allowedHomeDocumentTypeIds.Add(await CreateArticleListDocumentTypeAsync(apiClient, pagesFolderId, documentTypeId));

        documentTypeId = await CreateAuthorDocumentTypeAsync(apiClient, pagesFolderId);
        allowedHomeDocumentTypeIds.Add(await CreateAuthorListDocumentTypeAsync(apiClient, pagesFolderId, documentTypeId));
        
        documentTypeId = await CreateCategoryDocumentTypeAsync(apiClient, pagesFolderId);
        allowedHomeDocumentTypeIds.Add(await CreateCategoryListDocumentTypeAsync(apiClient, pagesFolderId, documentTypeId));
        
        allowedHomeDocumentTypeIds.Add(await CreateContactDocumentTypeAsync(apiClient, pagesFolderId));
        allowedHomeDocumentTypeIds.Add(await CreateContentDocumentTypeAsync(apiClient, pagesFolderId));
        allowedHomeDocumentTypeIds.Add(await CreateErrorDocumentTypeAsync(apiClient, pagesFolderId));
        allowedHomeDocumentTypeIds.Add(await CreateSearchDocumentTypeAsync(apiClient, pagesFolderId));
        allowedHomeDocumentTypeIds.Add(await CreateXmlSitemapDocumentTypeAsync(apiClient, pagesFolderId));

        await CreateHomeDocumentTypeAsync(apiClient, pagesFolderId, allowedHomeDocumentTypeIds);
    }
    
    private async Task<Guid> CreateArticleDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var templateId = TemplateId(TemplateNames.Article);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Article, "article", "icon-document-font", templateId,
            DocumentTypeNames.Compositions.ArticleControls, DocumentTypeNames.Compositions.ContentControls, DocumentTypeNames.Compositions.HeaderControls,
            DocumentTypeNames.Compositions.MainImageControls, DocumentTypeNames.Compositions.SeoControls, DocumentTypeNames.Compositions.VisibilityControls);
        
        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }

    private async Task<Guid> CreateArticleListDocumentTypeAsync(ApiClient apiClient, Guid folderId, Guid articleDocumentTypeId)
    {
        var templateId = TemplateId(TemplateNames.ArticleList);
        return await CreateListDocumentTypeAsync(apiClient, folderId, DocumentTypeNames.ArticleList, "articleList", "icon-thumbnail-list", articleDocumentTypeId, templateId);
    }

    private async Task<Guid> CreateAuthorDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var templateId = TemplateId(TemplateNames.Author);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Author, "author", "icon-user", templateId,
            DocumentTypeNames.Compositions.ContentControls, DocumentTypeNames.Compositions.HeaderControls, DocumentTypeNames.Compositions.MainImageControls,
            DocumentTypeNames.Compositions.SeoControls, DocumentTypeNames.Compositions.VisibilityControls);

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }

    private async Task<Guid> CreateAuthorListDocumentTypeAsync(ApiClient apiClient, Guid folderId, Guid authorDocumentTypeId)
    {
        var templateId = TemplateId(TemplateNames.AuthorList);
        return await CreateListDocumentTypeAsync(apiClient, folderId, DocumentTypeNames.AuthorList, "authorList", "icon-users", authorDocumentTypeId, templateId);
    }

    private async Task<Guid> CreateCategoryDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Category, "category", "icon-tag", null);

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }

    private async Task<Guid> CreateCategoryListDocumentTypeAsync(ApiClient apiClient, Guid folderId, Guid categoryDocumentTypeId)
        => await CreateListDocumentTypeAsync(apiClient, folderId, DocumentTypeNames.CategoryList, "categoryList", "icon-tags", categoryDocumentTypeId, null, DocumentTypeNames.Compositions.VisibilityControls);
    
    private async Task<Guid> CreateContactDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var templateId = TemplateId(TemplateNames.Contact);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Contact, "contact", "icon-mailbox", templateId,
            DocumentTypeNames.Compositions.ContactFormControls, DocumentTypeNames.Compositions.HeaderControls, DocumentTypeNames.Compositions.MainImageControls,
            DocumentTypeNames.Compositions.SeoControls, DocumentTypeNames.Compositions.VisibilityControls);

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }
   
    private async Task<Guid> CreateContentDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var templateId = TemplateId(TemplateNames.Content);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Content, "content", "icon-document", templateId,
            DocumentTypeNames.Compositions.ContentControls, DocumentTypeNames.Compositions.HeaderControls, DocumentTypeNames.Compositions.MainImageControls,
            DocumentTypeNames.Compositions.SeoControls, DocumentTypeNames.Compositions.VisibilityControls);

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }

    private async Task<Guid> CreateErrorDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var templateId = TemplateId(TemplateNames.Error);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Error, "error", "icon-application-error", templateId,
            DocumentTypeNames.Compositions.ContentControls, DocumentTypeNames.Compositions.HeaderControls, DocumentTypeNames.Compositions.MainImageControls,
            DocumentTypeNames.Compositions.SeoControls, DocumentTypeNames.Compositions.VisibilityControls);

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }

    private async Task<Guid> CreateSearchDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var templateId = TemplateId(TemplateNames.Search);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Search, "search", "icon-search", templateId,
            DocumentTypeNames.Compositions.HeaderControls, DocumentTypeNames.Compositions.MainImageControls,
            DocumentTypeNames.Compositions.SeoControls, DocumentTypeNames.Compositions.VisibilityControls);

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }

    private async Task<Guid> CreateXmlSitemapDocumentTypeAsync(ApiClient apiClient, Guid folderId)
    {
        var templateId = TemplateId(TemplateNames.XmlSitemap);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.XmlSitemap, "xMLSitemap", "icon-map", templateId, DocumentTypeNames.Compositions.VisibilityControls);

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }

    private async Task CreateHomeDocumentTypeAsync(ApiClient apiClient, Guid folderId, IEnumerable<Guid> allowedChildDocumentTypeIds)
    {
        var templateId = TemplateId(TemplateNames.Home);
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, DocumentTypeNames.Home, "home", "icon-home", templateId,
            DocumentTypeNames.Compositions.ContentControls, DocumentTypeNames.Compositions.FooterControls, DocumentTypeNames.Compositions.HeaderControls,
            DocumentTypeNames.Compositions.MainImageControls, DocumentTypeNames.Compositions.SeoControls);

        var sortOrder = 1;
        createRequestModel.AllowedDocumentTypes = allowedChildDocumentTypeIds.Select(id => new DocumentTypeSortModel
        {
            DocumentType = new() { Id = id },
            SortOrder = sortOrder++
        }).ToArray();

        createRequestModel.AllowedAsRoot = true;

        await apiClient.PostDocumentTypeAsync(createRequestModel);
    }

    private async Task<Guid> CreateListDocumentTypeAsync(ApiClient apiClient, Guid folderId, string name, string alias, string icon, Guid allowedChildDocumentTypeId, Guid? templateId)
        => await CreateListDocumentTypeAsync(apiClient, folderId, name, alias, icon, allowedChildDocumentTypeId, templateId,
            DocumentTypeNames.Compositions.ContentControls, DocumentTypeNames.Compositions.HeaderControls, DocumentTypeNames.Compositions.MainImageControls,
            DocumentTypeNames.Compositions.SeoControls, DocumentTypeNames.Compositions.VisibilityControls);

    private async Task<Guid> CreateListDocumentTypeAsync(ApiClient apiClient, Guid folderId, string name, string alias, string icon, Guid allowedChildDocumentTypeId, Guid? templateId, params string[] compositions)
    {
        var createRequestModel = CreateDocumentTypeRequestModel(folderId, name, alias, icon, templateId, compositions);
        createRequestModel.AllowedDocumentTypes =
        [
            new()
            {
                DocumentType = new() { Id = allowedChildDocumentTypeId },
                SortOrder = 1
            }
        ];
        createRequestModel.Collection = new() { Id = await GetDataTypeIdAsync(apiClient, DataTypeNames.ListViewContent) };

        await apiClient.PostDocumentTypeAsync(createRequestModel);
        return createRequestModel.Id!.Value;
    }
    private CreateDocumentTypeRequestModel CreateDocumentTypeRequestModel(Guid folderId, string name, string alias, string icon, Guid? templateId, params string[] compositions)
    {
        var requestModel = new CreateDocumentTypeRequestModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Alias = alias,
            Parent = new() { Id = folderId },
            Icon = $"{icon} color-blue",
            Compositions = compositions.Select(composition => new DocumentTypeCompositionModel
            {
                CompositionType = CompositionTypeModel.Composition,
                DocumentType = new() { Id = CompositionId(composition) }
            }).ToArray()
        };
        
        if (templateId.HasValue)
        {
            requestModel.AllowedTemplates = [new() { Id = templateId.Value }];
            requestModel.DefaultTemplate = new() { Id = templateId.Value };
        }

        return requestModel;
    }

    private Guid CompositionId(string name)
        => _compositions?.TryGetValue(name, out Guid id) is true
            ? id
            : throw new InvalidOperationException($"The composition could not be found: {name}");

    private Guid TemplateId(string name)
        => _templates?.TryGetValue(name, out Guid id) is true
            ? id 
            : throw new InvalidOperationException($"The template could not be found: {name}");
}