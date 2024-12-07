using Builder.Models.Document;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class MediaBuilder : BuilderBase
{
    private readonly ILogger<MediaBuilder> _logger;

    public MediaBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<MediaBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting media");

        var apiClient = GetApiClient();

        _logger.LogInformation("- fetching required items");

        var folderMediaTypeId = await GetMediaTypeIdAsync(apiClient, MediaTypeNames.Folder);
        var imageMediaTypeId = await GetMediaTypeIdAsync(apiClient, MediaTypeNames.Image);
        var vectorGraphicsMediaTypeId = await GetMediaTypeIdAsync(apiClient, MediaTypeNames.VectorGraphics);

        _logger.LogInformation("- adding folders");

        var socialIconsFolderId = await CreateFolder(apiClient, "Social Icons", folderMediaTypeId);
        var sampleImagesFolderId = await CreateFolder(apiClient, "Sample Images", folderMediaTypeId);
        var authorsFolderId = await CreateFolder(apiClient, "Authors", folderMediaTypeId);

        _logger.LogInformation("- adding images");

        await CreateImage(apiClient, "Profile Pic 2023", "Authors\\profile-pic-2023.png", imageMediaTypeId, authorsFolderId);
        
        await CreateImage(apiClient, "24 days people at codegarden", "Sample Images\\24-days-people-at-codegarden.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Authors", "Sample Images\\authors.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Bluetooth white keyboard", "Sample Images\\bluetooth-white-keyboard.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Candid Contributions", "Sample Images\\candid-contributions.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Chairs lamps", "Sample Images\\chairs-lamps.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Codegarden keynote", "Sample Images\\codegarden-keynote.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Community front row", "Sample Images\\community-front-row.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Desktop notebook glasses", "Sample Images\\desktop-notebook-glasses.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Diary", "Sample Images\\diary.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "First timers at codegarden", "Sample Images\\first-timers-at-codegarden.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Friendly chair", "Sample Images\\friendly-chair.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Front row audience smiles", "Sample Images\\front-row-audience-smiles.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Mastodon", "Sample Images\\mastodon.png", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Meetup organizers at codegarden", "Sample Images\\meetup-organizers-at-codegarden.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Package Manifest", "Sample Images\\package-manifest.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Phone pen binder", "Sample Images\\phone-pen-binder.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Podcast headphones coffee", "Sample Images\\podcast-coffee.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Say cheese", "Sample Images\\say-cheese.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Skrift at codegarden", "Sample Images\\skrift-at-codegarden.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Triangle table chairs", "Sample Images\\triangle-table-chairs.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Tutorials", "Sample Images\\tutorials.jpg", imageMediaTypeId, sampleImagesFolderId);
        await CreateImage(apiClient, "Umbracoffee codegarden", "Sample Images\\umbracoffee-codegarden.jpg", imageMediaTypeId, sampleImagesFolderId);

        await CreateImage(apiClient, "Cc Paypal", "Social Icons\\cc-paypal.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Discord", "Social Icons\\discord.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Github", "Social Icons\\github.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Github Alt", "Social Icons\\github-alt.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Mastodon", "Social Icons\\mastodon.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Paypal", "Social Icons\\paypal.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Share Nodes", "Social Icons\\share-nodes.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Square Github", "Social Icons\\square-github.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Square Twitter", "Social Icons\\square-twitter.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Twitter", "Social Icons\\twitter.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
        await CreateImage(apiClient, "Umbraco", "Social Icons\\umbraco.svg", vectorGraphicsMediaTypeId, socialIconsFolderId);
    }

    private async Task<Guid> CreateFolder(ApiClient apiClient, string name, Guid folderMediaTypeId)
    {
        var id = Guid.NewGuid();
        await apiClient.PostMediaAsync(
            new()
            {
                Id = id,
                Variants = [ new() { Name = name } ],
                MediaType = new() { Id = folderMediaTypeId }
            }
        );

        return id;
    }

    private async Task CreateImage(ApiClient apiClient, string name, string filePath, Guid mediaTypeId, Guid folderId)
    {
        filePath = $"Media\\{filePath}";
        var file = new FileInfo(filePath);
        if (file.Exists is false)
        {
            throw new ArgumentException($"Could not find the file on disk: {filePath}");
        }

        var temporaryFileId = Guid.NewGuid();
        await apiClient.PostTemporaryFileAsync(temporaryFileId, new FileParameter(file.OpenRead(), file.Name));
        await apiClient.PostMediaAsync(
            new()
            {
                Variants = [ new() { Name = name } ],
                MediaType = new() { Id = mediaTypeId },
                Parent = new () { Id = folderId },
                Values = 
                [
                    new ()
                    {
                        Alias = "umbracoFile",
                        Value = new TemporaryFileValue
                        {
                            TemporaryFileId = temporaryFileId
                        }
                    }
                ]
            }
        );
    }
}