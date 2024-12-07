namespace Builder.Models.DataType;

public class ContentPickerConfiguration
{
    // the starter kit only ever uses "content" (documents) as content picker type, so let's hardcode that here for simplicity. 
    public string Type => "content";

    public required DynamicRoot DynamicRoot { get; init; }
}