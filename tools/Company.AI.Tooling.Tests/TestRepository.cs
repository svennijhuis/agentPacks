using Company.AI.Tooling.Cli;
using Company.AI.Tooling.Generation;
using Company.AI.Tooling.Loading;
using Company.AI.Tooling.Validation;

namespace Company.AI.Tooling.Tests;

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
          "name": "company-engineering",
          "description": "Test plugin."
        }
        """;

    public TestRepository()
    {
        Root = Path.Combine(Path.GetTempPath(), "company-ai-tests", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path.Combine(Root, "plugins"));
        Directory.CreateDirectory(Path.Combine(Root, "tools"));
        Directory.CreateDirectory(Path.Combine(Root, "schemas"));

        foreach (var schema in Directory.GetFiles(SolutionSchemasDirectory, "*.json"))
        {
            File.Copy(schema, Path.Combine(Root, "schemas", Path.GetFileName(schema)));
        }

        WithExternalSources("""{ "sources": [] }""");
    }

    public string Root { get; }

    /// <summary>The vendored schemas from the real repository, so tests validate against the real thing.</summary>
    private static string SolutionSchemasDirectory
    {
        get
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current is not null)
            {
                var candidate = Path.Combine(current.FullName, "schemas");

                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            throw new InvalidOperationException("Could not locate the vendored schemas/ directory.");
        }
    }

    public string PluginDirectory(string plugin = "company-engineering") =>
        Path.Combine(Root, "plugins", plugin);

    public TestRepository WithPlugin(string plugin = "company-engineering", string? manifest = null)
    {
        var directory = PluginDirectory(plugin);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "plugin.json"), manifest ?? ValidManifest);

        return this;
    }

    public TestRepository WithMcp(string json, string plugin = "company-engineering")
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
        string plugin = "company-engineering")
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

    public TestRepository WithRawSkill(string directoryName, string content, string plugin = "company-engineering")
    {
        var directory = Path.Combine(PluginDirectory(plugin), "skills", directoryName);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "SKILL.md"), content);

        return this;
    }

    public TestRepository WithEmptySkillDirectory(string directoryName, string plugin = "company-engineering")
    {
        Directory.CreateDirectory(Path.Combine(PluginDirectory(plugin), "skills", directoryName));
        return this;
    }

    public TestRepository WithAgent(
        string fileName,
        string name,
        string description = "Reviews changes.",
        string plugin = "company-engineering")
    {
        var directory = Path.Combine(PluginDirectory(plugin), "agents");
        Directory.CreateDirectory(directory);

        File.WriteAllText(
            Path.Combine(directory, fileName),
            $"---\nname: {name}\ndescription: {description}\n---\n\nBody.\n");

        return this;
    }

    public TestRepository WithFile(string relativePath, string content)
    {
        var path = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);

        return this;
    }

    public TestRepository WithExternalSources(string json) => WithFile("external/sources.json", json);

    public TestRepository WithoutExternalSources()
    {
        File.Delete(Path.Combine(Root, "external", "sources.json"));
        return this;
    }

    /// <summary>Creates a symlink inside the plugin pointing at <paramref name="target"/>.</summary>
    public TestRepository WithSymlink(string relativePathInPlugin, string target, string plugin = "company-engineering")
    {
        var path = Path.Combine(PluginDirectory(plugin), relativePathInPlugin);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.CreateSymbolicLink(path, target);

        return this;
    }

    /// <summary>A plugin that is valid in every respect, used as the baseline for negative tests.</summary>
    public TestRepository WithValidPlugin() =>
        WithPlugin()
            .WithSkill("dotnet-review")
            .WithAgent("code-reviewer.agent.md", "code-reviewer");

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
