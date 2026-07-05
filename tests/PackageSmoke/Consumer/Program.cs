using System.Text.Json;
using MergePatch;

var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>(
    """{ "title": "Updated title", "description": null }""",
    options) ?? throw new InvalidOperationException("Patch deserialization returned null.");

Require(patch.Has.Title, "Generated Has.Title should be available and true.");
Require(patch.Has.Description, "Generated Has.Description should be available and true.");
Require(!patch.Has.InternalNote, "Generated Has.InternalNote should be available and false.");
Require(patch.Description is null, "Explicit null should be preserved.");

var document = new Document
{
    Title = "Original title",
    Description = "Original description",
    InternalNote = "Keep this value"
};

patch.ApplyTo(document);

Require(document.Title == "Updated title", "Generated ApplyTo should assign provided values.");
Require(document.Description is null, "Generated ApplyTo should assign explicit null values.");
Require(document.InternalNote == "Keep this value", "Generated ApplyTo should skip missing values.");

Console.WriteLine("Package smoke consumer passed.");

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

public sealed class Document
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? InternalNote { get; set; }
}

[MergePatch(typeof(Document))]
public partial class UpdateDocumentPatch
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? InternalNote { get; set; }
}
