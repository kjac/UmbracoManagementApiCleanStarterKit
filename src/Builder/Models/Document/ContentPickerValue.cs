namespace Builder.Models.Document;

public class ContentPickerValue
{
    // the starter kit only ever picks documents in content pickers, so let's hardcode that here for simplicity.
    public string Type => "document";

    public required Guid Unique { get; init; }
}