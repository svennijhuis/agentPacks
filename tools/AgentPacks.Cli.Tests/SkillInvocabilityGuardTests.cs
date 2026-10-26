namespace AgentPacks.Cli.Tests;

/// <summary>
/// Guard: no skill becomes a user entrypoint by accident. User entry goes through the commands only.
/// </summary>
public sealed class SkillInvocabilityGuardTests
{
    // pack-check is the only skill that had no `user-invocable: false` on main e84d469; it backs
    // the /pack-check setup command. Every other skill on main, entrypoint or loop, already set it.
    private static readonly HashSet<string> UserInvocableAllowlist = new(StringComparer.Ordinal) { "pack-check" };

    // On main, user-entrypoint skills set `disable-model-invocation: true` and omit `audience: loop`
    // (docs/ADD-SKILL.md). Every other skill is loaded only by agents and must say `audience: loop`.
    private static readonly HashSet<string> EntrypointSkills = new(StringComparer.Ordinal)
    {
        "squad", "scenarios-md", "security-audit-md", "pack-author-md", "pack-check",
    };

    [Fact]
    public void Every_skill_is_not_user_invocable_and_loop_skills_say_audience_loop()
    {
        var root = TestRepository.SourceRoot();
        var output = Directory.CreateTempSubdirectory("skill-guard-").FullName;
        try
        {
            TestRepository.RunValidateAllOut(output);
            var failures = new List<string>();
            var sources = Directory.GetFiles(Path.Combine(root, "plugins"), "SKILL.md", SearchOption.AllDirectories)
                .Where(path => !TestRepository.IsGeneratedPath(Path.GetRelativePath(root, path)))
                .ToList();
            var generated = Directory.GetFiles(Path.Combine(output, "plugins"), "SKILL.md", SearchOption.AllDirectories);
            Assert.NotEmpty(sources);
            Assert.NotEmpty(generated);

            foreach (var path in sources.Concat(generated))
            {
                var name = Path.GetFileName(Path.GetDirectoryName(path))!;
                var frontmatter = TestRepository.FrontmatterYaml(File.ReadAllText(path));
                var where = path.StartsWith(output, StringComparison.Ordinal) ? "generated " + Path.GetRelativePath(output, path) : Path.GetRelativePath(root, path);
                if (!UserInvocableAllowlist.Contains(name) && !HasLine(frontmatter, "user-invocable: false"))
                {
                    failures.Add($"{where}: missing user-invocable: false");
                }

                if (!EntrypointSkills.Contains(name) && !HasLine(frontmatter, "audience: loop"))
                {
                    failures.Add($"{where}: missing audience: loop");
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            Directory.Delete(output, true);
        }
    }

    private static bool HasLine(string frontmatter, string line) =>
        frontmatter.Split('\n').Any(l => l.Trim() == line);
}
