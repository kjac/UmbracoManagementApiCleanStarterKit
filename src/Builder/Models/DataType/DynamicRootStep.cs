namespace Builder.Models.DataType;

public class DynamicRootStep
{
    public required string Alias { get; init; }

    public required Guid[] AnyOfDocTypeKeys { get; init; }
}