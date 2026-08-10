using Company.AI.Tooling.Cli;

namespace Company.AI.Tooling;

internal static class Program
{
    private static int Main(string[] args) => CommandRouter.Run(args);
}
