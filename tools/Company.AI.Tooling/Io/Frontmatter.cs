using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Company.AI.Tooling.Io;

/// <summary>
/// A parsed YAML frontmatter block. Backed by a real YAML parser, so quoted values containing
/// colons, multiline scalars, comments, nested maps and duplicate keys all behave correctly.
/// </summary>
internal sealed class Frontmatter
{
    private readonly YamlMappingNode _root;

    private Frontmatter(YamlMappingNode root, string body)
    {
        _root = root;
        Body = body;
    }

    /// <summary>Markdown following the closing delimiter.</summary>
    public string Body { get; }

    public IEnumerable<string> Keys => _root.Children.Keys.OfType<YamlScalarNode>().Select(k => k.Value ?? string.Empty);

    public static Frontmatter? TryParse(string text, out string? error)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);

        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            error = "must start with a YAML frontmatter block delimited by ---.";
            return null;
        }

        var end = normalized.IndexOf("\n---", 3, StringComparison.Ordinal);

        if (end < 0)
        {
            error = "has an unterminated YAML frontmatter block.";
            return null;
        }

        var yaml = normalized[4..(end + 1)];
        var afterDelimiter = end + 4;
        var body = afterDelimiter < normalized.Length ? normalized[afterDelimiter..] : string.Empty;

        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));

            if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode mapping)
            {
                error = "frontmatter must be a YAML mapping.";
                return null;
            }

            error = null;
            return new Frontmatter(mapping, body.TrimStart('\n'));
        }
        catch (YamlException ex)
        {
            error = $"has invalid YAML frontmatter: {ex.Message}";
            return null;
        }
    }

    /// <summary>Scalar value for a key, or null when absent or not a scalar.</summary>
    public string? Scalar(string key) =>
        Node(key) is YamlScalarNode scalar ? scalar.Value : null;

    public YamlNode? Node(string key) =>
        _root.Children.TryGetValue(new YamlScalarNode(key), out var value) ? value : null;

    public bool Has(string key) => Node(key) is not null;

    /// <summary>True when the value is a mapping whose keys and values are all scalars.</summary>
    public bool IsStringMap(string key)
    {
        if (Node(key) is not YamlMappingNode mapping)
        {
            return false;
        }

        return mapping.Children.All(pair =>
            pair.Key is YamlScalarNode { Value: not null } &&
            pair.Value is YamlScalarNode { Value: not null });
    }
}
