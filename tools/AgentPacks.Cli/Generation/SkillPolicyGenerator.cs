using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Emits the Codex half of a user-invoked skill. Claude reads
/// <c>disable-model-invocation</c> from SKILL.md; Codex reads
/// <c>policy.allow_implicit_invocation</c> from <c>agents/openai.yaml</c>. A pack that sets only
/// one dialect is user-invoked on one client and model-invoked on the others.
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
                if (skill.Frontmatter?.Scalar("disable-model-invocation") != "true")
                {
                    continue;
                }

                var relative = Path.Combine(
                    "plugins",
                    plugin.DirectoryName,
                    "skills",
                    skill.DirectoryName,
                    CodexPolicyRelative.Replace('/', Path.DirectorySeparatorChar));

                files.Add(new GeneratedFile(relative, CodexPolicy));
            }
        }

        return files;
    }

    private const string CodexPolicy =
        "# Generated from disable-model-invocation: true in SKILL.md.\n" +
        "# Codex has no frontmatter flag; this is the matching dialect.\n" +
        "policy:\n" +
        "  allow_implicit_invocation: false\n";
}
