using System.Text.RegularExpressions;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Item 52: 4-provider skill hygiene from Astra principles (short when-to-use
/// descriptions, progressive disclosure to <c>references/</c>, Matt-tiny routers).
/// Not Astra personality.
/// </summary>
public sealed class SkillHygieneContractTests
{
    private const int DescriptionMaxLength = 120;
    private const int MattTinyBodyLines = 16;
    private const int RouterBodyLines = 40;
    private const int SquadLockedBodyLines = 87;

    private static readonly string[] DescriptionEssayTokens =
    [
        "Internal loop skill",
        "Loaded by the Squad",
        "Do not model-invoke",
        "Type the",
        "not as a user entrypoint",
        "request approval before installing",
        "push harder",
        "ask less",
        "over-ask",
        "Astra"
    ];

    private static readonly string[] AstraPersonalityTokens =
    [
        "push harder",
        "ask less",
        "over-ask",
        "overask",
        "over ask",
        "don't over-ask",
        "do not over-ask",
        "don't over ask"
    ];

    [Fact]
    public void Skill_descriptions_are_short_when_to_use()
    {
        foreach (var path in AuthoredSkillFiles())
        {
            var text = File.ReadAllText(path);
            var frontmatter = ParseFrontmatter(path, text);
            var description = frontmatter.Scalar("description") ?? string.Empty;
            var relative = RelativeToPlugins(path);

            Assert.False(string.IsNullOrWhiteSpace(description), $"{relative} is missing description.");
            Assert.DoesNotContain('\n', description);
            Assert.StartsWith("When ", description, StringComparison.Ordinal);
            Assert.EndsWith(".", description, StringComparison.Ordinal);
            Assert.True(
                description.Length <= DescriptionMaxLength,
                $"{relative} description is {description.Length} characters; when-to-use cap is {DescriptionMaxLength}.");

            foreach (var token in DescriptionEssayTokens)
            {
                Assert.DoesNotContain(token, description, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Skill_bodies_progressive_disclosure_to_references()
    {
        foreach (var path in AuthoredSkillFiles())
        {
            var text = File.ReadAllText(path);
            var frontmatter = ParseFrontmatter(path, text);
            var body = frontmatter.Body;
            var lines = body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
            var relative = RelativeToPlugins(path);
            var skillName = Directory.GetParent(path)!.Name;

            if (skillName.Equals("squad", StringComparison.Ordinal))
            {
                Assert.True(
                    lines <= SquadLockedBodyLines,
                    $"{relative} body is {lines} lines; squad is locked at {SquadLockedBodyLines}.");
                Assert.Contains("references/", body, StringComparison.Ordinal);
            }
            else
            {
                Assert.True(
                    lines <= RouterBodyLines,
                    $"{relative} body is {lines} lines; router cap is {RouterBodyLines}. Push detail into references/.");
                if (lines > MattTinyBodyLines)
                    Assert.Contains("references/", body, StringComparison.Ordinal);
            }

            foreach (var href in MarkdownHrefs(body))
            {
                if (!href.Contains("references/", StringComparison.Ordinal) ||
                    href.Contains("references/standards/", StringComparison.Ordinal) ||
                    !href.EndsWith(".md", StringComparison.Ordinal))
                {
                    continue;
                }

                var target = Path.GetFullPath(Path.Combine(Directory.GetParent(path)!.FullName, href));
                Assert.True(File.Exists(target), $"{relative} links to missing {href}.");
            }
        }

        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    [Fact]
    public void No_astra_personality_in_portable_skills()
    {
        foreach (var path in AuthoredSkillTreeMarkdown())
        {
            var text = File.ReadAllText(path);
            var relative = RelativeToPlugins(path);

            Assert.False(
                Regex.IsMatch(text, @"\bAstra\b", RegexOptions.IgnoreCase),
                $"{relative} names Astra; portable skills take principles only.");

            foreach (var token in AstraPersonalityTokens)
            {
                Assert.DoesNotContain(token, text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static IEnumerable<string> AuthoredSkillFiles() =>
        Directory.GetFiles(Path.Combine(SourceRoot(), "plugins"), "SKILL.md", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal);

    private static IEnumerable<string> AuthoredSkillTreeMarkdown() =>
        Directory.GetFiles(Path.Combine(SourceRoot(), "plugins"), "*.md", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}skills{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal);

    private static Frontmatter ParseFrontmatter(string path, string text)
    {
        var parsed = Frontmatter.TryParse(text, out var error);
        Assert.True(parsed is not null, $"{RelativeToPlugins(path)} frontmatter: {error}");
        return parsed!;
    }

    private static IEnumerable<string> MarkdownHrefs(string body)
    {
        foreach (Match match in Regex.Matches(body, @"\[[^\]]*\]\(([^)]+)\)"))
            yield return match.Groups[1].Value;
    }

    private static string RelativeToPlugins(string path) =>
        Path.GetRelativePath(Path.Combine(SourceRoot(), "plugins"), path);

    private static string SourceRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "plugins")) &&
                Directory.Exists(Path.Combine(directory.FullName, "tools", "AgentPacks.Cli")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the agentPacks source root.");
    }
}
