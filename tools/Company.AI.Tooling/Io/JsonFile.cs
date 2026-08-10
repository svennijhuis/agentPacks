using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;

namespace Company.AI.Tooling.Io;

internal static class JsonFile
{
    // Generated files are read by humans during review, so keep apostrophes, angle brackets and
    // ampersands as themselves rather than '-style escapes. The output is still valid JSON.
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    /// <summary>
    /// Parses a JSON file. A malformed document returns null and reports the reason, so the caller
    /// can carry on with the remaining plugins rather than aborting the run.
    /// </summary>
    public static JsonNode? TryRead(string path, out string? error)
    {
        try
        {
            var node = JsonNode.Parse(File.ReadAllText(path), documentOptions: ParseOptions);

            if (node is null)
            {
                error = "file contains JSON null, expected an object.";
                return null;
            }

            error = null;
            return node;
        }
        catch (JsonException ex)
        {
            error = $"is not valid JSON: {ex.Message}";
            return null;
        }
        catch (IOException ex)
        {
            error = $"could not be read: {ex.Message}";
            return null;
        }
    }

    /// <summary>Canonical serialization: indented, trailing newline. Used for writing and for --check.</summary>
    public static string Serialize(JsonNode node) => node.ToJsonString(WriteOptions) + Environment.NewLine;

    public static void Write(string path, JsonNode node)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(node));
    }
}
