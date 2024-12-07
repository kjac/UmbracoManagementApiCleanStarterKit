namespace Builder.Models.Document;

public class BlockListItem
{
    public required Guid ContentTypeKey { get; init; }

    public required Guid Key { get; init; }

    public required IEnumerable<BlockListItemValue> Values { get; init; }
}