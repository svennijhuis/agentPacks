using AgentPacks.Cli.Commands;
using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Loading;
using AgentPacks.Cli.Validation;
using AgentPacks.Cli.Importing;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Builds a throwaway repository on disk for one test. Fixtures are constructed in code rather
/// than committed as directory trees so that cases needing a symlink, an oversized description or
/// deliberately malformed JSON stay readable and survive a git checkout unchanged.
/// </summary>
internal sealed class TestRepository : IDisposable
{
    private const string ValidManifest = """
        {
          "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
          "name": "engineering",
          "description": "Test plugin."
        }
        """;

    public TestRepository()
    {
        Root = Path.Combine(Path.GetTempPath(), "agentpacks-tests", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path.Combine(Root, "plugins"));
        Directory.CreateDirectory(Path.Combine(Root, "tools"));
    }

    public string Root { get; }

    public string PluginDirectory(string plugin = "engineering") =>
        Path.Combine(Root, "plugins", plugin);

    public TestRepository WithPlugin(string plugin = "engineering", string? manifest = null)
    {
        var directory = PluginDirectory(plugin);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "plugin.json"), manifest ?? ValidManifest);

        var sources = Path.Combine(directory, ExternalSources.FileName);
        if (!File.Exists(sources))
        {
            File.WriteAllText(sources, "{ \"sources\": [] }\n");
        }

        return this;
    }

    public TestRepository WithMcp(string json, string plugin = "engineering")
    {
        File.WriteAllText(Path.Combine(PluginDirectory(plugin), "mcp.json"), json);
        return this;
    }

    /// <summary>Writes a skill. Passing null for the frontmatter name uses the directory name.</summary>
    public TestRepository WithSkill(
        string directoryName,
        string? name = null,
        string description = "Explains when to use this skill and what it does.",
        string? extraFrontmatter = null,
        string body = "Instructions.",
        string plugin = "engineering")
    {
        var directory = Path.Combine(PluginDirectory(plugin), "skills", directoryName);
        Directory.CreateDirectory(directory);

        var frontmatter = $"name: {name ?? directoryName}\ndescription: {description}";

        if (extraFrontmatter is not null)
        {
            frontmatter += "\n" + extraFrontmatter;
        }

        File.WriteAllText(
            Path.Combine(directory, "SKILL.md"),
            $"---\n{frontmatter}\n---\n\n{body}\n");

        return this;
    }

    public TestRepository WithRawSkill(string directoryName, string content, string plugin = "engineering")
    {
        var directory = Path.Combine(PluginDirectory(plugin), "skills", directoryName);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "SKILL.md"), content);

        return this;
    }

    public TestRepository WithEmptySkillDirectory(string directoryName, string plugin = "engineering")
    {
        Directory.CreateDirectory(Path.Combine(PluginDirectory(plugin), "skills", directoryName));
        return this;
    }

    /// <summary>Writes a neutral agent. Passing null for the name uses the filename.</summary>
    public TestRepository WithAgent(
        string fileName,
        string? name = null,
        string description = "Reviews a change for defects. Use before opening a pull request.",
        string? extraFrontmatter = "model: inherit\nreadonly: true",
        string body = "You review changes.",
        string plugin = "engineering") =>
        WithMarkdownComponent("agents", $"{fileName}.md", name ?? fileName, description, extraFrontmatter, body, plugin);

    public TestRepository WithCommand(
        string fileName,
        string? name = null,
        string description = "Reviews the current diff and reports findings.",
        string body = "Review the diff.",
        string plugin = "engineering") =>
        WithMarkdownComponent("commands", $"{fileName}.md", name ?? fileName, description, null, body, plugin);

    /// <summary>A rule carries no name: in Cursor's .mdc format the filename is the identity.</summary>
    public TestRepository WithRule(
        string fileName,
        string description = "Standards that apply while reviewing.",
        string scope = "alwaysApply: true",
        string body = "- Changes do one thing.",
        string plugin = "engineering") =>
        WithMarkdownComponent("rules", $"{fileName}.mdc", null, description, scope, body, plugin);

    private TestRepository WithMarkdownComponent(
        string directoryName,
        string fileName,
        string? name,
        string description,
        string? extraFrontmatter,
        string body,
        string plugin)
    {
        var directory = Path.Combine(PluginDirectory(plugin), directoryName);
        Directory.CreateDirectory(directory);

        var frontmatter = name is null ? $"description: {description}" : $"name: {name}\ndescription: {description}";

        if (extraFrontmatter is not null)
        {
            frontmatter += "\n" + extraFrontmatter;
        }

        File.WriteAllText(Path.Combine(directory, fileName), $"---\n{frontmatter}\n---\n\n{body}\n");

        return this;
    }

    /// <summary>Writes the neutral hook manifest. The scripts it names must exist separately.</summary>
    public TestRepository WithHooks(string json, string plugin = "engineering")
    {
        File.WriteAllText(Path.Combine(PluginDirectory(plugin), PluginLoader.HooksFileName), json);
        return this;
    }

    /// <summary>
    /// Writes a hook script pair. Omitting a half is how the "runs on one platform only" cases are
    /// built, which is exactly what the validator has to catch.
    /// </summary>
    public TestRepository WithScript(
        string name,
        bool posix = true,
        bool powerShell = true,
        string plugin = "engineering")
    {
        var directory = Path.Combine(PluginDirectory(plugin), "scripts");
        Directory.CreateDirectory(directory);

        if (posix)
        {
            File.WriteAllText(Path.Combine(directory, $"{name}.sh"), "#!/usr/bin/env bash\nexit 0\n");
        }

        if (powerShell)
        {
            File.WriteAllText(Path.Combine(directory, $"{name}.ps1"), "exit 0\n");
        }

        return this;
    }

    /// <summary>A plugin with one hook wired end to end: manifest entry plus both script halves.</summary>
    public TestRepository WithHook(string neutralEvent, string? matcher = null, string script = "guard")
    {
        var entry = matcher is null
            ? $$"""{ "script": "{{script}}" }"""
            : $$"""{ "script": "{{script}}", "matcher": "{{matcher}}" }""";

        return WithScript(script)
            .WithHooks($$"""
                { "hooks": { "{{neutralEvent}}": [{{entry}}] } }
                """);
    }

    public TestRepository WithFile(string relativePath, string content)
    {
        var path = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);

        return this;
    }

    public TestRepository WithExternalSources(string json, string plugin = "engineering") =>
        WithFile($"plugins/{plugin}/{ExternalSources.FileName}", json);

    public TestRepository WithoutExternalSources(string plugin = "engineering")
    {
        File.Delete(Path.Combine(PluginDirectory(plugin), ExternalSources.FileName));
        return this;
    }

    /// <summary>Creates a symlink inside the plugin pointing at <paramref name="target"/>.</summary>
    public TestRepository WithSymlink(string relativePathInPlugin, string target, string plugin = "engineering")
    {
        var path = Path.Combine(PluginDirectory(plugin), relativePathInPlugin);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.CreateSymbolicLink(path, target);

        return this;
    }

    /// <summary>A plugin that is valid in every respect, used as the baseline for negative tests.</summary>
    public TestRepository WithValidPlugin() =>
        WithPlugin().WithSkill("dotnet-review");

    public ValidationRun Validate()
    {
        var context = new RepositoryContext { Root = Root };
        var pipeline = new Pipeline(context);
        var plugins = pipeline.ValidateSource();

        return new ValidationRun(context, plugins, []);
    }

    public ValidationRun ValidateAndGenerate(CommandOptions? options = null)
    {
        var context = new RepositoryContext { Root = Root };
        var pipeline = new Pipeline(context);
        var plugins = pipeline.ValidateSource();
        var files = pipeline.Generate(plugins);

        pipeline.Emit(files, options ?? new CommandOptions());

        return new ValidationRun(context, plugins, files);
    }

    public ValidationRun CheckExternalSources()
    {
        var context = new RepositoryContext { Root = Root };
        new ExternalSourceMaterializer(context).Check(ExternalSources.Load(context));
        return new ValidationRun(context, [], []);
    }

    public ValidationRun MaterializeExternalSources()
    {
        var context = new RepositoryContext { Root = Root };
        new ExternalSourceMaterializer(context).Materialize(ExternalSources.Load(context));
        return new ValidationRun(context, [], []);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException)
        {
            // A leaked temp directory must never fail a test run.
        }
    }
}

internal sealed record ValidationRun(
    RepositoryContext Context,
    IReadOnlyList<PluginPackage> Plugins,
    IReadOnlyList<GeneratedFile> Generated)
{
    public IReadOnlyList<Diagnostic> Diagnostics => Context.Diagnostics.Diagnostics;

    public bool HasErrors => Context.Diagnostics.HasErrors;

    public string Text => Context.Diagnostics.Render();

    public GeneratedFile File(string relativePath) =>
        Generated.Single(f => f.RelativePath.Replace('\\', '/') == relativePath);

    public bool HasFile(string relativePath) =>
        Generated.Any(f => f.RelativePath.Replace('\\', '/') == relativePath);
}
