using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public abstract class DocumentTypeBuilderBase : BuilderBase
{
    protected DocumentTypeBuilderBase(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
    {
    }

    protected async Task<Guid> CreateFolderAsync(ApiClient apiClient, string name, Guid? parentId)
    {
        var id = Guid.NewGuid();
        await apiClient.PostDocumentTypeFolderAsync(
            new()
            {
                Id = id,
                Parent = parentId.HasValue ? new() { Id = parentId.Value } : null,
                Name = name
            });

        return id;
    }
}