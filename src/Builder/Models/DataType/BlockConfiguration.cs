namespace Builder.Models.DataType;

public class BlockConfiguration
{
    public required Guid ContentElementTypeKey { get; init; }

    public required Guid? SettingsElementTypeKey { get; init; }
}