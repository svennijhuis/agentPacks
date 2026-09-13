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
        var root = SourceRoot();

        foreach (var pack in new[] { "dotnet", "rust", "typescript" })
        {
            AssertRouterLinksReference(
                Path.Combine(root, "plugins", pack, "skills", $"{pack}-build"),
                "references/commands.md",
                "Targeted verify first",
                "Contention constraints");
            AssertRouterLinksReference(
                Path.Combine(root, "plugins", pack, "skills", $"{pack}-test-patterns"),
                "references/commands.md",
                "Targeted verify first",
                "Contention constraints");
            AssertRouterLinksReference(
                Path.Combine(root, "plugins", pack, "skills", $"{pack}-review"),
                "references/checklist.md",
                "Process findings in this order");
        }

        AssertRouterLinksReference(
            Path.Combine(root, "plugins", "dotnet", "skills", "dotnet-build"),
            "references/commands.md",
            "dotnet restore <solution>",
            "NU1101");
        AssertRouterLinksReference(
            Path.Combine(root, "plugins", "rust", "skills", "rust-build"),
            "references/commands.md",
            "cargo check --workspace",
            "rust-toolchain");
        AssertRouterLinksReference(
            Path.Combine(root, "plugins", "typescript", "skills", "typescript-build"),
            "references/commands.md",
            "tsc --noEmit",
            "pnpm-lock.yaml");

        var packCheck = Path.Combine(root, "plugins", "pack-check", "skills", "pack-check");
        AssertRouterLinksReference(
            packCheck,
            "references/detect.md",
            "Stack: none detected",
            "<lang>-build",
            "<lang>-test-patterns",
            "Never resolve or request installation for a detected stack outside the current change's scope");
        AssertRouterLinksReference(packCheck, "references/packs.md");

        foreach (var path in AuthoredSkillFiles())
        {
            var skillDir = Directory.GetParent(path)!.FullName;
            var skillName = Directory.GetParent(path)!.Name;
            var text = File.ReadAllText(path);
            var frontmatter = ParseFrontmatter(path, text);
            var body = frontmatter.Body;
            var lines = body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
            var relative = RelativeToPlugins(path);

            if (skillName.Equals("squad", StringComparison.Ordinal))
            {
                Assert.True(
                    lines <= SquadLockedBodyLines,
                    $"{relative} body is {lines} lines; squad is locked at {SquadLockedBodyLines}.");
                Assert.Contains("references/", body, StringComparison.Ordinal);
                continue;
            }

            Assert.True(
                lines <= RouterBodyLines,
                $"{relative} body is {lines} lines; router cap is {RouterBodyLines}. Push detail into references/.");

            var referencesDir = Path.Combine(skillDir, "references");
            if (!Directory.Exists(referencesDir))
                continue;

            foreach (var file in Directory.GetFiles(referencesDir, "*.md", SearchOption.AllDirectories))
            {
                var href = Path.GetRelativePath(skillDir, file).Replace('\\', '/');
                if (href.StartsWith("references/standards/", StringComparison.Ordinal) ||
                    href.StartsWith("references/examples/", StringComparison.Ordinal))
                {
                    continue;
                }

                Assert.Contains(href, body, StringComparison.Ordinal);
                Assert.True(File.Exists(file), $"{relative} is missing {href}.");
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

    private static void AssertRouterLinksReference(
        string skillDir, string href, params string[] movedContent)
    {
        var skillPath = Path.Combine(skillDir, "SKILL.md");
        var relative = RelativeToPlugins(skillPath);
        var body = ParseFrontmatter(skillPath, File.ReadAllText(skillPath)).Body;
        Assert.Contains(href, body, StringComparison.Ordinal);

        var target = Path.GetFullPath(Path.Combine(skillDir, href));
        Assert.True(File.Exists(target), $"{relative} links to missing {href}.");

        var reference = File.ReadAllText(target);
        foreach (var token in movedContent)
        {
            Assert.Contains(token, reference, StringComparison.Ordinal);
            Assert.DoesNotContain(token, body, StringComparison.Ordinal);
        }
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
