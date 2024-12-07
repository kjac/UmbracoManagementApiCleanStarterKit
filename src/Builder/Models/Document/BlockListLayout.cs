namespace Builder.Models.Document;

public class BlockListLayout
{
    public required Guid ContentKey { get; init; }

    public Guid? SettingsKey { get; init; }
}