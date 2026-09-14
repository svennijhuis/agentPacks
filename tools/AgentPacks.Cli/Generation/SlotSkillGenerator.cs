using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Writes the SKILL.md of every slot skill the loader rendered from <c>SKILL.source.md</c>. The
/// authored tree never holds the rendered file; the marketplace branch does, because that is the
/// tree clients install.
/// </summary>
internal static class SlotSkillGenerator
{
    public static IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins) =>
        plugins
            .SelectMany(plugin => plugin.Skills
                .Where(skill => skill.IsRendered)
                .Select(skill => new GeneratedFile(
                    Path.Combine("plugins", plugin.DirectoryName, "skills", skill.DirectoryName, "SKILL.md"),
                    skill.Text,
                    false)))
            .ToList();
}
