using System.Text;
using AgentPacks.Cli.Loading;
using YamlDotNet.RepresentationModel;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Renders neutral components into the per-client file formats. Frontmatter is rebuilt key by key
/// rather than copied: a key a client does not understand is dropped, so nothing authored can leak
/// into a dialect that would ignore or misread it.
/// </summary>
internal static class ComponentWriter
{
    /// <summary>A Markdown file with YAML frontmatter, in a stable key order.</summary>
    public static string Markdown(IEnumerable<KeyValuePair<string, string>> frontmatter, string body)
    {
        var builder = new StringBuilder();

        builder.Append("---\n");

        foreach (var (key, value) in frontmatter)
        {
            builder.Append(key).Append(": ").Append(value).Append('\n');
        }

        builder.Append("---\n\n").Append(body.TrimEnd('\n')).Append('\n');

        return builder.ToString();
    }

    /// <summary>Quotes a scalar so a colon, a hash or a leading dash cannot change the YAML shape.</summary>
    public static string Yaml(string value) =>
        "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    /// <summary>A YAML inline sequence, e.g. tools: [read, grep].</summary>
    public static string YamlList(IEnumerable<string> values) =>
        "[" + string.Join(", ", values.Select(Yaml)) + "]";

    /// <summary>Values of a sequence-valued frontmatter key, empty when absent or not a sequence.</summary>
    public static IReadOnlyList<string> Sequence(MarkdownComponent component, string key) =>
        component.Frontmatter?.Node(key) is YamlSequenceNode sequence
            ? sequence.Children.OfType<YamlScalarNode>().Select(n => n.Value ?? string.Empty).ToList()
            : [];

    public static bool Flag(MarkdownComponent component, string key) =>
        component.Frontmatter?.Scalar(key) == "true";

    /// <summary>A TOML document for a Codex agent. Only string values, so escaping stays simple.</summary>
    public static string Toml(IEnumerable<KeyValuePair<string, string>> values, string multilineKey, string multilineValue)
    {
        var builder = new StringBuilder();

        foreach (var (key, value) in values)
        {
            builder.Append(key).Append(" = ").Append(TomlString(value)).Append('\n');
        }

        builder.Append('\n')
            .Append(multilineKey)
            .Append(" = \"\"\"\n")
            .Append(multilineValue.TrimEnd('\n').Replace("\"\"\"", "\\\"\\\"\\\"", StringComparison.Ordinal))
            .Append("\n\"\"\"\n");

        return builder.ToString();
    }

    private static string TomlString(string value) =>
        "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("\"", "\\\"", StringComparison.Ordinal)
                    .Replace("\n", "\\n", StringComparison.Ordinal) + "\"";
}
