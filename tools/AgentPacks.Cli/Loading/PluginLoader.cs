using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Loading;

/// <summary>
/// Reads plugin directories into memory. Every failure becomes a diagnostic so loading continues
/// through the remaining plugins, mirroring the specification's failure isolation.
/// </summary>
internal static class PluginLoader
{
    public static IReadOnlyList<PluginPackage> Load(RepositoryContext context)
    {
        if (!Directory.Exists(context.PluginsRoot))
        {
            throw new FatalToolingException("plugins/ directory does not exist.");
        }

        var directories = Directory.GetDirectories(context.PluginsRoot)
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();

        if (directories.Count == 0)
        {
            context.Diagnostics.Policy("plugins", "no plugins found under plugins/.");
        }

        return directories.Select(directory => LoadPlugin(context, directory)).ToList();
    }

    private static PluginPackage LoadPlugin(RepositoryContext context, string directory)
    {
        var manifestPath = Path.Combine(directory, "plugin.json");
        var mcpPath = Path.Combine(directory, "mcp.json");

        return new PluginPackage
        {
            Directory = directory,
            ManifestPath = manifestPath,
            Manifest = ReadObject(context, manifestPath, required: true),
            McpPath = File.Exists(mcpPath) ? mcpPath : null,
            Mcp = File.Exists(mcpPath) ? ReadObject(context, mcpPath, required: false) : null,
            Skills = LoadSkills(context, directory)
        };
    }

    private static JsonObject? ReadObject(RepositoryContext context, string path, bool required)
    {
        if (!File.Exists(path))
        {
            if (required)
            {
                context.Diagnostics.SpecFatal(context.Relative(path), "is required and is missing.");
            }

            return null;
        }

        if (Directory.Exists(path))
        {
            context.Diagnostics.SpecFatal(context.Relative(path), "must be a file, but is a directory.");
            return null;
        }

        var node = JsonFile.TryRead(path, out var error);

        if (node is null)
        {
            context.Diagnostics.SpecFatal(context.Relative(path), error!);
            return null;
        }

        if (node is not JsonObject obj)
        {
            context.Diagnostics.SpecFatal(context.Relative(path), "must contain a JSON object.");
            return null;
        }

        return obj;
    }

    private static List<SkillDefinition> LoadSkills(RepositoryContext context, string pluginDirectory)
    {
        var skillsRoot = Path.Combine(pluginDirectory, "skills");
        var results = new List<SkillDefinition>();

        if (!Directory.Exists(skillsRoot))
        {
            if (File.Exists(skillsRoot))
            {
                context.Diagnostics.SpecFatal(
                    context.Relative(skillsRoot),
                    "exists but is not a directory, so the skills component cannot load.");
            }

            return results;
        }

        foreach (var directory in Directory.GetDirectories(skillsRoot).OrderBy(d => d, StringComparer.Ordinal))
        {
            var skillFile = Path.Combine(directory, "SKILL.md");

            if (!File.Exists(skillFile))
            {
                // Per the spec this directory simply is not discovered as a skill. We reject it
                // anyway, but as company policy rather than a conformance failure.
                context.Diagnostics.Policy(
                    context.Relative(directory),
                    "contains no SKILL.md, so no client discovers it as a skill. Remove it or add SKILL.md.");
                continue;
            }

            results.Add(new SkillDefinition(directory, skillFile, ReadFrontmatter(context, skillFile)));
        }

        return results;
    }

    private static Frontmatter? ReadFrontmatter(RepositoryContext context, string path)
    {
        string text;

        try
        {
            text = File.ReadAllText(path);
        }
        catch (IOException ex)
        {
            context.Diagnostics.SpecFatal(context.Relative(path), $"could not be read: {ex.Message}");
            return null;
        }

        var frontmatter = Frontmatter.TryParse(text, out var error);

        if (frontmatter is null)
        {
            context.Diagnostics.SpecFatal(context.Relative(path), error!);
        }

        return frontmatter;
    }
}
