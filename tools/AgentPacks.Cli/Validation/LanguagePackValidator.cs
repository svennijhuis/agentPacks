using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Validation;

/// <summary>
/// Enforces the contract a language pack fills for the delivery loop.
/// <para>
/// The loop's agents load a language pack's skills by name — <c>dotnet-build</c>,
/// <c>dotnet-test-patterns</c> and the rest — so the names are the entire interface. A skill called
/// <c>dotnet-testing</c> is not a broken skill: it loads, it validates, it appears in the
/// marketplace, and the loop simply never asks for it. Nothing downstream notices, which is why the
/// near-miss is checked here rather than left to review.
/// </para>
/// <para>
/// A pack opts in with the <c>language-pack</c> keyword in its manifest. Without it a plugin is a
/// role or capability pack and none of this applies.
/// </para>
/// </summary>
internal sealed class LanguagePackValidator(RepositoryContext context)
{
    /// <summary>
    /// How far a suffix may drift before it is treated as a typo rather than a different skill.
    /// Two edits catches 'test-pattern', 'reviews' and 'builds' while leaving genuinely different
    /// names such as 'error-handling' alone.
    /// </summary>
    private const int NearMissDistance = 2;

    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        foreach (var plugin in plugins.Where(IsLanguagePack))
        {
            Validate(plugin);
        }
    }

    /// <summary>
    /// Reads the opt-in keyword without assuming the manifest is well formed. This runs before
    /// <see cref="PluginValidator"/>'s findings are acted on, so a keywords array holding a number
    /// still reaches here — and a crash would replace that plugin's real diagnostic with a stack
    /// trace.
    /// </summary>
    private static bool IsLanguagePack(PluginPackage plugin) =>
        plugin.Manifest?["keywords"] is JsonArray keywords &&
        keywords.Any(k =>
            k is JsonValue value &&
            value.TryGetValue<string>(out var keyword) &&
            string.Equals(keyword, LanguagePackContract.Keyword, StringComparison.Ordinal));

    private void Validate(PluginPackage plugin)
    {
        var language = plugin.Name ?? plugin.DirectoryName;
        var manifest = context.Relative(plugin.ManifestPath);
        var names = plugin.Skills.Select(s => s.DirectoryName).ToHashSet(StringComparer.Ordinal);

        var missing = LanguagePackContract.RequiredSlots
            .Select(slot => LanguagePackContract.SkillName(language, slot))
            .Where(skill => !names.Contains(skill))
            .ToList();

        if (missing.Count > 0)
        {
            context.Diagnostics.Policy(
                manifest,
                $"declares the '{LanguagePackContract.Keyword}' keyword but is missing required slot " +
                $"skills: {string.Join(", ", missing.Select(skill => $"'{skill}'"))}. The delivery loop " +
                "requires both build and test support. See docs/ADD-LANGUAGE-PACK.md.");
        }

        foreach (var skill in plugin.Skills.OrderBy(s => s.DirectoryName, StringComparer.Ordinal))
        {
            ValidateSkillName(skill, language);
        }
    }

    private void ValidateSkillName(SkillDefinition skill, string language)
    {
        var prefix = language + "-";

        if (!skill.DirectoryName.StartsWith(prefix, StringComparison.Ordinal))
        {
            // A framework skill — 'aspnet-api-design' — is named for its framework, not the
            // language, and is reached through the slot skills rather than by the loop.
            return;
        }

        var suffix = skill.DirectoryName[prefix.Length..];
        var relative = context.Relative(skill.SkillFilePath);

        if (LanguagePackContract.Slots.Contains(suffix, StringComparer.Ordinal))
        {
            return;
        }

        var nearMiss = LanguagePackContract.Slots.FirstOrDefault(slot => Distance(suffix, slot) <= NearMissDistance);

        if (nearMiss is not null)
        {
            context.Diagnostics.Policy(
                relative,
                $"is named '{skill.DirectoryName}', which is one small edit from the contracted slot " +
                $"'{language}-{nearMiss}'. The loop loads slot skills by exact name, so a near miss " +
                "is a skill it never finds and nothing else reports. Rename it, or pick a name that " +
                "is not a slot.");
        }
    }

    /// <summary>
    /// Levenshtein distance, bounded by the two strings' lengths. Slot suffixes are short, so the
    /// straightforward two-row implementation is cheaper than anything smarter.
    /// </summary>
    private static int Distance(string left, string right)
    {
        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];

        for (var j = 0; j <= right.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= left.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= right.Length; j++)
            {
                var substitution = previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1);
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), substitution);
            }

            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }
}
