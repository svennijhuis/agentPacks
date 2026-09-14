using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// The authored skills of this repository as the loader sees them. A slot skill authored as
/// <c>SKILL.source.md</c> has no SKILL.md on <c>main</c>; its text is the render, so contract
/// tests read skills through here instead of scanning the disk for SKILL.md files.
/// </summary>
internal static class SourceSkills
{
    private static readonly Lazy<IReadOnlyList<PluginPackage>> Plugins = new(() =>
        PluginLoader.Load(new RepositoryContext { Root = TestRepository.SourceRoot() }));

    public static SkillDefinition Skill(string plugin, string skill) =>
        Plugins.Value
            .Single(p => p.DirectoryName == plugin)
            .Skills.Single(s => s.DirectoryName == skill);

    /// <summary>SKILL.md text: rendered for a <c>SKILL.source.md</c> skill, otherwise the file.</summary>
    public static string Text(string plugin, string skill) => Skill(plugin, skill).Text;

    /// <summary>Every authored skill, in plugin then directory order.</summary>
    public static IEnumerable<SkillDefinition> All() =>
        Plugins.Value
            .OrderBy(p => p.DirectoryName, StringComparer.Ordinal)
            .SelectMany(p => p.Skills.OrderBy(s => s.DirectoryName, StringComparer.Ordinal));
}
