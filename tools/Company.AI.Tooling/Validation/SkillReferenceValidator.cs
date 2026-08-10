using System.Text.RegularExpressions;
using Company.AI.Tooling.Loading;

namespace Company.AI.Tooling.Validation;

/// <summary>
/// Catches skills that point at things nobody installing this plugin would get.
/// <para>
/// Skills routinely delegate to a sibling — a wrapper whose whole body is "run a
/// <c>/grilling</c> session" is useless without the skill it names. A reference is satisfied by a
/// skill in this plugin or by a reviewed external source in the catalog; anything else dead-ends
/// the moment an agent follows the instruction, which no schema and no per-skill check would see.
/// </para>
/// </summary>
internal sealed partial class SkillReferenceValidator(RepositoryContext context)
{
    /// <summary>A slash command in backticks, the convention skills use to invoke one another.</summary>
    [GeneratedRegex(@"`/([a-z][a-z0-9-]*)`")]
    private static partial Regex SkillReference { get; }

    /// <summary>Markdown link target.</summary>
    [GeneratedRegex(@"\[[^\]]*\]\(([^)\s]+)")]
    private static partial Regex MarkdownLink { get; }

    /// <summary>
    /// Absolute filesystem paths that look like slash commands. Skills mention these as places to
    /// write scratch output, not as skills to invoke.
    /// </summary>
    private static readonly IReadOnlySet<string> FilesystemPaths =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "tmp", "usr", "etc", "var", "dev", "opt", "home", "root", "bin", "sbin", "mnt", "proc"
        };

    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        var external = ExternalSources.Load(context)
            .Select(s => s.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in plugins)
        {
            var available = plugin.Skills
                .Select(s => s.DirectoryName)
                .Concat(external)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var skill in plugin.Skills)
            {
                ValidateSkill(skill, available);
            }
        }
    }

    private void ValidateSkill(SkillDefinition skill, IReadOnlySet<string> available)
    {
        foreach (var file in Directory
                     .EnumerateFiles(skill.Directory, "*.md", SearchOption.AllDirectories)
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            var text = File.ReadAllText(file);
            var relative = context.Relative(file);

            ValidateSkillReferences(text, relative, skill, available);
            ValidateLinks(text, relative, file);
        }
    }

    private void ValidateSkillReferences(
        string text,
        string relative,
        SkillDefinition skill,
        IReadOnlySet<string> available)
    {
        var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in SkillReference.Matches(text))
        {
            var name = match.Groups[1].Value;

            if (FilesystemPaths.Contains(name) ||
                available.Contains(name) ||
                string.Equals(name, skill.DirectoryName, StringComparison.OrdinalIgnoreCase) ||
                !reported.Add(name))
            {
                continue;
            }

            context.Diagnostics.Policy(
                relative,
                $"invokes `/{name}`, but no skill named '{name}' ships in this plugin and none is " +
                "referenced in external/sources.json. Add it, or the instruction dead-ends.");
        }
    }

    private void ValidateLinks(string text, string relative, string file)
    {
        var directory = Path.GetDirectoryName(file)!;
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match match in MarkdownLink.Matches(text))
        {
            var target = match.Groups[1].Value.Split('#')[0].Trim();

            if (target.Length == 0 ||
                target.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                target.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                Path.IsPathRooted(target) ||
                !reported.Add(target))
            {
                continue;
            }

            if (File.Exists(Path.Combine(directory, target)) || Directory.Exists(Path.Combine(directory, target)))
            {
                continue;
            }

            context.Diagnostics.Policy(relative, $"links to '{target}', which does not exist.");
        }
    }
}
