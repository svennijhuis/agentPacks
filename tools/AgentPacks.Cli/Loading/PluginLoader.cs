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
            throw new FatalCliException("plugins/ directory does not exist.");
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
        var hooksPath = Path.Combine(directory, HooksFileName);

        return new PluginPackage
        {
            Directory = directory,
            ManifestPath = manifestPath,
            Manifest = ReadObject(context, manifestPath, required: true),
            McpPath = File.Exists(mcpPath) ? mcpPath : null,
            Mcp = File.Exists(mcpPath) ? ReadObject(context, mcpPath, required: false) : null,
            Skills = LoadSkills(context, directory),
            Agents = LoadMarkdownComponents(context, directory, "agents", "*.md", "agent"),
            Commands = LoadMarkdownComponents(context, directory, "commands", "*.md", "command"),
            Rules = LoadMarkdownComponents(context, directory, "rules", "*.mdc", "rule"),
            HooksPath = File.Exists(hooksPath) ? hooksPath : null,
            Hooks = File.Exists(hooksPath) ? ReadObject(context, hooksPath, required: false) : null,
            Scripts = LoadScripts(directory)
        };
    }

    /// <summary>
    /// The neutral hook manifest. It is deliberately not hooks/hooks.json: Claude, Cursor and Codex
    /// all auto-discover that path in three incompatible dialects, and Copilot claims the root
    /// hooks.json. This name is discovered by no client and generated into all four.
    /// </summary>
    public const string HooksFileName = "hooks.source.json";

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

    /// <summary>
    /// Loads one directory of single-file Markdown components. A directory that is absent is not an
    /// error: a plugin declares only the components it has.
    /// </summary>
    private static List<MarkdownComponent> LoadMarkdownComponents(
        RepositoryContext context,
        string pluginDirectory,
        string directoryName,
        string pattern,
        string kind)
    {
        var root = Path.Combine(pluginDirectory, directoryName);
        var results = new List<MarkdownComponent>();

        if (!Directory.Exists(root))
        {
            if (File.Exists(root))
            {
                context.Diagnostics.SpecFatal(
                    context.Relative(root),
                    $"exists but is not a directory, so the {kind} component cannot load.");
            }

            return results;
        }

        foreach (var file in Directory.GetFiles(root, pattern).OrderBy(f => f, StringComparer.Ordinal))
        {
            results.Add(new MarkdownComponent(kind, file, ReadFrontmatter(context, file)));
        }

        // A stray file in a component directory is nearly always a mis-named component: agents/foo.txt
        // or rules/bar.md is silently ignored by every client, which reads as "my rule does nothing".
        foreach (var stray in Directory.GetFiles(root)
                     .Where(f => !MatchesPattern(f, pattern))
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            context.Diagnostics.Policy(
                context.Relative(stray),
                $"is not a {pattern} file, so no client loads it as a {kind}. Rename or remove it.");
        }

        return results;
    }

    private static bool MatchesPattern(string path, string pattern) =>
        Path.GetExtension(path).Equals(Path.GetExtension(pattern), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Pairs scripts/ by basename. Hooks name a basename, never a command line, so the generator
    /// owns the invocation and the validator can insist both platform halves exist.
    /// </summary>
    private static List<ScriptDefinition> LoadScripts(string pluginDirectory)
    {
        var root = Path.Combine(pluginDirectory, "scripts");

        if (!Directory.Exists(root))
        {
            return [];
        }

        var posix = new Dictionary<string, string>(StringComparer.Ordinal);
        var powershell = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var file in Directory.GetFiles(root).OrderBy(f => f, StringComparer.Ordinal))
        {
            var name = Path.GetFileNameWithoutExtension(file);

            switch (Path.GetExtension(file))
            {
                case ".sh":
                    posix[name] = file;
                    break;

                case ".ps1":
                    powershell[name] = file;
                    break;

                // .cmd is the generated Windows shim. It sits beside the authored pair on the
                // published branch, where validation also runs, so it is ignored rather than
                // reported — the staleness sweep is what removes one that no longer belongs.
            }
        }

        return posix.Keys
            .Concat(powershell.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name => new ScriptDefinition(
                name,
                posix.GetValueOrDefault(name),
                powershell.GetValueOrDefault(name)))
            .ToList();
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
