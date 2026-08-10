using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Company.AI.Tooling.Loading;
using Json.Schema;

namespace Company.AI.Tooling.Validation;

/// <summary>
/// Fetches and runs the canonical Agent Plugins schemas selected by each document's $schema URL.
/// </summary>
internal sealed class SchemaValidator(RepositoryContext context)
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    // JsonSchema.Net registers a built schema globally under its $id and refuses to register the
    // same $id twice, so the cache outlives a single validator instance. Lazy guarantees the
    // factory runs once even when several validators race for the same schema.
    private static readonly ConcurrentDictionary<string, Lazy<JsonSchema>> Cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Validates a document against the schema its own <c>$schema</c> selects.
    /// Returns the declared schema identifier, or null when it was missing or unsupported.
    /// </summary>
    public string? Validate(JsonObject document, string path, string expectedSchemaUrl)
    {
        var relative = context.Relative(path);
        var declared = document["$schema"]?.GetValue<string>();

        if (declared is null)
        {
            context.Diagnostics.SpecFatal(relative, "must declare \"$schema\".");
            return null;
        }

        if (declared != expectedSchemaUrl)
        {
            if (!AgentPluginSpec.SupportedSchemaUrls.Contains(declared))
            {
                context.Diagnostics.SpecFatal(
                    relative,
                    $"declares unsupported schema '{declared}'. Supported: {expectedSchemaUrl}.");
                return null;
            }

            context.Diagnostics.SpecFatal(
                relative,
                $"declares '{declared}' but this document must use '{expectedSchemaUrl}'.");
            return null;
        }

        var schema = Load(declared);
        var results = schema.Evaluate(document.Deserialize<JsonElement>(), new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });

        if (results.IsValid)
        {
            return declared;
        }

        foreach (var detail in Flatten(results).Where(d => d.Errors is { Count: > 0 }))
        {
            foreach (var error in detail.Errors!)
            {
                var location = detail.InstanceLocation.ToString();
                var where = string.IsNullOrEmpty(location) ? "document root" : location;

                context.Diagnostics.SpecFatal(relative, $"schema violation at {where}: {error.Value}");
            }
        }

        return declared;
    }

    private static IEnumerable<EvaluationResults> Flatten(EvaluationResults results)
    {
        yield return results;

        foreach (var nested in (results.Details ?? []).SelectMany(Flatten))
        {
            yield return nested;
        }
    }

    private static JsonSchema Load(string schemaUrl)
    {
        return Cache.GetOrAdd(
            schemaUrl,
            url => new Lazy<JsonSchema>(
                () => Fetch(url),
                LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;
    }

    private static JsonSchema Fetch(string schemaUrl)
    {
        try
        {
            var content = Http.GetStringAsync(schemaUrl).GetAwaiter().GetResult();
            return JsonSchema.FromText(content);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new FatalToolingException($"Could not load official schema '{schemaUrl}': {ex.Message}");
        }
    }
}
