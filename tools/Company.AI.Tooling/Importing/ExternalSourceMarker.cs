using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Company.AI.Tooling.Io;

namespace Company.AI.Tooling.Importing;

internal sealed record ExternalSourceMarker(
    string Name,
    string Repository,
    string Path,
    string Commit,
    string ContentHash)
{
    public const string FileName = ".external-source.json";

    public JsonObject ToJson() => new()
    {
        ["_comment"] = "Generated from the owning plugin's external-skills.json. Do not edit this directory.",
        ["name"] = Name,
        ["repository"] = Repository,
        ["path"] = Path,
        ["commit"] = Commit,
        ["contentHash"] = ContentHash
    };

    public static ExternalSourceMarker? FromJson(JsonNode? node)
    {
        if (node is not JsonObject obj)
        {
            return null;
        }

        var name = obj["name"]?.GetValue<string>();
        var repository = obj["repository"]?.GetValue<string>();
        var path = obj["path"]?.GetValue<string>();
        var commit = obj["commit"]?.GetValue<string>();
        var hash = obj["contentHash"]?.GetValue<string>();

        return name is null || repository is null || path is null || commit is null || hash is null
            ? null
            : new ExternalSourceMarker(name, repository, path, commit, hash);
    }

    public static string HashDirectory(string directory)
    {
        using var sha = SHA256.Create();

        var files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .Where(f => System.IO.Path.GetFileName(f) != FileName)
            .Select(f => (Relative: PathUtils.Relative(directory, f), Full: f))
            .OrderBy(f => f.Relative, StringComparer.Ordinal);

        foreach (var file in files)
        {
            var name = Encoding.UTF8.GetBytes(file.Relative + "\n");
            sha.TransformBlock(name, 0, name.Length, null, 0);
            var content = File.ReadAllBytes(file.Full);
            sha.TransformBlock(content, 0, content.Length, null, 0);
        }

        sha.TransformFinalBlock([], 0, 0);
        return Convert.ToHexStringLower(sha.Hash!);
    }
}
