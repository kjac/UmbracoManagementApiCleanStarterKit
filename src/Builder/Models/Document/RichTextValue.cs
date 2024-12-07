namespace Builder.Models.Document;

public class RichTextValue
{
    public required string Markup { get; init; }

    public RichTextBlockValue Blocks => new();
}
