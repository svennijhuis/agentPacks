using System.Text.Json.Nodes;
using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Loading;

/// <summary>
/// Authored model tiers mapped to the identifier each client actually accepts.
/// <para>
/// Agents author a portable tier — <c>inherit</c>, <c>fast</c>, <c>standard</c> or
/// <c>frontier</c> — never a Claude-only alias such as <c>sonnet</c>. Generation emits the
/// client column. Codex emits the catalog id into generated agent TOML <c>model =</c>.
/// </para>
/// </summary>
internal sealed class ModelCatalog
{
    public const string FileName = "models.source.json";
    public const string SchemaPath = "schema/models.schema.json";
    public const string DefaultTier = "inherit";

    public static readonly IReadOnlyList<string> PortableTiers =
        ["inherit", "fast", "standard", "frontier"];

    private static readonly IReadOnlySet<string> ClaudeOnlyAliases =
        new HashSet<string>(StringComparer.Ordinal) { "opus", "sonnet", "haiku" };

    private readonly Dictionary<string, ClientModels> _tiers;

    public ModelCatalog(IReadOnlyDictionary<string, ClientModels> tiers)
    {
        _tiers = new Dictionary<string, ClientModels>(tiers, StringComparer.Ordinal);
    }

    /// <summary>The catalog this repository ships, used when a test repo has no file.</summary>
    public static ModelCatalog BuiltIn { get; } = new(new Dictionary<string, ClientModels>(StringComparer.Ordinal)
    {
        ["inherit"] = new("inherit", "inherit", "inherit", "inherit"),
        ["fast"] = new("haiku", "composer-2", "gpt-4.1", "gpt-5.6-luna"),
        ["standard"] = new("sonnet", "grok-4.5", "gpt-5", "gpt-5.6-terra"),
        ["frontier"] = new("opus", "claude-opus-5", "gpt-5", "gpt-5.6-sol")
    });

    public IReadOnlyCollection<string> TierNames => _tiers.Keys;

    public static bool IsClaudeOnlyAlias(string? model) =>
        model is not null && ClaudeOnlyAliases.Contains(model);

    public static bool IsPortableTier(string? model) =>
        model is not null && PortableTiers.Contains(model);

    /// <summary>
    /// Loads <see cref="FileName"/> from the repository root when present. A missing file is not
    /// an error — fixture repositories use <see cref="BuiltIn"/> — but a present file that cannot
    /// be parsed or that omits <c>inherit</c> is.
    /// </summary>
    public static ModelCatalog Load(RepositoryContext context)
    {
        var path = Path.Combine(context.Root, FileName);

        if (!File.Exists(path))
        {
            return BuiltIn;
        }

        var node = JsonFile.TryRead(path, out var error);
        var relative = context.Relative(path);

        if (node is not JsonObject document)
        {
            context.Diagnostics.SpecFatal(relative, error ?? "must be a JSON object.");
            return BuiltIn;
        }

        if (document["$schema"]?.GetValue<string>() != SchemaPath)
        {
            context.Diagnostics.SpecFatal(relative, $"must declare \"$schema\": \"{SchemaPath}\".");
        }

        if (document["version"] is not JsonValue version ||
            !version.TryGetValue<int>(out var value) || value != 1)
        {
            context.Diagnostics.SpecFatal(relative, "must declare integer 'version': 1.");
        }

        if (document["default"]?.GetValue<string>() != DefaultTier)
        {
            context.Diagnostics.SpecFatal(relative, $"must declare \"default\": \"{DefaultTier}\".");
        }

        if (document["tiers"] is not JsonObject tiers || tiers.Count == 0)
        {
            context.Diagnostics.SpecFatal(relative, "must define a non-empty 'tiers' object.");
            return BuiltIn;
        }

        var mapped = new Dictionary<string, ClientModels>(StringComparer.Ordinal);

        foreach (var (tier, entry) in tiers.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (entry is not JsonObject clients)
            {
                context.Diagnostics.SpecFatal(relative, $"tier '{tier}' must map to a client object.");
                continue;
            }

            var claude = clients["claude"]?.GetValue<string>();
            var cursor = clients["cursor"]?.GetValue<string>();
            var copilot = clients["copilot"]?.GetValue<string>();
            var codex = clients["codex"]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(claude) ||
                string.IsNullOrWhiteSpace(cursor) ||
                string.IsNullOrWhiteSpace(copilot) ||
                string.IsNullOrWhiteSpace(codex))
            {
                context.Diagnostics.SpecFatal(
                    relative,
                    $"tier '{tier}' must define non-empty claude, cursor, copilot and codex ids.");
                continue;
            }

            if (IsClaudeOnlyAlias(cursor))
            {
                context.Diagnostics.Policy(
                    relative,
                    $"tier '{tier}' maps Cursor to '{cursor}', which is a Claude alias. " +
                    "Cursor needs a real Cursor id such as inherit, composer-2, grok-4.5 or claude-opus-5.");
            }

            mapped[tier] = new ClientModels(claude, cursor, copilot, codex);
        }

        if (!mapped.ContainsKey(DefaultTier))
        {
            context.Diagnostics.SpecFatal(relative, $"must define the '{DefaultTier}' tier.");
            return BuiltIn;
        }

        return mapped.Count == 0 ? BuiltIn : new ModelCatalog(mapped);
    }

    /// <summary>Client identifier for an authored tier. Missing or unknown tiers become inherit.</summary>
    public string Resolve(string? authored, Client client)
    {
        var tier = string.IsNullOrWhiteSpace(authored) ? DefaultTier : authored;
        var models = _tiers.TryGetValue(tier, out var mapped) ? mapped : _tiers[DefaultTier];

        return client switch
        {
            Client.Claude => models.Claude,
            Client.Cursor => models.Cursor,
            Client.Copilot => models.Copilot,
            Client.Codex => models.Codex,
            _ => DefaultTier
        };
    }
}

/// <summary>One portable tier's identifier on each generated client.</summary>
internal sealed record ClientModels(string Claude, string Cursor, string Copilot, string Codex);
