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
        new ComponentValidator(context).Validate(plugins);
        new HookValidator(context).Validate(plugins);

        return plugins;
    }

    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var files = new ClaudeCompatGenerator(context).Generate(plugins)
            .Concat(new ClientTreeGenerator(context).Generate(plugins))
            .OrderBy(f => f.RelativePath, StringComparer.Ordinal)
            .ToList();

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
            var expected = TextFile.Normalize(file.Text);

            if (!options.Check)
            {
                TextFile.Write(destination, file.Text, file.Executable);
                continue;
            }

            var relative = file.RelativePath.Replace('\\', '/');

            if (!File.Exists(destination))
            {
                context.Diagnostics.Policy(relative, "is missing. Run 'generate' and commit the result.");
                continue;
            }

            if (!string.Equals(TextFile.ReadNormalized(destination), expected, StringComparison.Ordinal))
            {
                context.Diagnostics.Policy(
                    relative,
                    "does not match what the source generates. It is a generated file: " +
                    "run 'generate' and commit the result instead of editing it.");
            }
        }
    }

    /// <summary>
    /// Deletes generated files the source no longer produces. A plugin that drops its last MCP
    /// server stops generating a .mcp.json, and leaving the old one behind would keep Claude
    /// loading a server nobody declares any more. The same applies to every client tree: a deleted
    /// agent or hook has to disappear from all four, not just stop being regenerated.
    /// </summary>
    private void RemoveStaleFiles(IReadOnlyList<GeneratedFile> files, string root, CommandOptions options)
    {
        var expected = files
            .Select(f => f.RelativePath.Replace('\\', '/'))
            .ToHashSet(StringComparer.Ordinal);

        var pluginsRoot = Path.Combine(root, "plugins");

        if (!Directory.Exists(pluginsRoot))
        {
            return;
        }

        foreach (var pluginDirectory in Directory.GetDirectories(pluginsRoot)
                     .OrderBy(d => d, StringComparer.Ordinal))
        {
            var pluginName = Path.GetFileName(pluginDirectory);

            foreach (var path in Directory.GetFiles(pluginDirectory, "*", SearchOption.AllDirectories)
                         .OrderBy(p => p, StringComparer.Ordinal))
            {
                var pluginRelative = Path.GetRelativePath(pluginDirectory, path).Replace('\\', '/');

                if (!GeneratedPaths.IsGenerated(pluginRelative))
                {
                    continue;
                }

                var relative = $"plugins/{pluginName}/{pluginRelative}";

                if (expected.Contains(relative))
                {
                    continue;
                }

                if (options.Check)
                {
                    context.Diagnostics.Policy(
                        relative,
                        "is left over from a previous generation: the source no longer produces it. " +
                        "Run 'generate' and commit the deletion.");
                    continue;
                }

                File.Delete(path);
            }

            RemoveEmptyGeneratedDirectories(pluginDirectory, options);
        }
    }

    /// <summary>
    /// A client tree that loses its last file must not leave an empty directory behind: Claude and
    /// Cursor both treat the presence of a component directory as a declaration.
    /// </summary>
    private static void RemoveEmptyGeneratedDirectories(string pluginDirectory, CommandOptions options)
    {
        if (options.Check)
        {
            return;
        }

        foreach (var name in GeneratedPaths.OwnedDirectories)
        {
            RemoveIfEmpty(Path.Combine(pluginDirectory, name));
        }
    }

    /// <summary>Removes a directory once nothing is left under it, deepest first.</summary>
    private static void RemoveIfEmpty(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var child in Directory.GetDirectories(directory))
        {
            RemoveIfEmpty(child);
        }

        if (!Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
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
