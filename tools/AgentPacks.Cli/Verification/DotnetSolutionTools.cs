using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AgentPacks.Cli.Verification;

/// <summary>
/// Read-only solution and package facts. This is the MCP tool surface: three lookups, no writes.
/// </summary>
public static class DotnetSolutionTools
{
    public static readonly IReadOnlyList<string> ReadOnlyTools =
        ["list_projects", "list_packages", "describe_project"];

    public static bool IsAllowedTool(string name) =>
        ReadOnlyTools.Contains(name, StringComparer.Ordinal);

    public static IReadOnlyList<string> ListProjects(string solutionText)
    {
        if (solutionText.Contains("<Solution", StringComparison.Ordinal))
        {
            return XDocument.Parse(solutionText).Descendants()
                .Where(e => e.Name.LocalName == "Project")
                .Select(e => e.Attribute("Path")?.Value)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Cast<string>()
                .ToList();
        }

        return Regex.Matches(solutionText, @"Project\(""[^""]+""\)\s*=\s*""[^""]+"",\s*""([^""]+\.csproj)""")
            .Select(match => match.Groups[1].Value.Replace('\\', '/'))
            .ToList();
    }

    public static IReadOnlyList<(string Id, string? Version)> ListPackages(
        string projectText,
        string? packagesPropsText = null)
    {
        var versions = new Dictionary<string, string>(StringComparer.Ordinal);
        if (packagesPropsText is not null)
        {
            foreach (var element in XDocument.Parse(packagesPropsText).Descendants())
            {
                if (!string.Equals(element.Name.LocalName, "PackageVersion", StringComparison.Ordinal))
                    continue;
                var id = element.Attribute("Include")?.Value;
                var version = element.Attribute("Version")?.Value;
                if (id is not null && version is not null)
                    versions[id] = version;
            }
        }

        return XDocument.Parse(projectText).Descendants()
            .Where(e => string.Equals(e.Name.LocalName, "PackageReference", StringComparison.Ordinal))
            .Select(e => e.Attribute("Include")?.Value)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => (id!, versions.TryGetValue(id!, out var version) ? version : null))
            .ToList();
    }

    public static string DescribeProject(string path, IReadOnlyList<(string Id, string? Version)> packages) =>
        packages.Count == 0
            ? $"{path}: no package references"
            : $"{path}: " + string.Join(", ", packages.Select(p =>
                p.Version is null ? p.Id : $"{p.Id} {p.Version}"));
}
