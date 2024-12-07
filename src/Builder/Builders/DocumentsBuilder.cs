using Builder.Extensions;
using Builder.Models.Document;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Management.Api;

namespace Builder.Builders;

public class DocumentsBuilder : BuilderBase
{
    private readonly ILogger<DocumentsBuilder> _logger;

    private Dictionary<string, Guid> _documents = new();
    private Dictionary<string, Guid>? _socialIcons;
    private Dictionary<string, Guid>? _sampleImages;
    private Dictionary<string, Guid>? _authorImages;
    private Dictionary<string, Guid>? _contentElementTypes;
    private Dictionary<string, Guid>? _settingsElementTypes;
    private Dictionary<string, Guid>? _documentTypes;
    private Dictionary<string, Guid>? _templates;
    
    public DocumentsBuilder(ApiTokenService apiTokenService, IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<DocumentsBuilder> logger)
        : base(apiTokenService, httpClientFactory, umbracoConfiguration)
        => _logger = logger;

    public async Task BuildAsync()
    {
        _logger.LogInformation("Starting documents");

        var apiClient = GetApiClient();

        // start by fetching all the referenced items needed for creating the documents
        _logger.LogInformation("- fetching required items");
        _socialIcons = await GetMediaIdsAsync(apiClient, "Social Icons");
        _sampleImages = await GetMediaIdsAsync(apiClient, "Sample Images");
        _authorImages = await GetMediaIdsAsync(apiClient, "Authors");
        _contentElementTypes = await GetDocumentTypeIdsAsync(apiClient, "Elements", "Content Models");
        _settingsElementTypes = await GetDocumentTypeIdsAsync(apiClient, "Elements", "Setting Models");
        _documentTypes = await GetDocumentTypeIdsAsync(apiClient, "Pages");
        _templates = await GetTemplateIdsAsync(apiClient);

        // build the whole document structure
        _logger.LogInformation("- creating documents (first pass)");
        await CreateDocumentsAsync(apiClient);

        // perform a second pass of document updates for documents referencing other documents
        _logger.LogInformation("- updating documents (second pass)");
        await UpdateDocumentsAsync(apiClient);

        // publish the whole thing
        _logger.LogInformation("- publishing documents");
        await apiClient.PutDocumentByIdPublishWithDescendantsAsync(
            DocumentId(DocumentNames.Home),
            new()
            {
                IncludeUnpublishedDescendants = true
            });
    }

    
    private async Task CreateDocumentsAsync(ApiClient apiClient)
    {
        // certain documents reference other documents. to facilitate this, store the relevant document IDs by document names in _documents.
        
        var homeId = await CreateBlankAsync(apiClient, DocumentTypeNames.Home, null);
        _documents[DocumentNames.Home] = homeId;
        await CreateFeaturesAsync(apiClient, homeId);
        await CreateAboutAsync(apiClient, homeId);

        var blogId = await CreateBlankAsync(apiClient, DocumentTypeNames.ArticleList, homeId);
        _documents[DocumentNames.Blog] = blogId;
        _documents[BlogDocumentKey(DocumentNames.BlogCommunity)] = await CreateBlankAsync(apiClient, DocumentTypeNames.Article, blogId);
        _documents[BlogDocumentKey(DocumentNames.BlogPopularBlogs)] = await CreateBlankAsync(apiClient, DocumentTypeNames.Article, blogId);
        _documents[BlogDocumentKey(DocumentNames.BlogMeetups)] = await CreateBlankAsync(apiClient, DocumentTypeNames.Article, blogId);
        _documents[BlogDocumentKey(DocumentNames.BlogConferences)] = await CreateBlankAsync(apiClient, DocumentTypeNames.Article, blogId);
        _documents[BlogDocumentKey(DocumentNames.BlogPodcastsAndVideos)] = await CreateBlankAsync(apiClient, DocumentTypeNames.Article, blogId);
        _documents[BlogDocumentKey(DocumentNames.BlogYouTubeTutorials)] = await CreateBlankAsync(apiClient, DocumentTypeNames.Article, blogId);
        _documents[BlogDocumentKey(DocumentNames.BlogJoinTheUmbracoCommunityOnMastodon)] = await CreateBlankAsync(apiClient, DocumentTypeNames.Article, blogId);

        await CreateContactAsync(apiClient, homeId);
        await CreateErrorAsync(apiClient, homeId);
        await CreateXmlSitemapAsync(apiClient, homeId);
        await CreateSearchAsync(apiClient, homeId);

        var authorListId = await CreateAuthorsAsync(apiClient, homeId);
        _documents[AuthorDocumentKey(DocumentNames.AuthorsPaulSeal)] = await CreateAuthorsPaulSealAsync(apiClient, authorListId);

        var categoryListId = await CreateCategoriesAsync(apiClient, homeId);
        _documents[CategoryDocumentKey(DocumentNames.CategoriesCommunity)] = await CreateCategoryAsync(apiClient, categoryListId, "Community");
        _documents[CategoryDocumentKey(DocumentNames.CategoriesConferences)] = await CreateCategoryAsync(apiClient, categoryListId, "Conferences");
        _documents[CategoryDocumentKey(DocumentNames.CategoriesMeetups)] = await CreateCategoryAsync(apiClient, categoryListId, "Meetups");
        _documents[CategoryDocumentKey(DocumentNames.CategoriesPodcasts)] = await CreateCategoryAsync(apiClient, categoryListId, "Podcasts");
        _documents[CategoryDocumentKey(DocumentNames.CategoriesResources)] = await CreateCategoryAsync(apiClient, categoryListId, "Resources");
        _documents[CategoryDocumentKey(DocumentNames.CategoriesUmbraco)] = await CreateCategoryAsync(apiClient, categoryListId, "Umbraco");
        _documents[CategoryDocumentKey(DocumentNames.CategoriesVideos)] = await CreateCategoryAsync(apiClient, categoryListId, "Videos");
    }

    private async Task UpdateDocumentsAsync(ApiClient apiClient)
    {
        await UpdateHomeAsync(apiClient);
        await UpdateBlogAsync(apiClient);
        await UpdateBlogCommunityAsync(apiClient);
        await UpdateBlogPopularPostsAsync(apiClient);
        await UpdateBlogMeetupsAsync(apiClient);
        await UpdateBlogConferencesAsync(apiClient);
        await UpdateBlogPodcastsAndVideosAsync(apiClient);
        await UpdateBlogYouTubeTutorialsAsync(apiClient);
        await UpdateBlogJoinTheUmbracoCommunityOnMastodonAsync(apiClient);
    }

    #region Update documents
    
    private async Task UpdateHomeAsync(ApiClient apiClient)
    {
        var iconLinkRowContentTypeId = ContentElementTypeId(DocumentTypeNames.ContentElements.IconLinkRow);
        var iconLinkRowSettingsTypeId = SettingsElementTypeId(DocumentTypeNames.SettingsElements.IconLinkRow);
        var socialIconLinksValue = new BlockListValue();
        var socialLinks = new []
        {
            new { Icon = "Github Alt", Url = "https://github.com/prjseal", Name = "View this package repository on GitHub" },
            new { Icon = "Umbraco", Url = "https://marketplace.umbraco.com/package/clean", Name = "View this in the Umbraco Marketplace" },
            new { Icon = "Twitter", Url = "https://twitter.com/codesharepaul", Name = "Follow me on twitter" },
            new { Icon = "Share Nodes", Url = "https://codeshare.co.uk", Name = "Read my blog codeshare.co.uk" },
            new { Icon = "Discord", Url = "https://discord.gg/umbraco", Name = "Join the Umbraco discord server" },
            new { Icon = "Paypal", Url = "https://codeshare.co.uk/coffee", Name = "You can buy me a coffee on PayPal if you would like to" },
            new { Icon = "Mastodon", Url = "https://umbracocommunity.social/@CodeSharePaul", Name = "Follow me on Mastodon" }
        };
        foreach (var socialLink in socialLinks)
        {
            socialIconLinksValue.Add(
                iconLinkRowContentTypeId,
                [
                    new ()
                    {
                        Alias = "icon",
                        Value = new [] { new MediaPickerValue { MediaKey = SocialIconId(socialLink.Icon) } } 
                    },
                    new ()
                    {
                        Alias = "link",
                        Value = new [] { new MultiUrlPickerValue { Target = "_blank", Url = socialLink.Url, Name = socialLink.Name } }
                    }
                ],
                iconLinkRowSettingsTypeId,
                [
                    new () { Alias = "hide", Value = false }
                ]
            );
        }

        var contentRowsValue = new BlockListValue();
        AddLatestArticlesRow(contentRowsValue, 3, false);

        await UpdateAsync(
            apiClient,
            DocumentId(DocumentNames.Home),
            TemplateNames.Home,
            DocumentNames.Home,
            [
                new() { Alias = "contentRows", Value = contentRowsValue },
                new() { Alias = "socialIconLinks", Value = socialIconLinksValue },
                new() { Alias = "title", Value = "Clean Starter Kit" },
                new() { Alias = "subtitle", Value = "For Umbraco" },
                new()
                {
                    Alias = "mainImage",
                    Value = new MediaPickerValue[] { new() { MediaKey = SampleImageId("Bluetooth white keyboard") } }
                },
                new() { Alias = "isIndexable", Value = true },
                new() { Alias = "isFollowable", Value = true }
            ]);
    }

    private async Task UpdateBlogAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddLatestArticlesRow(contentRowsValue, 5, true);

        await UpdateAsync(
            apiClient,
            DocumentId(DocumentNames.Blog),
            TemplateNames.ArticleList,
            DocumentNames.Blog,
            null,
            "Many blog posts for you",
            contentRowsValue,
            "Desktop notebook glasses"
        );
    }

    private async Task UpdateBlogCommunityAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>There is a large community around umbraco, it is one of the main attractions when choosing it as your Content Management System of choice.</p>"
        );
        AddImageRow(contentRowsValue, "First timers at codegarden", "Umbraco community enjoying another great talk"); 
        AddRichTextRow(
            contentRowsValue,
            "<p>Not everyone is aware of all of the places where you can connect to the Umbraco community, so this post will list out different places you can communicate with the Umbraco community.</p>\n<h2><a rel=\"noopener\" href=\"https://our.umbraco.com\" target=\"_blank\">Our Umbraco Forum</a></h2>\n<p>Our is the official Umbraco forum. If you have a problem, you should search there first to see if it has been solved already. Then if it hasn't you should ask your question on there. You will get friendly responses from a community of people who want to help you succeed, not see you fail.</p>\n<p>There is also a <a rel=\"noopener\" href=\"https://our.umbraco.com/community/\" target=\"_blank\">community section</a> on Our Umbraco where you can find out way more about the Umbraco Community.</p>\n<h2><a rel=\"noopener\" href=\"https://discord.gg/umbraco\" target=\"_blank\">Discord Server</a></h2>\n<p>There is a growing community of people on the Discord Server now. This is also a great place for you to ask for help with Umbraco too.</p>\n<p>If you like to use discord, you can chat with other Umbraco members in real time. This is good for getting quick answers to questions or good for asking for people's opinions on things. There are many different channels to be a part of, so it is worth checking it out.</p>\n<h2><a rel=\"noopener\" href=\"https://www.facebook.com/groups/202933450108554/\" target=\"_blank\">Facebook Group</a></h2>\n<p>If you prefer to use Facebook, then this group might be of interest to you. It is called Umbraco Web Developers, but anyone can join, you don't have to be a developer, but it helps to be working with Umbraco.</p>\n<h2><a rel=\"noopener\" href=\"https://twitter.com/search?q=umbraco&amp;src=typed_query&amp;f=live\" target=\"_blank\" data-anchor=\"?q=umbraco&amp;src=typed_query&amp;f=live\">Twitter</a></h2>\n<p>Twitter is a great place to see the latest developments in the Umbraco community. You will see people reporting issues with Umbraco, asking questions about how to do things and arranging meetups. To tune into the chat on twitter you can search for Umbraco or filter by the #Umbraco hashtag.</p>"
        );

        await UpdateArticleAsync(
            apiClient,
            BlogDocumentId(DocumentNames.BlogCommunity),
            DocumentNames.BlogCommunity,
            null,
            "The friendly CMS",
            contentRowsValue,
            "First timers at codegarden",
            new DateTime(2023, 07, 28, 12, 00, 00),
            [DocumentNames.CategoriesCommunity]
        );
    }

    private async Task UpdateBlogPopularPostsAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>There are many blogs and magazines from Umbraco community members writing about Umbraco</p>\n<p>Here are some popular ones:</p>\n<h2><a rel=\"noopener\" href=\"https://skrift.io/\" target=\"_blank\">Skrift.io</a></h2>\n<p>Skrift isn't just \"another blog on the internet\". They are an online magazine with an ISSN (International Standard Serial Number), which means that as one of their authors, you are officially printed in a publication. Which means you will have great recognition in your field and it may help you land your next contract, client, raise or job.</p>\n<p>So have a read of the articles and consider writing for them too.</p>\n<h2><a rel=\"noopener\" href=\"https://24days.in/umbraco-cms/\" target=\"_blank\">24 days in Umbraco</a></h2>\n<p>The 24 Days In Umbraco Christmas Calendar. It started in December 2012, where they asked a bunch of Umbraco people if they had a favourite feature, a story or something else that they'd be willing to write a short article about. The 24 days team now post a new one on the site everyday through December.</p>\n<p>You can also write for 24 days, so have a read through the articles and contact the organisers if you would like to write for them.</p>"
        );
        AddImageRow(contentRowsValue, "Skrift at codegarden", "Skrift authors at Codegarden"); 
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://codeshare.co.uk/\" target=\"_blank\">codeshare.co.uk</a></h2>\n<p>Codeshare is a blog by me, Paul Seal, where i like to share tips about Umbraco and other web related topics. There are tools on there too such as a strong password generator.</p>\n<h2><a rel=\"noopener\" href=\"http://www.jondjones.com/learn-umbraco-cms\" target=\"_blank\">jondjones.com</a></h2>\n<p>Jon D Jones is a great source of knowledge about Umbraco and other CMSs. He writes very detailed tutorials to go with his videos.</p>\n<h2><a rel=\"noopener\" href=\"https://owain.codes/\" target=\"_blank\">Owain.codes</a></h2>\n<p>Owain's Umbraco Community thoughts, coding tutorials, what he is learning, general blogs about his experience with Umbraco and any other things that come to mind.</p>\n<h2><a rel=\"noopener\" href=\"https://cornehoskam.com/\" target=\"_blank\" title=\"Corné Hoskam\">CornéHoskam.com</a></h2>\n<p>Corné Hoskam is a very talented Umbraco Developer and speaker. He likes to share his knowledge and experience in his personal blog.</p>"
        );

        await UpdateArticleAsync(
            apiClient,
            BlogDocumentId(DocumentNames.BlogPopularBlogs),
            DocumentNames.BlogPopularBlogs,
            null,
            "Popular blogs and magazines about Umbraco",
            contentRowsValue,
            "24 days people at codegarden",
            new DateTime(2023, 07, 28, 12, 00, 00),
            [DocumentNames.CategoriesCommunity, DocumentNames.CategoriesUmbraco]
        );
    }

    private async Task UpdateBlogMeetupsAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>Until I had gone to my first meetup, I didn't realise how comforting and safe it would feel to be around like minded people. When I started going to meetups, I realised that there were other people out there who liked the same things I do, and I could have decent conversations with them face to face. With that in mind I would encourage to you attend a meetup if you can.</p>"
        );
        AddImageRow(contentRowsValue, "Community front row", "We love to hear great talks at meetups"); 
        AddRichTextRow(
            contentRowsValue,
            "<p>Here are some popular Umbraco meetups, some of them are virtual meetups.</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/Edinburgh-Umbraco-Users-Group/events/260304467/\" target=\"_blank\">Umbraco for Edinburgh Developers and Users</a></h2>\n<p>EDINBUUG is a chance to chat about all things Umbraco - whether you are someone who uses Umbraco for their website, you're a developer who builds Umbraco websites or maybe you are a project manager who works with clients who have Umbraco websites - come along. Everyone is welcome.</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/Australian-Umbraco-Meetups/events/tgbtfqyzlbjb/\" target=\"_blank\">Melbourne Umbracian Meetup</a></h2>\n<p>The purpose is to get together, discuss and learn a little about Umbraco having some fun while doing so. Whether there are two of us or 22, the event will go forward, so come along and take part!</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/The-London-Umbraco-Meetup/\" target=\"_blank\">The London Umbraco Meetup</a></h2>\n<p>The London Umbraco Meetup Group is a monthly meetup for Umbraco devotees and newbies to come along, learn and share their knowledge and to also gently spread the umbraco love to all.</p>\n<p>We aim to meet each month, where you can have a chat and a beer with other fellow Umbracians.</p>\n<p>We also try and mix it up by having talks by both Umbraco and non-Umbraco</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/Dutch-Umbraco-User-Group/\" target=\"_blank\">Dutch Umbraco User Group</a></h2>\n<p>To get Umbraco in the picture in the Netherlands: that's what we aim for with the Dutch Umbraco User Group (DUUG)! It's a platform for and by professionals working with the most applied .net CMS in Europe. DUUG will inspire, inform, convince and reinforce.</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/umbLeeds/\" target=\"_blank\">Umbraco Leeds Meetup</a></h2>\n<p>Umbraco Leeds is a monthly meetup group for anyone involved or interested in Umbraco. Whether you're a developer, content editor, project owner or just curious to hear about what it's all about - come along to one of our meetups and you'll be sure of a warm and friendly welcome.</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/Sydney-Umbraco-Meetup/\" target=\"_blank\">Sydney Umbraco Meetup</a></h2>\n<p>This is the group for passionate Umbraco developers and users in the Sydney area to meetup every couple of months. Sometimes it may be at the Glenmore on The Rocks or it could be a presentation based on Umbraco hosted by a local agency.</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/Belgian-Umbraco-User-Group/\" target=\"_blank\">Belgian Umbraco User Group Meetup</a></h2>\n<p>The Belgian Umbraco Meet Up is an open space to come learn and meet umbraco people and agencies in the Belgian area, we however welcome umbracians from all parts of the globe and you are welcome to attend.</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/umbracodkmeetup/\" target=\"_blank\">Umbraco DK Meetup</a></h2>\n<p>We are a networking group for everyone who loves Umbraco!</p>\n<p>Meetups are organized in Odense, Aarhus, Copenhagen and where there are enthusiasts who want to arrange something. There will be presentations, hands-on sessions, display of the delicious things you've made in Umbraco or something else.</p>\n<h2><a rel=\"noopener\" href=\"https://www.meetup.com/Glasgow-Umbraco-Users-Group-GLUUG/\" target=\"_blank\">Glasgow Umbraco User Group (GLUUG)</a></h2>\n<p>The Glasgow Umbraco Users' Group (GLUUG) is a networking group for anyone interested in Umbraco. We have regular social meetups where people can come and chat to friendly and like-minded Umbraco folk. We also have more formal presentation-based meetups where experts in the community share their knowledge and usually kick-start some lively debates!</p>\n<p>So come along and chat to some friendly folk over a beer and some pizza at our next meetup :)</p>"
        );

        await UpdateArticleAsync(
            apiClient,
            BlogDocumentId(DocumentNames.BlogMeetups),
            DocumentNames.BlogMeetups,
            null,
            "What's happening near you?",
            contentRowsValue,
            "Front row audience smiles",
            new DateTime(2023, 07, 30, 12, 00, 00),
            [DocumentNames.CategoriesMeetups]
        );
    }

    private async Task UpdateBlogConferencesAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>There are many Umbraco conferences held around the world.</p>\n<p>The main one is codegarden which is held in Odense, Denmark, about a 10 minute walk away from the Umbraco HQ.</p>\n<p>In 2019 approximately 750 people attended, making it the biggest codegarden ever.</p>"
        );
        AddImageRow(contentRowsValue, "Codegarden keynote", "750 people attended Codegarden 2019"); 
        AddRichTextRow(
            contentRowsValue,
            "<p>Here is a list of Umbraco related conferences from around the world:</p>\n<h2><a rel=\"noopener\" href=\"https://umbracospark.com/\" target=\"_blank\">Umbraco Spark</a></h2>\n<h4>Bristol, England - March</h4>\n<p>The Umbraco Spark innovation conference is a must for all Umbraco developers that want to find out what's going on with Umbraco. Focusing on innovation and forward thinking, we cover topics such as Umbraco Headless, .Net Core, Machine Learning / AI, personalisation, mobile apps, content as a service, and digital assistants.</p>\n<h2><a rel=\"noopener\" href=\"https://codegarden.umbraco.com/\" target=\"_blank\">Codegarden</a></h2>\n<h4>Odense, Denmark - June</h4>\n<p>Codegarden is the biggest Umbraco conference in the world. It's 3 days packed with inspiring talks about tech, business, UX and, of course, Umbraco. 3 days where you will deepen your Umbraco knowledge, get inspired, get to meet the global open source Umbraco community and simply have a fabulous time.</p>\n<p>If you’re working with Umbraco or if Umbraco is part of your business - this is the place to be!</p>"
        );
        AddVideoRow(contentRowsValue, "https://www.youtube.com/watch?v=CQJIl2xoDhc", "Codegarden 2022 | Official Aftermovie");
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://www.duugfest.nl/\" target=\"_blank\">DUUGFest</a></h2>\n<h4>Utrecht, Netherlands - October</h4>\n<p>DUUGFest is run by the Dutch Umbraco User Group</p>\n<h2><a rel=\"noopener\" href=\"https://codecab.in/\" target=\"_blank\">CODECABIN</a></h2>\n<h4>Peak District, England - September</h4>\n<p>CODECABIN is the premier, invite-only weekend away for Umbraco developers, providing time to code, learn and network in a completely relaxed and open environment away from the hustle and bustle of every day life.</p>\n<h2><a rel=\"noopener\" href=\"https://festival.umbracofoundation.co.uk/\" target=\"_blank\">Umbraco UK Festival</a></h2>\n<h4>London, England - November</h4>\n<p>The world's biggest community organised Umbraco event. Get ready for a jam packed event featuring incredible talks on development, front end, design &amp; UX, wellbeing, and business, deep-dive workshops hosted by community experts, and of course our legendary hackathon.</p>"
        );

        await UpdateArticleAsync(
            apiClient,
            BlogDocumentId(DocumentNames.BlogConferences),
            DocumentNames.BlogConferences,
            null,
            "Around the world",
            contentRowsValue,
            "Say cheese",
            new DateTime(2023, 07, 30, 12, 00, 00),
            [DocumentNames.CategoriesConferences]
        );
    }

    private async Task UpdateBlogPodcastsAndVideosAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>A great way to stay in touch with what is happening in the Umbraco community is by listening to podcasts and watching umbraCoffee.</p>"
        );
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://www.youtube.com/c/umbraCoffee/\" target=\"_blank\">umbraCoffee</a></h2>\n<p>umbraCoffee is a weekly YouTube show where each Friday at 11:30am UK / 12:30pm CET, the hosts - Marcin and Callum - together with their guests drive you through the weekly news and all things Umbraco related. So grab a cuppa, join them LIVE and enjoy!</p>"
        );
        AddVideoRow(contentRowsValue, "https://www.youtube.com/watch?v=8X_Hzm29tV8", "An episode of umbraCoffee from July 2023");
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://candidcontributions.com/\" target=\"_blank\">Candid Contributions</a></h2>\n<p>Candid Contributions is the home of the aptly named podcast where four experienced developers: Carole Logan, Emma Burstow, Laura Weatherhead and Lotte Pitcher talk all things open source - from code contributions to conference attendance; they aim to cover all aspects of life as an active member of an open-source community.</p>"
        );
        AddImageRow(contentRowsValue, "Candid Contributions", "Candid Contributions Podcast"); 
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://packagemanifest.fm/\" target=\"_blank\">Package Manifest</a></h2>\n<p>Package Manifest is a podcast all about Umbraco packages and the people who create them. It is hosted by Matt Brailsford, Kevin Jump and Lee Kelleher.</p>"
        );
        AddImageRow(contentRowsValue, "Package Manifest", "Package Manifest Podcast"); 

        await UpdateArticleAsync(
            apiClient,
            BlogDocumentId(DocumentNames.BlogPodcastsAndVideos),
            DocumentNames.BlogPodcastsAndVideos,
            "Podcasts and Videos",
            "From the Umbraco Community",
            contentRowsValue,
            "Podcast headphones coffee",
            new DateTime(2023, 08, 04, 08, 00, 00),
            [DocumentNames.CategoriesPodcasts, DocumentNames.CategoriesVideos]
        );
    }

    private async Task UpdateBlogJoinTheUmbracoCommunityOnMastodonAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>Recently it has been talked about that the Umbraco Community seems to have been spread out across different social media platforms since the takeover of Twitter. So the sense of community has been watered down.</p>\n<p>In an effort to boost that community spirit and to gather in one place in a platform \"similar\" to twitter, there has been a push to join the Umbraco Mastodon server.</p>\n<p>It's great to see lots of Umbraco related posts in your feed without ads or \"recommended\" posts.</p>\n<p>Joe Glombek from Bump Digital wrote an <a rel=\"noopener\" href=\"https://joe.gl/ombek/blog/umbraco-in-the-fediverse/\" target=\"_blank\">excellent blog post</a> all about what Mastodon is and how to join the Umbraco Mastodon server.</p>\n<p>So have a read of that post to find out more.</p>"
        );

        await UpdateArticleAsync(
            apiClient,
            BlogDocumentId(DocumentNames.BlogJoinTheUmbracoCommunityOnMastodon),
            "Join the Umbraco Community on Mastodon",
            null,
            null,
            contentRowsValue,
            "Mastodon",
            new DateTime(2023, 10, 03, 12, 00, 00),
            [DocumentNames.CategoriesCommunity, DocumentNames.CategoriesUmbraco]
        );
    }

    private async Task UpdateBlogYouTubeTutorialsAsync(ApiClient apiClient)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>There are lots of free videos on YouTube for you to be able to learn more about Umbraco.</p>"
        );
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://www.youtube.com/@UmbracoLearningBase\" target=\"_blank\">Umbraco Learning Base</a></h2>\n<p>Umbraco Learning Base is the official channel for the Happy Documentation and TV team at Umbraco HQ. On this channel, you can find videos and tutorials on how to get started working with the world's friendliest CMS Umbraco.</p>"
        );
        AddVideoRow(contentRowsValue, "https://www.youtube.com/watch?v=Yu29dE-0OoI", "Getting Started with Umbraco");
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://www.youtube.com/@CodeSharePaul\" target=\"_blank\">Paul Seal's YouTube Channel</a></h2>\n<p>There are several playlists of videos in this channel for you to learn how to build a website with Umbraco. Paul takes you step by step through the process and shows you the errors and mistakes along the way so you can see how to fix them yourself.</p>"
        );
        AddVideoRow(contentRowsValue, "https://www.youtube.com/watch?v=xeWh9-56UzY", "Umbraco 13 Tutorial - Episode 1 - Introduction");
        AddRichTextRow(
            contentRowsValue,
            "<h2><a rel=\"noopener\" href=\"https://www.youtube.com/@jondjones\" target=\"_blank\">jondjones YouTube Channel</a></h2>\n<p>Jon creates a new YouTube tutorial video every week, with a detailed blog post to follow along with too. He usually creates videos about Umbraco and other Content Management Systems.</p>"
        );
        AddVideoRow(contentRowsValue, "https://www.youtube.com/watch?v=arki6eMudG8", "Build a NextJs headless website with Umbraco and Content Delivery API");

        await UpdateArticleAsync(
            apiClient,
            BlogDocumentId(DocumentNames.BlogYouTubeTutorials),
            DocumentNames.BlogYouTubeTutorials,
            null,
            "For learning Umbraco",
            contentRowsValue,
            "Tutorials",
            new DateTime(2023, 08, 04, 06, 00, 00),
            [DocumentNames.CategoriesVideos]
        );
    }

    private async Task UpdateArticleAsync(ApiClient apiClient, Guid id, string name, string? title, string? subTitle, BlockListValue contentRowsValue, string mainImage, DateTime articleDate, IEnumerable<string> categories)
    {
        var categoryIds = categories.Select(CategoryDocumentId).ToArray();
        await UpdateAsync(
            apiClient,
            id,
            TemplateNames.Article,
            name,
            [
                new() { Alias = "contentRows", Value = contentRowsValue },
                new() { Alias = "title", Value = title },
                new() { Alias = "subtitle", Value = subTitle },
                new()
                {
                    Alias = "mainImage",
                    Value = new MediaPickerValue[] { new() { MediaKey = SampleImageId(mainImage) } }
                },
                new() { Alias = "articleDate", Value = articleDate },
                new()
                {
                    Alias = "author",
                    Value = new[] { new ContentPickerValue { Unique = AuthorDocumentId(DocumentNames.AuthorsPaulSeal) } }
                },
                new()
                {
                    Alias = "categories",
                    Value = categoryIds.Select(categoryId => new ContentPickerValue { Unique = categoryId }).ToArray()
                },
                new() { Alias = "isIndexable", Value = true },
                new() { Alias = "isFollowable", Value = true }
            ]);
    }

    #endregion
    
    #region Create documents
    
    private async Task<Guid> CreateBlankAsync(ApiClient apiClient, string documentType, Guid? parentId)
        => await CreateAsync(
            apiClient,
            parentId,
            documentType,
            null,
            Guid.NewGuid().ToString("N"),
            []
        );

    private async Task CreateFeaturesAsync(ApiClient apiClient, Guid homeId)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<h2>Rich Text Row</h2>\n<p>There is a simple rich text row for writing your usual formatted content in a WYSIWIG style with the usual settings including but not limited to: </p>\n<p><strong>Bold</strong></p>\n<p><em>Italic</em></p>\n<p style=\"text-align: left;\">Left align</p>\n<p style=\"text-align: right;\">Right align</p>\n<ul>\n<li>Bulleted List</li>\n</ul>\n<ol>\n<li>Ordered List</li>\n</ol>"
        );
        AddRichTextRow(
            contentRowsValue,
            "<h2>Image Row</h2>\n<p>You can use the image row to render a full width image.</p>"
        );
        AddImageRow(contentRowsValue, "Phone pen binder", "Image Row Example"); 
        AddRichTextRow(
            contentRowsValue,
            "<h2>Video Row</h2>\n<p>This lets you embed a YouTube video by just entering the normal URL of the video, and it just renders the preview image of it at first. Then when you click on it, it loads it in as the iframe, which is better for the end user as it doesn't download all of the YouTube assets until it is needed.</p>"
        );
        AddVideoRow(contentRowsValue, "https://www.youtube.com/watch?v=Dn2tI1--LOs", "What's next in C# - Mads Torgersen @ Umbraco Codegarden 2022");
        AddRichTextRow(
            contentRowsValue,
            "<h2>Code Snippet Row</h2>\n<p>There is a code snippet row to enable you to easily share code snippets in your website.</p>"
        );
        AddCodeSnippetRow(
            contentRowsValue,
            "Code from the codeSnippetRow.cshtml file",
            "@inherits UmbracoViewPage<BlockListItem>\r\n@using Umbraco.Cms.Core.Models.Blocks\r\n\r\n@{\r\n    var row = Model.Content as CodeSnippetRow;\r\n    var settings = Model.Settings as CodeSnippetRowSettings;\r\n    if (settings?.Hide ?? false) { return; }\r\n\r\n    SmidgeHelper.RequiresJs(\"~/clean-assets/js/highlight.default.min.css\");\r\n    SmidgeHelper.RequiresJs(\"~/clean-assets/js/highlight.min.js\");\r\n}\r\n\r\n<div class=\"row clearfix\">\r\n    <div class=\"col-md-12 column\">\r\n        <pre><code>@row.Code</code></pre>\r\n    </div>\r\n</div>"
        );
        AddRichTextRow(
            contentRowsValue,
            "<h2>Image Carousel Row</h2>\n<p>You can add a simple image carousel to a page by using the Image Carousel Row. In this row you have a multi image picker and you just choose the images you want to display.</p>"
        );
        AddImageCarouselRow(
            contentRowsValue,
            ["Chairs lamps", "Bluetooth white keyboard", "Phone pen binder", "Triangle table chairs", "Community front row", "Skrift at codegarden"]
        );

        await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.Content,
            TemplateNames.Content,
            "Features",
            null,
            "in this starter kit",
            contentRowsValue,
            "Desktop notebook glasses"
        );
    }
    
    private async Task CreateAboutAsync(ApiClient apiClient, Guid homeId)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>The Clean Starter Kit for Umbraco uses the Start Bootstrap Theme Clean Blog which is built using Bootstrap 5. It has been implemented in Umbraco as a Starter Kit by Paul Seal who has the blog <a rel=\"noopener\" href=\"https://codeshare.co.uk\" target=\"_blank\">codeshare.co.uk</a> and works for the Umbraco Gold Partner <a rel=\"noopener\" href=\"https://www.clerkswell.com\" target=\"_blank\">ClerksWell</a>.</p>\n<p>The idea of this starter kit is to provide you with a clean and simple website. It is ideally aimed at people who are new to Umbraco so they can install the starter kit, get used to Umbraco and then build upon the kit with their own requirements.</p>"
        );
        AddImageRow(contentRowsValue, "Friendly chair", "Umbraco, the friendly CMS"); 
        AddRichTextRow(
            contentRowsValue,
            "<p>With this starter kit you should be able to quickly and easily set up a new website and share your content with others. The aim is for you to start using Umbraco and fall in love with it like I did, as a user or as a developer, you will find out how enjoyable it is to use.</p>"
        );

        await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.Content,
            TemplateNames.Content,
            "About",
            null,
            "All about this starter kit",
            contentRowsValue,
            "Triangle table chairs"
        );
    }

    private async Task CreateContactAsync(ApiClient apiClient, Guid homeId)
        => await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.Contact,
            TemplateNames.Contact,
            "Contact",
            [
                new () { Alias = "title", Value = "Contact Us" },
                new () { Alias = "subtitle", Value = "Get in touch" },
                new ()
                {
                    Alias = "instructionMessage",
                    Value = new RichTextValue { Markup = "<p>Want to get in touch? Fill out the form below to send me a message and I will get back to you as soon as possible!</p>" }
                },
                new ()
                {
                    Alias = "successMessage",
                    Value = new RichTextValue { Markup = "<h2>Thank you</h2>\n<p>Thanks for your email. We will be in touch soon.</p>" }
                },
                new ()
                {
                    Alias = "errorMessage",
                    Value = new RichTextValue { Markup = "<h2>Error</h2>\n<p>Sorry there was a problem with submitting the form. Please try again.</p>" }
                },
                new ()
                {
                    Alias = "mainImage",
                    Value = new MediaPickerValue[] { new () { MediaKey = SampleImageId("Bluetooth white keyboard") } }
                },
                new () { Alias = "isIndexable", Value = true },
                new () { Alias = "isFollowable", Value = true }
            ]
        );

    private async Task CreateErrorAsync(ApiClient apiClient, Guid homeId)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>Sorry, we couldn't find the page you were looking for.</p>\n<p>Why not go back to the <a href=\"/\">home page</a> and start again?</p>"
        );

        await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.Error,
            TemplateNames.Error,
            "Error",
            [
                new() { Alias = "contentRows", Value = contentRowsValue },
                new() { Alias = "title", Value = "Page not found" },
                new()
                {
                    Alias = "mainImage",
                    Value = new MediaPickerValue[] { new() { MediaKey = SampleImageId("Triangle table chairs") } }
                },
                new() { Alias = "isIndexable", Value = false },
                new() { Alias = "isFollowable", Value = false },
                new() { Alias = "hideFromTopNavigation", Value = true },
                new() { Alias = "umbracoNaviHide", Value = true },
                new() { Alias = "hideFromXMLSitemap", Value = true },
            ]
        );
    }

    private async Task CreateXmlSitemapAsync(ApiClient apiClient, Guid homeId)
        => await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.XmlSitemap,
            TemplateNames.XmlSitemap,
            "XMLSitemap",
            [
                new() { Alias = "hideFromTopNavigation", Value = true },
                new() { Alias = "umbracoNaviHide", Value = true },
                new() { Alias = "hideFromXMLSitemap", Value = true },
            ]);

    private async Task CreateSearchAsync(ApiClient apiClient, Guid homeId)
        => await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.Search,
            TemplateNames.Search,
            "Search",
            [
                new()
                {
                    Alias = "mainImage",
                    Value = new MediaPickerValue[] { new() { MediaKey = SampleImageId("Phone pen binder") } }
                },
                new() { Alias = "isIndexable", Value = false },
                new() { Alias = "isFollowable", Value = false }
            ]);

    private async Task<Guid> CreateAuthorsAsync(ApiClient apiClient, Guid homeId)
        => await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.AuthorList,
            TemplateNames.AuthorList,
            "Authors",
            []
        );

    private async Task<Guid> CreateAuthorsPaulSealAsync(ApiClient apiClient, Guid authorListId)
    {
        var contentRowsValue = new BlockListValue();
        AddRichTextRow(
            contentRowsValue,
            "<p>Paul Seal is an Umbraco Tech Lead and multiple times Umbraco MVP who works for the Umbraco Gold Partners <a rel=\"noopener\" href=\"https://www.clerkswell.com\" target=\"_blank\" title=\"ClerksWell\">ClerksWell</a>.</p>\n<p>When he's not creating packages such as the Clean Starter Kit, which you are looking at right now, Paul likes to contribute to Open Source by submitting pull requests to different projects such as the Umbraco Source code, uSync Migrations and Contentment.</p>\n<p>Paul likes to write articles with code snippets to help people out when they are struggling to achieve something, usually for Umbraco.</p>"
        );

        return await CreateAsync(
            apiClient,
            authorListId,
            DocumentTypeNames.Author,
            TemplateNames.Author,
            "Paul Seal",
            null,
            null,
            contentRowsValue,
            AuthorImageId("Profile Pic 2023"),
            metaDescription: "Paul Seal is an Umbraco Tech Lead and multiple times Umbraco MVP who works for the Umbraco Gold Partners ClerksWell."
        );
    }

    private async Task<Guid> CreateCategoriesAsync(ApiClient apiClient, Guid homeId)
        => await CreateAsync(
            apiClient,
            homeId,
            DocumentTypeNames.CategoryList,
            null,
            "Categories",
            [
                new () { Alias = "hideFromTopNavigation", Value = true },
                new () { Alias = "umbracoNaviHide", Value = true },
                new () { Alias = "hideFromXMLSitemap", Value = true },
            ]
        );

    private async Task<Guid> CreateCategoryAsync(ApiClient apiClient, Guid categoryListId, string name)
        => await CreateAsync(
            apiClient,
            categoryListId,
            DocumentTypeNames.Category,
            null,
            name,
            []
        );

    #endregion
    
    #region Perform document updates
    
    private async Task UpdateAsync(ApiClient apiClient, Guid id, string template, string name, string? title, string? subTitle, BlockListValue contentRowsValue, string mainImage, string? metaDescription = null)
        => await UpdateAsync(
            apiClient,
            id,
            template,
            name,
            [
                new() { Alias = "contentRows", Value = contentRowsValue },
                new() { Alias = "title", Value = title },
                new() { Alias = "subtitle", Value = subTitle },
                new()
                {
                    Alias = "mainImage",
                    Value = new MediaPickerValue[] { new() { MediaKey = SampleImageId(mainImage) } }
                },
                new() { Alias = "isIndexable", Value = true },
                new() { Alias = "isFollowable", Value = true },
                new() { Alias = "metaDescription", Value = metaDescription }
            ]);
    
    private async Task UpdateAsync(ApiClient apiClient, Guid id, string template, string name, ICollection<DocumentValueModel> values)
        => await apiClient.PutDocumentByIdAsync(
            id,
            new()
            {
                Variants =
                [
                    new() { Name = name }
                ],
                Values = values,
                Template = new () { Id = TemplateId(template) }
            }
        );

    #endregion

    #region Perform document creation
    
    private async Task<Guid> CreateAsync(ApiClient apiClient, Guid parentId, string documentType, string? template, string name, string? title, string? subTitle, BlockListValue contentRowsValue, string mainImage, string? metaDescription = null)
        => await CreateAsync(
            apiClient,
            parentId,
            documentType,
            template,
            name,
            title,
            subTitle,
            contentRowsValue,
            SampleImageId(mainImage),
            metaDescription
        );

    private async Task<Guid> CreateAsync(ApiClient apiClient, Guid parentId, string documentType, string? template, string name, string? title, string? subTitle, BlockListValue contentRowsValue, Guid mainImageId, string? metaDescription = null)
        => await CreateAsync(
            apiClient,
            parentId,
            documentType,
            template,
            name,
            [
                new() { Alias = "contentRows", Value = contentRowsValue },
                new() { Alias = "title", Value = title },
                new() { Alias = "subtitle", Value = subTitle },
                new()
                {
                    Alias = "mainImage",
                    Value = new MediaPickerValue[] { new() { MediaKey = mainImageId } }
                },
                new() { Alias = "isIndexable", Value = true },
                new() { Alias = "isFollowable", Value = true },
                new() { Alias = "metaDescription", Value = metaDescription }
            ]);

    private async Task<Guid> CreateAsync(ApiClient apiClient, Guid? parentId, string documentType, string? template, string name, ICollection<DocumentValueModel> values)
    {
        var id = Guid.NewGuid();
        await apiClient.PostDocumentAsync(
            new()
            {
                Id = id,
                Parent = parentId.HasValue ? new() { Id = parentId.Value } : null,
                DocumentType = new() { Id = DocumentTypeId(documentType) },
                Template = template is not null ? new() { Id = TemplateId(template) } : null,
                Variants =
                [
                    new() { Name = name }
                ],
                Values = values
            }
        );

        return id;
    }

    #endregion

    #region Block list building
    
    private void AddRichTextRow(BlockListValue blockListValue, string markup)
        => blockListValue.AddRichTextRow(
            markup,
            ContentElementTypeId(DocumentTypeNames.ContentElements.RichTextRow),
            SettingsElementTypeId(DocumentTypeNames.SettingsElements.RichTextRow));

    private void AddImageRow(BlockListValue blockListValue, string image, string caption)
        => blockListValue.AddImageRow(
            image,
            caption,
            SampleImageId(image),
            ContentElementTypeId(DocumentTypeNames.ContentElements.ImageRow),
            SettingsElementTypeId(DocumentTypeNames.SettingsElements.ImageRow));
    
    private void AddVideoRow(BlockListValue blockListValue, string videoUrl, string caption)
        => blockListValue.AddVideoRow(
            videoUrl,
            caption,
            ContentElementTypeId(DocumentTypeNames.ContentElements.VideoRow),
            SettingsElementTypeId(DocumentTypeNames.SettingsElements.VideoRow));

    private void AddCodeSnippetRow(BlockListValue blockListValue, string title, string code)
        => blockListValue.AddCodeSnippetRow(
            title,
            code,
            ContentElementTypeId(DocumentTypeNames.ContentElements.CodeSnippetRow),
            SettingsElementTypeId(DocumentTypeNames.SettingsElements.CodeSnippetRow));

    private void AddImageCarouselRow(BlockListValue blockListValue, IEnumerable<string> images)
        => blockListValue.AddImageCarouselRow(
            images.Select(SampleImageId),
            ContentElementTypeId(DocumentTypeNames.ContentElements.ImageCarouselRow),
            SettingsElementTypeId(DocumentTypeNames.SettingsElements.LatestArticlesRow));

    private void AddLatestArticlesRow(BlockListValue blockListValue, int pageSize, bool showPagination)
        => blockListValue.AddLatestArticlesRow(
            pageSize,
            showPagination,
            DocumentId(DocumentNames.Blog),
            ContentElementTypeId(DocumentTypeNames.ContentElements.LatestArticlesRow),
            SettingsElementTypeId(DocumentTypeNames.SettingsElements.LatestArticlesRow));

    #endregion
    
    #region Shorthands for getting various IDs from related items
    
    private Guid SocialIconId(string name) => MediaId(name, _socialIcons);

    private Guid SampleImageId(string name) => MediaId(name, _sampleImages);

    private Guid AuthorImageId(string name) => MediaId(name, _authorImages);

    private Guid MediaId(string name, Dictionary<string, Guid>? candidates)
        => candidates?.TryGetValue(name, out Guid id) is true ? id : throw new InvalidOperationException($"The media could not be found: {name}");

    private Guid ContentElementTypeId(string name) => ElementTypeId(name, _contentElementTypes);

    private Guid SettingsElementTypeId(string name) => ElementTypeId(name, _settingsElementTypes);

    private Guid ElementTypeId(string name, Dictionary<string, Guid>? candidates)
        => candidates?.TryGetValue(name, out Guid id) is true ? id : throw new InvalidOperationException($"The element type not be found: {name}");
 
    private Guid DocumentTypeId(string name) => _documentTypes?.TryGetValue(name, out Guid id) is true
            ? id
            : throw new InvalidOperationException($"Could not find document type: {name}");

    private Guid BlogDocumentId(string name) => DocumentId(BlogDocumentKey(name));

    private Guid CategoryDocumentId(string name) => DocumentId(CategoryDocumentKey(name));

    private Guid AuthorDocumentId(string name) => DocumentId(AuthorDocumentKey(name));
    
    private Guid DocumentId(string name)
        => _documents.TryGetValue(name, out Guid id) ? id : throw new InvalidOperationException($"The document not be found: {name}");
 
    private Guid TemplateId(string name) => _templates?.TryGetValue(name, out Guid id) is true
        ? id
        : throw new InvalidOperationException($"The template not be found: {name}");

    private static string BlogDocumentKey(string name) => $"Blog:{name}";

    private static string CategoryDocumentKey(string name) => $"Categories:{name}";

    private static string AuthorDocumentKey(string name) => $"Authors:{name}";

    #endregion
    
    private static class DocumentNames
    {
        public const string Home = "Home";
        public const string Blog = "Blog";
        public const string BlogCommunity = "Community";
        public const string BlogPopularBlogs = "Popular blogs";
        public const string BlogMeetups = "Meetups";
        public const string BlogConferences = "Conferences";
        public const string BlogPodcastsAndVideos = "Podcasts and Videos";
        public const string BlogYouTubeTutorials = "YouTube Tutorials";
        public const string BlogJoinTheUmbracoCommunityOnMastodon = "Join the Umbraco Community on Mastodon";
        public const string AuthorsPaulSeal = "Paul Seal";
        public const string CategoriesCommunity = "Community";
        public const string CategoriesConferences = "Conferences";
        public const string CategoriesMeetups = "Meetups";
        public const string CategoriesPodcasts = "Podcasts";
        public const string CategoriesResources = "Resources";
        public const string CategoriesUmbraco = "Umbraco";
        public const string CategoriesVideos = "Videos";
    }
}