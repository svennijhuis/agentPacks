using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// A generated file, identified by its repository-relative path. Text is the authority: every
/// generator produces the exact bytes to write, so --check is a single string comparison whether
/// the output is JSON, Markdown, TOML or a shell script.
/// </summary>
internal sealed record GeneratedFile(string RelativePath, string Text, bool Executable = false)
{
    /// <summary>The parsed document for JSON output, so validators can inspect it without reparsing.</summary>
    public JsonNode? Json { get; private init; }

    /// <summary>The JSON document. Only valid for files produced by <see cref="FromJson"/>.</summary>
    public JsonNode Content => Json
        ?? throw new InvalidOperationException($"{RelativePath} is a text file, not a JSON document.");

    public static GeneratedFile FromJson(string relativePath, JsonNode content) =>
        new(relativePath, JsonFile.Serialize(content)) { Json = content };
}
