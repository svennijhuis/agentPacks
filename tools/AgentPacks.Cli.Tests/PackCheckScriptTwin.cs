using System.Diagnostics;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Runs a pack-check lint script and, when pwsh is on PATH, its PowerShell twin on the same
/// files. Both must agree on exit code and findings.
/// </summary>
internal static class PackCheckScriptTwin
{
    public static bool PwshAvailable => Probe.Value;

    public static string ScriptsRoot(string? root = null) =>
        TestRepository.PackCheckSkillScriptsRoot(root);

    public static string Script(string file, string? root = null) =>
        Path.Combine(ScriptsRoot(root), file);

    public static (int Exit, string Output) Run(string scriptBase, IReadOnlyList<string> files, string label)
    {
        var posix = Invoke("sh", [Script(scriptBase + ".sh"), .. files], scriptBase, label);
        if (PwshAvailable)
        {
            var powerShell = Invoke(
                "pwsh",
                ["-NoProfile", "-NonInteractive", "-File", Script(scriptBase + ".ps1"), .. files],
                scriptBase,
                label);
            Assert.Equal(posix.Exit, powerShell.Exit);
            Assert.Equal(
                posix.Output.Replace("\r", "", StringComparison.Ordinal),
                powerShell.Output.Replace("\r", "", StringComparison.Ordinal));
        }

        return posix;
    }

    private static readonly Lazy<bool> Probe = new(() =>
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("pwsh", "-NoProfile -Command exit")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            })!;
            process.WaitForExit(20000);
            return process.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    });

    private static (int Exit, string Output) Invoke(string shell, IReadOnlyList<string> arguments, string scriptBase, string label)
    {
        var start = new ProcessStartInfo(shell) { RedirectStandardOutput = true, UseShellExecute = false };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start)!;
        var read = process.StandardOutput.ReadToEndAsync();
        if (!process.WaitForExit(TimeSpan.FromSeconds(20)))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"{shell} {scriptBase} did not finish within 20 s on {label}.");
        }

        return (process.ExitCode, read.Result);
    }
}
