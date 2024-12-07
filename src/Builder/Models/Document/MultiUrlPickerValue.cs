namespace Builder.Models.Document;

public class MultiUrlPickerValue
{
    public string? Url { get; init; }

    public string? Target { get; init; }

    public string? Name { get; init; }

    // the starter kit only ever uses external links in multi URL pickers, so let's hardcode that here for simplicity.
    public string Type => "external";
}
