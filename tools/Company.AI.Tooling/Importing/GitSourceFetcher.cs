using System.Diagnostics;

namespace Company.AI.Tooling.Importing;

internal sealed record FetchResult(bool Success, string? Directory, string? Error);

internal static class GitSourceFetcher
{
    public static FetchResult Fetch(string repository, string path, string commit)
    {
        var workspace = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "agentpacks-import", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);

        foreach (var step in new[]
                 {
                     new[] { "init", "--quiet" },
                     new[] { "remote", "add", "origin", repository },
                     new[] { "sparse-checkout", "set", "--no-cone", path }
                 })
        {
            if (Run(workspace, step) is { Success: false } failure)
            {
                return Cleanup(workspace, failure.Error);
            }
        }

        var fetched = Run(workspace, ["fetch", "--quiet", "--depth", "1", "--filter=blob:none", "origin", commit]);

        if (!fetched.Success && Run(workspace, ["fetch", "--quiet", "--filter=blob:none", "origin"])
            is { Success: false } full)
        {
            return Cleanup(workspace, full.Error);
        }

        if (Run(workspace, ["checkout", "--quiet", commit]) is { Success: false } checkout)
        {
            return Cleanup(workspace, $"commit {commit} could not be checked out: {checkout.Error}");
        }

        var source = System.IO.Path.Combine(workspace, path.Replace('/', System.IO.Path.DirectorySeparatorChar));
        return Directory.Exists(source)
            ? new FetchResult(true, source, null)
            : Cleanup(workspace, $"path '{path}' does not exist at commit {commit}.");
    }

    public static void Discard(string? fetchedDirectory)
    {
        var workspace = fetchedDirectory;
        while (workspace is not null && !Directory.Exists(System.IO.Path.Combine(workspace, ".git")))
        {
            workspace = System.IO.Path.GetDirectoryName(workspace);
        }

        if (workspace is not null)
        {
            try { Directory.Delete(workspace, recursive: true); } catch (IOException) { }
        }
    }

    private static FetchResult Cleanup(string workspace, string? error)
    {
        try { Directory.Delete(workspace, recursive: true); } catch (IOException) { }
        return new FetchResult(false, null, error);
    }

    private static (bool Success, string Error) Run(string workingDirectory, string[] arguments)
    {
        var info = new ProcessStartInfo("git")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);

        try
        {
            using var process = Process.Start(info) ?? throw new InvalidOperationException();
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
