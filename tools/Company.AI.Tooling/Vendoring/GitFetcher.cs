using System.Diagnostics;

namespace Company.AI.Tooling.Vendoring;

internal sealed record FetchResult(bool Success, string? Directory, string? Error);

/// <summary>
/// Fetches one subdirectory of a repository at an exact commit. Uses a blobless, sparse checkout
/// so vendoring a single skill out of a large repository stays cheap.
/// </summary>
internal static class GitFetcher
{
    public static FetchResult Fetch(string repository, string path, string commit)
    {
        var workspace = Path.Combine(Path.GetTempPath(), "agentpacks-vendor", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);

        var steps = new[]
        {
            new[] { "init", "--quiet" },
            new[] { "remote", "add", "origin", repository },
            new[] { "sparse-checkout", "set", "--no-cone", path }
        };

        foreach (var step in steps)
        {
            if (Run(workspace, step) is { Success: false } failure)
            {
                return Cleanup(workspace, failure.Error);
            }
        }

        // Fetching the commit directly is the cheap path. Some servers refuse to serve an
        // arbitrary SHA, so fall back to fetching everything and checking the commit out.
        var fetched = Run(workspace, ["fetch", "--quiet", "--depth", "1", "--filter=blob:none", "origin", commit]);

        if (!fetched.Success)
        {
            var full = Run(workspace, ["fetch", "--quiet", "--filter=blob:none", "origin"]);

            if (!full.Success)
            {
                return Cleanup(workspace, full.Error);
            }
        }

        var checkedOut = Run(workspace, ["checkout", "--quiet", commit]);

        if (!checkedOut.Success)
        {
            return Cleanup(
                workspace,
                $"commit {commit} could not be checked out from {repository}: {checkedOut.Error}");
        }

        var source = Path.Combine(workspace, path.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(source))
        {
            return Cleanup(workspace, $"path '{path}' does not exist at commit {commit}.");
        }

        return new FetchResult(true, source, null);
    }

    public static void Discard(string? fetchedDirectory)
    {
        if (fetchedDirectory is null)
        {
            return;
        }

        // The fetch workspace is two levels up from the checked-out subdirectory's root.
        var workspace = fetchedDirectory;

        while (workspace is not null && !Directory.Exists(Path.Combine(workspace, ".git")))
        {
            workspace = Path.GetDirectoryName(workspace);
        }

        if (workspace is not null)
        {
            TryDelete(workspace);
        }
    }

    private static FetchResult Cleanup(string workspace, string? error)
    {
        TryDelete(workspace);
        return new FetchResult(false, null, error);
    }

    private static void TryDelete(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
            // A leaked temporary clone must not fail the command.
        }
    }

    private static (bool Success, string Error) Run(string workingDirectory, string[] arguments)
    {
        var info = new ProcessStartInfo("git")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        try
        {
            using var process = Process.Start(info)
                ?? throw new InvalidOperationException("git could not be started.");

            var error = process.StandardError.ReadToEnd();
            process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return (process.ExitCode == 0, error.Trim());
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return (false, "git is not available on PATH.");
        }
    }
}
