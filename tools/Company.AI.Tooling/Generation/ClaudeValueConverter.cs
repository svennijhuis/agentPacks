using System.Text.Json.Nodes;

namespace Company.AI.Tooling.Generation;

/// <summary>
/// Translates portable MCP configuration into Claude's dialect, field by field.
/// <para>
/// The specification expands <c>${PLUGIN_ROOT}</c> and <c>${PLUGIN_DATA}</c> in exactly three
/// places — <c>args</c>, <c>env</c> values and <c>cwd</c> — and explicitly not in environment
/// keys, <c>command</c>, remote URLs or HTTP headers. Rewriting every string that happens to look
/// like a path would corrupt opaque arguments, header values and query strings, so this converter
/// only touches fields whose semantics require it.
/// </para>
/// </summary>
internal static class ClaudeValueConverter
{
    private const string PluginRoot = "${PLUGIN_ROOT}";
    private const string PluginData = "${PLUGIN_DATA}";
    private const string ClaudePluginRoot = "${CLAUDE_PLUGIN_ROOT}";
    private const string ClaudePluginData = "${CLAUDE_PLUGIN_DATA}";

    public static JsonObject ConvertServers(JsonObject servers)
    {
        var result = new JsonObject();

        foreach (var entry in servers.OrderBy(s => s.Key, StringComparer.Ordinal))
        {
            result[entry.Key] = entry.Value is JsonObject server
                ? ConvertServer(server)
                : entry.Value?.DeepClone();
        }

        return result;
    }

    private static JsonObject ConvertServer(JsonObject server)
    {
        var result = new JsonObject();

        foreach (var property in server)
        {
            result[property.Key] = property.Key switch
            {
                "type" => ConvertTransport(property.Value),
                "command" => ConvertCommand(property.Value),
                "args" => ConvertArray(property.Value),
                "env" => ConvertEnvironment(property.Value),
                "cwd" => ConvertPath(property.Value),

                // url and headers are carried across untouched: no expansion applies to them.
                _ => property.Value?.DeepClone()
            };
        }

        return result;
    }

    /// <summary>Claude accepts both, but "http" is the value its own configuration uses.</summary>
    private static JsonNode? ConvertTransport(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text) && text == "streamable-http"
            ? JsonValue.Create("http")
            : node?.DeepClone();

    /// <summary>
    /// No placeholder expansion applies to a command. A plugin-relative "./x" is still rewritten so
    /// Claude resolves it against the installed plugin; a bare executable name is left alone.
    /// </summary>
    private static JsonNode? ConvertCommand(JsonNode? node)
    {
        if (node is not JsonValue value || !value.TryGetValue<string>(out var command))
        {
            return node?.DeepClone();
        }

        return command.StartsWith("./", StringComparison.Ordinal)
            ? JsonValue.Create($"{ClaudePluginRoot}/{command[2..]}")
            : JsonValue.Create(command);
    }

    private static JsonNode? ConvertArray(JsonNode? node)
    {
        if (node is not JsonArray array)
        {
            return node?.DeepClone();
        }

        var result = new JsonArray();

        foreach (var item in array)
        {
            result.Add(ExpandPlaceholders(item));
        }

        return result;
    }

    /// <summary>Values are expanded; keys never are.</summary>
    private static JsonNode? ConvertEnvironment(JsonNode? node)
    {
        if (node is not JsonObject env)
        {
            return node?.DeepClone();
        }

        var result = new JsonObject();

        foreach (var variable in env)
        {
            result[variable.Key] = ExpandPlaceholders(variable.Value);
        }

        return result;
    }

    private static JsonNode? ConvertPath(JsonNode? node)
    {
        if (node is not JsonValue value || !value.TryGetValue<string>(out var path))
        {
            return node?.DeepClone();
        }

        return path.StartsWith("./", StringComparison.Ordinal)
            ? JsonValue.Create($"{ClaudePluginRoot}/{path[2..]}")
            : JsonValue.Create(Expand(path));
    }

    private static JsonNode? ExpandPlaceholders(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text)
            ? JsonValue.Create(Expand(text))
            : node?.DeepClone();

    private static string Expand(string value) => value
        .Replace(PluginRoot, ClaudePluginRoot, StringComparison.Ordinal)
        .Replace(PluginData, ClaudePluginData, StringComparison.Ordinal);
}
