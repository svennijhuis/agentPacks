using System.Runtime.InteropServices;

namespace AgentPacks.Cli.Io;

internal static class TextFile
{
    /// <summary>
    /// Writes generated text. Newlines are normalised to \n so the same source produces the same
    /// bytes on every platform: the marketplace branch is compared with --check in CI, and a
    /// Windows checkout writing \r\n would report drift against a Linux runner's output.
    /// </summary>
    public static void Write(string path, string text, bool executable = false)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Normalize(text));

        // Hook scripts are executed by the client directly, so the bit has to survive generation.
        // Windows has no equivalent mode, and the .cmd shim is what runs there anyway.
        if (executable && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            File.SetUnixFileMode(
                path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        }
    }

    public static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);

    /// <summary>Reads a file for comparison, normalising newlines the same way Write does.</summary>
    public static string ReadNormalized(string path) => Normalize(File.ReadAllText(path));
}
