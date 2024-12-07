namespace Builder.Models.Document;

public class MediaPickerValue
{

    // the model requires a Key property. as we're only creating content here, we don't need to
    // keep track of it, so just use a new Guid.
    public Guid Key => Guid.NewGuid();

    public required Guid MediaKey { get; init; }
}
