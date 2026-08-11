using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;
using AgentPacks.Cli.Validation;

namespace AgentPacks.Cli.Commands;

internal enum ExitCode
{
    Success = 0,
    ValidationFailed = 1,
    UsageError = 2
}

internal sealed record CommandOptions
{
    /// <summary>Compare generated output against what is committed instead of writing it.</summary>
    public bool Check { get; init; }

    /// <summary>Write generated output beneath this root instead of the repository. Used by PR CI.</summary>
    public string? OutputRoot { get; init; }
}

/// <summary>Shared steps: load, validate the source, generate, validate the result.</summary>
internal sealed class Pipeline(RepositoryContext context)
{
    public RepositoryContext Context => context;

    public IReadOnlyList<PluginPackage> ValidateSource()
    {
        var plugins = PluginLoader.Load(context);
        var schemaValidator = new SchemaValidator(context);

        new PluginValidator(context, schemaValidator).Validate(plugins);
        new SkillValidator(context).Validate(plugins);
        new SkillReferenceValidator(context).Validate(plugins);
        new McpValidator(context).Validate(plugins);

        return plugins;
    }

    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var files = new ClaudeCompatGenerator(context).Generate(plugins);
        new CompatibilityValidator(context).Validate(files);

        return files;
    }

    /// <summary>Writes generated files, or reports which ones drifted when checking.</summary>
    public void Emit(IReadOnlyList<GeneratedFile> files, CommandOptions options)
    {
        var root = options.OutputRoot ?? context.Root;

        RemoveStaleFiles(files, root, options);

        foreach (var file in files)
        {
            var destination = Path.Combine(root, file.RelativePath);
            var expected = JsonFile.Serialize(file.Content);

            if (!options.Check)
            {
                JsonFile.Write(destination, file.Content);
                continue;
            }

            var relative = file.RelativePath.Replace('\\', '/');

            if (!File.Exists(destination))
            {
                context.Diagnostics.Policy(relative, "is missing. Run 'generate-claude' and commit the result.");
                continue;
            }

            if (!string.Equals(File.ReadAllText(destination), expected, StringComparison.Ordinal))
            {
                context.Diagnostics.Policy(
                    relative,
                    "does not match what the source generates. It is a generated file: " +
                    "run 'generate-claude' and commit the result instead of editing it.");
            }
        }
    }

    /// <summary>
    /// Deletes generated files the source no longer produces. A plugin that drops its last MCP
    /// server stops generating a .mcp.json, and leaving the old one behind would keep Claude
    /// loading a server nobody declares any more.
    /// </summary>
    private void RemoveStaleFiles(IReadOnlyList<GeneratedFile> files, string root, CommandOptions options)
    {
        var expected = files
            .Select(f => f.RelativePath.Replace('\\', '/'))
            .ToHashSet(StringComparer.Ordinal);

        if (!Directory.Exists(context.PluginsRoot))
        {
            return;
        }

        foreach (var pluginDirectory in Directory.GetDirectories(context.PluginsRoot)
                     .OrderBy(d => d, StringComparer.Ordinal))
        {
            var relative = $"plugins/{Path.GetFileName(pluginDirectory)}/.mcp.json";

            if (expected.Contains(relative))
            {
                continue;
            }

            var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(path))
            {
                continue;
            }

            if (options.Check)
            {
                context.Diagnostics.Policy(
                    relative,
                    "is left over from a previous generation: the plugin no longer declares MCP servers. " +
                    "Run 'generate-claude' and commit the deletion.");
                continue;
            }

            File.Delete(path);
        }
    }

    /// <summary>Prints findings and maps them to a process exit code.</summary>
    public ExitCode Report(string successMessage)
    {
        var rendered = context.Diagnostics.Render();

        if (rendered.Length > 0)
        {
            Console.Error.Write(rendered);
        }

        if (context.Diagnostics.HasErrors)
        {
            return ExitCode.ValidationFailed;
        }

        Console.WriteLine(successMessage);
        return ExitCode.Success;
    }
}
