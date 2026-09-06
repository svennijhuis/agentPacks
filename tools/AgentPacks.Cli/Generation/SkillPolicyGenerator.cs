using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Emits client-specific invocation flags that the portable SKILL.md cannot carry alone.
/// Claude reads <c>disable-model-invocation</c> from SKILL.md; Codex reads
/// <c>policy.allow_implicit_invocation</c> from <c>agents/openai.yaml</c>. Copilot reads
/// <c>user-invocable</c> — loop-audience skills are not user entrypoints, so the generated
/// Copilot copy always sets <c>user-invocable: false</c>.
/// </summary>
internal static class SkillPolicyGenerator
{
    public const string CodexPolicyRelative = "agents/openai.yaml";

    public static IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var files = new List<GeneratedFile>();

        foreach (var plugin in plugins)
        {
            foreach (var skill in plugin.Skills)
            {
                if (skill.Frontmatter?.Scalar("disable-model-invocation") == "true")
                {
                    var relative = Path.Combine(
                        "plugins",
                        plugin.DirectoryName,
                        "skills",
                        skill.DirectoryName,
                        CodexPolicyRelative.Replace('/', Path.DirectorySeparatorChar));

                    files.Add(new GeneratedFile(relative, CodexPolicy));
                }

                if (skill.Frontmatter?.StringMap("metadata") is { } metadata &&
                    metadata.TryGetValue("audience", out var audience) &&
                    audience == "loop")
                {
                    var relative = Path.Combine(
                        "plugins",
                        plugin.DirectoryName,
                        "com.github.copilot",
                        "skills",
                        skill.DirectoryName,
                        "SKILL.md");

                    var source = File.ReadAllText(skill.SkillFilePath);
                    files.Add(new GeneratedFile(relative, WithUserInvocableFalse(source)));
                }
            }
        }

        return files;
    }

    internal static string WithUserInvocableFalse(string skillMarkdown)
    {
        if (skillMarkdown.Contains("user-invocable: false", StringComparison.Ordinal))
        {
            return skillMarkdown;
        }

        if (skillMarkdown.Contains("user-invocable: true", StringComparison.Ordinal))
        {
            return skillMarkdown.Replace(
                "user-invocable: true",
                "user-invocable: false",
                StringComparison.Ordinal);
        }

        const string open = "---\n";
        if (skillMarkdown.StartsWith(open, StringComparison.Ordinal))
        {
            return open + "user-invocable: false\n" + skillMarkdown[open.Length..];
        }

        const string openCrlf = "---\r\n";
        if (skillMarkdown.StartsWith(openCrlf, StringComparison.Ordinal))
        {
            return openCrlf + "user-invocable: false\r\n" + skillMarkdown[openCrlf.Length..];
        }

        return skillMarkdown;
    }

    private const string CodexPolicy =
        "# Generated from disable-model-invocation: true in SKILL.md.\n" +
        "# Codex has no frontmatter flag; this is the matching dialect.\n" +
        "policy:\n" +
        "  allow_implicit_invocation: false\n";
}
