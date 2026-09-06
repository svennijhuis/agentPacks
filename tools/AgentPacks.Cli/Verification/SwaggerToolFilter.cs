using System.Text.Json.Nodes;

namespace AgentPacks.Cli.Verification;

/// <summary>
/// Tiny swagger→MCP example: GET operations on an allowlisted tag, capped.
/// Not a generator and not one tool per endpoint.
/// </summary>
public static class SwaggerToolFilter
{
    public const int MaxTools = 8;

    public static IReadOnlyList<string> SelectGetTools(
        string openApiJson,
        IReadOnlyCollection<string> allowedTags)
    {
        var allowed = allowedTags.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var root = JsonNode.Parse(openApiJson) as JsonObject
            ?? throw new InvalidOperationException("OpenAPI document must be a JSON object.");
        var paths = root["paths"] as JsonObject
            ?? throw new InvalidOperationException("OpenAPI document must have a paths object.");

        var tools = new List<string>();
        foreach (var path in paths)
        {
            if (path.Value is not JsonObject operations)
                continue;
            if (operations["get"] is not JsonObject get)
                continue;

            var tags = (get["tags"] as JsonArray)?
                .Select(node => node?.GetValue<string>())
                .Where(tag => tag is not null)
                .Cast<string>()
                .ToList() ?? [];

            if (tags.Count == 0 || !tags.Any(allowed.Contains))
                continue;

            var name = get["operationId"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            tools.Add(name);
            if (tools.Count == MaxTools)
                break;
        }

        return tools;
    }
}
