using AgentPacks.Cli.Loading;
using AgentPacks.Cli.Importing;

namespace AgentPacks.Cli.Commands;

internal static class CommandRouter
{
    // The program name is derived so the banner survives renames and matches however the
    // process was started (dotnet run, a built binary, or a packaged tool).
    private static readonly string Usage = $"""
        Usage: {AppDomain.CurrentDomain.FriendlyName} <command> [options]

        Commands:
          validate          Validate the portable Agent Plugins source.
          generate-claude   Validate, then write the generated Claude compatibility files.
          validate-all      Validate the source, generate, and validate the generated output.
          materialize-external
                            Generate real skills/ directories from external URL sources.

        Options:
          --check           Compare generated files with what is committed; write nothing.
          --out <dir>       Write generated files beneath <dir>. Required for generate-claude writes.
        """;

    public static int Run(string[] args)
    {
        try
        {
            return (int)Execute(args);
        }
        catch (FatalCliException ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return (int)ExitCode.ValidationFailed;
        }
    }

    private static ExitCode Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine(Usage);
            return ExitCode.UsageError;
        }

        var command = args[0].ToLowerInvariant();

        if (command is "-h" or "--help" or "help")
        {
            Console.WriteLine(Usage);
            return ExitCode.Success;
        }

        if (!TryParseOptions(args.Skip(1).ToArray(), out var options))
        {
            return ExitCode.UsageError;
        }

        var pipeline = new Pipeline(RepositoryLocator.Locate());

        return command switch
        {
            "validate" => Validate(pipeline),
            "generate-claude" => GenerateClaude(pipeline, options),
            "validate-all" => ValidateAll(pipeline, options),
            "materialize-external" => MaterializeExternal(pipeline, options),
            _ => UnknownCommand(command)
        };
    }

    private static ExitCode Validate(Pipeline pipeline)
    {
        pipeline.ValidateSource();
        return pipeline.Report("Source validation passed.");
    }

    private static ExitCode GenerateClaude(Pipeline pipeline, CommandOptions options)
    {
        if (!options.Check && options.OutputRoot is null)
        {
            Console.Error.WriteLine(
                "error: generate-claude writes generated output and requires '--out <dir>'. " +
                "GitHub publication passes its marketplace workspace explicitly.");
            return ExitCode.UsageError;
        }

        var plugins = pipeline.ValidateSource();

        // Generating from invalid source would bake the problem into the committed output.
        if (pipeline.Context.Diagnostics.HasErrors)
        {
            return pipeline.Report(string.Empty);
        }

        pipeline.Emit(pipeline.Generate(plugins), options);

        return pipeline.Report(options.Check
            ? "Generated Claude compatibility files are up to date."
            : "Generated Claude compatibility files.");
    }

    private static ExitCode ValidateAll(Pipeline pipeline, CommandOptions options)
    {
        var plugins = pipeline.ValidateSource();

        if (pipeline.Context.Diagnostics.HasErrors)
        {
            return pipeline.Report(string.Empty);
        }

        var files = pipeline.Generate(plugins);

        // validate-all is read-only unless the caller explicitly requests an output directory
        // or asks to compare generated output with --check.
        if (options.OutputRoot is not null || options.Check)
        {
            pipeline.Emit(files, options);
        }

        return pipeline.Report("Source and generated Claude compatibility validation passed.");
    }

    private static ExitCode MaterializeExternal(Pipeline pipeline, CommandOptions options)
    {
        var materializer = new ExternalSourceMaterializer(pipeline.Context);
        var sources = ExternalSources.Load(pipeline.Context);

        if (options.Check)
        {
            materializer.Check(sources);
            return pipeline.Report("Generated external skills are up to date.");
        }

        if (options.OutputRoot is null)
        {
            Console.Error.WriteLine(
                "error: materialize-external requires '--out <repository-root>' because it writes " +
                "the generated marketplace in place. GitHub publication supplies this explicitly.");
            return ExitCode.UsageError;
        }

        materializer.Materialize(sources);
        return pipeline.Report("Generated external skills from URL sources.");
    }

    private static ExitCode UnknownCommand(string command)
    {
        Console.Error.WriteLine($"error: unknown command '{command}'.");
        Console.Error.WriteLine(Usage);
        return ExitCode.UsageError;
    }

    private static bool TryParseOptions(string[] args, out CommandOptions options)
    {
        var check = false;
        string? outputRoot = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--check":
                    check = true;
                    break;

                case "--out" when i + 1 < args.Length:
                    outputRoot = Path.GetFullPath(args[++i]);
                    break;

                case "--out":
                    Console.Error.WriteLine("error: --out requires a directory.");
                    options = new CommandOptions();
                    return false;

                default:
                    Console.Error.WriteLine($"error: unknown option '{args[i]}'.");
                    options = new CommandOptions();
                    return false;
            }
        }

        options = new CommandOptions { Check = check, OutputRoot = outputRoot };
        return true;
    }
}
