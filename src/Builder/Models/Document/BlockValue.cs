namespace Builder.Models.Document;

public abstract class BlockValue
{
    protected abstract string Alias { get; }
    
    public Dictionary<string, List<BlockListLayout>> Layout { get; } = new();

    public List<BlockListItem> ContentData { get; } = new();

    public List<BlockListItem> SettingsData { get; } = new();

    public List<BlockListItemExpose> Expose { get; } = new();
    
    public void Add(Guid contentTypeKey, IEnumerable<BlockListItemValue> contentValues, Guid? settingsTypeKey = null, IEnumerable<BlockListItemValue>? settingsValues = null)
    {
        if (Layout.ContainsKey(Alias) is false)
        {
            Layout[Alias] = [];
        }

        var contentKey = Guid.NewGuid();
        var settingsKey = settingsTypeKey.HasValue && settingsValues != null ? Guid.NewGuid() : (Guid?)null;

        Layout[Alias].Add(new BlockListLayout { ContentKey = contentKey, SettingsKey = settingsKey });

        ContentData.Add(new BlockListItem
        {
            ContentTypeKey = contentTypeKey,
            Key = contentKey,
            Values = contentValues
        });

        if (settingsKey.HasValue)
        {
            SettingsData.Add(new BlockListItem
            {
                ContentTypeKey = settingsTypeKey!.Value,
                Key = settingsKey.Value,
                Values = settingsValues!
            });
        }

        Expose.Add(new() { ContentKey = contentKey });
    }
}