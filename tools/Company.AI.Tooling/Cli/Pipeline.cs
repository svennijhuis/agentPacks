using Company.AI.Tooling.Generation;
using Company.AI.Tooling.Io;
using Company.AI.Tooling.Loading;
using Company.AI.Tooling.Validation;

namespace Company.AI.Tooling.Cli;

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
