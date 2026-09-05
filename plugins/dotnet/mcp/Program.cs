using System.Text.RegularExpressions;
using System.Xml.Linq;

// Local read-only CLI. No network. No credentials. No hosted Roslyn.
if (args is ["--list-tools"])
{
    Console.WriteLine("list_projects");
    Console.WriteLine("list_packages");
    Console.WriteLine("describe_project");
    foreach (var tool in RoslynLookup.ReadOnlyTools)
        Console.WriteLine(tool);
    return 0;
}

if (args is ["--list-projects", var solution] && File.Exists(solution))
{
    foreach (var project in ListProjects(File.ReadAllText(solution)))
        Console.WriteLine(project);
    return 0;
}

if (args is ["--list-packages", var projectPath, ..] && File.Exists(projectPath))
{
    var props = args.Length > 2 && File.Exists(args[2]) ? File.ReadAllText(args[2]) : null;
    foreach (var (id, version) in ListPackages(File.ReadAllText(projectPath), props))
        Console.WriteLine(version is null ? id : $"{id} {version}");
    return 0;
}

if (args is ["--list-symbols", var source] && File.Exists(source))
{
    foreach (var symbol in RoslynLookup.ListSymbols(File.ReadAllText(source)))
        Console.WriteLine(symbol);
    return 0;
}

if (args is ["--find-references", var refSource, var name] && File.Exists(refSource))
{
    foreach (var line in RoslynLookup.FindReferences(File.ReadAllText(refSource), name))
        Console.WriteLine(line);
    return 0;
}

if (args is ["--list-diagnostics", var diagSource] && File.Exists(diagSource))
{
    foreach (var diagnostic in RoslynLookup.ListDiagnostics(File.ReadAllText(diagSource)))
        Console.WriteLine(diagnostic);
    return 0;
}

Console.Error.WriteLine("Local read-only solution CLI. Use --list-tools, --list-projects <sln>, --list-packages <csproj> [Directory.Packages.props], --list-symbols <cs>, --find-references <cs> <name>, or --list-diagnostics <cs>.");
Console.Error.WriteLine("Or run: dotnet sln <solution> list   and   dotnet list <csproj> package");
return 1;

static IReadOnlyList<string> ListProjects(string text)
{
    if (text.Contains("<Solution", StringComparison.Ordinal))
    {
        return XDocument.Parse(text).Descendants()
            .Where(e => e.Name.LocalName == "Project")
            .Select(e => e.Attribute("Path")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToList();
    }

    return Regex.Matches(text, @"Project\(""[^""]+""\)\s*=\s*""[^""]+"",\s*""([^""]+\.csproj)""")
        .Select(match => match.Groups[1].Value.Replace('\\', '/'))
        .ToList();
}

static IReadOnlyList<(string Id, string? Version)> ListPackages(string projectText, string? packagesProps)
{
    var versions = new Dictionary<string, string>(StringComparer.Ordinal);
    if (packagesProps is not null)
    {
        foreach (var element in XDocument.Parse(packagesProps).Descendants())
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
