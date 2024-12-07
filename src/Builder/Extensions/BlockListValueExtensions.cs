using Builder.Models.Document;

namespace Builder.Extensions;

public static class BlockListValueExtensions
{
    public static void AddRichTextRow(this BlockListValue blockListValue, string markup, Guid contentTypeId, Guid settingsTypeId)
        => blockListValue.Add(
            contentTypeId,
            [
                new () { Alias = "content", Value = new RichTextValue { Markup = markup } }
            ],
            settingsTypeId,
            [
                new () { Alias = "hide", Value = false }
            ]
        );

    public static void AddImageRow(this BlockListValue blockListValue, string image, string caption, Guid imageId, Guid contentTypeId, Guid settingsTypeId)
        => blockListValue.Add(
            contentTypeId,
            [
                new ()
                {
                    Alias = "image",
                    Value = new MediaPickerValue[] { new () { MediaKey = imageId } }
                },
                new () { Alias = "caption", Value = caption }
            ],
            settingsTypeId,
            [
                new () { Alias = "hide", Value = false }
            ]
        );

    public static void AddVideoRow(this BlockListValue blockListValue, string videoUrl, string caption, Guid contentTypeId, Guid settingsTypeId)
        => blockListValue.Add(
            contentTypeId,
            [
                new () { Alias = "videoUrl", Value = videoUrl },
                new () { Alias = "caption", Value = caption }
            ],
            settingsTypeId,
            [
                new () { Alias = "hide", Value = false }
            ]
        );

    public static void AddCodeSnippetRow(this BlockListValue blockListValue, string title, string code, Guid contentTypeId, Guid settingsTypeId)
        => blockListValue.Add(
            contentTypeId,
            [
                new () { Alias = "title", Value = title },
                new () { Alias = "code", Value = code }
            ],
            settingsTypeId,
            [
                new () { Alias = "hide", Value = false }
            ]
        );

    public static void AddImageCarouselRow(this BlockListValue blockListValue, IEnumerable<Guid> imageIds, Guid contentTypeId, Guid settingsTypeId)
        => blockListValue.Add(
            contentTypeId,
            [
                
                new ()
                {
                    Alias = "images",
                    Value = imageIds.Select(imageId => new MediaPickerValue { MediaKey = imageId }).ToArray() 
                }
            ],
            settingsTypeId,
            [
                new () { Alias = "hide", Value = false }
            ]
        );

    public static void AddLatestArticlesRow(this BlockListValue blockListValue, int pageSize, bool showPagination, Guid articleListId, Guid contentTypeId, Guid settingsTypeId)
        => blockListValue.Add(
            contentTypeId,
            [
                new () { Alias = "articleList", Value = articleListId },
                new () { Alias = "pageSize", Value = pageSize },
                new () { Alias = "showPagination", Value = showPagination }
            ],
            settingsTypeId,
            [
                new () { Alias = "hide", Value = false }
            ]
        );
}