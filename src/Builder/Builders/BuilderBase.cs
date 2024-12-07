using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public abstract class BuilderBase
{
    private readonly ApiTokenService _apiTokenService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UmbracoConfiguration _umbracoConfiguration;

    private Dictionary<string, Guid>? _dataTypes;
    private Dictionary<string, Guid>? _mediaTypes;

    protected BuilderBase(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration)
    {
        _apiTokenService = apiTokenService;
        _httpClientFactory = httpClientFactory;
        _umbracoConfiguration = umbracoConfiguration.Value;
    }
    
    protected ApiClient GetApiClient()
    {
        var accessToken = _apiTokenService.GetAccessToken()
            ?? throw new InvalidOperationException("Could not get an access token.");

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.SetBearerToken(accessToken);
        return new ApiClient(_umbracoConfiguration.Host, httpClient);
    }
    
    protected async Task<Guid> GetDataTypeIdAsync(ApiClient apiClient, string name)
    {
        if (_dataTypes is null)
        {
            _dataTypes = (await apiClient.GetTreeDataTypeRootAsync(0, 100, false))?.Items.ToDictionary(item => item.Name, item => item.Id);
            if (_dataTypes is null)
            {
                throw new InvalidOperationException("Could not fetch data types");
            }
        }

        if (_dataTypes.TryGetValue(name, out var itemId) is false)
        {
            throw new InvalidOperationException($"The data type did not exist: {name}");
        }

        return itemId;
    }

    protected async Task<Guid> GetMediaTypeIdAsync(ApiClient apiClient, string name)
    {
        if (_mediaTypes is null)
        {
            _mediaTypes = (await apiClient.GetTreeMediaTypeRootAsync(0, 100, false))?.Items.ToDictionary(item => item.Name, item => item.Id);
            if (_mediaTypes is null)
            {
                throw new InvalidOperationException("Could not fetch media types");
            }
        }

        if (_mediaTypes.TryGetValue(name, out var itemId) is false)
        {
            throw new InvalidOperationException($"The media type did not exist: {name}");
        }

        return itemId;
    }

    protected async Task<Dictionary<string, Guid>> GetDocumentTypeIdsAsync(ApiClient apiClient, params string[] folderPath)
    {
        ICollection<DocumentTypeTreeItemResponseModel>? items;
        if (folderPath.Any() is false)
        {
            items = (await apiClient.GetTreeDocumentTypeRootAsync(0, 100, false)).Items;
        }
        else
        {
            Guid? parentId = null;
            foreach (var folderName in folderPath)
            {
                var folderItems = parentId.HasValue
                    ? (await apiClient.GetTreeDocumentTypeChildrenAsync(parentId.Value, 0, 100, true)).Items
                    : (await apiClient.GetTreeDocumentTypeRootAsync(0, 100, true)).Items;

                var folderItem = folderItems?.FirstOrDefault(i => i.Name == folderName);
                if (folderItem is null)
                {
                    throw new ArgumentException($"Could not find child document type folder: {folderName}");
                }

                parentId = folderItem.Id;
            }

            items = (await apiClient.GetTreeDocumentTypeChildrenAsync(parentId!.Value, 0, 100, false)).Items;
        }

        var documentTypeItems = items?.Where(i => i.IsFolder is false).ToArray();
        
        if (documentTypeItems?.Any() is not true)
        {
            throw new ArgumentException($"Could not find child document types for path: {string.Join("/", folderPath)}");
        }

        return documentTypeItems.ToDictionary(item => item.Name, item => item.Id);
    }

    protected async Task<Dictionary<string, Guid>> GetMediaIdsAsync(ApiClient apiClient, string folderName)
    {
        var folderItems = (await apiClient.GetTreeMediaRootAsync(0, 100, null)).Items;
        var folderItem = folderItems?.FirstOrDefault(i => i.Variants.FirstOrDefault()?.Name == folderName);
        if (folderItem is null)
        {
            throw new ArgumentException($"Could not find media folder: {folderName}");
        }

        var mediaItems = (await apiClient.GetTreeMediaChildrenAsync(folderItem.Id, 0, 100, null)).Items;
        if (mediaItems?.Any() is not true)
        {
            throw new ArgumentException($"Could not find child media for in folder: {folderName}");
        }

        return mediaItems.ToDictionary(item => item.Variants.First().Name, item => item.Id);
    }

    protected async Task<Dictionary<string, Guid>> GetTemplateIdsAsync(ApiClient apiClient)
    {
        var rootTemplates = (await apiClient.GetTreeTemplateRootAsync(0, 100)).Items.ToDictionary(item => item.Name, item => item.Id);
        if (
            rootTemplates.Any() is false
            || rootTemplates.TryGetValue(TemplateNames.Master, out var masterTemplateId) is false
            || rootTemplates.TryGetValue(TemplateNames.XmlSitemap, out var xmlSitemapTemplateId) is false
        )
        {
            throw new InvalidOperationException("Could not find the required templates at root level");
        }

        var pageTemplates = (await apiClient.GetTreeTemplateChildrenAsync(masterTemplateId, 0, 100)).Items.ToDictionary(item => item.Name, item => item.Id);
        pageTemplates[TemplateNames.Master] = masterTemplateId;
        pageTemplates[TemplateNames.XmlSitemap] = xmlSitemapTemplateId;

        return pageTemplates;
    }

    protected static class DataTypeNames
    {
        public const string BlockListIconList = "[BlockList] Icon List";

        public const string BlockListMainContent = "[BlockList] Main Content";

        public const string ContentPickerAuthors = "[MNTP] Authors";

        public const string ContentPickerCategories = "[MNTP] Categories";

        public const string DocumentPicker = "Content Picker";

        public const string DatePickerWithTime = "Date Picker with time";

        public const string DropdownSpacing = "[Dropdown] Spacing";

        public const string ListViewContent = "List View - Content";
        
        public const string MediaPickerImage = "Image Media Picker";

        public const string MediaPickerMultipleImage = "Multiple Image Media Picker";

        public const string MediaPickerSvg = "[MediaPicker] SVG Image";

        public const string Numeric = "Numeric";

        public const string RichTextEditor = "Richtext editor";

        public const string SliderSpacing = "[Slider] Spacing";

        public const string Tags = "Tags";

        public const string TextArea = "Textarea";

        public const string TextString = "Textstring";

        public const string Toggle = "True/false";

        public const string ToggleDefaultTrue = "[Toggle] Default True";

        public const string UrlPickerSingle = "[MultiUrlPicker] Single Url Picker";
    }

    protected static class MediaTypeNames
    {
        public const string Folder = "Folder";

        public const string Image = "Image";

        public const string VectorGraphics = "Vector Graphics (SVG)";
    }

    protected static class DocumentTypeNames
    {
        public const string Home = "Home";

        public const string ArticleList = "Article List";

        public const string Article = "Article";

        public const string Contact = "Contact";

        public const string Error = "Error";

        public const string XmlSitemap = "XML Sitemap";

        public const string Search = "Search";

        public const string AuthorList = "Author List";

        public const string Author = "Author";

        public const string CategoryList = "Category List";

        public const string Category = "Category";

        public const string Content = "Content";

        public static class Compositions
        {
            public static string ArticleControls = "Article Controls";
            
            public static string ContentControls = "Content Controls";
            
            public static string HeaderControls = "Header Controls";
            
            public static string FooterControls = "Footer Controls";
            
            public static string MainImageControls = "Main Image Controls";
            
            public static string SeoControls = "SEO Controls";
            
            public static string VisibilityControls = "Visibility Controls";

            public static string ContactFormControls = "Contact Form Controls";

            public static string HideProperty = "Hide Property";

            public static string SpacingProperties = "Spacing Properties";
        }
        
        public static class ContentElements
        {
            public const string RichTextRow = "Rich Text Row";

            public const string ImageRow = "Image Row";

            public const string VideoRow = "Video Row";

            public const string CodeSnippetRow = "Code Snippet Row";

            public const string ImageCarouselRow = "Image Carousel Row";

            public const string LatestArticlesRow = "Latest Articles Row";

            public const string IconLinkRow = "Icon Link Row";
        }

        public static class SettingsElements
        {
            public const string RichTextRow = "Rich Text Row Settings";

            public const string ImageRow = "Image Row Settings";

            public const string VideoRow = "Video Row Settings";

            public const string CodeSnippetRow = "Code Snippet Row Settings";

            public const string ImageCarouselRow = "Image Carousel Row Settings";

            public const string LatestArticlesRow = "Latest Articles Row Settings";

            public const string IconLinkRow = "Icon Link Row Settings";
        }
    }

    public static class TemplateNames
    {
        public const string Master = "Master";
        
        public const string Article = "Article";
        
        public const string ArticleList = "Article List";
        
        public const string Author = "Author";
        
        public const string AuthorList = "Author List";
        
        public const string Contact = "Contact";
        
        public const string Content = "Content";
        
        public const string Error = "Error";
        
        public const string Home = "Home";
        
        public const string Search = "Search";
        
        public const string XmlSitemap = "XMLSitemap";
    }
}