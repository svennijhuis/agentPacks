using AgentPacks.Cli.Commands;

namespace AgentPacks.Cli;

internal static class Program
{
    private static int Main(string[] args) => CommandRouter.Run(args);
}
