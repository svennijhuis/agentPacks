using Company.AI.Tooling.Loading;
using Company.AI.Tooling.Vendoring;

namespace Company.AI.Tooling.Cli;

internal static class CommandRouter
{
    private const string Usage = """
        Usage: company-ai-tooling <command> [options]

        Commands:
          validate          Validate the portable Agent Plugins source.
          generate-claude   Validate, then write the generated Claude compatibility files.
          validate-all      Validate the source, generate, and validate the generated output.
          vendor            Fetch reviewed external skills at their pinned commits into skills/.

        Options:
          --check           Compare generated or vendored files with what is committed; write nothing.
          --out <dir>       Write generated files beneath <dir> instead of the repository root.
        """;

    public static int Run(string[] args)
    {
        try
        {
            return (int)Execute(args);
        }
        catch (FatalToolingException ex)
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
            "vendor" => Vendor(pipeline, options),
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

        pipeline.Emit(pipeline.Generate(plugins), options);

        return pipeline.Report("Source and generated Claude compatibility validation passed.");
    }

    private static ExitCode Vendor(Pipeline pipeline, CommandOptions options)
    {
        var sources = ExternalSources.Load(pipeline.Context);
        var vendorer = new Vendorer(pipeline.Context);

        if (options.Check)
        {
            vendorer.Check(sources);
            return pipeline.Report("Vendored external skills are up to date.");
        }

        vendorer.Vendor(sources);

        return pipeline.Report(sources.Count == 0
            ? "No external sources are approved, so nothing was vendored."
            : "Vendored external skills.");
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
