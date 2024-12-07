namespace Builder.Models.DataType;

public class DynamicRoot
{
    public required string OriginAlias { get; init; }

    public required Guid OriginKey { get; init; }

    public required DynamicRootStep[] QuerySteps { get; init; }
}